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

public abstract class RenderSystem : DSystem, IPostUpdateSystem
{
    public void PostUpdate(double dt)
    {
        Render(dt);
    }

    protected abstract void Render(double dt);
}

public class SceneRenderSystem : RenderSystem
{
    QueryDescription scenes = new QueryDescription().WithAll<ActiveState,EntityID,SceneComponent>(); 
    QueryDescription validRenderEntites = new QueryDescription().WithAll<ActiveState,EntityID,SceneMember,RenderItem,Position>();      

    
    QueryDescription renderedSprites = new QueryDescription().WithAll<ActiveState,Position,SpriteRenderer,SceneMember>();      
    QueryDescription renderedShapes = new QueryDescription().WithAll<ActiveState,Position,ShapeRenderer,SceneMember>();      
    QueryDescription renderedParticles = new QueryDescription().WithAll<ActiveState,Position,ParticleEmitterComponent,SceneMember>();      

    
    private List<Scene> scenesToUpdate = new List<Scene>();
    protected override void Render(double dt)
    {
        scenesToUpdate.Clear();
        Engine.ECSWorld.Query(in scenes, (Arch.Core.Entity e, ref ActiveState a, ref EntityID managedID, ref SceneComponent scene) => {
            if(!a.Active){return;}
            if (Engine.MountedScenes.ContainsKey(managedID.ID) && Engine.SceneLookup[managedID.ID].Status == SceneActiveStatus.Active)
            {
                scenesToUpdate.Add(Engine.SceneLookup[managedID.ID]);
            }
        });
        
        //note that entites are implicitly added to the global scene if no explicit scene is set, so every entity is in a scene
        foreach (var updatingScene in scenesToUpdate.OrderByDescending(scene => Engine.MountedScenes[scene.ID]))
        {
            List< (int managedID, RenderItem renderItem)> sceneOrderedEntities = new ();
            Engine.ECSWorld.Query(in validRenderEntites, (Arch.Core.Entity e, ref ActiveState a, ref EntityID managedID, ref SceneMember sceneInfo, ref RenderItem r) =>
            {
                if(!a.Active || sceneInfo.SceneID != updatingScene.ID){return;}
                sceneOrderedEntities.Add((managedID.ID,r));
            });

            Zinc.Entity renderEntity;
            foreach (var item in sceneOrderedEntities.OrderByDescending(x => x.renderItem.RenderOrder))
            {
                renderEntity = Engine.EntityLookup[item.managedID];
                if (renderEntity.ECSEntity.Has<SpriteRenderer>())
                {
                    ref var r = ref renderEntity.ECSEntity.Get<SpriteRenderer>();
                    if (!r.Texture.DataLoaded)
                    {
                        r.Texture.Load();
                    }
                    Engine.DrawTexturedRect((renderEntity as Anchor).GetWorldPosition(new Position(-r.PivotX,-r.PivotY)),r);
                }
                
                else if (renderEntity.ECSEntity.Has<ShapeRenderer>())
                {
                    ref var r = ref renderEntity.ECSEntity.Get<ShapeRenderer>();
                    Engine.DrawShape((renderEntity as Anchor).GetWorldPosition(), r);
                }
                
                else if (renderEntity.ECSEntity.Has<ParticleEmitterComponent>())
                {
                    var p = (renderEntity as Anchor).GetWorldPosition();
                    ref var emitter = ref renderEntity.ECSEntity.Get<ParticleEmitterComponent>();
                     // Update the particles
                    List<int> activeIndices = new List<int>();
                    // List<int> newIndicies = new List<int>();
                    var possibleParticleSlots = emitter.Config.EmissionRate * dt;
                    emitter.Accumulator += possibleParticleSlots;
                    var freeSlots = (int)emitter.Accumulator;

                    // Reactivate old inactive particles if possible
                    for (int i = 0; i < emitter.Particles.Count; i++)
                    {
                        if (emitter.Particles[i].Active)
                        {
                            emitter.Particles[i].Age += dt;
                            if (emitter.Particles[i].Age > emitter.Config.ParticleConfig.Lifespan)
                            {
                                //staged for inactive
                                if (freeSlots == 0)
                                {
                                    //go inactive if no free slots
                                    emitter.Particles[i].Active = false;
                                    continue;
                                }
                                
                                //otherwise remake this particle
                                emitter.Particles[i].Reset();
                                emitter.Particles[i].Config = new ParticleEmitterComponent.ParticleConfig(emitter.Config.ParticleConfig);
                                emitter.Particles[i].Config.EmissionPoint = new(p.X, p.Y);
                                emitter.Particles[i].Resolve();
                        
                                activeIndices.Add(i);
                                freeSlots--;
                                emitter.Accumulator--;
                            }

                            else
                            {
                                //particle still active
                                emitter.Particles[i].Resolve();
                                emitter.Particles[i].X += emitter.Particles[i].DX;
                                emitter.Particles[i].Y += emitter.Particles[i].DY;
                                activeIndices.Add(i);
                            }
                        }
                        else if (freeSlots > 0)
                        {
                            //reset and toggle to active if there is a slot 
                            emitter.Particles[i].Reset();
                            emitter.Particles[i].Active = true;
                            emitter.Particles[i].Config = new ParticleEmitterComponent.ParticleConfig(emitter.Config.ParticleConfig);
                            emitter.Particles[i].Config.EmissionPoint = new(p.X, p.Y);
                            emitter.Particles[i].Resolve();
                        
                            activeIndices.Add(i);
                            freeSlots--;
                            emitter.Accumulator--;
                        }
                    }

                    // Create new particles if needed and maximum limit is not reached
                    while (freeSlots > 0 && emitter.Particles.Count < emitter.Config.MaxParticles)
                    {
                        ParticleEmitterComponent.Particle newParticle = new();
                        newParticle.Active = true;
                        emitter.Particles.Add(newParticle);
                        newParticle.Resolve();
                        activeIndices.Add(emitter.Particles.Count - 1);
                        freeSlots--;
                        emitter.Accumulator--;
                    }

                    // Draw the particles
                    if (activeIndices.Count > 0)
                    {
                        Engine.DrawParticles(p, emitter, activeIndices);
                    }
                }
            }
        }
    }
}

public class SpriteRenderSystem : RenderSystem
{
    QueryDescription query = new QueryDescription().WithAll<ActiveState,Position,SpriteRenderer>();      // Should have all specified components
    protected override void Render(double dt)
    {
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref ActiveState a, ref SpriteRenderer r, ref Position p) =>
        {
            if(!a.Active){return;}
            if (!r.Texture.DataLoaded)
            {
                r.Texture.Load();
            }
            Engine.DrawTexturedRect(p,r);
        });
    }
}

public class ShapeRenderSystem : RenderSystem
{
    QueryDescription query = new QueryDescription().WithAll<ActiveState,Position,ShapeRenderer>();      // Should have all specified components
    protected override void Render(double dt)
    {
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e,ref ActiveState a,  ref ShapeRenderer r, ref Position p) =>
        {
            if(!a.Active){return;}
            Engine.DrawShape(p, r);
        });
    }
}

public class ParticleRenderSystem : RenderSystem
{
    QueryDescription query = new QueryDescription().WithAll<ActiveState,Position,ParticleEmitterComponent>();      // Should have all specified components
    protected override void Render(double dt)
    {
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref Position p,ref ActiveState a,  ref ParticleEmitterComponent emitter) =>
        {
            if(!a.Active){return;}
            // Update the particles
            List<int> activeIndices = new List<int>();
            // List<int> newIndicies = new List<int>();
            var possibleParticleSlots = emitter.Config.EmissionRate * dt;
            emitter.Accumulator += possibleParticleSlots;
            var freeSlots = (int)emitter.Accumulator;

            // Reactivate old inactive particles if possible
            for (int i = 0; i < emitter.Particles.Count; i++)
            {
                if (emitter.Particles[i].Active)
                {
                    emitter.Particles[i].Age += dt;
                    if (emitter.Particles[i].Age > emitter.Config.ParticleConfig.Lifespan)
                    {
                        //staged for inactive
                        if (freeSlots == 0)
                        {
                            //go inactive if no free slots
                            emitter.Particles[i].Active = false;
                            continue;
                        }
                        
                        //otherwise remake this particle
                        emitter.Particles[i].Reset();
                        emitter.Particles[i].Config = new ParticleEmitterComponent.ParticleConfig(emitter.Config.ParticleConfig);
                        emitter.Particles[i].Config.EmissionPoint = new(p.X, p.Y);
                        emitter.Particles[i].Resolve();
                
                        activeIndices.Add(i);
                        freeSlots--;
                        emitter.Accumulator--;
                    }

                    else
                    {
                        //particle still active
                        emitter.Particles[i].Resolve();
                        emitter.Particles[i].X += emitter.Particles[i].DX;
                        emitter.Particles[i].Y += emitter.Particles[i].DY;
                        activeIndices.Add(i);
                    }
                }
                else if (freeSlots > 0)
                {
                    //reset and toggle to active if there is a slot 
                    emitter.Particles[i].Reset();
                    emitter.Particles[i].Active = true;
                    emitter.Particles[i].Config = new ParticleEmitterComponent.ParticleConfig(emitter.Config.ParticleConfig);
                    emitter.Particles[i].Config.EmissionPoint = new(p.X, p.Y);
                    emitter.Particles[i].Resolve();
                
                    activeIndices.Add(i);
                    freeSlots--;
                    emitter.Accumulator--;
                }
            }

            // Create new particles if needed and maximum limit is not reached
            while (freeSlots > 0 && emitter.Particles.Count < emitter.Config.MaxParticles)
            {
                ParticleEmitterComponent.Particle newParticle = new();
                newParticle.Active = true;
                emitter.Particles.Add(newParticle);
                newParticle.Resolve();
                activeIndices.Add(emitter.Particles.Count - 1);
                freeSlots--;
                emitter.Accumulator--;
            }

            // Draw the particles
            if (activeIndices.Count > 0)
            {
                Engine.DrawParticles(p, emitter, activeIndices);
            }
        });
    }
}