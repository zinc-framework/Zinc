using System.Reflection;

namespace Zinc;

public static partial class Engine
{
    /// <summary>
    /// A folder for what the game keeps from one run to the next and players can add to: saves,
    /// settings, mods. Zinc's Application.persistentDataPath. Unlike Resources.ContentDirectory it's
    /// per user, writable, and outside the install, so updates leave it alone and a signed macOS
    /// bundle stays intact. It's {user data}/{Company}/{Product}, from the game's csproj
    /// (which the SDK defaults to the AssemblyName), and is created the first time it's asked for:
    /// AppData\LocalLow on Windows, ~/Library/Application Support on macOS, ~/.local/share on Linux.
    /// </summary>
    public static string PersistentDataPath => persistentDataPath.Value;

    static readonly Lazy<string> persistentDataPath = new(() =>
    {
        var userData = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow")
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        // No user data folder when there's no home directory (some CI and container setups).
        if (string.IsNullOrEmpty(userData)) userData = Path.GetTempPath();

        // Only missing from an assembly built with GenerateAssemblyInfo off.
        var game = Assembly.GetEntryAssembly();
        var company = game?.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
        var product = game?.GetCustomAttribute<AssemblyProductAttribute>()?.Product;

        var path = Path.Combine(userData,
            FolderName(string.IsNullOrWhiteSpace(company) ? "zincDefaultCompany" : company),
            FolderName(string.IsNullOrWhiteSpace(product) ? "zincDefaultGame" : product));
        Directory.CreateDirectory(path);
        return path;
    });

    // The name with whatever the file system won't take swapped out, e.g. the colon in "Studio: Games".
    static string FolderName(string name) => string.Join('_', name.Trim().Split(Path.GetInvalidFileNameChars()));
}
