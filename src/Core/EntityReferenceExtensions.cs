using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc.Core;

public static class EntityReferenceExtensions
{
    public static void Destroy(this ref EntityReference e)
    {
        e.Entity.Add(new Destroy());
    }

    // should probably store EntityReference in scenemap?
    // public static void DestroyImmediate(this EntityReference e)
    // {
    //     Engine.SceneEntityMap[Scene].Remove(this);
    //     Engine.ECSWorld.Destroy(e.Entity);
    // }
}