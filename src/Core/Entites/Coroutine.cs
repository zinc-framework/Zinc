using System.Collections;
using Zinc.Core;

namespace Zinc;

[Component<CoroutineComponent>]
public partial class Coroutine : SceneObject
{
    public Coroutine(IEnumerator coroutineMethod, string name = "coroutine", Action? completionCallback = null, bool startEnabled = true, Scene? scene = null) : base(startEnabled, scene)
    {
        CoroutineMethod = coroutineMethod;
        CoroutineName = name;
        CompletionCallback = completionCallback;
    }
}

public abstract class CustomYieldInstruction
{
    public abstract bool KeepWaiting { get; }
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

    public override bool KeepWaiting
    {
        get
        {
            Console.WriteLine($"{Engine.Time}");
            Console.WriteLine($"{runTime.time}");
            Console.WriteLine($"{(float)runTime} < {duration} : {(float)runTime < duration}");
            return runTime < duration;
        }
    }
}