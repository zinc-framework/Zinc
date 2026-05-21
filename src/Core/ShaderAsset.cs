using Zinc.Internal.Sokol;

namespace Zinc.Core;

// ShaderAsset is the typesafe handle the Zinc.Magic generator emits for each
// `@program` found in a res/shaders/*.glsl file (mirrors TextureAsset). The
// generated typed uniform structs live next to it under Res.Shaders.<program>.
public static partial class Assets
{
    public record ShaderAsset(string Name)
    {
        // Sentinel meaning "use sgp's built-in pipeline" — the render path skips set_pipeline for it,
        // so default-shaded objects render exactly as before. Every Shape/Sprite defaults to this.
        public static readonly ShaderAsset Default = new("__sgp_default__") { IsDefault = true };
        public bool IsDefault { get; init; }

        private sg_shader _shader;
        private sg_pipeline _pipeline;
        private bool _loaded;

        public bool Loaded => _loaded;

        // Builds the native shader (from the build-time registry) and the sgp
        // pipeline. has_vs_color matches sgp's vertex layout, which carries the
        // per-vertex color the default/custom vertex shader reads as `color`.
        public unsafe void Load()
        {
            if (_loaded) return;
            _shader = ShaderRegistry.Make(Name);
            sgp_pipeline_desc pd = default;
            pd.shader = _shader;
            pd.has_vs_color = 1;
            _pipeline = GP.make_pipeline(&pd);
            _loaded = true;
        }

        public sg_pipeline Pipeline { get { if (!_loaded) Load(); return _pipeline; } }
        public sg_shader Shader { get { if (!_loaded) Load(); return _shader; } }

        /// <summary>Bind this shader's pipeline for subsequent sgp draws.</summary>
        public void Apply() => GP.set_pipeline(Pipeline);

        /// <summary>Restore sgp's default (built-in) pipeline.</summary>
        public static void ResetPipeline() => GP.reset_pipeline();
    }
}
