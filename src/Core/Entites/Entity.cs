using System.Numerics;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;
using Zinc.Internal.STB;
using Volatile;

namespace Zinc;

[Component<EntityID>]
[Component<AdditionalEntityInfo>]
[Component<ActiveState>]
public partial class EntityBase
{
    public Arch.Core.EntityReference ECSEntityReference;
    public Arch.Core.Entity ECSEntity => ECSEntityReference.Entity;
    public EntityBase(bool startEnabled)
    {
        ECSEntityReference = Engine.ECSWorld.Reference(CreateECSEntity(Engine.ECSWorld));
        ID = Engine.GetNextEntityID();
        Engine.EntityLookup.Add(ID, this);
        Engine.ECSEntityToManagedEntityIDLookup.Add(ECSEntity.Id, ID);
        Active = startEnabled;
    }
    public void Destroy() => ECSEntity.Add<Destroy>();
    public void DestroyImmediate()
    {
        OnDestroy();
        Engine.EntityLookup.Remove(ID);
        Engine.ECSEntityToManagedEntityIDLookup.Remove(ECSEntity.Id);
        Engine.ECSWorld.Destroy(ECSEntity);
    }
    protected virtual void OnDestroy() {}
}

[Component<Position>]
[Component<SceneMember>]
public partial class SceneEntity : EntityBase
{
    public Scene Scene => Engine.SceneLookup[SceneID];
    public SceneEntity(bool startEnabled, Scene? scene = null) 
        : base(startEnabled)
    {
        SceneID = scene != null ? scene.ID : Engine.TargetScene.ID;
        Engine.SceneEntityMap[SceneID].Add(ID);
    }

    protected override void OnDestroy()
    {
        Engine.SceneEntityMap[SceneID].Remove(ID);
    }
}

[Component<UpdateListener>]
public partial class Entity : SceneEntity
{
    public Entity(bool startEnabled, Scene? scene = null, Action<EntityBase, double>? update = null) 
        : base(startEnabled, scene)
    {
        Update = update;
    }
}