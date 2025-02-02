using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zinc.Internal.STB;
using Arch.Core;
using Zinc.Core;
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
        new GridSystem(),
        new SceneUpdateSystem(),
        new TemporaryObjectSystem(),
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
    public static bool TryGetEntity(int id, out Entity e)
    {
        return EntityLookup.TryGetValue(id, out e);
    }
    public static Entity GetEntity(int id)
    {
        return EntityLookup[id];
    }
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
    
    
    internal struct FontState
    {
        public unsafe void* FONSContext;
    }
    internal static FontState font_state;
    public static bool Clear = true;
    public static bool ShowMenu = true;

    public static core_state state = default;
    public static sgimgui_t gfx_dbgui = default;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void Initialize()
    {
        Console.WriteLine("Initializing Zinc");
        //sokol init
        sg_desc desc = default;
        desc.environment = Glue.sglue_environment();
        //call our own logger
        desc.logger.func = &Sokol_Logger;
        //call native logger
        // desc.logger.func = (delegate* unmanaged[Cdecl]<sbyte*, uint, uint, sbyte*, uint, sbyte*, void*, void>)NativeLibrary.GetExport(NativeLibrary.Load("libs/sokol"), "slog_func");
        Gfx.setup(&desc);
        Console.WriteLine("gfx setup");

        sgl_desc_t gl_desc = default;
        GL.setup(&gl_desc);

        simgui_desc_t imgui_desc = default;
        imgui_desc.logger.func = &Sokol_Logger;
        ImGUI.setup(&imgui_desc);
        
        sgimgui_desc_t sg_imgui_desc = default;
        gfx_dbgui.buffer_window.open = 1;
        gfx_dbgui.image_window.open = 1;
        gfx_dbgui.sampler_window.open = 1;
        gfx_dbgui.shader_window.open = 1;
        gfx_dbgui.pipeline_window.open = 1;
        gfx_dbgui.attachments_window.open = 1;
        gfx_dbgui.frame_stats_window.open = 1;
        gfx_dbgui.capture_window.open = 1;
        gfx_dbgui.caps_window.open = 1;
        fixed (sgimgui_t* ctx = &gfx_dbgui)
        {
            GfxDebugGUI.sgimgui_init(ctx,&sg_imgui_desc);
        }

        sgp_desc gp_desc = default;
        gp_desc.max_vertices = 1000000;
        GP.setup(&gp_desc);
        Console.WriteLine("gp setup");

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
        Console.WriteLine("checkerboard setup");
        
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
        Console.WriteLine("systems setup");
        
        Console.WriteLine("creating physics world");
        PhysicsWorld = new ();
        Console.WriteLine("creating ecs world");
        ECSWorld = World.Create();
        Console.WriteLine("creating global scene");
        GlobalScene = new(){Name = "Global Scene"};
        Console.WriteLine("assigned global scene");

        
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
        
        DPIScale = App.dpi_scale();
        var atlasDim = round_pow2(512.0f * DPIScale);
        sfons_desc_t font_desc = default;
        font_desc.width = atlasDim;
        font_desc.height = atlasDim;
        font_state.FONSContext = Fontstash.create(&font_desc);
        Console.WriteLine("fontstash setup");

        
        GlobalScene.Mount(-1);
        GlobalScene.Load(() => {GlobalScene.Start();});
        Console.WriteLine("global scene mounted");

        Cursor = new() { Name = "Cursor" };

        Events.SceneUnmounted += OnSceneUnmounted;
        Palettes.SetActivePalette(Palettes.ENDESGA);
        Console.WriteLine("invoking setup");
        Setup?.Invoke();

        //get closest power of two
        int round_pow2(float v) {
            uint vi = ((uint) v) - 1;
            for (uint i = 0; i < 5; i++) {
                vi |= (vi >> (1<<(int)i));
            }
            return (int) (vi + 1);
        }
    }


    public static int Width;
    public static int Height;
    public static float DPIScale;

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

        Fontstash.fonsClearState(font_state.FONSContext);

        simgui_frame_desc_t imgui_frame = default;
        imgui_frame.width = Width;
        imgui_frame.height = Height;
        imgui_frame.delta_time = DeltaTime;
        imgui_frame.dpi_scale = DPIScale;
        ImGUI.new_frame(&imgui_frame);

        if(ShowMenu)
        {
            Core.ImGUI.BeginMainMenuBar();
            if (Core.ImGUI.BeginMenu("Zinc"))
            {
                Core.ImGUI.Checkbox("Show Stats", ref showStats);
                Core.ImGUI.Checkbox("Show IMGUI Demo", ref showIMGUIDemo);
                Core.ImGUI.Checkbox("Draw Debug Overlay", ref drawDebugOverlay);
                Core.ImGUI.Checkbox("Draw Debug Colliders", ref drawDebugColliders);
                
                if (Core.ImGUI.BeginMenu("Sokol"))
                {
                    Core.ImGUI.Checkbox("Capabilities", ref gfx_dbgui.caps_window.open);
                    Core.ImGUI.Checkbox("Frame Stats", ref gfx_dbgui.frame_stats_window.open);
                    Core.ImGUI.Checkbox("Buffers", ref gfx_dbgui.buffer_window.open);
                    Core.ImGUI.Checkbox("Images", ref gfx_dbgui.image_window.open);
                    Core.ImGUI.Checkbox("Samplers", ref gfx_dbgui.sampler_window.open);
                    Core.ImGUI.Checkbox("Shaders", ref gfx_dbgui.shader_window.open);
                    Core.ImGUI.Checkbox("Pipelines", ref gfx_dbgui.pipeline_window.open);
                    Core.ImGUI.Checkbox("Attachments", ref gfx_dbgui.attachments_window.open);
                    Core.ImGUI.Checkbox("Capture", ref gfx_dbgui.capture_window.open);
                    Core.ImGUI.EndMenu();
                }

                Core.ImGUI.EndMenu();

            }
            
            Core.ImGUI.EndMainMenuBar();
        }
        
        if (showStats)
        {
            int ec = 0;
            foreach (var l in SceneEntityMap.Values)
            {
                ec += l.Count;
            }
            Core.ImGUI.ShowStats($"{t}ms",$"Entities: {ec}",$"{InputSystem.MouseX},{InputSystem.MouseY}");
        }

        fixed (bool* dem_ptr = &showIMGUIDemo)
        {
            if (showIMGUIDemo)
            {
                ImGUI.igShowDemoWindow(dem_ptr);
            }
        }

        fixed (sgimgui_t* ctx = &gfx_dbgui)
        {
            GfxDebugGUI.sgimgui_draw(ctx);
        }
        
        float ratio = Width/(float)Height;

        GL.defaults();
        GL.matrix_mode_projection();
        GL.ortho(0.0f, Width,Height, 0.0f, -100.0f, +100.0f);

        // Begin recording draw commands for a frame buffer of size (width, height).
        GP.begin(Width, Height);
        // Set frame buffer drawing region to (0,0,width,height).
        GP.viewport(0, 0, Width, Height);
        // Set drawing coordinate space to (left=-ratio, right=ratio, top=1, bottom=-1).
        // GP.sgp_project(-ratio, ratio, 1.0f, -1.0f);

        // Clear the frame buffer.
        if (Clear)
        {
            GP.set_color(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
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
        fixed (sg_pass_action* pass_ptr = &state.pass_action)
        {
            sg_pass pass = default;
            pass.action = *pass_ptr;
            pass.swapchain = Glue.sglue_swapchain();
            // Gfx.begin_default_pass(pass, Width, Height);
            Gfx.begin_pass(&pass);
            // draw with sokol gl (font)
            // Dispatch all draw commands to Sokol GFX.
            GP.flush();
            // Finish a draw command queue, clearing it.
            GP.end();
            DebugText.draw();
            GL.draw();
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

    public static void DrawTexturedRect(Anchor a, SpriteRenderer r)
    {
        GP.set_color(r.Color.R, r.Color.G, r.Color.B, r.Color.A);
        GP.set_blend_mode(sgp_blend_mode.SGP_BLENDMODE_BLEND);
        GP.set_image(0,r.Texture.Data);
        var world = a.GetWorldTransform();
        world.transform.Decompose(out var world_pos, out var world_rotation, out var scale);
        GP.push_transform();
        float pivotX = r.Pivot.X * r.Width;
        float pivotY = r.Pivot.Y * r.Height;
        GP.translate(world_pos.X, world_pos.Y);
        GP.translate(-pivotX, -pivotY);
        GP.rotate_at(world_rotation,pivotX,pivotY);
        GP.scale_at(world.scale.X, world.scale.Y,pivotX,pivotY);
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
    
    public static void DrawText(Anchor a, TextRenderer r, int fontID)
    {
        var n = System.Text.Encoding.UTF8.GetBytes(r.text);
        unsafe
        {
            Fontstash.fonsSetSize(font_state.FONSContext, r.size*DPIScale);
            Fontstash.fonsSetFont(font_state.FONSContext, fontID);
            // Fontstash.fonsVertMetrics(font_state.FONSContext, null, null, &lh);
            // Fontstash.fonsSetColor(font_state.FONSContext, white);
            uint white = Fontstash.rgba(255, 255, 255, 255);
            Fontstash.fonsSetColor(font_state.FONSContext, white);
            Fontstash.fonsSetAlign(font_state.FONSContext, (int)(FONSalign.FONS_ALIGN_BASELINE | FONSalign.FONS_ALIGN_MIDDLE));
            Fontstash.fonsSetSpacing(font_state.FONSContext, r.spacing*DPIScale);
            Fontstash.fonsSetBlur(font_state.FONSContext, r.blur);
            
            fixed (byte* n_p = n)
            {
                var width = Fontstash.fonsTextBounds(font_state.FONSContext, 0, 0, (sbyte*)n_p, null, null);
                // var width = Fontstash.fonsLineBounds(font_state.FONSContext, 0, 0, (sbyte*)n_p, null, null);
                GL.push_matrix();
                GL.translate(a.X,a.Y,0);
                float pivotX = r.Pivot.X * width;
                // float pivotY = r.Pivot.Y * r.Height;
                GL.translate(-pivotX, 0,0);
                GL.translate(pivotX, 0,0);
                GL.rotate(a.Rotation,0,0,1);
                GL.scale(a.ScaleX, a.ScaleY,1);
                GL.translate(-pivotX, 0,0);
                var dx = Fontstash.fonsDrawText(font_state.FONSContext, 0, 0, (sbyte*)n_p, null);
                GL.pop_matrix();
                // Console.WriteLine($"{dx} {b}");
            }

            // GL.draw();

        }
    }
    
    public static void DrawShape(Anchor a, ShapeRenderer r)
    {
        //argb
        //rgba
        GP.set_color(r.Color.R, r.Color.G, r.Color.B, r.Color.A);
        GP.set_blend_mode(sgp_blend_mode.SGP_BLENDMODE_NONE);
        var world = a.GetWorldTransform();
        world.transform.Decompose(out var world_pos, out var world_rotation, out var scale);
        GP.push_transform();
        float pivotX = r.Pivot.X * r.Width;
        float pivotY = r.Pivot.Y * r.Height;
        GP.translate(-pivotX, -pivotY);
        GP.translate(world_pos.X, world_pos.Y);
        GP.rotate_at(world_rotation,pivotX,pivotY);
        GP.scale_at(world.scale.X, world.scale.Y,pivotX,pivotY);
        GP.draw_filled_rect(0, 0, r.Width, r.Height);
        GP.pop_transform();
        GP.reset_color();
    }

    public static void DrawParticles(Anchor a, ParticleEmitterComponent c, double dt)
    {
        GP.set_blend_mode(sgp_blend_mode.SGP_BLENDMODE_BLEND);
        
        var world = a.GetWorldTransform();
        world.transform.Decompose(out var world_pos, out var world_rotation, out var scale);

        Vector2 particle_pos = Vector2.Zero;
        float particle_width = 0f;
        float particle_height = 0f;
        float particle_rot = 0f;
        Color particle_color = new Color(0,0,0,1f);
        for (int i = 0; i < c.Count; i++)
        {
            if(c.Active[i])
            {
                c.Resolve(i, dt, ref particle_pos, ref particle_width, ref particle_height, ref particle_rot, ref particle_color);
                GP.push_transform();
                GP.set_color(particle_color.R,particle_color.G, particle_color.B, particle_color.A);
                // GP.translate(world_pos.X + particle_pos.X, world_pos.Y + particle_pos.Y);
                GP.translate(c.SpawnLocation[i].X + particle_pos.X, c.SpawnLocation[i].Y + particle_pos.Y);
                GP.rotate_at(world_rotation + particle_rot, particle_width / 2f, particle_height / 2f);
                switch (c.Config.Type)
                {
                    case ParticleEmitterConfig.ParticlePrimitiveType.Rectangle:
                        GP.draw_filled_rect(0,0,particle_width,particle_height);
                        break;
                    case ParticleEmitterConfig.ParticlePrimitiveType.Line:
                        GP.draw_line(0,0,particle_width,particle_height);
                        break;
                    case ParticleEmitterConfig.ParticlePrimitiveType.Triangle:
                        GP.draw_filled_triangle(0,0,particle_width,0,particle_width / 2f,particle_height);
                        break;
                }
                GP.pop_transform();
                GP.reset_color();
            }
        }
            // case ParticleEmitterComponent.ParticleConfig.ParticlePrimitiveType.LineStrip:
            //     var pts = new Zinc.NativeInterop.Utils.NativeArray<sgp_vec2>(activeIndicies.Count);
            //     int first = activeIndicies[0];
            //     int ct = 0;
            //     foreach (var i in activeIndicies)
            //     {
            //         pts[ct] = new sgp_vec2() { x = c.Particles[i].X, y = c.Particles[i].Y };
            //         ct++;
            //     }
            //     GP.push_transform();
            //     GP.set_color(c.Particles[first].Color.internal_color.r, c.Particles[first].Color.internal_color.g, c.Particles[first].Color.internal_color.b, c.Particles[first].Color.internal_color.a);

            //     // GP.sgp_translate(p.x, p.y);
            //     GP.translate(c.Particles[first].Config.EmissionPoint.X + c.Particles[first].X,c.Particles[first].Config.EmissionPoint.Y + c.Particles[first].Y);
            //     GP.rotate_at(c.Particles[first].Rotation, c.Particles[first].Width / 2f, c.Particles[first].Height / 2f);
            //     // GP.sgp_scale_at(scaleX, scaleY, f.width / 2f, f.height / 2f); we dont scale, just use width/height
            //     unsafe
            //     {
            //         GP.draw_lines_strip(pts.Ptr,(uint)activeIndicies.Count);
            //     }
            //     GP.pop_transform();
            //     break;
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
        fixed (sgimgui_t* ctx = &gfx_dbgui)
        {
            GfxDebugGUI.sgimgui_discard(ctx);
        }
        ImGUI.shutdown();
        // Gfx.destroy_image(image);
        unsafe
        {
            Fontstash.destroy(font_state.FONSContext);
        }
        GP.shutdown();
        GL.shutdown();
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