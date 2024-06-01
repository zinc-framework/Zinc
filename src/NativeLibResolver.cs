using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace Zinc;

internal static class NativeLibResolver
{
    // static class Constants
    // {
    //     public const string sokol = "sokol";
    //     public const string sokol_osx = "runtimes/osx-arm64/native/libsokol.dylib";
    //     public const string sokol_linux = "libsokol.dylib";
    //     public const string sokol_win = "sokol.dll";
    //     
    //     
    //     public const string cute = "cute";
    //     public const string cute_osx = "runtimes/osx-arm64/native/libcute.dylib";
    //     public const string cute_linux = "libcute.dylib";
    //     public const string cute_win = "cute.dll";
    //     
    //     public const string stb = "stb";
    //     public const string stb_osx = "runtimes/osx-arm64/native/libstb.dylib";
    //     public const string stb_linux = "libstb.dylib";
    //     public const string stb_win = "stb.dll";
    // }
    //
    // static NativeLibResolver()
    // {
    //     NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
    // }
    //
    // public static void kick(){}
    //
    // static string GetLibraryName(string libraryName)
    //     => libraryName switch
    //     {
    //         Constants.sokol => Environment.OSVersion.Platform switch
    //         {
    //             PlatformID.Win32NT => Constants.sokol_win,
    //             PlatformID.Unix => Constants.sokol_osx,
    //             _ => Constants.sokol_linux,
    //         },
    //         Constants.cute => Environment.OSVersion.Platform switch
    //         {
    //             PlatformID.Win32NT => Constants.cute_win,
    //             PlatformID.Unix => Constants.cute_osx,
    //             _ => Constants.cute_linux,
    //         },
    //         Constants.stb => Environment.OSVersion.Platform switch
    //         {
    //             PlatformID.Win32NT => Constants.stb_win,
    //             PlatformID.Unix => Constants.stb_osx,
    //             _ => Constants.stb_linux,
    //         },
    //         _ => libraryName,
    //     };
    //
    // public static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    // {
    //     // Console.WriteLine(Environment.OSVersion.Platform);
    //     var platformDependentName = GetLibraryName(libraryName);
    //     // Console.WriteLine($"resolving {libraryName} to {platformDependentName}");
    //     IntPtr handle;
    //     var loaded = NativeLibrary.TryLoad(platformDependentName, assembly, searchPath, out handle);
    //     if (!loaded)
    //     {
    //         Console.WriteLine($"Failed loading {libraryName}");
    //     }
    //     else
    //     {
    //         Console.WriteLine($"Loaded {libraryName}");
    //     }
    //     return handle;
    // }
}