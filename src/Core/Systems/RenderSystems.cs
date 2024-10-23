using Arch.Core;
using Arch.Core.Extensions;
using Zinc.Core;

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
                renderEntity =  Engine.GetEntity(item.managedID);
                if (renderEntity.ECSEntity.Has<SpriteRenderer>())
                {
                    ref var r = ref renderEntity.ECSEntity.Get<SpriteRenderer>();
                    if (!r.Texture.DataLoaded)
                    {
                        r.Texture.Load();
                    }
                    Engine.DrawTexturedRect(renderEntity as Anchor,r);
                }
                
                else if (renderEntity.ECSEntity.Has<ShapeRenderer>())
                {
                    ref var r = ref renderEntity.ECSEntity.Get<ShapeRenderer>();
                    Engine.DrawShape(renderEntity as Anchor, r);
                }
                
                else if (renderEntity.ECSEntity.Has<ParticleEmitterComponent>())
                {
                    ref var emitter = ref renderEntity.ECSEntity.Get<ParticleEmitterComponent>();
                    (renderEntity as Anchor).GetWorldTransform().transform.Decompose(out var world_pos, out var world_rotation, out var scale);
                    if(emitter.Config.EmitOnce && emitter.Config.HasEmit){return;}
                    var possibleParticleSlots = emitter.Config.EmissionRate * dt;
                    emitter.Accumulator += possibleParticleSlots;
                    var requestedNewParticles = (int)emitter.Accumulator > emitter.MaxParticles ? emitter.MaxParticles : (int)emitter.Accumulator;

                    var limit = emitter.Count + requestedNewParticles > emitter.MaxParticles ? emitter.MaxParticles : emitter.Count + requestedNewParticles;
                    //update particle lifetimes
                    for (int i = 0; i < limit; i++)
                    {
                        if(emitter.Active[i])
                        {
                            emitter.Age[i] += dt;
                            if (emitter.Age[i] > emitter.Config.Lifespan)
                            {
                                //turn off this particle, dont need it anymore
                                emitter.Active[i] = false;
                                emitter.Count--;
                                if (emitter.Count < emitter.MaxParticles)
                                {
                                    emitter.InitParticle(i,world_pos);
                                    requestedNewParticles--;
                                    emitter.Count++;
                                    emitter.Accumulator--;
                                }
                            }
                        }
                        else if (requestedNewParticles > 0 && emitter.Count < emitter.MaxParticles)
                        {
                            emitter.InitParticle(i,world_pos);
                            requestedNewParticles--;
                            emitter.Accumulator--;
                            emitter.Count++;
                        }
                    }

                    Engine.DrawParticles(renderEntity as Anchor,emitter,dt);
                }
            }
        }
    }
}