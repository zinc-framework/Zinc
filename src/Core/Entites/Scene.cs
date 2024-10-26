using Arch.Core;
using Arch.Core.Extensions;
using Zinc.Core;

namespace Zinc;

[Component<SceneComponent>]
public partial class Scene : Entity
{
    public class SceneRootAnchor(Scene scene) : Anchor(true, scene)
    {
        // we hide this ability for SceneRootAnchors
        new public void SetParent(Anchor parent) {}
    }
    public static implicit operator Anchor(Scene p) => p.root!; 
    private SceneRootAnchor? root;
    private int sceneRenderCounter = 0;
    public int GetNextSceneRenderCounter()
    {
        var curr = sceneRenderCounter;
        sceneRenderCounter++;
        return curr;
    }

    public SceneActiveStatus Status = SceneActiveStatus.Inactive; 
    public SceneMountStatus MountStatus = SceneMountStatus.Unmounted; 
    public SceneLoadStatus LoadStatus = SceneLoadStatus.Unloaded;

    public Scene(bool startEnabled = true) : base(startEnabled)
    {
        Engine.SceneLookup.Add(ID,this);
    }

    public virtual void Update(double dt){}
/*
 * mount -> load -> start -> unmount
 */
    public void Mount(int depth)
    {
        Engine.MountedScenes.Add(ID,depth);
        Engine.SceneEntityMap.Add(ID,[]);
        Console.WriteLine("mounting scene");
        root = new SceneRootAnchor(this);
        MountStatus = SceneMountStatus.Mounted;
    }

    public void Unmount(Action? callback = null)
    {
        if (MountStatus == SceneMountStatus.Mounted)
        {
            Console.WriteLine("unmounting scene");
            Entity staged;
            var sceneEntityIDs = new List<int>(Engine.SceneEntityMap[ID]);
            foreach (var eid in sceneEntityIDs)
            {
                Engine.TryGetEntity(eid,out staged);
                if(staged is Anchor) {continue;} //we handle anchors seperatly below
                staged.Destroy(); //destroys things like coroutines that are attached to a scene but not an anchor
            }
            Cleanup();
            root!.Destroy();
            root = null;
            Engine.SceneEntityMap[ID].Clear();
            Events.SceneUnmounted?.Invoke(this,callback);
        }
    }
    public void Load(Action loadedCallback)
    {
        if (LoadStatus != SceneLoadStatus.Loaded)
        {
            LoadStatus = SceneLoadStatus.Loading;
            Preload();
            LoadStatus = SceneLoadStatus.Loaded;
        }
        loadedCallback?.Invoke();
    }
    
    public virtual void Preload(){}
    public void Start(bool setAsTargetScene = true)
    {
        if (MountStatus != SceneMountStatus.Mounted)
        {
            Console.WriteLine("mount and load scene before starting it");
            return;
        }
        if (LoadStatus != SceneLoadStatus.Loaded)
        {
            if (LoadStatus == SceneLoadStatus.Loading)
            {
                Console.WriteLine("scene still loading");
            }
            else
            {
                Console.WriteLine("load scene before starting it");
            }
            return;
        }
        if (setAsTargetScene)
        {
            Engine.SetTargetScene(this);
        }
        Create();
        Status = SceneActiveStatus.Active;
    }
    public virtual void Create(){}
    public virtual void Cleanup(){}
}

public enum SceneLoadStatus
{
    Unloaded,
    Loading,
    Loaded
}

public enum SceneMountStatus
{
    Unmounted,
    Mounted
}
public enum SceneActiveStatus
{
    Inactive,
    Active
}