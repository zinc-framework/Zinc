using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace Zinc.Core;

internal static class NativeLibResolver
{
    static class Constants
    {
        public const string sokol = "sokol";
        public const string sokol_osx = "libs/runtimes/osx-arm64/native/libsokol.dylib";
        public const string sokol_linux = "libs/runtimes/linux-x64/native/libsokol.so";
        public const string sokol_win = "libs/runtimes/win-x64/native/sokol.dll";

        public const string box2d = "box2d";
        public const string box2d_osx = "libs/runtimes/osx-arm64/native/libbox2d.dylib";
        public const string box2d_linux = "libs/runtimes/linux-x64/native/libbox2d.so";
        public const string box2d_win = "libs/runtimes/win-x64/native/box2d.dll";

        public const string stb = "stb";
        public const string stb_osx = "libs/runtimes/osx-arm64/native/libstb.dylib";
        public const string stb_linux = "libs/runtimes/linux-x64/native/libstb.so";
        public const string stb_win = "libs/runtimes/win-x64/native/stb.dll";

        public const string zinc_platform = "zinc_platform";
        public const string zinc_platform_osx = "libs/runtimes/osx-arm64/native/libzinc_platform.dylib";
        public const string zinc_platform_linux = "libs/runtimes/linux-x64/native/libzinc_platform.so";
        public const string zinc_platform_win = "libs/runtimes/win-x64/native/zinc_platform.dll";
    }
    
    static NativeLibResolver()
    {
        // NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
        NativeLibrary.SetDllImportResolver(typeof(Zinc.Scene).Assembly, DllImportResolver);
    }

    public static void kick(){}
    
    // Pick the platform-specific path for a logical lib name. Each lib has a (win, osx, linux)
    // triple; ForPlatform selects one.
    //
    // NB: do NOT use Environment.OSVersion.Platform here — it returns PlatformID.Unix on BOTH
    // Linux and macOS, so Linux would resolve to the .dylib path and never load. RuntimeInformation
    // distinguishes them correctly.
    static string GetLibraryName(string libraryName)
        => libraryName switch
        {
            Constants.sokol         => ForPlatform(Constants.sokol_win,         Constants.sokol_osx,         Constants.sokol_linux),
            Constants.box2d         => ForPlatform(Constants.box2d_win,         Constants.box2d_osx,         Constants.box2d_linux),
            Constants.stb           => ForPlatform(Constants.stb_win,           Constants.stb_osx,           Constants.stb_linux),
            Constants.zinc_platform => ForPlatform(Constants.zinc_platform_win, Constants.zinc_platform_osx, Constants.zinc_platform_linux),
            _ => libraryName,
        };

    static string ForPlatform(string win, string osx, string linux)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return win;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))     return osx;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))   return linux;
        // Unknown platform: hand the bare name to the loader and let default probing try.
        return linux;
    }
    
    public static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // Console.WriteLine(Environment.OSVersion.Platform);
        var platformDependentName = GetLibraryName(libraryName);
        // Console.WriteLine($"resolving {libraryName} to {platformDependentName}");
        IntPtr handle;
        var loaded = NativeLibrary.TryLoad(platformDependentName, assembly, searchPath, out handle);
        if (!loaded)
        {
            Console.WriteLine($"NativeLibResolver Failed loading {libraryName} - do you update the platform paths for where the DLL is?");
        }
        return handle;
    }
}