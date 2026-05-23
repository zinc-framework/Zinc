using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core.Extensions;
using Zinc.Internal.Sokol;

namespace Zinc;

// Per-object shader override + latched uniform bytes. Baked into Shape/Sprite; default Shader
// (Resources.Shader.Default) means "use sgp's built-in pipeline", i.e. today's behavior. The byte
// buffers hold the most-recent uniform block(s) set in Update until the renderer uploads them at draw.
//
// Fields are internal: the public surface is the `.Material` accessor + `.Shader` property, exposed
// onto entities by the [EntityAccessible] members below via the generator.
[Arch.AOT.SourceGenerator.Component]
public record struct MaterialComponent : IComponent
{
    internal Resources.Shader _shader;
    internal byte[] VsBytes;
    internal byte[] FsBytes;
    internal int VsSize;
    internal int FsSize;

    // Per-channel image/sampler bindings for a custom shader's texture slots (sgp channels). Managed
    // arrays (lazily allocated) — NOT [InlineArray], which corrupts Arch component storage. The masks
    // record which channels are bound so the renderer binds/resets only those.
    // == sokol_gp's SGP_TEXTURE_SLOTS, set in Zinc.Bootstrapper's sokol.c (upstream default is 4).
    internal const int TextureSlots = 8;
    // Max combined (vs+fs) uniform bytes per draw == SGP_UNIFORM_CONTENT_SLOTS * sizeof(float).
    // MUST match the native sokol_gp build: Zinc.Bootstrapper's sokol.c sets SGP_UNIFORM_CONTENT_SLOTS=64
    // (-> 256 bytes; upstream default is 8 -> 32). Raising it further requires a Zinc.Bootstrapper
    // rebuild + bumping this. We validate against it so an over-cap block throws a clear error instead
    // of silently overflowing sokol_gp's buffer (the SOKOL_ASSERT is compiled out in release builds).
    internal const int MaxUniformBytes = 256;
    internal sg_image[] Images;
    internal sg_sampler[] Samplers;
    internal int ImageMask;
    internal int SamplerMask;

    /// <summary>Custom sgp shader for this object; defaults to the sgp built-in pipeline.</summary>
    [EntityAccessible]
    public Resources.Shader Shader
    {
        get => _shader ?? Resources.Shader.Default;
        set => _shader = value;
    }

    [EntityAccessible]
    public static MaterialAccessor Material(Entity self) => new(self);
}

// Handle reached via `entity.Material`. Uniforms are set by handing it the generated std140
// record-struct for a shader stage's block; the whole block is copied into the latch buffer and
// uploaded at draw via sgp_set_uniform. This mirrors sokol's sg_apply_uniforms model (hand it the
// block's bytes) — the generated struct's [StructLayout(Size=...)] equals the std140 block size, so
// the upload size is correct by construction (satisfies sokol's VALIDATE_AU_SIZE).
public readonly struct MaterialAccessor
{
    private readonly Entity _entity;
    internal MaterialAccessor(Entity entity) => _entity = entity;

    /// <summary>Set the fragment uniform block (slot 1) from its generated std140 struct.</summary>
    public void SetFragmentUniforms<TFs>(in TFs fs) where TFs : unmanaged
    {
        ref var m = ref _entity.ECSEntity.Get<MaterialComponent>();
        CheckUniformSize(m.VsSize + Unsafe.SizeOf<TFs>());
        m.FsSize = Write(ref m.FsBytes, in fs);
    }

    /// <summary>Set the vertex uniform block (slot 0) from its generated std140 struct.</summary>
    public void SetVertexUniforms<TVs>(in TVs vs) where TVs : unmanaged
    {
        ref var m = ref _entity.ECSEntity.Get<MaterialComponent>();
        CheckUniformSize(Unsafe.SizeOf<TVs>() + m.FsSize);
        m.VsSize = Write(ref m.VsBytes, in vs);
    }

    /// <summary>Set both the vertex (slot 0) and fragment (slot 1) uniform blocks.</summary>
    public void SetUniforms<TVs, TFs>(in TVs vs, in TFs fs)
        where TVs : unmanaged where TFs : unmanaged
    {
        ref var m = ref _entity.ECSEntity.Get<MaterialComponent>();
        CheckUniformSize(Unsafe.SizeOf<TVs>() + Unsafe.SizeOf<TFs>());
        m.VsSize = Write(ref m.VsBytes, in vs);
        m.FsSize = Write(ref m.FsBytes, in fs);
    }

    // sokol_gp stores combined vs+fs uniforms in a fixed per-draw buffer; overflowing it silently
    // corrupts memory in release builds. Fail loudly with an actionable message instead.
    private static void CheckUniformSize(int combinedBytes)
    {
        if (combinedBytes > MaterialComponent.MaxUniformBytes)
            throw new InvalidOperationException(
                $"Combined vertex+fragment uniform size ({combinedBytes} bytes) exceeds sokol_gp's per-draw " +
                $"limit of {MaterialComponent.MaxUniformBytes} bytes (SGP_UNIFORM_CONTENT_SLOTS=" +
                $"{MaterialComponent.MaxUniformBytes / 4}). Raise SGP_UNIFORM_CONTENT_SLOTS in Zinc.Bootstrapper " +
                "and rebuild the native sokol lib, or shrink the uniform block.");
    }

    /// <summary>Bind a texture to a custom shader's image channel (maps to sgp_set_image(channel)).</summary>
    public void SetImage(int channel, Resources.Texture texture)
    {
        if ((uint)channel >= MaterialComponent.TextureSlots)
            throw new ArgumentOutOfRangeException(nameof(channel), channel,
                $"sokol_gp allows {MaterialComponent.TextureSlots} texture channels (SGP_TEXTURE_SLOTS).");
        if (!texture.DataLoaded) texture.Load();
        ref var m = ref _entity.ECSEntity.Get<MaterialComponent>();
        m.Images ??= new sg_image[MaterialComponent.TextureSlots];
        m.Images[channel] = texture.Data;
        m.ImageMask |= 1 << channel;
    }

    /// <summary>Bind a sampler to a custom shader's image channel (maps to sgp_set_sampler(channel)).</summary>
    public void SetSampler(int channel, Sampler sampler)
    {
        if ((uint)channel >= MaterialComponent.TextureSlots)
            throw new ArgumentOutOfRangeException(nameof(channel), channel,
                $"sokol_gp allows {MaterialComponent.TextureSlots} texture channels (SGP_TEXTURE_SLOTS).");
        ref var m = ref _entity.ECSEntity.Get<MaterialComponent>();
        m.Samplers ??= new sg_sampler[MaterialComponent.TextureSlots];
        m.Samplers[channel] = sampler.Handle;
        m.SamplerMask |= 1 << channel;
    }

    private static int Write<T>(ref byte[] buffer, in T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (buffer is null || buffer.Length < size) buffer = new byte[size];
        MemoryMarshal.Write(buffer, in value);
        return size;
    }
}
