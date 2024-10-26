using System.Collections;

namespace Zinc;
//should make this into an entity so it gets cleaned up properly when scenes are unloaded
public record struct CoroutineComponent(IEnumerator CoroutineMethod, string CoroutineName, Action CompletionCallback, Stack<IEnumerator> ExecutionStack = null) : IComponent;