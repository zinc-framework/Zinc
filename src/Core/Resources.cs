using Zinc.Internal.Sokol;
using System.IO;
using Zinc.Internal.STB;

namespace Zinc;

public record SpriteData(Resources.Texture Texture, Rect Rect);
public record AnimatedSpriteData(Resources.Texture Texture, HashSet<Animation> Animations);
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


