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
public partial class Entity
{
    public Arch.Core.EntityReference ECSEntityReference;
    public Arch.Core.Entity ECSEntity => ECSEntityReference.Entity;
    public Entity(bool startEnabled)
    {
        Console.WriteLine("Entity constructor");
        ECSEntityReference = Engine.ECSWorld.Reference(CreateECSEntity(Engine.ECSWorld));
        ID = Engine.GetNextEntityID();
        Engine.EntityLookup.Add(ID, this);
        Engine.ECSEntityToManagedEntityIDLookup.Add(ECSEntity.Id, ID);
        Active = startEnabled;
    }
    public void Destroy() => ECSEntity.Add<Destroy>();
    public void DestroyImmediate(bool removeFromECSWorld = true)
    {
        OnDestroy();
        Engine.EntityLookup.Remove(ID);
        Engine.ECSEntityToManagedEntityIDLookup.Remove(ECSEntity.Id);
        if(removeFromECSWorld)
        {
            Engine.ECSWorld.Destroy(ECSEntity);
        }
    }
    protected virtual void OnDestroy() {}
}

/// <summary>
/// An anchor is a point in a scene
/// </summary>
[Component<Position>]
[Component<SceneMember>]
public partial class Anchor : Entity
{
    public Scene Scene => Engine.SceneLookup[SceneID];
    public bool IsSceneRoot => this is Scene.SceneRootAnchor && Scene.Root == this;
    private Anchor parent;
    private List<Anchor> children = new();
    public Anchor(bool startEnabled, Scene? scene = null, Anchor? parent = null, List<Anchor>? children = null) 
        : base(startEnabled)
    {
        Console.WriteLine("Anchor constructor");
        SceneID = scene != null ? scene.ID : Engine.TargetScene.ID;
        Engine.SceneEntityMap[SceneID].Add(ID);
        Anchor? targetParent = null;
        if(!(this is Scene.SceneRootAnchor))
        {
            //we add ourselves to either the passed in parent or the root of the scene we are in
            //only scene root anchors get a null parent
            targetParent = parent != null ? parent : Engine.SceneLookup[SceneID].Root;
        }
        this.parent = targetParent!;
        if(targetParent != null)
        {
            targetParent.children.Add(this);
        }

        if(children != null)
        {
            foreach (var c in children)
            {
                c.SetParent(this);
            }
        }
    }

    public void SetParent(Anchor parent)
    {
        this.parent.children.Remove(this);
        parent.children.Add(this);
    }

    protected override void OnDestroy()
    {
        foreach (var c in children)
        {
            c.Destroy();
        }
        Engine.SceneEntityMap[SceneID].Remove(ID);
    }

    private Position LocalPosition => ECSEntity.Get<Position>();
    private Matrix3x2 LocalTransform => LocalPosition; //implicit conversion to matrix
    private Matrix3x2 GetWorldTransform(Position? offset = null)
    {
        Matrix3x2 worldTransform = !offset.HasValue ? LocalTransform : LocalPosition + offset.Value; 
        var currentAnchor = this;

        while (currentAnchor.parent != null)
        {
            worldTransform *= currentAnchor.parent.LocalTransform;
            currentAnchor = currentAnchor.parent;
        }

        return worldTransform;
    }

    public Position GetWorldPosition(Position? offset = null)
    {
        var worldTransform = GetWorldTransform(offset);
        // Extract position, scale, and rotation from the matrix
        System.Numerics.Vector2 position = worldTransform.Translation;
        System.Numerics.Vector2 scale = new System.Numerics.Vector2(
            (float)Math.Sqrt(worldTransform.M11 * worldTransform.M11 + worldTransform.M21 * worldTransform.M21),
            (float)Math.Sqrt(worldTransform.M12 * worldTransform.M12 + worldTransform.M22 * worldTransform.M22)
        );
        float rotation = (float)Math.Atan2(worldTransform.M21, worldTransform.M11);

        return new Position(position.X, position.Y, scale.X, scale.Y, rotation);
    }
}


[Component<UpdateListener>]
public partial class SceneEntity : Anchor
{
    public SceneEntity(bool startEnabled, Scene? scene = null, Action<Entity, double>? update = null, Anchor? parent = null, List<Anchor>? children = null) 
        : base(startEnabled, scene, parent,children)
    {
        Update = update;
    }
}