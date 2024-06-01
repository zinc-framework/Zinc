using Arch.Core.Extensions;

namespace Zinc;

public class ParticleEmitter : Entity
{
    public ParticleEmitterComponent.EmitterConfig Config;
    public ParticleEmitter(ParticleEmitterComponent.EmitterConfig config, Scene? scene = null, bool startEnabled = true) : base(startEnabled,scene)
    {
        Config = config;
        sceneRenderOrder = Scene.GetNextSceneRenderCounter();
        ECSEntity.Add(
            new RenderItem(sceneRenderOrder),
            new ParticleEmitterComponent(config));
    }
    
    private int sceneRenderOrder;
    public int SceneRenderOrder
    {
        get => sceneRenderOrder;
        set
        {
            ref var r = ref ECSEntity.Get<RenderItem>();
            r.renderOrder = value;
            sceneRenderOrder = value;
        }
    }
}