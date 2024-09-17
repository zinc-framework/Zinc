using System.Collections;
using System.Diagnostics;
using System.Numerics;
using Arch.CommandBuffer;
using Arch.Core;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Core.ImGUI;
using Zinc.Internal.Sokol;

namespace Zinc;

public class CoroutineSystem : DSystem, IUpdateSystem
{
    QueryDescription coroutine = new QueryDescription().WithAll<Coroutine>();
    public void Update(double dt)
    {
        CommandBuffer cb = new(Engine.ECSWorld);
        Engine.ECSWorld.Query(in coroutine, (Arch.Core.Entity e, ref Coroutine c) =>  {
            // Ensure the stack is initialized
            if (c.executionStack == null)
            {
                c.executionStack = new Stack<IEnumerator>();
                c.executionStack.Push(c.coroutine); // Assuming 'coroutine' is the initial IEnumerator
            }

            if (c.executionStack.Count > 0)
            {
                var currentCoroutine = c.executionStack.Peek();
                if (!currentCoroutine.MoveNext())
                {
                    c.executionStack.Pop(); // Finished, so remove it
                    if (c.executionStack.Count > 0)
                    {
                        return; // Exit early, as there's another coroutine to resume next update
                    }
                    // Otherwise, this was the last coroutine
                    c.completionCallback?.Invoke();
                    Console.WriteLine("DESTROYING COROUTINE " + c.name);
                    cb.Add(in e, new Destroy());
                }
                else
                {
                    // Handle yield return of another IEnumerator
                    var currentYield = currentCoroutine.Current;
                    if (currentYield == null)
                    {
                        // yield return null;
                        // Handle waiting for the next frame
                    }
                    else if (currentYield is IEnumerator nestedCoroutine)
                    {
                        c.executionStack.Push(nestedCoroutine);
                    }
                    else if (currentYield is Coroutines.CustomYieldInstruction customYield)
                    {
                        if (!customYield.KeepWaiting)
                        {
                            c.executionStack.Pop();
                        }
                    }
                }
            }
        });
        cb.Playback();
    }
}