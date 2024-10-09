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

public class DebugOverlaySystem : DSystem, IUpdateSystem
{
    QueryDescription entityPosition = new QueryDescription().WithAll<ActiveState,Position,EntityID,AdditionalEntityInfo>();
    QueryDescription colliderDebug = new QueryDescription().WithAll<ActiveState,Collider,Position,EntityID,AdditionalEntityInfo>();
    float colliderTick = 0.1f;
    float acc = 0f;
    public void Update(double dt)
    {
        acc += (float)dt;

        Engine.ECSWorld.Query(in entityPosition,
            (Arch.Core.Entity e, ref Position p, ref ActiveState a, ref EntityID o, ref AdditionalEntityInfo info) =>
            {
                if(!a.Active){return;}
                var activeEntity = Engine.GetEntity(o.ID);
                if(activeEntity == Engine.Cursor || activeEntity is TemporaryShape){return;}
                if(activeEntity is Anchor anchor)
                {
                    anchor.GetWorldTransform().transform.Decompose(out var winPos, out var rot, out var scale);
                    ImGUIHelper.Wrappers.SetNextWindowPosition(new Vector2(winPos.X-8,winPos.Y-8));
                    ImGUIHelper.Wrappers.SetNextWindowSize(200,20);
                    ImGUIHelper.Wrappers.SetNextWindowBGAlpha(0f);
                    ImGUIHelper.Wrappers.Begin($"{anchor.ID}", 
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | 
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground | 
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoMouseInputs |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoMove |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoBringToFrontOnFocus);
                    ImGUIHelper.Wrappers.Text($"{anchor.Name}");
                    ImGUIHelper.Wrappers.DrawCircle(winPos,2);
                    ImGUIHelper.Wrappers.End();

                }
            });

        if (Engine.drawDebugColliders)
        {
            Engine.ECSWorld.Query(in colliderDebug,
                (Arch.Core.Entity e, ref Position p, ref ActiveState a, ref Collider c, ref EntityID o, ref AdditionalEntityInfo info) =>
                {
                    if(!a.Active){return;}
                    var activeEntity = Engine.GetEntity(o.ID);
                    if(activeEntity == Engine.Cursor || activeEntity is TemporaryShape){return;}

                    var bounds = Utils.GetBounds(o.ID,c);
                    if(activeEntity is Anchor anchor)
                    {
                        anchor.GetWorldTransform().transform.Decompose(out var winPos, out var rot, out var scale);
                        if(acc > colliderTick)
                        {
                            foreach (var corner in bounds)
                            {
                                new TemporaryShape(colliderTick,4,4,color:Palettes.ENDESGA[12]){X = corner.X,Y = corner.Y,Collider_Active=false,RenderOrder=-1};
                            }
                        }
                        var db = info.DebugText;
                        // draw with imgui but idk tbh, the temp entity works pretty well
                        // ImGUIHelper.Wrappers.SetNextWindowBGAlpha(0f);
                        // ImGUIHelper.Wrappers.Window($"{anchor.ID}", winPos - new Vector2(100,100), new Vector2(500,500), () => {
                        //     // ImGUIHelper.Wrappers.Text($"test");
                        //     ImGUIHelper.Wrappers.DrawQuad(bounds);
                        // },condition:ImGuiCond_.ImGuiCond_Always,flags:ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground | ImGuiWindowFlags_.ImGuiWindowFlags_NoMouseInputs | ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoResize | ImGuiWindowFlags_.ImGuiWindowFlags_NoBringToFrontOnFocus);
                    }
                });
        }

        if(acc > colliderTick)
        {
            acc = 0f;
        }
    }
}