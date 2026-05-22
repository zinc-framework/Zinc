using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core.Extensions;

namespace Zinc;

// Marks a component member (method or property) that the Zinc.ECSGenerator should surface directly
// on any entity that uses the component. Instance members forward through a `ref` to the stored
// component (so mutations persist); a parameter of type Zinc.Entity is filled with the owning entity
// (and, if it's the only parameter, the member is exposed as a property). This is how MaterialComponent
// exposes `.Material`, and the general hook any component can use to expose behavior to its entities.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class EntityAccessibleAttribute : Attribute { }

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
        m.FsSize = Write(ref m.FsBytes, in fs);
    }

    /// <summary>Set the vertex uniform block (slot 0) from its generated std140 struct.</summary>
    public void SetVertexUniforms<TVs>(in TVs vs) where TVs : unmanaged
    {
        ref var m = ref _entity.ECSEntity.Get<MaterialComponent>();
        m.VsSize = Write(ref m.VsBytes, in vs);
    }

    /// <summary>Set both the vertex (slot 0) and fragment (slot 1) uniform blocks.</summary>
    public void SetUniforms<TVs, TFs>(in TVs vs, in TFs fs)
        where TVs : unmanaged where TFs : unmanaged
    {
        ref var m = ref _entity.ECSEntity.Get<MaterialComponent>();
        m.VsSize = Write(ref m.VsBytes, in vs);
        m.FsSize = Write(ref m.FsBytes, in fs);
    }

    private static int Write<T>(ref byte[] buffer, in T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (buffer is null || buffer.Length < size) buffer = new byte[size];
        MemoryMarshal.Write(buffer, in value);
        return size;
    }
}
