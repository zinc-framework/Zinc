using System.Numerics;
using Arch.Core.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zinc.Core;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;
using Zinc.Internal.STB;

namespace Zinc;

public record Tag(string Value)
{
    public static implicit operator Tag(string value) => new(value);
}

[Component<EntityID>]
[Component<AdditionalEntityInfo>]
[Component<ActiveState>]
public partial class Entity
{
    public Arch.Core.EntityReference ECSEntityReference;
    public Arch.Core.Entity ECSEntity => ECSEntityReference.Entity;
    public HashSet<Tag> Tags = new();
    public Entity(bool startEnabled)
    {
        ECSEntityReference = Engine.ECSWorld.Reference(CreateECSEntity(Engine.ECSWorld));
        AssignDefaultValues();
        ID = Engine.GetNextEntityID();
        Engine.EntityLookup.Add(ID, this);
        Active = startEnabled;
    }

    public bool GetTags<T>(out List<T> tags) where T : Tag
    {
        tags = new List<T>();
        if(Tags.OfType<T>().Count() > 0)
        {
            tags = Tags.OfType<T>().ToList();
            return true;
        }
        return false;
    }
    public bool GetTag<T>(out T? tag) where T : Tag
    {
        tag = default;
        var tags = Tags.OfType<T>();
        if(tags.Count() > 0)
        {
            tag = tags.First();
            return true;
        }
        return false;
    }
    public bool Tagged<T>() => Tags.Any(t => t is T);
    public bool Tagged(Tag tag) => Tags.Contains(tag);
    public bool Tagged(params Tag[] tags)
    {
        foreach (var tag in tags)
        {
            if(!Tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }
    public bool NotTagged(params Tag[] tags)
    {
        foreach (var tag in tags)
        {
            if (Tags.Contains(tag))
            {
                return false;
            }
        }
        return true;
    }
    public void Tag(Tag tag) => Tags.Add(tag);
    public void Tag(params Tag[] tags) => Tags.UnionWith(tags);
    public void Untag(Tag tag) => Tags.Remove(tag);
    public void Untag(params Tag[] tags) => Tags.ExceptWith(tags);
    public bool StagedForDestruction => ECSEntity.Has<Destroy>();
    public void Destroy()
    {
        OnDestroy();
        ECSEntity.Add<Destroy>();
    }
    protected virtual void OnDestroy() {}
}