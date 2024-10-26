using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

public class CollisionCallbackSystem : DSystem, IUpdateSystem
{
    QueryDescription query = new QueryDescription().WithAll<CollisionEvent,CollisionMeta,EventMeta>();
    QueryDescription mouseQuery = new QueryDescription().WithAll<MouseEvent,EventMeta>();
    Entity entity1 = null;
    Entity entity2 = null;
    public void Update(double dt)
    {
        List<MouseEvent> mouseEvents = new List<MouseEvent>();
        Engine.ECSWorld.Query(in mouseQuery, (Arch.Core.Entity e, ref MouseEvent m, ref EventMeta em) =>
        {
            if (!em.dirty)
            {
                // mouseEvents.Add(new MouseEvent(m.mouseState,m.mods,m.scrollX,m.scrollY));
                mouseEvents.Add(m);
            }
        });


        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref CollisionEvent ce, ref CollisionMeta cm, ref EventMeta em) =>
        {
            entity1 = Engine.GetEntity(ce.entity1ManagedID);
            entity2 = Engine.GetEntity(ce.entity2ManagedID);
            //make sure nothing else has instrcutred us to be destoryed so we are a "valid" destruction
            if (!entity1.StagedForDestruction && !entity2.StagedForDestruction)
            {
                switch (cm.state)
                {
                    case CollisionState.Starting:
                        entity1.ECSEntity.Get<Collider>().OnStart?.Invoke(entity1,entity2);
                        entity2.ECSEntity.Get<Collider>().OnStart?.Invoke(entity2,entity1);
                        break;
                    case CollisionState.Continuing:
                        entity1.ECSEntity.Get<Collider>().OnContinue?.Invoke(entity1,entity2);
                        entity2.ECSEntity.Get<Collider>().OnContinue?.Invoke(entity2, entity1);
                        break;
                    case CollisionState.Ending:
                        entity1.ECSEntity.Get<Collider>().OnEnd?.Invoke(entity1,entity2);
                        entity2.ECSEntity.Get<Collider>().OnEnd?.Invoke(entity2,entity1);
                        break;
                    case CollisionState.Invalid:
                        em.dirty = true;
                        break;
                }

                if(cm.state != CollisionState.Invalid && (entity1 == Engine.Cursor || entity2 == Engine.Cursor))
                {
                    PropogateMouseEvents(entity1.ECSEntity,entity2.ECSEntity,entity1 == Engine.Cursor,entity2 == Engine.Cursor,cm.state);
                }
            }
            else
            {
                cm.state = CollisionState.Invalid;
                em.dirty = true;
            }
        });

        void PropogateMouseEvents(Arch.Core.Entity e1, Arch.Core.Entity e2, bool e1IsCursor, bool e2IsCursor, CollisionState collisionState)
        {
            var target = e1IsCursor ? e2 : e1;
            var managedtarget = e1IsCursor ? entity2 : entity1;
            //we sort these events here by the enum value to ensure priority for callbacks
            foreach (var me in mouseEvents.OrderBy(x => x.mouseState))
            {
                switch (me.mouseState)
                {
                    case InputSystem.MouseState.Up:
                        target.Get<Collider>().OnMouseUp?.Invoke(managedtarget,me.mods);
                        break;
                    case InputSystem.MouseState.Pressed:
                        target.Get<Collider>().OnMousePressed?.Invoke(managedtarget,me.mods);
                        break;
                    case InputSystem.MouseState.Down:
                        target.Get<Collider>().OnMouseDown?.Invoke(managedtarget,me.mods);
                        break;
                    case InputSystem.MouseState.Scroll:
                        target.Get<Collider>().OnMouseScroll?.Invoke(managedtarget,me.mods,me.scrollX,me.scrollY);
                        break;
                }
            }

            switch (collisionState)
            {
                case CollisionState.Starting:
                    target.Get<Collider>().OnMouseEnter?.Invoke(managedtarget,Engine.InputSystem.FrameModifiers);
                    break;
                case CollisionState.Continuing:
                    target.Get<Collider>().OnMouseOver?.Invoke(managedtarget,Engine.InputSystem.FrameModifiers);
                    break;
                case CollisionState.Ending:
                    target.Get<Collider>().OnMouseExit?.Invoke(managedtarget,Engine.InputSystem.FrameModifiers);
                    break;
                case CollisionState.Invalid:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(collisionState), collisionState, null);
            }
        }
    }
}

public class CollisionSystem : DSystem, IUpdateSystem
{
    QueryDescription activeColliders = new QueryDescription().WithAll<ActiveState,EntityID,Collider,Position>().WithNone<Destroy>();
    QueryDescription colQuery = new QueryDescription().WithAll<EventMeta,CollisionMeta,CollisionEvent>();
    private List<(Arch.Core.Entity e,Collider c,int managedID)> colliders = new();
    
    private Dictionary<int, CollisionEvent> bufferedCollisionEvents = new();
    Entity entity1 = null;
    Entity entity2 = null;
    public void Update(double dt)
    {
        colliders.Clear();
        bufferedCollisionEvents.Clear();

        //run collision checks to find candiates for new collision events and add them to a buffer
        Engine.ECSWorld.Query(in activeColliders, (Arch.Core.Entity e, ref ActiveState a, ref EntityID managedID, ref Position p, ref Collider c) =>
        {
            e.Get<Collider>().ActiveCollisions.Clear(); //we clear active collisions here each frame, even if collider is inactive
            if(!c.Active || !a.Active){return;}
            //this checks for collisions between all active colliders
            //could be optimized to only check against colliders in the same scene / layer / etc
            for (int i = 0; i < colliders.Count; i++)
            {
                if (e.Id != colliders[i].e.Id  && Zinc.Collision.CheckCollision(managedID.ID,c,colliders[i].managedID,colliders[i].c))
                {
                    entity1 =  Engine.GetEntity(managedID.ID);
                    entity2 =  Engine.GetEntity(colliders[i].managedID);
                    //we hash off managed entity IDs, as these never repeat or recycle
                    //hash order is from lowest to highest ID
                    var hash = entity1.ID > entity2.ID ? HashCode.Combine(entity2.ID, entity1.ID) : HashCode.Combine(entity1.ID, entity2.ID);
                    var ce = new CollisionEvent(managedID.ID,colliders[i].managedID);
                    bufferedCollisionEvents.TryAdd(hash,ce);
                }
            }
            colliders.Add((e,c,managedID.ID));
        });
        
        //look through all current collision events and either update their state based on our buffered collsiions
        //or mark them for destruction if they are no longer valid
        Engine.ECSWorld.Query(in colQuery, (Arch.Core.Entity e, ref CollisionMeta cm, ref EventMeta em, ref CollisionEvent ce) =>
        {
            entity1 =  Engine.GetEntity(ce.entity1ManagedID);
            entity2 =  Engine.GetEntity(ce.entity2ManagedID);
            if (!entity1.StagedForDestruction && !entity2.StagedForDestruction && cm.state != CollisionState.Invalid)
            {
                em.dirty = false; //keep the event alive
                if (bufferedCollisionEvents.ContainsKey(cm.hash)) //if we have buffered a collision that already exists
                {
                    entity1.ECSEntity.Get<Collider>().ActiveCollisions.Add(entity2);
                    entity2.ECSEntity.Get<Collider>().ActiveCollisions.Add(entity1);

                    cm.state = CollisionState.Continuing;
                    bufferedCollisionEvents.Remove(cm.hash);
                }
                else
                {
                    cm.state = CollisionState.Ending;
                    em.dirty = true;
                }
            }
            else
            {
                //invalid collisions happen if one of the entites is destroyed as part of a callback
                //in which case we just mark this dirty and dont touch the state
                em.dirty = true;
                if (bufferedCollisionEvents.ContainsKey(cm.hash)) //if we have buffered a collision that already exists
                {
                    bufferedCollisionEvents.Remove(cm.hash);
                }
            }
        });
        
        
        Zinc.Entity e1,e2;
        foreach (var e in bufferedCollisionEvents)
        {
            e1 = Engine.GetEntity(e.Value.entity1ManagedID);
            e2 = Engine.GetEntity(e.Value.entity2ManagedID);
            e1.ECSEntity.Get<Collider>().ActiveCollisions.Add(e2);
            e2.ECSEntity.Get<Collider>().ActiveCollisions.Add(e1);
            Engine.ECSWorld.Create(
                new EventMeta(e.Value.GetType().ToString()),
                new CollisionMeta(e.Key),
                e.Value);
        }
    }
}
