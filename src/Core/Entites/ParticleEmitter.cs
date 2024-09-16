using Arch.Core.Extensions;

namespace Zinc;

[Component<RenderItem>]
[Component<ParticleEmitterComponent>("Emitter")]
public partial class ParticleEmitter : Entity
{
    public ParticleEmitterComponent.EmitterConfig Config;
    private readonly Action<EntityBase, double>? _updateWrapper;
    public ParticleEmitter(ParticleEmitterComponent.EmitterConfig config, Scene? scene = null, bool startEnabled = true, Action<ParticleEmitter,double>? update = null) : base(startEnabled,scene)
    {
        Config = config;
        RenderOrder = Scene.GetNextSceneRenderCounter();
        ECSEntity.Set(new ParticleEmitterComponent(config));

        if (update != null)
        {
            _updateWrapper = (baseEntity, dt) => update((ParticleEmitter)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}