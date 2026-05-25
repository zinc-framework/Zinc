using System.Runtime.InteropServices;
using Zinc.Internal.Sokol;

namespace Zinc;

// GPU->CPU screenshot readback + PNG encode.
//
// sokol_gfx exposes no portable pixel readback (floooh/sokol#282 is still open even in the version we
// build), so we dispatch on the active backend and hand the backend-native texture handle to a small
// helper compiled into the stb lib, which does the backend-specific copy and then stbi_write_png.
//
// Status by backend:
//   - Metal (macOS/iOS): implemented & tested (see libs/stb/build/screenshot.m in Zinc.Bootstrapper).
//   - D3D11 (Windows) / GLCORE+GLES3 (Linux/web): the C# dispatch is here, and the native side is
//     written, but those native paths are only compile-checked (no hardware to run them on). They need
//     a Zinc.Bootstrapper rebuild for those RIDs + a real run to be considered verified.
//
// A CAMetalDrawable/swapchain image is framebufferOnly and cannot be read back; always capture an
// offscreen RenderTarget image (a normal, readable texture).
internal static unsafe class ScreenshotWriter
{
    // --- native entry points (libstb) ---
    // Metal: blit the texture into a shared staging buffer on sokol's command queue, then write the PNG.
    [DllImport("stb", EntryPoint = "zinc_write_texture_png", CallingConvention = CallingConvention.Cdecl)]
    private static extern int zinc_write_texture_png(void* mtlTexture, void* mtlQueue, byte* path, int flipY);

    // D3D11: copy to a STAGING texture, Map, swizzle, write the PNG.
    [DllImport("stb", EntryPoint = "zinc_write_d3d11_texture_png", CallingConvention = CallingConvention.Cdecl)]
    private static extern int zinc_write_d3d11_texture_png(void* device, void* context, void* tex2d, int width, int height, byte* path, int flipY);

    // GL/GLES: attach the texture to a temporary FBO, glReadPixels, write the PNG.
    [DllImport("stb", EntryPoint = "zinc_write_gl_texture_png", CallingConvention = CallingConvention.Cdecl)]
    private static extern int zinc_write_gl_texture_png(uint glTexture, int width, int height, byte* path, int flipY);

    /// <summary>
    /// Write a sokol color image to <paramref name="path"/> as an 8-bit RGBA PNG. Set
    /// <paramref name="flipY"/> when the image was rendered with a flipped (sampling) projection so the
    /// saved file is upright. Returns false (and logs) on unsupported backends or native errors.
    /// </summary>
    public static bool SaveImage(sg_image img, int width, int height, string path, bool flipY)
    {
        if (img.id == Gfx.SG_INVALID_ID) return false;

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var pathBytes = System.Text.Encoding.UTF8.GetBytes(path + '\0');
        int flip = flipY ? 1 : 0;
        var backend = Gfx.query_backend();
        try
        {
            fixed (byte* p = pathBytes)
            {
                switch (backend)
                {
                    case sg_backend.SG_BACKEND_METAL_MACOS:
                    case sg_backend.SG_BACKEND_METAL_IOS:
                    case sg_backend.SG_BACKEND_METAL_SIMULATOR:
                    {
                        var info = Gfx.mtl_query_image_info(img);
                        void* tex = info.tex[info.active_slot];
                        return zinc_write_texture_png(tex, Gfx.mtl_command_queue(), p, flip) != 0;
                    }
                    case sg_backend.SG_BACKEND_D3D11:
                    {
                        var info = Gfx.d3d11_query_image_info(img);
                        return zinc_write_d3d11_texture_png(Gfx.d3d11_device(), Gfx.d3d11_device_context(), info.tex2d, width, height, p, flip) != 0;
                    }
                    case sg_backend.SG_BACKEND_GLCORE:
                    case sg_backend.SG_BACKEND_GLES3:
                    {
                        var info = Gfx.gl_query_image_info(img);
                        return zinc_write_gl_texture_png(info.tex[info.active_slot], width, height, p, flip) != 0;
                    }
                    default:
                        Console.WriteLine($"[Screenshot] backend {backend} not supported — skipped");
                        return false;
                }
            }
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("[Screenshot] native stb library not found — screenshot skipped");
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            Console.WriteLine($"[Screenshot] readback for backend {backend} is not compiled into this stb build — screenshot skipped");
            return false;
        }
    }
}
