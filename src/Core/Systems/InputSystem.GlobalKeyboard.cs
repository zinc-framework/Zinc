namespace Zinc;

// Keyboard input for a click-through window.
//
// Keyboard events only ever go to the focused window, and a click-through window can't be
// clicked to become one, so the moment the user touches another app sokol's KEY_DOWN/KEY_UP
// stop arriving for good. This is the keyboard half of the fix Engine.Frame applies to the
// mouse via DesktopWindow.TryGetCursorPosition: poll the OS instead of waiting on the window.
//
// The platform work lives in zinc_platform (DesktopWindow.TryGetKeysDown), which reports by
// sokol keycode, so this file is the same on every OS. Polling can't see OS auto-repeat, so
// Down fires once per physical press, alongside Pressed, rather than at the repeat rate.
public partial class InputSystem
{
    // One slot per Key value; the enum's numeric values are sokol's, and MENU is the last.
    static readonly int KeySlotCount = (int)Key.MENU + 1;
    static readonly Key[] PollableKeys =
        Enum.GetValues<Key>().Where(k => k != Key.INVALID).Distinct().ToArray();

    readonly byte[] polledNow = new byte[KeySlotCount];
    readonly bool[] polledKeyDown = new bool[KeySlotCount];
    bool anyPolledKeyDown;

    static List<Modifiers> PolledModifiers(ReadOnlySpan<byte> down)
    {
        var mods = new List<Modifiers>();
        if (down[(int)Key.LEFT_SHIFT] != 0 || down[(int)Key.RIGHT_SHIFT] != 0) mods.Add(Modifiers.SHIFT);
        if (down[(int)Key.LEFT_CONTROL] != 0 || down[(int)Key.RIGHT_CONTROL] != 0) mods.Add(Modifiers.CTRL);
        if (down[(int)Key.LEFT_ALT] != 0 || down[(int)Key.RIGHT_ALT] != 0) mods.Add(Modifiers.ALT);
        if (down[(int)Key.LEFT_SUPER] != 0 || down[(int)Key.RIGHT_SUPER] != 0) mods.Add(Modifiers.SUPER);
        return mods;
    }

    /// <summary>
    /// Read the keyboard from the OS and fire the same Key events a sokol KEY_DOWN/KEY_UP
    /// would. Called by Engine.Frame every frame; <paramref name="active"/> says whether the
    /// window is currently in the state where polling is the only source of key input
    /// (click-through on, focus elsewhere). When it goes inactive, any keys the poll reported
    /// down are released so they don't stay held forever.
    /// </summary>
    internal void PollGlobalKeyboard(bool active)
    {
        if (!active && !anyPolledKeyDown) return;

        if (active)
        {
            if (!DesktopWindow.TryGetKeysDown(polledNow)) return; // platform can't answer
        }
        else
        {
            Array.Clear(polledNow);
        }

        List<Modifiers>? mods = null;
        anyPolledKeyDown = false;
        foreach (var key in PollableKeys)
        {
            int slot = (int)key;
            bool down = polledNow[slot] != 0;
            bool was = polledKeyDown[slot];
            anyPolledKeyDown |= down;
            if (down == was) continue;
            polledKeyDown[slot] = down;
            mods ??= PolledModifiers(polledNow);
            if (down)
            {
                KeyPressed(key, mods);
                Events.Key.Pressed?.Invoke(key, mods);
                KeyDown(key, mods);
                Events.Key.Down?.Invoke(key, mods);
            }
            else
            {
                KeyUp(key, mods);
                Events.Key.Up?.Invoke(key, mods);
            }
        }
    }
}
