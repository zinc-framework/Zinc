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
| **Desc** | shdc reflection (`*.yaml`) + per-stage source | **One file per (program, slang) pair** — e.g. `Shader_sdf__hlsl5.g.cs`, `Shader_sdf__metal_macos.g.cs`, etc. Each emits a `Make(sg_backend) -> sg_shader` factory and a `[ModuleInitializer]` that calls `Res.Assets.<program>.RegisterFactory(sg_backend.SG_BACKEND_XXX, Make)`. Only after a real build. |

At module load every per-slang `Init()` registers its factory under the corresponding `sg_backend`;
`Resources.Shader.Load()` looks up the *runtime* backend at first draw and runs that one factory. So
there's no shader registry, but there IS a backend-keyed dictionary per shader (see
[Per-backend factory dispatch](#per-backend-factory-dispatch) below).

Both phases live in the **Zinc.Magic** source generator (a separate package; `ShaderGen.cs`). The std140
struct's `Size` equals the block's std140 size, which is the *entire* runtime uniform contract — see below.

### Slang → backend mapping

The `Make(sg_backend)` factories are named after shdc's slang identifier; the `RegisterFactory` call
maps that to the runtime backend Zinc resolves via `Gfx.query_backend()`:

| Slang | `sg_backend` const | Used on |
|---|---|---|
| `metal_macos` | `SG_BACKEND_METAL_MACOS` | macOS |
| `metal_ios` / `metal_sim` | `SG_BACKEND_METAL_IOS` / `SG_BACKEND_METAL_SIMULATOR` | iOS |
| `hlsl5` (or `hlsl4`) | `SG_BACKEND_D3D11` | Windows |
| `glsl430` (or `glsl410`) | `SG_BACKEND_GLCORE` | Linux desktop |
| `glsl300es` (or `glsl310es`) | `SG_BACKEND_GLES3` | Web (emscripten), Android |
| `wgsl` | `SG_BACKEND_WGPU` | (reserved; Zinc has no WGPU runtime yet) |

## Build integration

`Zinc/Zinc.Shaders.targets` (imported by `Zinc.csproj`, and by any project that authors shaders) runs
sokol-shdc over `res/**/*.glsl` before `CoreCompile`:

- shdc binary: `Zinc/libs/tools/<host-rid>/sokol-shdc` (osx-arm64/osx-x64/win-x64/linux-*). The same
  binary is invoked regardless of the *target* slangs being produced — shdc cross-compiles.
- Output: `obj/<cfg>/<tfm>/shaders/`:
  - one combined `<name>_reflection.yaml` per shader, whose top-level `shaders:` list has one entry
    per slang (each carrying its own per-stage source paths and slang-specific register fields);
  - per-stage per-slang sources: `*.metal` / `*.hlsl` / `*.glsl` / `*.wgsl`.

  All of the above get added to `@(AdditionalFiles)` so the generator can read them.
- **Slangs are configurable.** `<ZincShaderSlang>` (in your project's csproj, or its default in
  `Zinc.Shaders.targets`) is a colon-separated list of slangs passed to shdc as a single
  `-l a:b:c:d` invocation. Default is `hlsl5:glsl430:metal_macos:glsl300es`, which covers
  Windows / Linux / macOS / Web. Trim it to a smaller set if you only ship for some of them — the
  factories you don't emit are just dead weight.
- **Skipped in design-time / IntelliSense builds** — editing code never invokes the shader compiler; the
  generator reuses the artifacts from the last real build.

## Runtime model

- **`Resources.Shader`** (`src/Core/Resources.cs`) — shaped like `Resources.Texture`: `Name`, `Loaded`,
  `Data` (the native `sg_shader`), `Pipeline` (lazy), `Apply()`, `Load(forceReload)`. Its "source" is a
  `Dictionary<sg_backend, Func<sg_backend, sg_shader>>` of generated factories — one entry per slang the
  build emitted, populated by per-slang `[ModuleInitializer]`s before `sg_setup()` runs. `Load()` queries
  `Gfx.query_backend()` and runs the matching factory to build the `sg_shader` + sgp pipeline; throws
  `NotSupportedException` (with the list of compiled backends) if the runtime backend wasn't built.
  `Resources.Shader.Default` is a sentinel meaning "use sgp's built-in pipeline".
- **`MaterialComponent`** (`src/Core/Material.cs`) — a per-object `[Component]` (baked into `Shape`/`Sprite`)
  holding the shader override + latched uniform byte buffers. Unset shader reads as `Default`.
- **Uniforms are typed std140 structs.** sokol's uniform model is `sg_apply_uniforms(slot, bytes)` — you
  hand it the whole block's bytes. So you fill the generated `record struct` and pass it to
  `entity.Material.SetFragmentUniforms<T>(in)` / `SetVertexUniforms<T>(in)` / `SetUniforms<TVs,TFs>(in,in)`,
  which memcpys the block into the latch buffer. `Unsafe.SizeOf<T>()` equals the struct's `[StructLayout]`
  Size equals the std140 block size, so the upload size is correct by construction. There are no named
  uniform setters and no runtime layout table — the generated struct is the single source of layout truth.
- **Images & samplers** (for shaders with texture channels, e.g. the `effect` demo) are bound per-object
  with `entity.Material.SetImage(channel, Resources.Texture)` / `SetSampler(channel, Sampler)`,
  where `channel` is sgp's `set_image`/`set_sampler` slot. The generator emits the shader's
  `views`/`samplers`/`texture_sampler_pairs` from shdc reflection into the `sg_shader_desc`. `Sampler`
  (`src/Core/Rendering/Sampler.cs`) is a small `readonly record struct` that creates its `sg_sampler` on
  construction (so build it from `Create()`/`Update()`, not a static init); filter/wrap are the friendly
  `Filter` / `Wrap` enums (`TypeMappings.cs`).
- **`Engine.DrawShape` / `DrawTexturedRect`** (`src/Core.cs`, `ApplyMaterial` / `ResetMaterial`) bind the
  custom pipeline + latched uniforms + any bound images/samplers, draw, then reset. If the shader is
  `Default`, they leave rendering on sgp's built-in.
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
- **transform** — baked into vertex positions.
- **blend mode** — pipeline state, exposed per-object as `shape.Renderer_BlendMode` (a `BlendMode` enum in
  `TypeMappings.cs`); shapes default to `None` (opaque), sprites to `Blend`.

For offscreen rendering there's `RenderTarget` (`src/Core/Rendering/RenderTarget.cs`): draw into it with
`Render(...)`, then sample its `.Texture` on any Sprite/Shape (see the `SGP_Zinc_Framebuffer` demo).

So `Set*Uniforms` only apply to a *custom* shader; setting uniforms on a default-shaded object throws.

## Per-backend factory dispatch

`Make()` builds an `sg_shader` via `sg_make_shader`, which requires a live GPU device — but the
`Res.Assets.<program>` handle is a `static` initialized at module load, **before** `sg_setup()`. So shader
creation must be deferred to first use (`Load()`, at first draw). The factories *are* that deferral; they
also bake the exact `sg_shader_desc` from shdc reflection (so each slang carries its own register/binding
layout) and are backend-parameterized (`Make(sg_backend)`).

Each generated `Shader_<name>__<slang>.g.cs` registers itself for one specific `sg_backend`:

```csharp
[ModuleInitializer]
internal static void Init() => Res.Assets.sdf.RegisterFactory(sg_backend.SG_BACKEND_D3D11, Make);
```

If the build emits all four default slangs, each shader handle ends up with four entries in its
`factories` dictionary. `Load()` picks the one for the runtime backend; the unused ones stay in memory
(a few KB of base64-encoded shader source each) but never run. No registry, no central dispatch — just
one dictionary on the per-shader handle, populated cooperatively by the per-slang module initializers.

### Back-compat: `SetFactory` (deprecated)

`Resources.Shader.SetFactory(Func<sg_backend, sg_shader>)` is kept as an `[Obsolete]` shim that
registers under `SG_BACKEND_METAL_MACOS`. It's there so projects pinning Zinc.Magic ≤ 1.0.7 (which
emitted a single-Metal `SetFactory(Make)` call) still link. New code should never call it.

## Limits (current)

- **Per-draw uniform/texture caps** are sokol_gp compile-time constants set in Zinc.Bootstrapper's
  `sokol.c`: `SGP_UNIFORM_CONTENT_SLOTS=64` (256 bytes of combined vs+fs uniform per draw) and
  `SGP_TEXTURE_SLOTS=8` (texture channels per draw). The material API bounds-checks against these
  (mirrored as `MaterialComponent.MaxUniformBytes` / `TextureSlots`) and throws a clear error rather than
  silently overflowing sokol_gp's buffer. Raising them further = bump those defines in Zinc.Bootstrapper's
  `sokol.c`, rebuild the native lib (`./build.sh sokol:build` or `.\build.cmd sokol:build` on Windows;
  copy into the Zinc.Libs submodule), and bump the mirrored C# consts. A native rebuild is *not* needed
  to add a new uniforms-only or textured shader — only to exceed the caps.
- **No Linux verification yet.** The `glsl430` codegen path is in place and the generated C# looks
  correct, but it hasn't been run against an actual `SG_BACKEND_GLCORE` runtime. First Linux user is
  the verifier — expect the GL vertical-flip / pixel-format tweaks already documented in
  `screenshot_other.c` to be the kind of thing that might also bite the shader path.
- **WGSL is reserved.** Zinc has no WebGPU engine backend yet; the slang is parsed and the codegen
  would emit a `Shader_*__wgsl.g.cs` if you added `wgsl` to `<ZincShaderSlang>`, but it would never be
  picked up by `Load()` since `Gfx.query_backend()` will never return `SG_BACKEND_WGPU` under the
  current Engine boot.

Both image/sampler shaders and offscreen rendering have working demos: `SGP_Example_Effect` (custom
shader, 2 textures + 2 samplers + uniforms, via the material system) and `SGP_Example_Framebuffer`
(offscreen render target via the sokol view API + a nested GP queue, drawn raw in `Scene.Update`).

## Where the code lives

| Piece | Location |
|---|---|
| `Resources.Shader` | `src/Core/Resources.cs` |
| `Sampler`, `Filter` / `Wrap` enums | `src/Core/Rendering/Sampler.cs`, `src/Core/TypeMappings.cs` |
| `MaterialComponent`, `MaterialAccessor`, `[EntityAccessible]`, cap consts | `src/Core/Components/MaterialComponent.cs` |
| `ApplyMaterial` / `ResetMaterial` + the `DrawShape`/`DrawTexturedRect` hooks | `src/Core.cs` |
| native cap defines (`SGP_UNIFORM_CONTENT_SLOTS` / `SGP_TEXTURE_SLOTS`) | Zinc.Bootstrapper `libs/sokol/build/sokol.c` |
| shdc build target | `Zinc.Shaders.targets` |
| shdc binaries | `libs/tools/<rid>/sokol-shdc` |
| `.glsl` → C# generator | **Zinc.Magic** package (`ShaderGen.cs`) |
| `.Shader`/`.Material` entity accessors | **Zinc.ECSGenerator** package (`[EntityAccessible]`) |

## Maintainer notes / gotchas

- `MaterialComponent` uses managed `byte[]` latch buffers, **not** `[InlineArray]` — an inline-array field
  inside a managed Arch component corrupts archetype storage.
- In `Zinc.Shaders.targets`, reference `$(IntermediateOutputPath)` only **inside** the targets (it's empty
  in a top-level `PropertyGroup` because the import precedes the implicit SDK targets).
- **Pin every C-string for the full `sg_make_shader` call.** The generated `Make()` factories pass
  pointers to shader source, entry-point names, HLSL semantic names, and GL sampler-pair names into a
  `sg_shader_desc` that `sg_make_shader` then reads asynchronously. ShaderGen emits one nested
  `fixed (byte* xP = x) fixed (byte* yP = y) … { … return Gfx.make_shader(&d); }` block so every pinned
  pointer is valid through the whole call. Per-statement `fixed` would leave dangling pointers in `d` —
  do not "simplify" that codegen.
- **shdc per-slang field names differ.** `msl_buffer_n` vs `hlsl_register_b_n` vs (GL: no register at
  all, sokol uses `slot` directly); HLSL needs `hlsl_sem_name` + `hlsl_sem_index` on each attr; GL
  needs `glsl_name` on each `texture_sampler_pair`. The `SlangBindings` table in `ShaderGen.cs` is the
  one place that mapping lives — add new slangs by extending that table, not by sprinkling `if (slang ==
  ...)` checks through `EmitProgram`.
- **Iterating on the generators** (Zinc.Magic / Zinc.ECSGenerator): bump the package version in its
  csproj each rebuild (NuGet won't re-extract a same-version package), add a local feed to your
  consuming repo's `nuget.config` pointing at `Zinc.Magic/Zinc.Magic/bin/Debug`, and run
  `dotnet build-server shutdown` (the analyzer DLL is cached in the build node). Releases publish to
  nuget.org via a tag-push GitHub Action in each generator repo.
