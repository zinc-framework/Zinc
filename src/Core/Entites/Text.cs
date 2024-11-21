using Arch.Core;
using Arch.Core.Extensions;

namespace Zinc;

[Component<RenderItem>]
[Component<TextRenderer>("Renderer")]
[Component<Collider>("Collider")]
public partial class Text : SceneEntity
{
    private readonly Action<Entity, double>? _updateWrapper;
    public int FontID { get; init;}
    public Text(string displayText, string fontPath, float size = 32, float blur = 0f, float spacing = 1f, Color color = null, Scene? scene = null, bool startEnabled = true, Action<Text,double>? update = null, Anchor? parent = null, List<Anchor>? children = null)
        : base(startEnabled,scene,parent:parent,children:children)
    {
        Renderer_fontPath = fontPath;
        Renderer_text = displayText;
        Renderer_fontPath = fontPath;
        Renderer_size = size;
        Renderer_blur = blur;
        Renderer_spacing = spacing;

        Renderer_Pivot = new System.Numerics.Vector2(0.5f);
        Collider_Pivot = new System.Numerics.Vector2(0.5f);
        Renderer_Color = color ?? Palettes.GetRandomColor();
        Collider_Width = 100;
        Collider_Height = 100;
        RenderOrder = Scene.GetNextSceneRenderCounter();
        Collider_Active = false;

        //TODO: move this to Resources, pass in the font directly
        FontID = Zinc.Core.Text.LoadFont(fontPath,displayText);

        if (update != null && _updateWrapper == null)
        {
            _updateWrapper = (baseEntity, dt) => update((Text)baseEntity, dt);
            Update = _updateWrapper;
        }


    }
}