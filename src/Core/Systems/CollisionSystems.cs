using System.Collections;
using System.Diagnostics;
using System.Numerics;
using Arch.CommandBuffer;
using Arch.Core;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Core.ImGUI;
using Zinc.Internal.Sokol;

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
            // Console.WriteLine("HANDLING COLLISION EVENT BETWEEN " + ce.e1.Entity.Get<HasManagedOwner>().e.Name + " and " + ce.e2.Entity.Get<HasManagedOwner>().e.Name + " WITH HASH " + cm.hash);
            entity1 = Engine.EntityLookup[ce.entity1ManagedID];
            entity2 = Engine.EntityLookup[ce.entity2ManagedID];
            //make sure nothing else has instrcutred us to be destoryed so we are a "valid" destruction
            if (!entity1.ECSEntity.Has<Destroy>() && !entity2.ECSEntity.Has<Destroy>())
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

                if(cm.state != CollisionState.Invalid)
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
            if (e1IsCursor || e2IsCursor)
            {
                var target = e1IsCursor ? e2 : e1;
                var managedtarget = e1IsCursor ? entity2 : entity1;
                foreach (var me in mouseEvents)
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
}

 public class CollisionSystem : DSystem, IUpdateSystem
{
    QueryDescription query = new QueryDescription().WithAll<ActiveState,EntityID,Collider,Position>().WithNone<Destroy>();
    QueryDescription colQuery = new QueryDescription().WithAll<EventMeta,CollisionMeta,CollisionEvent>();
    private List<(Arch.Core.Entity e,Collider c,int managedID)> colliders = new();
    
    private Dictionary<int, CollisionEvent> bufferedCollisionEvents = new();
    Entity entity1 = null;
    Entity entity2 = null;
    public void Update(double dt)
    {
        colliders.Clear();
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref ActiveState a, ref EntityID managedID, ref Position p, ref Collider c) =>
        {
            if(!c.Active || !a.Active){return;}
            for (int i = 0; i < colliders.Count; i++)
            {
                if (e.Id != colliders[i].e.Id && Zinc.Collision.CheckCollision(managedID.ID,colliders[i].c,colliders[i].managedID,colliders[i].c))
                {
                    entity1 = Engine.EntityLookup[managedID.ID];
                    entity2 = Engine.EntityLookup[colliders[i].managedID];
                    // var hash = HashCode.Combine(e.Id, colliders[i].e.Id);
                    var order = new List<int> { e.Id, colliders[i].e.Id }.OrderDescending();
                    var hash = HashCode.Combine(order.First(), order.Last());
                    var ce = new CollisionEvent(managedID.ID,colliders[i].managedID);
                    if (!bufferedCollisionEvents.ContainsKey(hash) && CollisionEventValid(entity1,entity2))
                    {
                        bufferedCollisionEvents.Add(
                            hash, ce);
                    }
                }
            }
            colliders.Add((e,c,managedID.ID));
        });
        
        Engine.ECSWorld.Query(in colQuery,
            (Arch.Core.Entity e, ref CollisionMeta cm, ref EventMeta em, ref CollisionEvent ce) =>
            {
                entity1 = Engine.EntityLookup[ce.entity1ManagedID];
                entity2 = Engine.EntityLookup[ce.entity2ManagedID];
                if (CollisionEventValid(entity1,entity2) && cm.state != CollisionState.Invalid)
                {
                    em.dirty = false; //keep the event alive
                    if (bufferedCollisionEvents
                        .ContainsKey(cm.hash)) //if we have buffered a collision that already exists
                    {
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
                    if (bufferedCollisionEvents
                        .ContainsKey(cm.hash)) //if we have buffered a collision that already exists
                    {
                        bufferedCollisionEvents.Remove(cm.hash);
                    }
                }
            });
        
        

        foreach (var e in bufferedCollisionEvents)
        {
            // Console.WriteLine("SPAWNING NEW COLLISION EVENT BETWEEN " + e.Value.e1.Entity.Get<HasManagedOwner>().e.Name + " and " + e.Value.e2.Entity.Get<HasManagedOwner>().e.Name + " WITH HASH " + e.Key);
            Engine.ECSWorld.Create(
                new EventMeta(e.Value.GetType().ToString()),
                new CollisionMeta(e.Key),
                e.Value);
        }
        
        bool CollisionEventValid(Zinc.Entity e1, Zinc.Entity e2)
        {
            return !e1.ECSEntity.Has<Destroy>() && !e2.ECSEntity.Has<Destroy>();
        }
    }
}