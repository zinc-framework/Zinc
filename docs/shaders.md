# Custom shaders & materials

Zinc renders 2D with [sokol_gp](https://github.com/edubart/sokol_gp) (`GP.*`), which has a fixed
built-in pipeline. This doc covers **custom shaders**: authoring a shader as
[sokol-shdc](https://github.com/floooh/sokol-tools) GLSL, having it compiled at build into typed C#,
and binding it (plus uniforms) to an entity through the material system.

## Quick start

1. Drop an annotated GLSL file in your project's `res/shaders/`, e.g. `res/shaders/sdf.glsl`, with a
   `@program sdf …` directive.
2. `dotnet build` — the build runs sokol-shdc and generates the C# bindings.
3. Use it on any `Shape`/`Sprite`:

```csharp
var rect = new Shape(Engine.Width, Engine.Height);
rect.Shader = Res.Assets.sdf;                       // generated handle
rect.Update = (self, dt) =>
{
    ((Shape)self).Material.SetFragmentUniforms(new Res.Shaders.sdf.Fs   // generated std140 struct
    {
        iResolution = new Vector2(Engine.Width, Engine.Height),
        iTime = (float)Engine.Time,
    });
};
```

`Res.Assets.sdf` and `Res.Shaders.sdf.Fs` exist in the IDE **before** you build (generated from the
`.glsl` alone), so you can write this code first; the actual compiled shader is wired in at build.

## Authoring the `.glsl` (the sgp contract)

Use shdc's [annotated GLSL](https://github.com/floooh/sokol-tools/blob/master/docs/sokol-shdc.md)
(`@module` / `@vs` / `@fs` / `@program`). A custom sgp pipeline must follow sgp's vertex contract:

```glsl
@module sdf

@vs vs
in vec4 coord;          // xy = position, zw = texcoord  (attr 0)
in vec4 color;          // per-vertex color              (attr 1)
out vec2 texUV;
out vec4 iColor;
void main() {
    gl_Position = vec4(coord.xy, 0.0, 1.0);   // sgp pre-transforms verts on the CPU; no MVP uniform
    texUV = coord.zw;
    iColor = color;
}
@end

@fs fs
layout(binding=1) uniform sdf_fs_params {      // binding=1 == sgp's FRAGMENT uniform slot
    vec2 iResolution;
    float iTime;
};
in vec2 texUV;
in vec4 iColor;
out vec4 fragColor;
void main() { /* … */ fragColor = vec4(col, 1.0) * iColor; }
@end

@program sdf vs fs
```

- **Vertex attrs are fixed:** attr 0 = `coord`, attr 1 = `color`. sgp bakes the transform/projection
  into `coord` on the CPU, so the vertex shader is just `gl_Position = vec4(coord.xy, 0, 1)` — there is
  no model/view/projection uniform.
- **Uniform slots:** the **vertex** uniform block is sgp slot 0 (`layout(binding=0)`), the **fragment**
  block is slot 1 (`layout(binding=1)`). Set them with `SetVertexUniforms` / `SetFragmentUniforms`.
- `has_vs_color = true` is set on the pipeline, so the `color` attribute is always available.

## What gets generated

Generation happens in **two phases** so the typed surface is available in the editor without running
the shader compiler:

| Phase | Source | Emits |
|---|---|---|
| **Stub** | the `.glsl` alone (no shdc) | `Res.Assets.<program>` — a `Resources.Shader` handle; and `Res.Shaders.<program>.Vs` / `.Fs` — the std140 uniform blocks as `record struct`s (`[StructLayout(Explicit, Size=blockSize)]` + `[FieldOffset]`). Always IDE-available. |
| **Desc** | shdc reflection (`*.yaml`) + per-stage source | the compiled `Make(sg_backend) -> sg_shader` factory, plus a `[ModuleInitializer]` that wires it onto the stub handle via `Res.Assets.<program>.SetFactory(Make)`. Only after a real build. |

There is **no shader registry**: the per-shader `Make` is attached directly to the handle (a one-entry
"vtable" — see [Why a factory](#why-a-factory)).

Both phases live in the **Zinc.Magic** source generator (a separate package; `ShaderGen.cs`). The std140
struct's `Size` equals the block's std140 size, which is the *entire* runtime uniform contract — see below.

## Build integration

`Zinc/Zinc.Shaders.targets` (imported by `Zinc.csproj`, and by any project that authors shaders) runs
sokol-shdc over `res/**/*.glsl` before `CoreCompile`:

- shdc binary: `Zinc/libs/tools/<host-rid>/sokol-shdc` (osx-arm64/osx-x64/win-x64/linux-*).
- Output: `obj/<cfg>/<tfm>/shaders/*.shdc.* ` (bare_yaml reflection + per-stage Metal source), added to
  `@(AdditionalFiles)` so the generator can read them.
- **Skipped in design-time / IntelliSense builds** — editing code never invokes the shader compiler; the
  generator reuses the artifacts from the last real build.

## Runtime model

- **`Resources.Shader`** (`src/Core/Resources.cs`) — shaped like `Resources.Texture`: `Name`, `Loaded`,
  `Data` (the native `sg_shader`), `Pipeline` (lazy), `Apply()`, `Load(forceReload)`. Its "source" is the
  generated `Make` factory (wired via `SetFactory`); `Load()` runs it to build the `sg_shader` and the sgp
  pipeline. `Resources.Shader.Default` is a sentinel meaning "use sgp's built-in pipeline".
- **`MaterialComponent`** (`src/Core/Material.cs`) — a per-object `[Component]` (baked into `Shape`/`Sprite`)
  holding the shader override + latched uniform byte buffers. Unset shader reads as `Default`.
- **Uniforms are typed std140 structs.** sokol's uniform model is `sg_apply_uniforms(slot, bytes)` — you
  hand it the whole block's bytes. So you fill the generated `record struct` and pass it to
  `entity.Material.SetFragmentUniforms<T>(in)` / `SetVertexUniforms<T>(in)` / `SetUniforms<TVs,TFs>(in,in)`,
  which memcpys the block into the latch buffer. `Unsafe.SizeOf<T>()` equals the struct's `[StructLayout]`
  Size equals the std140 block size, so the upload size is correct by construction. There are no named
  uniform setters and no runtime layout table — the generated struct is the single source of layout truth.
- **`Engine.DrawShape` / `DrawTexturedRect`** (`src/Core.cs`, `ApplyMaterial`) bind the custom pipeline +
  latched uniforms, draw, then reset. If the shader is `Default`, they leave rendering on sgp's built-in.
- **`entity.Shader` and `entity.Material`** are emitted by **Zinc.ECSGenerator** (a separate package) from
  `MaterialComponent`'s `[EntityAccessible]` members — a general mechanism that forwards a component's
  members onto entities that use it. Any component can opt in the same way.

## The built-in (default) sgp shader

The built-in pipeline has **no uniform block** (default VS bakes the transform into `coord`; default FS
is `texture(...) * iColor`). Its per-instance inputs are *not* uniforms:

- **color** — a per-vertex attribute via `set_color`, exposed as `shape.Renderer_Color` (kept separate
  from the material API on purpose — not every material has a color, and the tint is a geometry-draw
  concern, not a shader uniform).
- **texture/sampler** — `set_image` / `set_sampler` (the Sprite's texture).
- **transform** — baked into vertex positions; **blend mode** — pipeline state.

So `Set*Uniforms` only apply to a *custom* shader; setting uniforms on a default-shaded object throws.

## Why a factory

`Make()` builds an `sg_shader` via `sg_make_shader`, which requires a live GPU device — but the
`Res.Assets.<program>` handle is a `static` initialized at module load, **before** `sg_setup()`. So shader
creation must be deferred to first use (`Load()`, at first draw). The factory is that deferral; it's also
shader-specific (it bakes the exact `sg_shader_desc` from shdc reflection) and backend-parameterized
(`Make(sg_backend)`, the seam for future non-Metal backends). Attaching it to the stub handle via
`SetFactory` is what lets the always-available handle (stub) bind to build-generated creation logic
without a registry.

## Limits (current)

- **Metal (macOS) only.** shdc is invoked for `metal_macos`; other backends throw `NotSupportedException`
  at `Load()`. Adding HLSL/GL/WGSL is more `-l` slangs + per-backend reflection branches in Zinc.Magic.
- **No image/sampler shaders yet** — so sokol_gp's `effect` (multi-texture) and `framebuffer` (offscreen)
  samples aren't ported. Uniforms-only shaders (e.g. the SDF demo) are the supported path.

## Where the code lives

| Piece | Location |
|---|---|
| `Resources.Shader` | `src/Core/Resources.cs` |
| `MaterialComponent`, `MaterialAccessor`, `[EntityAccessible]` | `src/Core/Material.cs` |
| `ApplyMaterial` + the `DrawShape`/`DrawTexturedRect` hooks | `src/Core.cs` |
| shdc build target | `Zinc.Shaders.targets` |
| shdc binaries | `libs/tools/<rid>/sokol-shdc` |
| `.glsl` → C# generator | **Zinc.Magic** package (`ShaderGen.cs`) |
| `.Shader`/`.Material` entity accessors | **Zinc.ECSGenerator** package (`[EntityAccessible]`) |

## Maintainer notes / gotchas

- `MaterialComponent` uses managed `byte[]` latch buffers, **not** `[InlineArray]` — an inline-array field
  inside a managed Arch component corrupts archetype storage.
- In `Zinc.Shaders.targets`, reference `$(IntermediateOutputPath)` only **inside** the targets (it's empty
  in a top-level `PropertyGroup` because the import precedes the implicit SDK targets).
- Iterating on the generators (Zinc.Magic / Zinc.ECSGenerator) uses a local NuGet feed in the consuming
  repo; bump the package version each rebuild (NuGet won't re-extract a same-version package) and
  `dotnet build-server shutdown` (the analyzer DLL is cached in the build node). Releases publish to
  nuget.org via a tag-push GitHub Action in each generator repo.
