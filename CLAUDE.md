# Zinc

Zinc is a small C# 2D game framework built on the sokol family of headers (windowing, GFX, immediate-mode 2D, ImGui), Box2D 3.x (physics + collision math), and the [Arch ECS](https://github.com/genaray/Arch). Source generators stitch authored `Entity` classes to ECS components so user code reads as OO while storage stays cache-friendly.

This file orients an AI assistant or a new contributor to the *internals* of the framework. For a tutorial, run the demos in the parent `Zinc.Demos` project.

## Top-level layout

| Path | What lives there |
|---|---|
| `Zinc.csproj` | The framework's csproj. Targets `net10.0`, references `Arch`, `Arch.AOT.SourceGenerator`, `Depot.SourceGenerator`, `Zinc.ECSGenerator`, `Zinc.Magic`. |
| `src/Core.cs` | The `Engine` static class. Owns the boot/init/frame/cleanup callbacks, the ECS world, the physics world, the scene registry, the default systems, and the immediate-mode draw helpers (`DrawTexturedRect`, `DrawShape`, `DrawText`, `DrawParticles`). Everything starts here. |
| `src/Core/` | Engine-internal modules: physics wrapper, collision API, resources, ECS components, systems, entity types, ImGui C# wrapper, utility code. |
| `src/Quick.cs` | One-call convenience helpers (centering, palettes, random). |
| `src/NativeUtils.cs` | `NativeArray<T>` (unmanaged buffer with managed lifetime) + `AsSgRange` for sokol uploads. |
| `src/Bindings/` | **Submodule** ([zinc-framework/Zinc.Bindings](https://github.com/zinc-framework/Zinc.Bindings)). All P/Invoke bindings: sokol (`sokol/Sokol.*.cs`), Box2D (`box2d/Box2D.cs`), STB (`stb/`). Do not hand-edit — regenerate upstream. |
| `libs/` | **Submodule** ([zinc-framework/Zinc.Libs](https://github.com/zinc-framework/Zinc.Libs)). Prebuilt native dylibs/dlls. Loaded at runtime by `NativeLibResolver`. |
| `data/`, `logos/` | Default fonts / palettes / images shipped with the framework. Copied to output via the csproj. |

## The single source of truth: `Engine`

`Engine` is a `static partial class` and is the only top-level thing user code talks to. Read `src/Core.cs` first; the rest of the framework is supporting cast.

### Boot path

```
Program.cs → Engine.Run(RunOptions) → Engine.Boot(opts) →
  NativeLibResolver.kick() (registers DllImport resolver)
  fills sapp_desc + delegates → App.run(&desc)   // sokol drives the main loop
```

`App.run` is the sokol_app entry point. It does NOT return until the window closes; the sokol library calls back into our delegates for `Initialize` / `Event` / `Frame` / `Cleanup`. All four are `[UnmanagedCallersOnly(CallConvs=[Cdecl])]` static methods on `Engine`. Read `Core.cs` for the exact order of operations in each.

Two facts about the frame loop that aren't obvious from the code:
- `DeltaTime` is set from `App.frame_duration()` (EMA-smoothed) for animation/UI; physics is stepped separately with `App.frame_duration_unfiltered()` so spikes aren't masked from the accumulator.
- Input events are *not* processed inside `Event` directly. They're recorded as one-frame `FrameEvent` ECS entities and consumed during the update phase by `InputSystem`, after ImGui has had a chance to claim them.

## Scenes and entities

### Scenes (`src/Core/Entites/Scene.cs`)

Lifecycle: `Mount(depth)` → `Load(callback)` → `Start()`. `Unmount(callback)` schedules removal at end of frame. Multiple scenes can be mounted simultaneously; `Engine.TargetScene` is the one that receives input focus. Each scene owns a list of entity IDs in `Engine.SceneEntityMap`.

### Entities (`src/Core/Entites/`)

Folder name is misspelled (`Entites`), intentionally not fixed. Contains:

- `Entity` — base type. Wraps an `Arch.Core.Entity` (`ECSEntity` property).
- `Anchor` — transform/parenting helper. Most renderables are `Anchor`-derived.
- `SceneEntity` / `SceneObject` — scene-aware entities.
- Concrete renderables: `Shape`, `Sprite`, `AnimatedSprite`, `Text`, `Pointer`, `ParticleEmitter`, `Grid`, `Coroutine`, `Temporary`.

Each concrete entity class is decorated with `[Component<T>("Name")]` attributes (from `Zinc.ECSGenerator` / `Depot.SourceGenerator`). The source generator emits property accessors so user code can do `myShape.Collider_Active = true` instead of touching the underlying ECS component. Example, see `Pointer.cs`:

```csharp
[Component<Position>]
[Component<Collider>("Collider")]
[Component<UpdateListener>]
public partial class Pointer : Entity { ... }
```

The `"Collider"` string is the *prefix* generated property names get (`Collider_Width`, `Collider_Active`, ...). Omitting the string uses the bare component name.

### Components (`src/Core/Components/`)

`record struct`s with `[Arch.AOT.SourceGenerator.Component]` so Arch can pack them into archetypes. Notable ones:

- `Position(X, Y)` — implicitly converts to `System.Numerics.Vector2`.
- `Collider` — width/height/pivot/active + lifecycle and mouse callbacks + `ActiveCollisions` set + an `IsPoint` flag (when true, width/height/pivot are ignored and the entity's `Position` *is* the collider).
- `SpriteRenderer`, `ShapeRenderer`, `TextRenderer`, `ParticleEmitterComponent` — render data.
- `EventMeta(eventType, dirty)` — wraps one-shot ECS events; `EventCleaningSystem` destroys them after one frame.
- `Destroy` — tagged on entities pending destruction; reaped by `DestructionSystem`.
- `EntityID(ID)` — managed-side ID; pairs every ECS entity with a `Zinc.Entity` instance via `Engine.EntityLookup`.

## Systems (`src/Core/Systems/`)

Interfaces: `IPreUpdateSystem`, `IUpdateSystem`, `IPostUpdateSystem`, `ICleanupSystem`. Frame loop iterates `Engine.ActiveSystems` in order. Default set:

- `FrameAnimationSystem` (Pre) — advance sprite-sheet animations.
- `InputSystem` (Update) — consumes `FrameEvent` entities, dispatches keyboard/mouse events, updates `MouseX/Y`, fires user `Events.Key.*` / `Events.Mouse.*` actions.
- `GridSystem`, `SceneUpdateSystem`, `TemporaryObjectSystem`, `CoroutineSystem` (Update).
- `CollisionSystem` (Update) — pairwise polygon/point intersection (see Collision below).
- `CollisionCallbackSystem` (Update) — dispatches `OnStart/OnContinue/OnEnd` and propagates mouse events from the cursor to collided colliders.
- `SceneRenderSystem` (PostUpdate) — walks active scenes, renders sprites/shapes/text/particles via `Engine.Draw*` helpers.
- `EventCleaningSystem` (Cleanup) — two-frame TTL for event-tagged ECS entities.

## Physics (`src/Core/Physics.cs`)

`PhysicsWorld` wraps `b2World`. Owns a Gaffer-on-Games-style fixed-timestep accumulator:

- `FixedTimeStep = 1/60s`, `SubStepCount = 4`, hard cap `MaxFrameTime = 0.25s` on incoming `dt` to prevent spiral-of-death.
- Driven from `Frame()` with `App.frame_duration_unfiltered()` so the EMA-smoothed `frame_duration()` doesn't mask real spikes feeding the accumulator.
- Exposes `Alpha = accumulator / FixedTimeStep` for renderers that want sub-step interpolation between physics states.

`PhysicsBody` wraps `b2BodyId`. Methods: `Position`, `Angle`, `Set(pos, angle)`, `AddForce`, `AddBoxShape(w, h, density, friction)`, `AddPolygonShapeWorldSpace(points, density, friction)`.

Coordinate convention: Box2D is agnostic about units, so we pick `(0, 980)` gravity to pull "down" in our Y-down screen space.

## Collision (`src/Core/Collision.cs`)

Separate from physics — this is pure shape-query math against `Collider` components (no `b2World` involvement). All queries dispatch through `Collider.IsPoint`:

| `a.IsPoint` | `b.IsPoint` | path |
|---|---|---|
| false | false | `b2CollidePolygons` (4 world-space corners → `b2ComputeHull` → `b2MakePolygon` → manifold) |
| true | false | `b2PointInPolygon` (skips `b2ComputeHull` entirely) |
| false | true | symmetric to above |
| true | true | `false` always (two infinitesimal points never overlap) |

The `Polygon` class in `Collision.cs` carries a `Valid` flag — `b2ComputeHull` aggressively welds points within `LINEAR_SLOP` (default `0.005`), so a too-small or degenerately-rotated 4-corner box can collapse to <3 unique points. In that case `Valid=false` and the queries short-circuit. This is why `Pointer` uses `IsPoint=true` instead of a tiny box: any pixel-scale collider lives near Box2D's slop threshold.

Public API:
- `CheckCollision(SceneEntity, SceneEntity)` / `(int, Collider, int, Collider)` → bool
- `GetClosestPoints(Entity, Entity)` / `(int, Collider, int, Collider)` → `(Vector2? a, Vector2? b)` / `(Vector2, Vector2)`
- `GetCollisionInfo(...)` → `CollisionInfo` (polygon-vs-polygon only; bails for point pairings since `b2Manifold` doesn't exist for points)
- `PointInsidePolygon(point, polyEntityId, c)` — used internally by the dispatch
- `ClosestPointOnPolygon(point, polyEntityId, c)` — uses `b2ShapeDistance` with a single-vertex proxy

## Rendering pipeline

Zinc has *five* simultaneous draw paths active each frame, all flushed inside one `sg_pass`:

1. **`GP.*` (sokol_gp)** — the workhorse 2D batcher for `Engine.DrawTexturedRect` / `DrawShape` / `DrawParticles`. Custom fork at [zinc-framework/sokol_gp](https://github.com/zinc-framework/sokol_gp/tree/sokol-view-objects) that caches one `sg_view` per texture channel internally, so `GP.set_image(channel, image)` interoperates with sokol_gfx's view-object resource model.
2. **`GL.*` (sokol_gl)** — used for vector text (`Fontstash` draws into it).
3. **`Fontstash`** — vector-font glyph atlas (sokol_fontstash). State cleared each frame via `fonsClearState`; flushed via `RenderSystems.cs`'s `Fontstash.flush`.
4. **`DebugText.*` (sokol_debugtext)** — retro pixel fonts (KC85, C64, ORIC, etc.). Used for HUD overlays.
5. **`ImGUI.*` (sokol_imgui via dcimgui)** — UI. Last so it overlays everything.

Per frame, in order: `GP.flush() → GP.end() → DebugText.draw() → GL.draw() → ImGUI.render()`.

## Native lib loading (`src/Core/Utility/NativeLibResolver.cs`)

A static `DllImportResolver` is registered for the framework assembly that maps logical lib names (`sokol`, `box2d`, `stb`) to platform-specific paths under `libs/runtimes/{osx-arm64,win-x64,browser-wasm}/native/`. `Engine.Boot` calls `NativeLibResolver.kick()` to force the static ctor before any P/Invoke runs.

## Conventions

- **Coordinates**: Y-down, origin top-left. Default window is 1280×720 (from `Program.cs`).
- **Entity update lambdas**: `new Shape(update: (self, dt) => { ... })`. The lambda's `self` is strongly typed.
- **Property prefixes**: `Foo_Bar` always means "the `Bar` field of the `Foo` component attached to this entity" — generated, not hand-written. Search the codebase for the underlying field on the `record struct` if you need to know what it does.
- **One frame per ECS event**: events created via `EventMeta` live for one full frame and are auto-destroyed. Don't hold references to event entities across frames.
- **Don't allocate UTF-8 in the hot path**: `System.Text.Encoding.UTF8.GetBytes(constantString)` is fine once, terrible per frame. See the cached `_sokolGfxMenuTitle` / `_sokolAppMenuTitle` in `Core.cs` for the pattern.
- **Engine globals**: `Engine.Cursor`, `Engine.PhysicsWorld`, `Engine.ECSWorld`, `Engine.GlobalScene`, `Engine.TargetScene`, `Engine.Width`, `Engine.Height`, `Engine.DeltaTime`, `Engine.FrameCount`. Treated as readable from anywhere.

## Debug toggles

Pressing `,` toggles `Engine.ShowMenu` (hides all framework UI). Pressing `C` toggles `Engine.Clear` (whether GP clears the framebuffer each frame — useful for trails). The "Zinc" menu has runtime checkboxes for stats HUD, ImGui demo window, debug-overlay system, and the collider debug renderer.
