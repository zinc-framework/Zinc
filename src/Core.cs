using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zinc.Internal.STB;
using Arch.Core;
using Zinc.Core;
using Zinc.Core.ImGUI;
using FontStashSharp;
using Utils = Zinc.NativeInterop.Utils;

namespace Zinc;
using Internal.Sokol;

public static partial class Engine
{
    private static int entityIDCounter = 0;
    public static int GetNextEntityID()
    {
        var curr = entityIDCounter;
        entityIDCounter++;
        return curr;
    }
    
    public static Action Update;
    public static Action Setup;
    public static InputSystem InputSystem = new InputSystem();
    static DestructionSystem DestructionSystem = new DestructionSystem();
    static EntityUpdateSystem EntityUpdate = new ();
    static DebugOverlaySystem DebugOverlay = new ();
    public static string DebugTextStr = "";
    static Color ClearColor;

    public static void SetClearColor(Color c)
    {
        ClearColor = c;
    }

    private static HashSet<DSystem> DefaultSystems = new HashSet<DSystem>()
    {
        //preupdate
        new FrameAnimationSystem(),
        //update
        InputSystem,
        new SceneUpdateSystem(),
        new CoroutineSystem(),
        new CollisionSystem(),
        new CollisionCallbackSystem(),
        //render - postupdate
        new SceneRenderSystem(),
        // new EntityRenderSystem(),
        
        //we dont process raw render stuff
        // new ParticleRenderSystem(),
        // new SpriteRenderSystem(),
        // new ShapeRenderSystem(),
        //cleanup
        new EventCleaningSystem(),
    };
    static HashSet<DSystem> ActiveSystems = new();
    public static void RegisterSystem(DSystem system)
    {
        ActiveSystems.Add(system);
    }
    public static void RegisterSystems(HashSet<DSystem> systems)
    {
        ActiveSystems.UnionWith(systems);
    }
    public static void UnregisterSystems(HashSet<DSystem> systems)
    {
        ActiveSystems.ExceptWith(systems);
    }
    public static void UnregisterSystem(DSystem system)
    {
        ActiveSystems.Remove(system);
    }

    public static uint idCounter;
    public static Volatile.VoltWorld PhysicsWorld;
    public static World ECSWorld;
    public static Scene GlobalScene;
    public static Pointer Cursor;
    public static Scene TargetScene { get; private set; } = GlobalScene;
    public static void SetTargetScene(Scene s)
    {
        if (s.MountStatus != SceneMountStatus.Mounted)
        {
            Console.WriteLine("scene must be mounted to be target");
            return;
        }
        if (s.LoadStatus != SceneLoadStatus.Loaded)
        {
            if (s.LoadStatus == SceneLoadStatus.Loading)
            {
                Console.WriteLine("scene cant be target until loading finished");
            }
            else
            {
                Console.WriteLine("scene must be loaded before becoming target");
            }
            return;
        }

        TargetScene = s;
    }

    /// <summary>
    /// Key is the managed entity ID, value is the managed entity
    /// </summary>
    public static Dictionary<int, Entity> EntityLookup = new();
    /// <summary>
    /// Key is the Scene ID, value is the Scene
    /// </summary>
    public static Dictionary<int, Scene> SceneLookup = new();
    /// <summary>
    /// Key is the Scene ID, value is the list of managed entity IDs
    /// </summary>
    public static Dictionary<int, List<int>> SceneEntityMap = new();
    /// <summary>
    /// Key is the ID of the Scene, value is the depth
    /// </summary>
    public static Dictionary<int, int> MountedScenes = new();

    public static List<(Scene scene,Action callback)> scenesStagedForUnmounting = new ();
    private static bool hasScenesStagedForUnmounting = false;

    static void OnSceneUnmounted(Scene s, Action callback)
    {
        scenesStagedForUnmounting.Add((s, callback));
        hasScenesStagedForUnmounting = true;
    }

    
    public record RunOptions(int width, int height, string appName, Action setup = null, Action update = null);

    private static RunOptions defaultOpts = new(500, 500, "dinghy",null,null);
    public static void Run(RunOptions opts = null)
    {
        if (opts != null)
        {
            if (opts.update != null)
            {
                Update += opts.update;
            }
            if (opts.setup != null)
            {
                Setup += opts.setup;
            }
        }
        Boot(opts == null ? defaultOpts : opts);
    }


    static internal void Boot(RunOptions opts)
    {
        NativeLibResolver.kick(); //inits the static lib resolver 
        
        unsafe
        {
            var window_title = System.Text.Encoding.UTF8.GetBytes(opts.appName);
            fixed (byte* ptr = window_title)
            {
                //init
                sapp_desc desc = default;
                desc.width = opts.width;
                desc.height = opts.height;
                desc.icon.sokol_default = 1;
                desc.window_title = (sbyte*)ptr;
                desc.init_cb = &Initialize;
                desc.event_cb = &Event;
                desc.frame_cb = &Frame;
                desc.cleanup_cb = &Cleanup;
                desc.logger.func = &Sokol_Logger;
                App.run(&desc);
            }
        }
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void Event(sapp_event* e)
    {
        sapp_event ev = *e;
         if (ImGUI.handle_event(e) > 0)
         {
             /*
              * if you're using sokol_app.h, from inside the sokol_app.h event callback,
                 call:

                 bool simgui_handle_event(const sapp_event* ev);

                 The return value is the value of ImGui::GetIO().WantCaptureKeyboard,
                 if this is true, you might want to skip keyboard input handling
                 in your own event handler.

                 If you want to use the ImGui functions for checking if a key is pressed
                 (e.g. ImGui::IsKeyPressed()) the following helper function to map
                 an sapp_keycode to an ImGuiKey value may be useful:

                 int simgui_map_keycode(sapp_keycode c);

                 Note that simgui_map_keycode() can be called outside simgui_setup()/simgui_shutdown().
              */
         }
         else
         {
             ECSWorld.Create(
                 new EventMeta("FRAME_EVENT"),
                 new FrameEvent(ev));
         }
        // InputSystem.FrameEvents.Add(ev);

        // Console.WriteLine(ev);
        // var width = App.width();
        // var height = App.height();
        // Console.WriteLine(e->type);
    }

    public struct core_state
    {
        public sg_pass_action pass_action;
        public imageInfo checkerboard;
        public sg_sampler smp;
    }

    public struct imageInfo
    {
        public int width;
        public int height;
        public sg_image img;
    }
    
    
    public unsafe struct font_context
    {
        public void* ctx;
    }

    public static bool Clear = true;

    public static core_state state = default;
    public static font_context font_state = default;
    
    public static sg_imgui_t gfx_dbgui = default;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void Initialize()
    {
        //sokol init
        sg_desc desc = default;
        desc.context = Glue.sapp_sgcontext();
        //call our own logger
        desc.logger.func = &Sokol_Logger;
        //call native logger
        // desc.logger.func = (delegate* unmanaged[Cdecl]<sbyte*, uint, uint, sbyte*, uint, sbyte*, void*, void>)NativeLibrary.GetExport(NativeLibrary.Load("libs/sokol"), "slog_func");
        Gfx.setup(&desc);

        sgl_desc_t gl_desc = default;
        GL.setup(&gl_desc);

        simgui_desc_t imgui_desc = default;
        imgui_desc.logger.func = &Sokol_Logger;
        ImGUI.setup(&imgui_desc);
        
        sg_imgui_desc_t sg_imgui_desc = default;
        gfx_dbgui.buffers.open = 1;
        gfx_dbgui.images.open = 1;
        gfx_dbgui.samplers.open = 1;
        gfx_dbgui.shaders.open = 1;
        gfx_dbgui.pipelines.open = 1;
        gfx_dbgui.passes.open = 1;
        gfx_dbgui.capture.open = 1;
        fixed (sg_imgui_t* ctx = &gfx_dbgui)
        {
            GfxDebugGUI.init(ctx,&sg_imgui_desc);
        }

        sgp_desc gp_desc = default;
        gp_desc.max_vertices = 1000000;
        GP.setup(&gp_desc);

        sdtx_desc_t debug_text_desc = default;
        debug_text_desc.fonts[0] = DebugText.font_kc853();
        debug_text_desc.fonts[1] = DebugText.font_kc854();
        debug_text_desc.fonts[2] = DebugText.font_z1013();
        debug_text_desc.fonts[3] = DebugText.font_cpc();
        debug_text_desc.fonts[4] = DebugText.font_c64();
        debug_text_desc.fonts[5] = DebugText.font_oric();
        debug_text_desc.logger.func = &Sokol_Logger;
        DebugText.setup(&debug_text_desc);
            
        ClearColor = new Color(Palettes.ONE_BIT_MONITOR_GLOW[0]);
        
        
        // a checkerboard texture
        var checkerboardTexSize = 128;
        var checkSize = checkerboardTexSize / 8;
        uint WHITE = 0xFFFFFFFF;
        uint BLUE = 0xFFFF0000;
        var pixels = new Zinc.NativeInterop.Utils.NativeArray<uint>(checkerboardTexSize*checkerboardTexSize);
        for (int i = 0; i < (checkerboardTexSize*checkerboardTexSize); i++)
        {
            var x = i % checkerboardTexSize;
            var y = i / checkerboardTexSize;
            if ((y / checkSize) % 2 == 0)
            {
                pixels[i] = (x / checkSize) % 2 == 0 ? BLUE : WHITE;
            }
            else
            {
                pixels[i] = (x / checkSize) % 2 == 0 ? WHITE : BLUE;
            }
        }

        var checkerboard_desc = default(sg_image_desc);
        checkerboard_desc.width = checkerboardTexSize;
        checkerboard_desc.height = checkerboardTexSize;
        checkerboard_desc.data.subimage.e0_0 = pixels.AsSgRange();
        state.checkerboard.img = Gfx.make_image(&checkerboard_desc);
        state.checkerboard.width = checkerboardTexSize;
        state.checkerboard.height = checkerboardTexSize;
        
        // ... and a sampler
        sg_sampler_desc sample_desc = default;
        sample_desc.min_filter = sg_filter.SG_FILTER_LINEAR;
        sample_desc.mag_filter = sg_filter.SG_FILTER_LINEAR;
        sample_desc.mipmap_filter = sg_filter.SG_FILTER_LINEAR;
        sample_desc.max_anisotropy = 8;
        // sample_desc.mipmap_filter = sg_filter.SG_FILTER_NEAREST;
        state.smp = Gfx.make_sampler(&sample_desc);
        // GP.sgp_set_sampler(0,state.smp);
        
        Width = App.width();
        Height = App.height();

        ActiveSystems = new HashSet<DSystem>(DefaultSystems);
        
        PhysicsWorld = new ();
        ECSWorld = World.Create();
        GlobalScene = new(){Name = "Global Scene"};

        
        //USE SOKOL_FONTSTASH - NOT WORKING BECAUSE CANT GENERATE FONTSTASH.H BINDINGS
        // sfons_desc_t font_desc = default;
        // font_desc.width = 128;
        // font_desc.height = 128;
        // font_state.ctx = Fontstash.create(&font_desc);
        // Fontstash.destroy(font_state.ctx);
        
        // var settings = new FontSystemSettings
        // {
        //     FontResolutionFactor = 2,
        //     KernelWidth = 2,
        //     KernelHeight = 2
        // };
        //
        // fontSystem = new FontSystem(settings);
        // fontSystem.AddFont(File.ReadAllBytes(@"data/fonts/hack/Hack-Bold.ttf"));
        
        GlobalScene.Mount(-1);
        GlobalScene.Load(() => {GlobalScene.Start();});

        Cursor = new() { Name = "Cursor" };

        Events.SceneUnmounted += OnSceneUnmounted;
        Setup?.Invoke();
    }

    public static FontSystem fontSystem;
    public static FontstashRenderer fontRenderer = new();



    public static int Width;
    public static int Height;

    private static float angle_deg = 0;
    private static float scale = 0;

    public static ulong FrameCount;
    public static double DeltaTime;
    public static double Time;


    public static bool showStats = true;
    public static bool showIMGUIDemo = false;
    public static bool drawDebugOverlay = false;
    public static bool drawDebugColliders = false;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void Frame()
    {
        FrameCount = App.frame_count();
        DeltaTime = App.frame_duration();
        Time += DeltaTime;
        
        // float t = (float)App.frame_duration() * 60.0f;
        float t = (float)DeltaTime * 1000.0f;
        // Console.WriteLine($"{t}ms");
        Width = App.width();
        Height = App.height();

        simgui_frame_desc_t imgui_frame = default;
        imgui_frame.width = Width;
        imgui_frame.height = Height;
        imgui_frame.delta_time = DeltaTime;
        imgui_frame.dpi_scale = App.dpi_scale();
        ImGUI.new_frame(&imgui_frame);

        ImGUIHelper.Wrappers.BeginMainMenuBar();
        if (ImGUIHelper.Wrappers.BeginMenu("Dinghy"))
        {
            ImGUIHelper.Wrappers.Checkbox("Show Stats", ref showStats);
            ImGUIHelper.Wrappers.Checkbox("Show IMGUI Demo", ref showIMGUIDemo);
            ImGUIHelper.Wrappers.Checkbox("Draw Debug Overlay", ref drawDebugOverlay);
            ImGUIHelper.Wrappers.Checkbox("Draw Debug Colliders", ref drawDebugColliders);
            foreach (var i in MountedScenes)
            {
                var scene = SceneLookup[i.Key];
                ImGUIHelper.Wrappers.Text($"{scene.Name} {scene.Status}");
            }
            ImGUIHelper.Wrappers.EndMenu();
        }
        
        if (ImGUIHelper.Wrappers.BeginMenu("Sokol"))
        {
            ImGUIHelper.Wrappers.Checkbox("Capabilities", ref gfx_dbgui.caps.open);
            ImGUIHelper.Wrappers.Checkbox("Frame Stats", ref gfx_dbgui.frame_stats.open);
            ImGUIHelper.Wrappers.Checkbox("Buffers", ref gfx_dbgui.buffers.open);
            ImGUIHelper.Wrappers.Checkbox("Images", ref gfx_dbgui.images.open);
            ImGUIHelper.Wrappers.Checkbox("Samplers", ref gfx_dbgui.samplers.open);
            ImGUIHelper.Wrappers.Checkbox("Shaders", ref gfx_dbgui.shaders.open);
            ImGUIHelper.Wrappers.Checkbox("Pipelines", ref gfx_dbgui.pipelines.open);
            ImGUIHelper.Wrappers.Checkbox("Passes", ref gfx_dbgui.passes.open);
            ImGUIHelper.Wrappers.Checkbox("Capture", ref gfx_dbgui.capture.open);
            ImGUIHelper.Wrappers.EndMenu();
        }
        ImGUIHelper.Wrappers.EndMainMenuBar();
        
        if (showStats)
        {
            int ec = 0;
            foreach (var l in SceneEntityMap.Values)
            {
                ec += l.Count;
            }
            ImGUIHelper.Wrappers.ShowStats($"{t}ms",$"Entities: {ec}",$"{InputSystem.MouseX},{InputSystem.MouseY}");
        }

        fixed (bool* dem_ptr = &showIMGUIDemo)
        {
            if (showIMGUIDemo)
            {
                ImGUI.igShowDemoWindow(dem_ptr);
            }
        }

        fixed (sg_imgui_t* ctx = &gfx_dbgui)
        {
            GfxDebugGUI.draw(ctx);
        }
        
        float ratio = Width/(float)Height;

        // Begin recording draw commands for a frame buffer of size (width, height).
        GP.begin(Width, Height);
        // Set frame buffer drawing region to (0,0,width,height).
        GP.viewport(0, 0, Width, Height);
        // Set drawing coordinate space to (left=-ratio, right=ratio, top=1, bottom=-1).
        // GP.sgp_project(-ratio, ratio, 1.0f, -1.0f);

        // Clear the frame buffer.
        if (Clear)
        {
            GP.set_color(ClearColor.internal_color.r, ClearColor.internal_color.g, ClearColor.internal_color.b, ClearColor.internal_color.a);
            GP.clear();
            GP.reset_color();
        }
        
        Cursor.X = InputSystem.MouseX;
        Cursor.Y = InputSystem.MouseY;

        foreach (var s in ActiveSystems)
        {
            //TODO: need to sort systems by priority
            if (s is IPreUpdateSystem us)
            {
                us.PreUpdate(DeltaTime);
            }
        }
        PhysicsWorld.Update();
        Update?.Invoke();
        EntityUpdate.Update(DeltaTime);
        foreach (var s in ActiveSystems)
        {
            //TODO: need to sort systems by priority
            if (s is IUpdateSystem us)
            {
                if(s is EntityUpdateSystem) {continue;} //we handle this above
                us.Update(DeltaTime);
            }
        }
        foreach (var s in ActiveSystems)
        {
            //TODO: need to sort systems by priority
            if (s is IPostUpdateSystem ps)
            {
                ps.PostUpdate(DeltaTime);
            }
        }
        
        if (drawDebugOverlay)
        { 
            DebugOverlay.Update(DeltaTime);
        }
        
        // var text = "MYSTERY DUNGEON HAND";
        // var scale = new Vector2(1, 1);
        //
        // var font = fontSystem.GetFont(48*App.dpi_scale());
        // var size = font.MeasureString(text, scale, characterSpacing:10.1f);
        // var normalized_font_pivot = new Vector2(size.X / 2.0f, size.Y / 2.0f);
        // font.DrawText(fontRenderer, text, new Vector2(Engine.Width/2f, Engine.Height/2f), FSColor.LightCoral);
        // foreach (var tex in fontRenderer._textureManager.CreatedFontTextures)
        // {
        //     tex.PumpDraw();
        // }
        // drawDebugText(DebugFont.C64,$"MYSTERY DUNGEON HAND PROTOTYPE");

        // setting this to load instead of clear allows us to toggle sokol_gp clearing
        state.pass_action.colors.e0.load_action = sg_load_action.SG_LOADACTION_LOAD;
        fixed (sg_pass_action* pass = &state.pass_action)
        {
            Gfx.begin_default_pass(pass, Width, Height);
            // Dispatch all draw commands to Sokol GFX.
            GP.flush();
            // Finish a draw command queue, clearing it.
            GP.end();
            DebugText.draw();
            ImGUI.render();
            Gfx.end_pass();
            Gfx.commit();
        }
        
        foreach (var s in ActiveSystems)
        {
            if (s is ICleanupSystem cs)
            {
                cs.Cleanup();
            }
        }
        
        DestructionSystem.DestroyObjects();

        if (hasScenesStagedForUnmounting)
        {
            foreach (var s in scenesStagedForUnmounting)
            {
                UnmountScene(s);
            }
            scenesStagedForUnmounting.Clear();
        }
    }

    static void UnmountScene((Scene scene, Action callback) s)
    {
        SceneEntityMap.Remove(s.scene.ID);
        MountedScenes.Remove(s.scene.ID);
        s.scene.MountStatus = SceneMountStatus.Unmounted;
        s.callback?.Invoke();
    }

    public enum DebugFont
    {
        KC853,
        KC854,
        Z1013,
        AMSTRAD,
        C64,
        ORIC
    }
    static void drawDebugText(DebugFont f, string debugText)
    {
        DebugText.canvas(Width*0.5f, Height*0.5f);
        DebugText.origin(0.5f, 2.0f);
        DebugText.home();
        printFont(debugText);

        void printFont(string t)
        {
            DebugText.font((int)f);
            DebugText.color3b(255, 255, 0);
            unsafe
            {
                var text = System.Text.Encoding.UTF8.GetBytes(t);
                fixed (byte* ptr = text)
                {
                    DebugText.puts((sbyte*)ptr);
                }
            }
            DebugText.crlf();
        }
    }

    public static void DrawTexturedRect(Position p, SpriteRenderer r)
    {
        GP.set_color(1.0f, 1.0f, 1.0f, 1.0f);
        GP.set_blend_mode(sgp_blend_mode.SGP_BLENDMODE_BLEND);
        GP.set_image(0,r.Texture.Data);
        GP.push_transform();
        GP.translate(p.X - r.PivotX,p.Y - r.PivotY);
        GP.rotate_at(p.Rotation, r.PivotX, r.PivotY);
        GP.scale_at(p.ScaleX, p.ScaleY, r.PivotX, r.PivotY);
        GP.draw_textured_rect(0,
            //this is the rect to draw the source "to", basically can scale the rect (maybe do wrapping?)
            //we assume this is the width and height of the frame itself
            r.SizeRect.InternalRect,
            //this is the rect index into the texture itself
            r.Rect.InternalRect);
        GP.pop_transform();
        // GP.draw_filled_rect(x,y,img.internalData.width,img.internalData.height);
        GP.reset_image(0);
    }
    
    public static void DrawText(Position p, TextRenderer r, sg_image i,sgp_rect src)
    {
        //NOTE: this doesn't work!
        GP.set_color(1.0f, 1.0f, 1.0f, 1.0f);
        GP.set_blend_mode(sgp_blend_mode.SGP_BLENDMODE_BLEND);
        GP.set_image(0,i);
        GP.push_transform();
        GP.translate(p.X - r.PivotX,p.Y - r.PivotY);
        GP.rotate_at(p.Rotation, r.PivotX, r.PivotY);
        GP.scale_at(p.ScaleX, p.ScaleY, r.PivotX, r.PivotY);
        GP.draw_textured_rect(0,
            //this is the rect to draw the source "to", basically can scale the rect (maybe do wrapping?)
            //we assume this is the width and height of the frame itself
            src,
            //this is the rect index into the texture itself
            src);
        GP.pop_transform();
        // GP.draw_filled_rect(x,y,img.internalData.width,img.internalData.height);
        GP.reset_image(0);
    }
    
    public static void DrawShape(Position p, ShapeRenderer r)
    {
        //argb
        //rgba
        GP.set_color(r.Color.internal_color.r, r.Color.internal_color.g, r.Color.internal_color.b, r.Color.internal_color.a);
        GP.set_blend_mode(sgp_blend_mode.SGP_BLENDMODE_NONE);
        GP.push_transform();
        GP.translate(p.X - r.PivotX,p.Y - r.PivotY);
        GP.rotate_at(p.Rotation, r.PivotX, r.PivotY);
        GP.scale_at(p.ScaleX, p.ScaleY, r.PivotX, r.PivotY);
        GP.draw_filled_rect(0,0,r.Width,r.Height);
        GP.pop_transform();
        GP.reset_color();
    }
    
    public static void DrawParticles(Position p, ParticleEmitterComponent c, List<int> activeIndicies)
    {
        GP.set_blend_mode(sgp_blend_mode.SGP_BLENDMODE_BLEND);
        switch (c.Config.ParticleConfig.ParticleType)
        {
            case ParticleEmitterComponent.ParticleConfig.ParticlePrimitiveType.Rectangle:
                foreach (var i in activeIndicies)
                {
                    GP.push_transform();
                    GP.set_color(c.Particles[i].Color.internal_color.r, c.Particles[i].Color.internal_color.g, c.Particles[i].Color.internal_color.b, c.Particles[i].Color.internal_color.a);
                    // GP.sgp_translate(p.x, p.y); makes all particles move as if emission point was p.x,p.y
                    GP.translate(c.Particles[i].Config.EmissionPoint.X + c.Particles[i].X,c.Particles[i].Config.EmissionPoint.Y + c.Particles[i].Y);
                    GP.rotate_at(c.Particles[i].Rotation, c.Particles[i].Width / 2f, c.Particles[i].Height / 2f);
                    // GP.scale_at(scaleX, scaleY, f.width / 2f, f.height / 2f); we dont scale, just use width/height
                    GP.draw_filled_rect(0,0,c.Particles[i].Width,c.Particles[i].Height);
                    GP.pop_transform();
                }
                break;
            case ParticleEmitterComponent.ParticleConfig.ParticlePrimitiveType.Line:
                foreach (var i in activeIndicies)
                {
                    GP.push_transform();
                    GP.set_color(c.Particles[i].Color.internal_color.r, c.Particles[i].Color.internal_color.g, c.Particles[i].Color.internal_color.b, c.Particles[i].Color.internal_color.a);
                    // GP.sgp_translate(p.x, p.y); makes all particles move as if emission point was p.x,p.y
                    GP.translate(c.Particles[i].Config.EmissionPoint.X + c.Particles[i].X,c.Particles[i].Config.EmissionPoint.Y + c.Particles[i].Y);
                    GP.rotate_at(c.Particles[i].Rotation, c.Particles[i].Width / 2f, c.Particles[i].Height / 2f);
                    // GP.scale_at(scaleX, scaleY, f.width / 2f, f.height / 2f); we dont scale, just use width/height
                    GP.draw_line(0,0,c.Particles[i].Width,c.Particles[i].Height);
                    GP.pop_transform();
                }
                break;
            case ParticleEmitterComponent.ParticleConfig.ParticlePrimitiveType.Triangle:
                foreach (var i in activeIndicies)
                {
                    GP.push_transform();
                    GP.set_color(c.Particles[i].Color.internal_color.r, c.Particles[i].Color.internal_color.g, c.Particles[i].Color.internal_color.b, c.Particles[i].Color.internal_color.a);
                    // GP.sgp_translate(p.x, p.y); makes all particles move as if emission point was p.x,p.y
                    GP.translate(c.Particles[i].Config.EmissionPoint.X + c.Particles[i].X,c.Particles[i].Config.EmissionPoint.Y + c.Particles[i].Y);
                    GP.rotate_at(c.Particles[i].Rotation, c.Particles[i].Width / 2f, c.Particles[i].Height / 2f);
                    // GP.scale_at(scaleX, scaleY, f.width / 2f, f.height / 2f); we dont scale, just use width/height
                    GP.draw_filled_triangle(0,0,c.Particles[i].Width,0,c.Particles[i].Width / 2f,c.Particles[i].Height);
                    GP.pop_transform();
                }
                break;
            case ParticleEmitterComponent.ParticleConfig.ParticlePrimitiveType.LineStrip:
                var pts = new Zinc.NativeInterop.Utils.NativeArray<sgp_vec2>(activeIndicies.Count);
                int first = activeIndicies[0];
                int ct = 0;
                foreach (var i in activeIndicies)
                {
                    pts[ct] = new sgp_vec2() { x = c.Particles[i].X, y = c.Particles[i].Y };
                    ct++;
                }
                GP.push_transform();
                GP.set_color(c.Particles[first].Color.internal_color.r, c.Particles[first].Color.internal_color.g, c.Particles[first].Color.internal_color.b, c.Particles[first].Color.internal_color.a);

                // GP.sgp_translate(p.x, p.y);
                GP.translate(c.Particles[first].Config.EmissionPoint.X + c.Particles[first].X,c.Particles[first].Config.EmissionPoint.Y + c.Particles[first].Y);
                GP.rotate_at(c.Particles[first].Rotation, c.Particles[first].Width / 2f, c.Particles[first].Height / 2f);
                // GP.sgp_scale_at(scaleX, scaleY, f.width / 2f, f.height / 2f); we dont scale, just use width/height
                unsafe
                {
                    GP.draw_lines_strip(pts.Ptr,(uint)activeIndicies.Count);
                }
                GP.pop_transform();
                break;
            // case ParticleEmitterComponent.ParticleConfig.ParticlePrimitiveType.TriangleStrip:
            //     var strip_pts = new Utils.NativeArray<sgp_vec2>(activeIndicies.Count);
            //     int first_strip_pt = activeIndicies[0];
            //     int pt_ct = 0;
            //     foreach (var i in activeIndicies)
            //     {
            //         strip_pts[pt_ct] = new sgp_vec2() { x = c.Particles[i].X, y = c.Particles[i].Y };
            //         pt_ct++;
            //     }
            //     GP.sgp_push_transform();
            //     GP.sgp_set_color(c.Particles[first_strip_pt].Color.internal_color.g, c.Particles[first_strip_pt].Color.internal_color.b, c.Particles[first_strip_pt].Color.internal_color.a, c.Particles[0].Color.internal_color.r);
            //     // GP.sgp_translate(p.x, p.y);
            //     GP.sgp_translate(c.Particles[first_strip_pt].Config.EmissionPoint.X + c.Particles[first_strip_pt].X,c.Particles[first_strip_pt].Config.EmissionPoint.Y + c.Particles[first_strip_pt].Y);
            //     GP.sgp_rotate_at(c.Particles[first_strip_pt].Rotation, c.Particles[first_strip_pt].Width / 2f, c.Particles[first_strip_pt].Height / 2f);
            //     // GP.sgp_scale_at(scaleX, scaleY, f.width / 2f, f.height / 2f); we dont scale, just use width/height
            //     unsafe
            //     {
            //         GP.sgp_draw_filled_triangles_strip(strip_pts.Ptr,(uint)activeIndicies.Count);
            //     }
            //     GP.sgp_pop_transform();
            //     break;
            
        }
        GP.reset_color();
    }

    public enum LogLevel
    {
        PANIC,
        ERROR,
        WARN,
        INFO
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void Sokol_Logger(sbyte* tag, uint log_level, uint log_item, sbyte* message, uint line_nr, sbyte* filename, void* user_data)
    {
        var TagStr = new string(tag); //Marshal.PtrToStringAnsi((IntPtr)tag);
        var MesageStr = new string(message);
        var FilenameStr = new string(filename);
        Console.WriteLine($"[{(LogLevel)log_level}][{TagStr}] {(sg_log_item)log_item} {FilenameStr} Line={line_nr}");
        Console.WriteLine($"    {MesageStr}\n");
        // System.Diagnostics.Debug.WriteLine($"Tag={TagStr} Level={log_level} Item={(sg_log_item)log_item} Message={MesageStr} Line={line_nr} FileName={FilenameStr}\n");
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void Cleanup()
    {
        Events.SceneUnmounted -= OnSceneUnmounted;
        fixed (sg_imgui_t* ctx = &gfx_dbgui)
        {
            GfxDebugGUI.discard(ctx);
        }
        ImGUI.shutdown();
        // Gfx.destroy_image(image);
        GP.shutdown();
        // GL.shutdown();
        Gfx.shutdown();
    }
    
    // static unsafe sg_shader makeShaderFromSource()
    // {
    //     var desc = default(sg_shader_desc);
    //     byte[] fc = System.Text.Encoding.UTF8.GetBytes("frag_color");
    //     byte[] uv = System.Text.Encoding.UTF8.GetBytes("uv");
    //     byte[] fs_source = File.ReadAllBytes("shaders/compiled/_loadpng_hlsl4_fs.hlsl");
    //     byte[] vs_source = File.ReadAllBytes("shaders/compiled/_loadpng_hlsl4_vs.hlsl");
    //     fixed (byte* fc_ptr = fc, uv_ptr = uv, fs_source_ptr = fs_source, vs_source_ptr = vs_source)
    //     {
    //         desc.attrs.e0.sem_name = (sbyte*)fc_ptr;
    //         desc.attrs.e1.sem_name = (sbyte*)uv_ptr;
    //         desc.vs.source = (sbyte*)vs_source_ptr;
    //         desc.fs.source = (sbyte*)fs_source_ptr;
    //     }
    // }
    

    public static bool LoadImage(string path, out int width, out int height, out sg_image img)
    {
        var fileBytes = File.ReadAllBytes(path);
        return LoadImage(fileBytes, out width, out height, out img);
    }

    public static bool LoadImage(byte[] bytes, out int width, out int height, out sg_image img)
    {
        img = default;
        height = 0;
        width = 0;
        unsafe
        {
            fixed (byte* imgptr = bytes)
            {
                int imgx, imgy, channels;
                var ok = STB.stbi_info_from_memory(imgptr, bytes.Length, &imgx, &imgy, &channels);
                if (ok == 0)
                {
                    return false;
                }
                // STB.stbi_set_flip_vertically_on_load(1);
                var stbimg = STB.stbi_load_from_memory(imgptr, bytes.Length, &imgx,&imgy, &channels, 4);
                sg_image_desc stb_img_desc = default;
                stb_img_desc.width = imgx;
                stb_img_desc.height = imgy;
                stb_img_desc.pixel_format = sg_pixel_format.SG_PIXELFORMAT_RGBA8;
                
                stb_img_desc.data.subimage.e0_0.ptr = stbimg;
                stb_img_desc.data.subimage.e0_0.size = (nuint)(imgx * imgy * 4);

                img = Gfx.make_image(&stb_img_desc);
                STB.stbi_image_free(stbimg);
                width = imgx;
                height = imgy;
            }

            return true;
        }
    }
}