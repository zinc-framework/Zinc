using Zinc.Internal.Sokol;

namespace Zinc;

[Arch.AOT.SourceGenerator.Component]
public record struct EventMeta(string eventType, bool dirty = false);
[Arch.AOT.SourceGenerator.Component]
public readonly record struct FrameEvent(sapp_event e);
[Arch.AOT.SourceGenerator.Component]
public record MouseEvent(InputSystem.MouseState mouseState, MouseButton button,List<Modifiers> mods, float scrollX = 0, float scrollY = 0);