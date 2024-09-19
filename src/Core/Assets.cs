namespace Zinc.Core;

//Assets are typed references to files on disk
public static partial class Assets
{
    public record Asset(string Path);
    public record TextureAsset(string Path) : Asset(Path) 
    {
        public Resources.Texture Texture = new (Path,loadImmediate:false);
        public void Load(bool forceReload = false) => Texture.Load(forceReload);
        //public Rect R = ... - maybe someway of predeclaring Rects with meta?
        public Sprite ToSprite(Rect? r = null, Scene? scene = null, bool startEnabled = true)
        {
            return new Sprite(ToSpriteData(r),scene,startEnabled);
        }

        public SpriteData ToSpriteData(Rect? r = null)
        {
            Texture.Load();
            return new SpriteData(Texture,r != null ? (Rect)r : Texture.GetFullRect());
        }
    }
}