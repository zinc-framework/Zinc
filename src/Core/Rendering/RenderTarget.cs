using System;
using Zinc.Internal.Sokol;

namespace Zinc;

// An offscreen render target: draw into it with Render(...), then sample the result via .Texture (e.g.
// on a Sprite/Shape). Hides sokol's view/attachment plumbing — it allocates the color (+ MSAA resolve +
// depth) images, views and attachments, matching the color/depth format + sample count that sokol_gp
// baked into its pipelines (from GP.query_desc), so the offscreen pass validates. Construct it when sokol
// is up (Create()/Update(), not a static initializer) and Dispose() it when done (e.g. Scene.Cleanup).
public sealed class RenderTarget
{
    public int Width { get; }
    public int Height { get; }

    /// <summary>The rendered result, sampleable on any Sprite/Shape.</summary>
    public Resources.Texture Texture { get; }

    private readonly sg_image _color, _resolve, _depth;
    private readonly sg_view _colorView, _resolveView, _depthView;
    private readonly sg_attachments _attachments;
    private readonly bool _hasResolve, _hasDepth;
    private bool _disposed;

    public unsafe RenderTarget(int width, int height)
    {
        Width = width;
        Height = height;

        // Match what sokol_gp draws with, or the offscreen pass/pipeline formats won't agree.
        sgp_desc d = GP.query_desc();
        sg_pixel_format colorFmt = d.color_format;
        sg_pixel_format depthFmt = d.depth_format;
        int sampleCount = d.sample_count;
        _hasResolve = sampleCount > 1;
        _hasDepth = depthFmt != sg_pixel_format.SG_PIXELFORMAT_NONE;

        sg_image_desc colorDesc = default;
        colorDesc.usage.color_attachment = 1;
        colorDesc.width = width;
        colorDesc.height = height;
        colorDesc.pixel_format = colorFmt;
        colorDesc.sample_count = sampleCount;
        _color = Gfx.make_image(&colorDesc);
        sg_image sampleImg = _color;

        if (_hasResolve)
        {
            sg_image_desc resolveDesc = default;
            resolveDesc.usage.resolve_attachment = 1;
            resolveDesc.width = width;
            resolveDesc.height = height;
            resolveDesc.pixel_format = colorFmt;
            resolveDesc.sample_count = 1;
            _resolve = Gfx.make_image(&resolveDesc);
            sampleImg = _resolve; // can't sample an MSAA image directly
        }

        if (_hasDepth)
        {
            sg_image_desc depthDesc = default;
            depthDesc.usage.depth_stencil_attachment = 1;
            depthDesc.width = width;
            depthDesc.height = height;
            depthDesc.pixel_format = depthFmt;
            depthDesc.sample_count = sampleCount;
            _depth = Gfx.make_image(&depthDesc);
        }

        sg_view_desc cvd = default;
        cvd.color_attachment.image = _color;
        _colorView = Gfx.make_view(&cvd);
        if (_hasResolve)
        {
            sg_view_desc rvd = default;
            rvd.resolve_attachment.image = _resolve;
            _resolveView = Gfx.make_view(&rvd);
        }
        if (_hasDepth)
        {
            sg_view_desc dvd = default;
            dvd.depth_stencil_attachment.image = _depth;
            _depthView = Gfx.make_view(&dvd);
        }

        _attachments = default;
        _attachments.colors.e0 = _colorView;
        if (_hasResolve) _attachments.resolves.e0 = _resolveView;
        if (_hasDepth) _attachments.depth_stencil = _depthView;

        Texture = new Resources.Texture(sampleImg, width, height);
    }

    /// <summary>
    /// Render into this target. The callback issues GP draws in a top-left (0,0)-(Width,Height) space;
    /// the offscreen pass (begin/flush/end) is set up around it. Call from within a frame (Scene.Update),
    /// before whatever samples the result is drawn.
    /// </summary>
    public unsafe void Render(Action draw, Color clear = null)
    {
        GP.begin(Width, Height);
        GP.project(0, Width, Height, 0);
        draw();

        sg_pass pass = default;
        pass.action.colors.e0.load_action = sg_load_action.SG_LOADACTION_CLEAR;
        if (clear != null)
            pass.action.colors.e0.clear_value = new sg_color { r = clear.R, g = clear.G, b = clear.B, a = clear.A };
        pass.attachments = _attachments;
        Gfx.begin_pass(&pass);
        GP.flush();
        GP.end();
        Gfx.end_pass();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Gfx.destroy_view(_colorView);
        Gfx.destroy_image(_color);
        if (_hasResolve) { Gfx.destroy_view(_resolveView); Gfx.destroy_image(_resolve); }
        if (_hasDepth) { Gfx.destroy_view(_depthView); Gfx.destroy_image(_depth); }
    }
}
