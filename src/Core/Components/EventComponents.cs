using Zinc.Internal.Sokol;

namespace Zinc;

public record struct EventMeta(string eventType, bool dirty = false);
public readonly record struct FrameEvent(sapp_event e);
public record MouseEvent(InputSystem.MouseState mouseState, MouseButton button,List<Modifiers> mods, float scrollX = 0, float scrollY = 0);