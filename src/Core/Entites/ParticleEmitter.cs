using Arch.Core.Extensions;

namespace Zinc;

[Component<RenderItem>]
[Component<ParticleEmitterComponent>("Emitter")]
public partial class ParticleEmitter : SceneEntity
{
    private readonly Action<Entity, double>? _updateWrapper;
    public ParticleEmitter(int maxParticles, ParticleEmitterConfig? config = null, Scene? scene = null, bool startEnabled = true, Action<ParticleEmitter,double>? update = null, Anchor? parent = null, List<Anchor>? children = null) 
        : base(startEnabled,scene,parent:parent,children:children)
    {
        RenderOrder = Scene.GetNextSceneRenderCounter();
        ECSEntity.Set(new ParticleEmitterComponent(maxParticles,config));

        if (update != null && _updateWrapper == null)
        {
            _updateWrapper = (baseEntity, dt) => update((ParticleEmitter)baseEntity, dt);
            Update = _updateWrapper;
        }
    }
}