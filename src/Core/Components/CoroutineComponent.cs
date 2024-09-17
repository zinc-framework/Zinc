using System.Collections;

namespace Zinc;

public record struct Coroutine(IEnumerator coroutine, string name, Action completionCallback, Stack<IEnumerator> executionStack = null);