using Arch.Core;
using Zinc.Internal.Sokol;

namespace Zinc;

public partial class InputSystem : DSystem, IUpdateSystem
{
    internal class KeyBindingAttribute : Attribute
    {
        public Zinc.Key Key {get; private set;}
        public KeyBindingAttribute(Key key)
        {
            Key = key;
        }
    }
    
    internal class KeyStateAttribute : Attribute
    {
        public KeyState State {get; private set;}
        public KeyStateAttribute(KeyState state)
        {
            State = state;
        }
    }



    public static partial class Events
    {
        //could definitely source generate this
        [KeyBinding(Zinc.Key.SPACE)]
        public static class SPACE
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.APOSTROPHE)]
        public static class APOSTROPHE
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.COMMA)]
        public static class COMMA
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.MINUS)]
        public static class MINUS
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.PERIOD)]
        public static class PERIOD
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.SLASH)]
        public static class SLASH
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num0)]
        public static class Num0
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num1)]
        public static class Num1
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num2)]
        public static class Num2
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num3)]
        public static class Num3
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num4)]
        public static class Num4
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num5)]
        public static class Num5
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num6)]
        public static class Num6
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num7)]
        public static class Num7
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num8)]
        public static class Num8
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Num9)]
        public static class Num9
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.SEMICOLON)]
        public static class SEMICOLON
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.EQUAL)]
        public static class EQUAL
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.A)]
        public static class A
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.B)]
        public static class B
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.C)]
        public static class C
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.D)]
        public static class D
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.E)]
        public static class E
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F)]
        public static class F
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.G)]
        public static class G
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.H)]
        public static class H
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.I)]
        public static class I
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.J)]
        public static class J
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.K)]
        public static class K
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.L)]
        public static class L
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.M)]
        public static class M
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.N)]
        public static class N
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.O)]
        public static class O
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.P)]
        public static class P
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Q)]
        public static class Q
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.R)]
        public static class R
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.S)]
        public static class S
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.T)]
        public static class T
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.U)]
        public static class U
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.V)]
        public static class V
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.W)]
        public static class W
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.X)]
        public static class X
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Y)]
        public static class Y
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.Z)]
        public static class Z
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.LEFT_BRACKET)]
        public static class LEFT_BRACKET
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.BACKSLASH)]
        public static class BACKSLASH
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.RIGHT_BRACKET)]
        public static class RIGHT_BRACKET
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.GRAVE_ACCENT)]
        public static class GRAVE_ACCENT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.WORLD_1)]
        public static class WORLD_1
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.WORLD_2)]
        public static class WORLD_2
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.ESCAPE)]
        public static class ESCAPE
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.ENTER)]
        public static class ENTER
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.TAB)]
        public static class TAB
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.BACKSPACE)]
        public static class BACKSPACE
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.INSERT)]
        public static class INSERT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.DELETE)]
        public static class DELETE
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.RIGHT)]
        public static class RIGHT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.LEFT)]
        public static class LEFT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.DOWN)]
        public static class DOWN
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.UP)]
        public static class UP
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.PAGE_UP)]
        public static class PAGE_UP
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.PAGE_DOWN)]
        public static class PAGE_DOWN
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.HOME)]
        public static class HOME
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.END)]
        public static class END
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.CAPS_LOCK)]
        public static class CAPS_LOCK
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.SCROLL_LOCK)]
        public static class SCROLL_LOCK
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.NUM_LOCK)]
        public static class NUM_LOCK
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.PRINT_SCREEN)]
        public static class PRINT_SCREEN
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.PAUSE)]
        public static class PAUSE
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F1)]
        public static class F1
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F2)]
        public static class F2
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F3)]
        public static class F3
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F4)]
        public static class F4
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F5)]
        public static class F5
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F6)]
        public static class F6
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F7)]
        public static class F7
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F8)]
        public static class F8
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F9)]
        public static class F9
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F10)]
        public static class F10
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F11)]
        public static class F11
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F12)]
        public static class F12
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F13)]
        public static class F13
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F14)]
        public static class F14
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F15)]
        public static class F15
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F16)]
        public static class F16
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F17)]
        public static class F17
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F18)]
        public static class F18
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F19)]
        public static class F19
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F20)]
        public static class F20
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F21)]
        public static class F21
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F22)]
        public static class F22
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F23)]
        public static class F23
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F24)]
        public static class F24
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.F25)]
        public static class F25
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_0)]
        public static class KP_0
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_1)]
        public static class KP_1
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_2)]
        public static class KP_2
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_3)]
        public static class KP_3
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_4)]
        public static class KP_4
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_5)]
        public static class KP_5
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_6)]
        public static class KP_6
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_7)]
        public static class KP_7
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_8)]
        public static class KP_8
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_9)]
        public static class KP_9
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_DECIMAL)]
        public static class KP_DECIMAL
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_DIVIDE)]
        public static class KP_DIVIDE
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_MULTIPLY)]
        public static class KP_MULTIPLY
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_SUBTRACT)]
        public static class KP_SUBTRACT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_ADD)]
        public static class KP_ADD
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_ENTER)]
        public static class KP_ENTER
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.KP_EQUAL)]
        public static class KP_EQUAL
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.LEFT_SHIFT)]
        public static class LEFT_SHIFT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.LEFT_CONTROL)]
        public static class LEFT_CONTROL
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.LEFT_ALT)]
        public static class LEFT_ALT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.LEFT_SUPER)]
        public static class LEFT_SUPER
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.RIGHT_SHIFT)]
        public static class RIGHT_SHIFT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.RIGHT_CONTROL)]
        public static class RIGHT_CONTROL
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.RIGHT_ALT)]
        public static class RIGHT_ALT
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.RIGHT_SUPER)]
        public static class RIGHT_SUPER
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
        [KeyBinding(Zinc.Key.MENU)]
        public static class MENU
        {
            [KeyState(KeyState.Up)] public static Action<List<Modifiers>> Up;
            [KeyState(KeyState.Down)] public static Action<List<Modifiers>> Down;
            [KeyState(KeyState.Pressed)] public static Action<List<Modifiers>> Pressed;
            [KeyState(KeyState.Any)] public static Action<KeyState,List<Modifiers>> Event;
        }
    }
}