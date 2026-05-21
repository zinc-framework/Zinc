using System;
using System.Collections.Generic;
using Zinc.Internal.Sokol;

namespace Zinc;

/// <summary>
/// Holds the per-shader native factories emitted at build time from sokol-shdc
/// reflection. Each generated shader registers itself here via a
/// <c>[ModuleInitializer]</c>, so the table is populated before any scene runs.
///
/// Authoring (the typesafe <c>ShaderAsset</c> handle and the std140 uniform
/// record-structs) is produced by the Zinc.Magic source generator from the
/// <c>.glsl</c> alone and is always available; the compiled <c>sg_shader</c>
/// behind it only exists once the shdc build target has run. That's why a missing
/// entry is a runtime error pointing back at the build, not a compile error.
/// </summary>
public static class ShaderRegistry
{
    private static readonly Dictionary<string, Func<sg_backend, sg_shader>> _factories = new();

    /// <summary>Called by generated <c>[ModuleInitializer]</c> code; not for hand use.</summary>
    public static void Register(string name, Func<sg_backend, sg_shader> factory) => _factories[name] = factory;

    public static bool IsRegistered(string name) => _factories.ContainsKey(name);

    /// <summary>Builds the native shader for the current backend. Throws if the shdc build step hasn't produced it.</summary>
    public static sg_shader Make(string name)
    {
        if (!_factories.TryGetValue(name, out var factory))
            throw new InvalidOperationException(
                $"Shader '{name}' has no compiled backend registered. Ensure res/shaders/{name}.glsl exists and that the shdc build target ran (a normal `dotnet build`, not a design-time/IntelliSense build).");
        return factory(Gfx.query_backend());
    }
}
