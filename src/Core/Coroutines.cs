using System.Collections;

namespace Zinc.Core;

public static class Coroutines
{
    public static void Start(IEnumerator c, string name, Action completionCallback = null)
    {
        Engine.ECSWorld.Create(
            new Coroutine(c,name,completionCallback)
        );
    }

    // public static IEnumerator WaitForSeconds(float seconds)
    // {
    //     TimeSince ts = 0;
    //     while (ts < seconds)
    //     {
    //         yield return null;
    //     }
    // }
    
    // Base class for yield instructions
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

        public override bool KeepWaiting => runTime < duration;
    }
}