using Zinc.Internal.Sokol;
using System.IO;
using Zinc.Internal.STB;

namespace Zinc;

public record SpriteData(Resources.Texture Texture, Rect Rect);
public record AnimatedSpriteData(Resources.Texture Texture, HashSet<Animation> Animations) : SpriteData(Texture, Rect.Empty);
public static class Resources
{
    // Resources are types that are loadable
    public record Texture
    {
        private string path;

        public string Path
        {
            get => path;
            init
            {
                path = System.IO.Path.GetFullPath(value);
            }
        }
        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;
        public sg_image Data { get; private set; }
        public bool DataLoaded { get; private set; }
        /// <summary>
        /// Create a texture from a path. Populates Width/Height via a loading the file (so a bit slower that applying that directly)
        /// </summary>
        /// <param name="path"></param>
        public Texture(string path, int width = -1, int height = -1, bool loadImmediate = true)
        {
            Path = path;
            Width = width;
            Height = height;
            if (loadImmediate)
            {
                Load();
            }
        }

        /// <summary>Wrap an already-created sg_image (e.g. a RenderTarget's color attachment) as a Texture.</summary>
        public Texture(sg_image data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;
            DataLoaded = true;
        }
        public bool Load(bool forceReload = false)
        {
            if (DataLoaded && !forceReload) { return true; }
            if (Engine.LoadImage(Path, out var width, out var height, out var img))
            {
                Width = width;
                Height = height;
                Data = img;
                DataLoaded = true;
                return true;
            }
            DataLoaded = false;
            return false;
        }

        public Rect GetFullRect()
        {
            if (Width == -1 || Height == -1)
            {
                Console.WriteLine("bad rect fetch for unloaded texture data");
                return Rect.Empty;
            }
            //note if you set width/height we believe you
            return new Rect(0, 0, Width, Height);
        }

        public SpriteData Slice(Rect rect)
        {
            return new SpriteData(this, rect);
        }
    }

    // A loadable custom sgp shader, shaped like Texture: an identifier (Name) plus a "source" that
    // Load() turns into the native sg_shader (Data) + its sgp pipeline. Where Texture's source is a
    // file path (read at Load), a Shader's source is a backend->sg_shader factory compiled from
    // sokol-shdc. Two-phase generation, NO registry: Zinc.Magic emits the `Res.Assets.<program>`
    // handle from the .glsl alone (always IDE-available so you can code against a new shader before
    // building), and the shdc build step wires the compiled factory onto that handle via SetFactory.
    // The std140 uniform blocks are the generated record-structs under Res.Shaders.<program>.
    public record Shader
    {
        public string Name { get; init; } = "";
        /// <summary>The sgp built-in pipeline sentinel; the renderer skips set_pipeline for it.</summary>
        public bool IsDefault { get; init; }

        private Func<sg_backend, sg_shader>? factory;
        private sg_pipeline pipeline;

        public bool Loaded { get; private set; }
        public sg_shader Data { get; private set; }

        /// <summary>Default = "use sgp's built-in pipeline" (today's behavior for un-shadered objects).</summary>
        public static readonly Shader Default = new() { Name = "__sgp_default__", IsDefault = true };

        private Shader() { }                  // sentinel only
        public Shader(string name) { Name = name; }

        /// <summary>Generated-use: wires the build-compiled factory onto this stub handle. Not for hand use.</summary>
        public void SetFactory(Func<sg_backend, sg_shader> factory) => this.factory = factory;

        /// <summary>Build the native sg_shader for the current backend and its sgp pipeline.</summary>
        public unsafe bool Load(bool forceReload = false)
        {
            if (Loaded && !forceReload) return true;
            if (IsDefault) return false;
            if (factory is null)
                throw new InvalidOperationException(
                    $"Shader '{Name}' has no compiled backend — make sure res/shaders/{Name}.glsl was built " +
                    "(a real `dotnet build`, not a design-time/IntelliSense build).");
            Data = factory(Gfx.query_backend());
            sgp_pipeline_desc pd = default;
            pd.shader = Data;
            pd.has_vs_color = 1; // sgp's vertex layout carries the per-vertex color the shaders read
            pipeline = GP.make_pipeline(&pd);
            Loaded = true;
            return true;
        }

        public sg_pipeline Pipeline { get { if (!Loaded) Load(); return pipeline; } }

        /// <summary>Bind this shader's pipeline for subsequent sgp draws.</summary>
        public void Apply() => GP.set_pipeline(Pipeline);
    }
}

public record Animation(string Name, Rect[] Frames, float animationTime = 1f)
{
    public int FrameCount { get; } = Frames.Length;
    public float FrameTime { get; } = animationTime / Frames.Length;
}

public readonly record struct Rect(float startX, float startY, float width, float height)
{
    public Rect(float width, float height) : this(0, 0, width, height) { }
    public static Rect Empty = new Rect(0, 0, 0, 0);
    public readonly Internal.Sokol.sgp_rect InternalRect { get; } = new sgp_rect()
    {
        x = startX,
        y = startY,
        w = width,
        h = height
    };
    
    public static implicit operator Rect((float startX, float startY, float width, float height) tuple)
    {
        return new Rect(tuple.startX, tuple.startY,tuple.width,tuple.height);
    }

}


