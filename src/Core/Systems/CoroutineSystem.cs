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
    QueryDescription coroutine = new QueryDescription().WithAll<EntityID,CoroutineComponent,ActiveState>().WithNone<Destroy>();
    public void Update(double dt)
    {
        CommandBuffer cb = new(Engine.ECSWorld);
        Engine.ECSWorld.Query(in coroutine, (Arch.Core.Entity e, ref ActiveState active, ref EntityID eID, ref CoroutineComponent cc) =>  {
            var managedEntity = Engine.GetEntity(eID.ID) as Coroutine;
            bool isPaused = !active.Active;
            // Ensure the stack is initialized
            if (cc.ExecutionStack == null)
            {
                cc.ExecutionStack = new Stack<IEnumerator>();
                cc.ExecutionStack.Push(cc.CoroutineMethod); // Assuming 'coroutine' is the initial IEnumerator
            }

            if (cc.ExecutionStack.Count > 0)
            {
                var currentCoroutine = cc.ExecutionStack.Peek();

                // If it's a CustomYieldInstruction, update its pause state
                if (currentCoroutine.Current is CustomYieldInstruction customYield)
                {
                    customYield.Paused = isPaused;
                }

                if (!isPaused && !currentCoroutine.MoveNext())
                {
                    cc.ExecutionStack.Pop(); // Finished, so remove it
                    if (cc.ExecutionStack.Count > 0)
                    {
                        return; // Exit early, as there's another coroutine to resume next update
                    }
                    // Otherwise, this was the last coroutine
                    cc.CompletionCallback?.Invoke();
                    Console.WriteLine("DESTROYING COROUTINE " + cc.CoroutineName);
                    cb.Add(in e, new Destroy());
                }
                else if(!isPaused)
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
                        cc.ExecutionStack.Push(nestedCoroutine);
                    }
                    else if (currentYield is CustomYieldInstruction newCustomYield)
                    {
                        cc.ExecutionStack.Push(newCustomYield.Wait());
                    }
                }
            }
        });
        cb.Playback();
    }
}