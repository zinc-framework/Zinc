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
    public bool Paused { get; set; } = false;
    public float Duration {get; set;} = 1.0f;
    protected float ElapsedTime { get; set; } = 0f;
    public abstract IEnumerator Wait();
}

// Implementation for waiting for a duration
public class WaitForSeconds : CustomYieldInstruction
{
    public WaitForSeconds(float duration)
    {
        Duration = duration;
    }

    public override IEnumerator Wait()
    {
        while (ElapsedTime < Duration)
        {
            if (!Paused)
            {
                ElapsedTime += (float)Engine.DeltaTime;
            }
            yield return null;
        }
        yield return null;
    }
}

public abstract class CustomYieldWithValue<T> : CustomYieldInstruction
{
    public Action<T>? ValueUpdated { get; set; }
    public CustomYieldWithValue(float duration = 1.0f, Action<T>? ValueUpdated = null)
    {
        Duration = duration;
        this.ValueUpdated = ValueUpdated;
    }

    public override IEnumerator Wait()
    {
        while (ElapsedTime < Duration)
        {
            if (!Paused)
            {
                ElapsedTime += (float)Engine.DeltaTime;
                ValueUpdated?.Invoke(GetSampleFromTime(ElapsedTime));
            }
            yield return null;
        }
        // Ensure we hit the final value
        ValueUpdated?.Invoke(GetSampleFromTime(Duration));
    }
    protected abstract T GetSampleFromTime(double time);
}


