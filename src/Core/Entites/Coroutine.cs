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

    public Coroutine(CustomYieldInstruction coroutineMethod, string name = "coroutine", Action? completionCallback = null, bool startAutomatically = true, Scene? scene = null) : base(startAutomatically, scene)
    {
        CoroutineMethod = coroutineMethod.Wait();
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

// Waits out a pending window resize, so what follows can trust Engine.Width/Height.
//
// The engine already holds scene Create() and screenshots back on its own, so this is for code
// that resizes the window mid-run and wants to sequence against it - repositioning things after
// going fullscreen, say. It resolves on the first frame the size is real, and immediately if no
// resize is pending, so it is safe to yield unconditionally. It cannot outlast Engine's settle
// budget, so a resize that never arrives lets the coroutine continue rather than stranding it.
public class WaitForWindowResize : CustomYieldInstruction
{
    public override IEnumerator Wait()
    {
        while (Engine.WindowSettling)
        {
            yield return null;
        }
        yield return null;
    }
}

public abstract class CustomYieldWithValue<T> : CustomYieldInstruction
{
    public Action<T>? ValueUpdated { get; set; }
    public Action<float>? ProgressUpdated { get; set; }
    public CustomYieldWithValue(float duration = 1.0f, Action<T>? ValueUpdated = null,Action<float>? ProgressUpdated = null)
    {
        Duration = duration;
        this.ValueUpdated = ValueUpdated;
        this.ProgressUpdated = ProgressUpdated;
    }

    public override IEnumerator Wait()
    {
        while (ElapsedTime < Duration)
        {
            if (!Paused)
            {
                ElapsedTime += (float)Engine.DeltaTime;
                ProgressUpdated?.Invoke(ElapsedTime/Duration);
                ValueUpdated?.Invoke(GetSampleFromTime(ElapsedTime));
            }
            yield return null;
        }
        // Ensure we hit the final value
        ProgressUpdated?.Invoke(1.0f);
        ValueUpdated?.Invoke(GetSampleFromTime(Duration));
    }
    protected abstract T GetSampleFromTime(double time);
}

// Wait until a condition is true
public class WaitUntil : CustomYieldInstruction
{
    private readonly Func<bool> _condition;

    public WaitUntil(Func<bool> condition)
    {
        _condition = condition;
    }

    public override IEnumerator Wait()
    {
        while (!_condition())
        {
            yield return null;
        }
    }
}

// Wait while a condition is true
public class WaitWhile : CustomYieldInstruction
{
    private readonly Func<bool> _condition;

    public WaitWhile(Func<bool> condition)
    {
        _condition = condition;
    }

    public override IEnumerator Wait()
    {
        while (_condition())
        {
            yield return null;
        }
    }
}
