using System.Collections;
using System.Diagnostics;
using System.Numerics;
using Arch.CommandBuffer;
using Arch.Core;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Core.ImGUI;
using Zinc.Internal.Sokol;

namespace Zinc;

public class DSystem {}
public interface IPreUpdateSystem
{
    void PreUpdate(double dt);
}
public interface IUpdateSystem
{
    void Update(double dt);
}
public interface IPostUpdateSystem
{
    void PostUpdate(double dt);
}
public interface ICleanupSystem
{
    void Cleanup();
}