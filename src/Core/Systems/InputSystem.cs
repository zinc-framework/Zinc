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

public class InputSystem : DSystem, IUpdateSystem
{
    QueryDescription frameEvents = new QueryDescription().WithAll<EventMeta, FrameEvent>(); // Should have all specified components
    public enum MouseState
    {
        Up,
        Pressed,
        Scroll,
        Down
    }
    public static float MouseX;
    public static float MouseY;
    public static class Events
    {
        public static class Key
        {
            public static Action<Zinc.Key,List<Modifiers>> Pressed;
            public static Action<Zinc.Key, List<Modifiers>> Down;
            public static Action<Zinc.Key, List<Modifiers>> Up;
            public static Action<uint> Char;
        }

        public static class Mouse
        {
            public static Action<List<Modifiers>> Down;
            public static Action<List<Modifiers>> Pressed;
            public static Action<List<Modifiers>> Up;
            public static Action<float,float,float,float,List<Modifiers>> Move;
            public static Action<float,float,List<Modifiers>> Scroll;
        }

        public static class Window
        {
            public static Action<List<Modifiers>> MouseEnter;
            public static Action<List<Modifiers>> MouseLeave;
            public static Action Resized;
            public static Action Focused;
            public static Action Unfocused;
            public static Action Suspended;
            public static Action Resumed;
            public static Action RequestedQuit;
        }
    }

    private bool lmb_up = true;
    private bool rmb_up = true;
    private bool mmb_up = true;
    public List<Modifiers> FrameModifiers;
    public void Update(double dt)
    {
        Engine.ECSWorld.Query(in frameEvents, (Arch.Core.Entity entity, ref EventMeta em, ref FrameEvent frameEvent) =>
        {
            if(!em.dirty)
            {
                FrameModifiers = GetModifier(frameEvent.e.modifiers);
                switch (frameEvent.e.type)
                {
                    case sapp_event_type.SAPP_EVENTTYPE_INVALID:
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_KEY_DOWN:
                        if (frameEvent.e.key_repeat > 0)
                        {
                            Events.Key.Pressed?.Invoke((Key)frameEvent.e.key_code,FrameModifiers);
                        }
                        Events.Key.Down?.Invoke((Key)frameEvent.e.key_code,FrameModifiers);
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_KEY_UP:
                        Events.Key.Up?.Invoke((Key)frameEvent.e.key_code,FrameModifiers);
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_CHAR:
                        Events.Key.Char?.Invoke(frameEvent.e.char_code);
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_MOUSE_DOWN:

                        MouseButton downButton = frameEvent.e.mouse_button switch
                        {
                            sapp_mousebutton.SAPP_MOUSEBUTTON_LEFT => downButton = MouseButton.LEFT,
                            sapp_mousebutton.SAPP_MOUSEBUTTON_RIGHT => downButton = MouseButton.RIGHT,
                            sapp_mousebutton.SAPP_MOUSEBUTTON_MIDDLE => downButton = MouseButton.MIDDLE,
                            _ => MouseButton.INVALID,
                        };

                        bool createPressedEvent = false;
                        switch (downButton)
                        {
                            case MouseButton.MIDDLE when mmb_up:
                                createPressedEvent = true;
                                mmb_up = false;
                                break;
                            case MouseButton.LEFT when lmb_up:
                                createPressedEvent = true;
                                lmb_up = false;
                                break;
                            case MouseButton.RIGHT when rmb_up:
                                createPressedEvent = true;
                                rmb_up = false;
                                break;
                        }

                        if (createPressedEvent)
                        {
                            Events.Mouse.Pressed?.Invoke(FrameModifiers);
                            Engine.ECSWorld.Create(
                                new EventMeta("MOUSE_PRESSED"),
                                new MouseEvent(MouseState.Pressed,downButton,FrameModifiers));
                        }
                        Events.Mouse.Down?.Invoke(FrameModifiers);
                        Engine.ECSWorld.Create(
                            new EventMeta("MOUSE_DOWN"),
                            new MouseEvent(MouseState.Down,downButton,FrameModifiers));
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_MOUSE_UP:
                        
                        MouseButton upButton = frameEvent.e.mouse_button switch
                        {
                            sapp_mousebutton.SAPP_MOUSEBUTTON_LEFT => downButton = MouseButton.LEFT,
                            sapp_mousebutton.SAPP_MOUSEBUTTON_RIGHT => downButton = MouseButton.RIGHT,
                            sapp_mousebutton.SAPP_MOUSEBUTTON_MIDDLE => downButton = MouseButton.MIDDLE,
                            _ => MouseButton.INVALID,
                        };

                        switch (upButton)
                        {
                            case MouseButton.MIDDLE:
                                mmb_up = true;
                                break;
                            case MouseButton.LEFT:
                                lmb_up = true;
                                break;
                            case MouseButton.RIGHT:
                                rmb_up = true;
                                break;
                        }
                        Events.Mouse.Up?.Invoke(FrameModifiers);
                        Engine.ECSWorld.Create(
                            new EventMeta("MOUSE_UP"),
                            new MouseEvent(MouseState.Up,upButton,FrameModifiers));
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_MOUSE_SCROLL:
                        Events.Mouse.Scroll?.Invoke(frameEvent.e.scroll_x,frameEvent.e.scroll_y,FrameModifiers);
                        Engine.ECSWorld.Create(
                            new EventMeta("MOUSE_SCROLL"),
                            new MouseEvent(MouseState.Scroll, MouseButton.INVALID, FrameModifiers, frameEvent.e.scroll_x, frameEvent.e.scroll_y));
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_MOUSE_MOVE:
                        MouseX = frameEvent.e.mouse_x;
                        MouseY = frameEvent.e.mouse_y;
                        Events.Mouse.Move?.Invoke(frameEvent.e.mouse_x,frameEvent.e.mouse_y,frameEvent.e.mouse_dx,frameEvent.e.mouse_dy,FrameModifiers);
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_MOUSE_ENTER:
                        Events.Window.MouseEnter?.Invoke(FrameModifiers);
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_MOUSE_LEAVE:
                        Events.Window.MouseLeave?.Invoke(FrameModifiers);
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_TOUCHES_BEGAN:
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_TOUCHES_MOVED:
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_TOUCHES_ENDED:
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_TOUCHES_CANCELLED:
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_RESIZED:
                        Events.Window.Resized?.Invoke();
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_ICONIFIED:
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_RESTORED:
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_FOCUSED:
                        Events.Window.Focused?.Invoke();
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_UNFOCUSED:
                        Events.Window.Unfocused?.Invoke();
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_SUSPENDED:
                        Events.Window.Suspended?.Invoke();
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_RESUMED:
                        Events.Window.Resumed?.Invoke();
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_QUIT_REQUESTED:
                        Events.Window.RequestedQuit?.Invoke();
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_CLIPBOARD_PASTED:
                        break;
                    case sapp_event_type.SAPP_EVENTTYPE_FILES_DROPPED:
                        break;
                }
            em.dirty = true;
            }
        });
    }
    
    List<Modifiers> GetModifier(uint v)
    {
        var mods = new List<Modifiers>();
        if ((v & App.SAPP_MODIFIER_SHIFT) > 0) mods.Add(Modifiers.SHIFT);
        if ((v & App.SAPP_MODIFIER_CTRL) > 0) mods.Add(Modifiers.CTRL);
        if ((v & App.SAPP_MODIFIER_ALT) > 0) mods.Add(Modifiers.ALT);
        if ((v & App.SAPP_MODIFIER_SUPER) > 0) mods.Add(Modifiers.SUPER);
        if ((v & App.SAPP_MODIFIER_LMB) > 0) mods.Add(Modifiers.LMB);
        if ((v & App.SAPP_MODIFIER_RMB) > 0) mods.Add(Modifiers.RMB);
        if ((v & App.SAPP_MODIFIER_MMB) > 0) mods.Add(Modifiers.MMB);
        return mods;
    }
}
