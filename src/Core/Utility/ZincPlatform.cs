using System.Runtime.InteropServices;

namespace Zinc;

/// <summary>
/// Managed surface for the zinc_platform native lib's cross-platform virtual-memory API
/// (libs/zinc_platform/build/zinc_memory.c). Reserve an address range, commit/decommit physical
/// pages on demand, release, and guard-protect — the OS-level primitive that backs arena / bump
/// allocators (per-frame scratch, transient load buffers) where you want a large reserved range
/// but only pay for the pages you touch. Windows routes to the VirtualAlloc family; POSIX to
/// mmap/munmap/mprotect/madvise(MADV_DONTNEED).
///
/// These DllImports MUST live in the Zinc assembly: Zinc registers its DllImportResolver on
/// THIS assembly (see NativeLibResolver), which is what rewrites the bare "zinc_platform" name
/// to libs/runtimes/&lt;rid&gt;/native/zinc_platform.{dll,dylib,so}. A [DllImport("zinc_platform")]
/// from a different assembly would bypass that resolver and fail to load, so any consumer that
/// needs raw virtual memory calls through this class instead of importing the symbols itself.
/// </summary>
public static unsafe class ZincPlatform
{
    private const string Lib = "zinc_platform";
    private const CallingConvention Cdecl = CallingConvention.Cdecl;

    [DllImport(Lib, EntryPoint = "zinc_mem_reserve",         CallingConvention = Cdecl)]
    private static extern void* zinc_mem_reserve(long size);
    [DllImport(Lib, EntryPoint = "zinc_mem_commit",          CallingConvention = Cdecl)]
    private static extern void  zinc_mem_commit(void* ptr, long size);
    [DllImport(Lib, EntryPoint = "zinc_mem_decommit",        CallingConvention = Cdecl)]
    private static extern void  zinc_mem_decommit(void* ptr, long size);
    [DllImport(Lib, EntryPoint = "zinc_mem_release",         CallingConvention = Cdecl)]
    private static extern void  zinc_mem_release(void* ptr, long size);
    [DllImport(Lib, EntryPoint = "zinc_mem_protect",         CallingConvention = Cdecl)]
    private static extern void  zinc_mem_protect(void* ptr, long size);
    [DllImport(Lib, EntryPoint = "zinc_mem_requires_commit", CallingConvention = Cdecl)]
    private static extern int   zinc_mem_requires_commit();
    [DllImport(Lib, EntryPoint = "zinc_mem_page_size",       CallingConvention = Cdecl)]
    private static extern long  zinc_mem_page_size();

    /// <summary>Reserve an address range without committing physical pages. Returns null on failure.</summary>
    public static void* MemReserve(long size) => zinc_mem_reserve(size);
    /// <summary>Commit (back with physical pages) a previously reserved range. No-op on POSIX.</summary>
    public static void MemCommit(void* ptr, long size) => zinc_mem_commit(ptr, size);
    /// <summary>Release the physical pages of a range while keeping the address space reserved.</summary>
    public static void MemDecommit(void* ptr, long size) => zinc_mem_decommit(ptr, size);
    /// <summary>Release the whole reserved range back to the OS.</summary>
    public static void MemRelease(void* ptr, long size) => zinc_mem_release(ptr, size);
    /// <summary>Make a range inaccessible (guard/poison page) for debugging.</summary>
    public static void MemProtect(void* ptr, long size) => zinc_mem_protect(ptr, size);
    /// <summary>True if the platform needs an explicit commit step after reserve (Windows).</summary>
    public static bool MemRequiresCommit() => zinc_mem_requires_commit() != 0;
    /// <summary>System virtual-memory page size in bytes.</summary>
    public static long MemPageSize() => zinc_mem_page_size();
}
