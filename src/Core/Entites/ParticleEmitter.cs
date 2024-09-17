using Arch.Core.Extensions;

namespace Zinc;

[Component<RenderItem>]
[Component<ParticleEmitterComponent>("Emitter")]
public partial class ParticleEmitter : SceneEntity
{
    public ParticleEmitterComponent.EmitterConfig Config;
    private readonly Action<Entity, double>? _updateWrapper;
    public ParticleEmitter(ParticleEmitterComponent.EmitterConfig config, Scene? scene = null, bool startEnabled = true, Action<ParticleEmitter,double>? update = null, Anchor? parent = null, List<Anchor>? children = null) 
        : base(startEnabled,scene,parent:parent,children:children)
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