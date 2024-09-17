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

        Entity entity1 = null;
        Entity entity2 = null;
        bool e1IsCursor = false;
        bool e2IsCursor = false;
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref CollisionEvent ce, ref CollisionMeta cm, ref EventMeta em) =>
        {
            // Console.WriteLine("HANDLING COLLISION EVENT BETWEEN " + ce.e1.Entity.Get<HasManagedOwner>().e.Name + " and " + ce.e2.Entity.Get<HasManagedOwner>().e.Name + " WITH HASH " + cm.hash);
            if (CollisionEventValid(ref ce))
            {
                entity1 = Engine.EntityLookup[Engine.ECSEntityToManagedEntityIDLookup[ce.e1.Entity.Id]];
                entity2 = Engine.EntityLookup[Engine.ECSEntityToManagedEntityIDLookup[ce.e2.Entity.Id]];
                e1IsCursor = entity1 == Engine.Cursor;
                e2IsCursor = entity2 == Engine.Cursor;
                // bool e1IsCursor = Engine.Cursor.ECSEntityReference.Entity.Id == ce.e1.Entity.Id;
                // bool e2IsCursor = Engine.Cursor.ECSEntityReference.Entity.Id == ce.e2.Entity.Id;
                bool finalCollisionValid = false;
                switch (cm.state)
                {
                    case CollisionState.Starting:
                        ce.e1.Entity.Get<Collider>().OnStart?.Invoke(entity1,entity2);
                        if (CollisionEventValid(ref ce)) {
                            ce.e2.Entity.Get<Collider>().OnStart?.Invoke(entity2,entity1);
                            if (!CollisionEventValid(ref ce)) {
                                em.dirty = true;
                                cm.state = CollisionState.Invalid;
                            }
                            else
                            {
                                finalCollisionValid = true;
                            }
                        } 
                        else {
                            em.dirty = true;
                            cm.state = CollisionState.Invalid;
                        }
                        break;
                    case CollisionState.Continuing:
                        ce.e1.Entity.Get<Collider>().OnContinue?.Invoke(entity1,entity2);
                        if (CollisionEventValid(ref ce)) {
                            ce.e2.Entity.Get<Collider>().OnContinue?.Invoke(entity2, entity1);
                            if (!CollisionEventValid(ref ce)) {
                                em.dirty = true;
                                cm.state = CollisionState.Invalid;
                            }
                            else
                            {
                                finalCollisionValid = true;
                            }
                        }
                        else  {
                            em.dirty = true;
                            cm.state = CollisionState.Invalid;
                        }
    
                        break;
                    case CollisionState.Ending:
                        ce.e1.Entity.Get<Collider>().OnEnd?.Invoke(entity1,entity2);
                        if (CollisionEventValid(ref ce)) {
                            ce.e2.Entity.Get<Collider>().OnEnd?.Invoke(entity2,entity1);
                            if (!CollisionEventValid(ref ce)) {
                                em.dirty = true;
                                cm.state = CollisionState.Invalid;
                            }
                            else
                            {
                                finalCollisionValid = true;
                            }
                        }
                        else {
                            em.dirty = true;
                            cm.state = CollisionState.Invalid;
                        }
                        break;
                    case CollisionState.Invalid:
                        em.dirty = true;
                        break;
                }

                if (finalCollisionValid)
                {
                    PropogateMouseEvents(ce.e1.Entity,ce.e2.Entity,e1IsCursor,e2IsCursor,cm.state);
                }
            }
        });

        bool CollisionEventValid(ref CollisionEvent ce)
        {
            return entityCheck(ce.e1) && entityCheck(ce.e2);

            bool entityCheck(Arch.Core.EntityReference e)
            { 
                return e.Entity.IsAlive() && !e.Entity.Has<Destroy>() && /*(e.Entity.Has<Active>() ? e.Entity.Get<Active>().active : true) &&*/
                     //this is a hack that gets around a thing i think is happening in arch
                     //where if i destroy an entity as part of collider iteration, something about the collision event itself
                     //updates in memory to point to a different entity, so you get weird things where something is "colliding"
                     //with an event
                     //https://github.com/genaray/Arch/wiki/Utility-Features#command-buffers
                     //"Entity creation, deletion, and structural changes can potentially happen during a query or entity iteration.
                     //However, one must be careful about this, changes to entities during a query can easily lead to unexpected behavior.
                     //A destruction or structural change leads to a copy to another archetype and the current slot is
                     //replaced by another entity. This must always be expected. Depending on when and how you perform these
                     //operations in a query, this can lead to problems or not be noticed at all.
                     e.Entity.Has<Collider>();
            }
        }

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
                    // var hash = HashCode.Combine(e.Id, colliders[i].e.Id);
                    var order = new List<int> { e.Id, colliders[i].e.Id }.OrderDescending();
                    var hash = HashCode.Combine(order.First(), order.Last());
                    var ce = new CollisionEvent(Engine.ECSWorld.Reference(e),
                        Engine.ECSWorld.Reference(colliders[i].e));
                    if (!bufferedCollisionEvents.ContainsKey(hash) && CollisionEventValid(ref ce))
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
                if (CollisionEventValid(ref ce) && cm.state != CollisionState.Invalid)
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
        
        bool CollisionEventValid(ref CollisionEvent ce)
        {
            return ce.e1.IsAlive() && ce.e2.IsAlive() && !ce.e1.Entity.Has<Destroy>() && !ce.e2.Entity.Has<Destroy>();
        }
    }
}