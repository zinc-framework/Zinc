using System.Collections;
using Zinc.Core;

namespace Zinc;

[Component<CoroutineComponent>]
public partial class Coroutine : SceneObject
{
    public Coroutine(IEnumerator coroutineMethod, string name = "coroutine", Action? completionCallback = null, bool startAutomatically = true, Scene? scene = null) : base(startAutomatically, scene)
    {
        CoroutineMethod = coroutineMethod;
        CoroutineName = name;
        CompletionCallback = completionCallback;
    }

    public void Reset(bool startAfterReset = true)
    {
        ExecutionStack = null;
        Active = startAfterReset;
    }
    public void Start() => Active = true;
    public void Pause() => Active = false;
}

public abstract class CustomYieldInstruction
{
    public abstract IEnumerator Wait();
}


// Implementation for waiting for a duration
public class WaitForSeconds : CustomYieldInstruction
{
    private float duration;
    private TimeSince runTime;
    public WaitForSeconds(float duration)
    {
        this.duration = duration;
        runTime = 0;
    }

    public override IEnumerator Wait()
    {
        while (runTime < duration)
        {
            yield return null;
        }
    }
}