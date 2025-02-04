using System.Numerics;
using Arch.Core.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zinc.Core;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;
using Zinc.Internal.STB;

namespace Zinc;


/// <summary>
/// A SceneObject is something that is associated with a scene
/// </summary>
[Component<SceneMember>]
public partial class SceneObject : Entity
{
    public Scene Scene => Engine.SceneLookup[SceneID];
    public SceneObject(bool startEnabled, Scene? scene = null) 
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