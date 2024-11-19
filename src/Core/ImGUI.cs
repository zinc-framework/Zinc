using Zinc.Internal.Sokol;
using System.Numerics;

namespace Zinc.Core;

public static class ImGUI
{
    [Flags]
    public enum WindowFlags
    {
        None = ImGuiWindowFlags_.ImGuiWindowFlags_None,
        NoTitleBar = ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar,
        NoResize = ImGuiWindowFlags_.ImGuiWindowFlags_NoResize,
        NoMove = ImGuiWindowFlags_.ImGuiWindowFlags_NoMove,
        NoScrollbar = ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar,
        NoScrollWithMouse = ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse,
        NoCollapse = ImGuiWindowFlags_.ImGuiWindowFlags_NoCollapse,
        AlwaysAutoResize = ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize,
        NoBackground = ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground,
        NoSavedSettings = ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings,
        NoMouseInputs = ImGuiWindowFlags_.ImGuiWindowFlags_NoMouseInputs,
        MenuBar = ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar,
        HorizontalScrollbar = ImGuiWindowFlags_.ImGuiWindowFlags_HorizontalScrollbar,
        NoFocusOnAppearing = ImGuiWindowFlags_.ImGuiWindowFlags_NoFocusOnAppearing,
        NoBringToFrontOnFocus = ImGuiWindowFlags_.ImGuiWindowFlags_NoBringToFrontOnFocus,
        AlwaysVerticalScrollbar = ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysVerticalScrollbar,
        AlwaysHorizontalScrollbar = ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysHorizontalScrollbar,
        NoNavInputs = ImGuiWindowFlags_.ImGuiWindowFlags_NoNavInputs,
        NoNavFocus = ImGuiWindowFlags_.ImGuiWindowFlags_NoNavFocus,
        UnsavedDocument = ImGuiWindowFlags_.ImGuiWindowFlags_UnsavedDocument,
        NoDocking = ImGuiWindowFlags_.ImGuiWindowFlags_NoDocking,
        NoNav = ImGuiWindowFlags_.ImGuiWindowFlags_NoNav,
        NoDecoration = ImGuiWindowFlags_.ImGuiWindowFlags_NoDecoration,
        NoInputs = ImGuiWindowFlags_.ImGuiWindowFlags_NoInputs,
        NavFlattened = ImGuiWindowFlags_.ImGuiWindowFlags_NavFlattened,
        ChildWindow = ImGuiWindowFlags_.ImGuiWindowFlags_ChildWindow,
        Tooltip = ImGuiWindowFlags_.ImGuiWindowFlags_Tooltip,
        Popup = ImGuiWindowFlags_.ImGuiWindowFlags_Popup,
        Modal = ImGuiWindowFlags_.ImGuiWindowFlags_Modal,
        ChildMenu = ImGuiWindowFlags_.ImGuiWindowFlags_ChildMenu,
        DockNodeHost = ImGuiWindowFlags_.ImGuiWindowFlags_DockNodeHost,
    }

    [Flags]
    public enum SliderFlags
    {
        None = ImGuiSliderFlags_.ImGuiSliderFlags_None,
        AlwaysClamp = ImGuiSliderFlags_.ImGuiSliderFlags_AlwaysClamp,
        Logarithmic = ImGuiSliderFlags_.ImGuiSliderFlags_Logarithmic,
        NoRoundToFormat = ImGuiSliderFlags_.ImGuiSliderFlags_NoRoundToFormat,
        NoInput = ImGuiSliderFlags_.ImGuiSliderFlags_NoInput,
        InvalidMask_ = ImGuiSliderFlags_.ImGuiSliderFlags_InvalidMask_
    }

    [Flags]
    public enum InputFlags
    {
        None = ImGuiInputFlags_.ImGuiInputFlags_None,
        Repeat = ImGuiInputFlags_.ImGuiInputFlags_Repeat,
        RepeatRateDefault = ImGuiInputFlags_.ImGuiInputFlags_RepeatRateDefault,
        RepeatRateNavMove = ImGuiInputFlags_.ImGuiInputFlags_RepeatRateNavMove,
        RepeatRateNavTweak = ImGuiInputFlags_.ImGuiInputFlags_RepeatRateNavTweak,
        RepeatRateMask = ImGuiInputFlags_.ImGuiInputFlags_RepeatRateMask_,
        CondHovered = ImGuiInputFlags_.ImGuiInputFlags_CondHovered,
        CondActive = ImGuiInputFlags_.ImGuiInputFlags_CondActive,
        CondDefault = ImGuiInputFlags_.ImGuiInputFlags_CondDefault_,
        CondMask = ImGuiInputFlags_.ImGuiInputFlags_CondMask_,
        LockThisFrame = ImGuiInputFlags_.ImGuiInputFlags_LockThisFrame,
        LockUntilRelease = ImGuiInputFlags_.ImGuiInputFlags_LockUntilRelease,
        RouteFocused = ImGuiInputFlags_.ImGuiInputFlags_RouteFocused,
        RouteGlobalLow = ImGuiInputFlags_.ImGuiInputFlags_RouteGlobalLow,
        RouteGlobal = ImGuiInputFlags_.ImGuiInputFlags_RouteGlobal,
        RouteGlobalHigh = ImGuiInputFlags_.ImGuiInputFlags_RouteGlobalHigh,
        RouteMask = ImGuiInputFlags_.ImGuiInputFlags_RouteMask_,
        RouteAlways = ImGuiInputFlags_.ImGuiInputFlags_RouteAlways,
        RouteUnlessBgFocused = ImGuiInputFlags_.ImGuiInputFlags_RouteUnlessBgFocused,
        RouteExtraMask = ImGuiInputFlags_.ImGuiInputFlags_RouteExtraMask_,
        SupportedByIsKeyPressed = ImGuiInputFlags_.ImGuiInputFlags_SupportedByIsKeyPressed,
        SupportedByShortcut = ImGuiInputFlags_.ImGuiInputFlags_SupportedByShortcut,
        SupportedBySetKeyOwner = ImGuiInputFlags_.ImGuiInputFlags_SupportedBySetKeyOwner,
        SupportedBySetItemKeyOwner = ImGuiInputFlags_.ImGuiInputFlags_SupportedBySetItemKeyOwner,
    }

    [Flags]
    public enum Condition
    {
        None = ImGuiCond_.ImGuiCond_None,
        Always = ImGuiCond_.ImGuiCond_Always,
        Once = ImGuiCond_.ImGuiCond_Once,
        FirstUseEver = ImGuiCond_.ImGuiCond_FirstUseEver,
        Appearing = ImGuiCond_.ImGuiCond_Appearing,
    }

    public static void Menu(string name, Action drawMenu, bool enabled = true)
    {
        if (BeginMenu(name, enabled))
        {
            drawMenu?.Invoke();
            EndMenu();
        }
    }
    public static unsafe bool BeginMenu(string name, bool enabled = true)
    {
        var b = System.Text.Encoding.UTF8.GetBytes(name);
        fixed (byte* ptr = b)
        {
            return Internal.Sokol.ImGUI.igBeginMenu((sbyte*)ptr,(byte)(enabled ? 1 : 0)) > 0;
        }
    }
    public static void EndMenu() => Internal.Sokol.ImGUI.igEndMenu();
    public static void SetNextWindowPosition(Vector2 p,Condition condition = Condition.Always)
    {
        SetNextWindowPosition(p.X,p.Y,condition,0,0);
    }
    public static void SetNextWindowPosition(float x, float y, Condition condition, float pivot_x, float pivot_y)
    {
        Internal.Sokol.ImGUI.igSetNextWindowPos(new(){x = x, y=y}, (int)condition, new ImVec2(){x = pivot_x,y=pivot_y});
    }
    public static void SetNextWindowSize(Vector2 size, Condition condition = Condition.Always) => SetNextWindowSize(size.X,size.Y,condition);
    public static unsafe void SetNextWindowSize(float width, float height, Condition condition = Condition.Always)
    {
        Internal.Sokol.ImGUI.igSetNextWindowSize(new ImVec2(){x =width,y=height},(int)condition);
    }
    
    public static void SetNextWindowBGAlpha(float alpha)
    {
        Internal.Sokol.ImGUI.igSetNextWindowBgAlpha(alpha);
    }
    public static unsafe void DrawQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        var p = Internal.Sokol.ImGUI.igGetWindowDrawList();
        Internal.Sokol.ImGUI.ImDrawList_AddQuadFilled(Internal.Sokol.ImGUI.igGetWindowDrawList(),a.ToImVec2(),b.ToImVec2(),c.ToImVec2(),d.ToImVec2(),Palettes.ONE_BIT_MONITOR_GLOW[1]);
    }
    public static unsafe void DrawCircle(Vector2 center, float radius)
    {
        var p = Internal.Sokol.ImGUI.igGetWindowDrawList();
        Internal.Sokol.ImGUI.ImDrawList_AddCircleFilled(Internal.Sokol.ImGUI.igGetWindowDrawList(),center.ToImVec2(),radius,Palettes.ONE_BIT_MONITOR_GLOW[1],64);
    }
    public static unsafe void DrawQuad(Vector2[] points)
    {
        var p = Internal.Sokol.ImGUI.igGetWindowDrawList();
        Internal.Sokol.ImGUI.ImDrawList_AddQuadFilled(Internal.Sokol.ImGUI.igGetWindowDrawList(),points[0].ToImVec2(),points[1].ToImVec2(),points[2].ToImVec2(),points[3].ToImVec2(),Palettes.ONE_BIT_MONITOR_GLOW[1]);
    }
    
    public static unsafe bool MenuItem(string name, string shortcut = "", bool selected = false, bool enabled = true)
    {
        var n = System.Text.Encoding.UTF8.GetBytes(name);
        var s = System.Text.Encoding.UTF8.GetBytes(shortcut);
        fixed (byte* n_p = n, s_p = s)
        {
            return Internal.Sokol.ImGUI.igMenuItem_Bool((sbyte*)n_p,(sbyte*)s_p,(byte)(selected ? 1 : 0),(byte)(enabled ? 1 : 0)) > 0;
        }
    }

    public static unsafe bool Checkbox(string name, ref bool value)
    {
        var n = System.Text.Encoding.UTF8.GetBytes(name);
        fixed (byte* n_p = n)
        {
            fixed (bool* bool_p = &value)
            {
                return Internal.Sokol.ImGUI.igCheckbox((sbyte*)n_p,bool_p) > 0;
            }
        }
    }
    
    public static unsafe bool Checkbox(string name, ref byte value)
    {
        var n = System.Text.Encoding.UTF8.GetBytes(name);
        bool v = value != 0;
        fixed (byte* n_p = n)
        {
            var r = Internal.Sokol.ImGUI.igCheckbox((sbyte*)n_p,&v) > 0;
            value = (byte)(v ? 1 : 0);
            return r;
        }
    }

    public static unsafe void Text(string text)
    {
        var t = System.Text.Encoding.UTF8.GetBytes(text);
        fixed (byte* t_p = t)
        {
            Internal.Sokol.ImGUI.igText((sbyte*)t_p);
        }
    }

    public static unsafe void TextInput(string label, ref string value, uint maxLength = 256, InputFlags flags = InputFlags.None)
    {
        var t = System.Text.Encoding.UTF8.GetBytes(label);
        var v = System.Text.Encoding.UTF8.GetBytes(string.IsNullOrEmpty(value) ? " " : value);
        fixed (byte* t_p = t, v_p = v)
        {
            Internal.Sokol.ImGUI.igInputText((sbyte*)t_p, (sbyte*)v_p, maxLength, (int)flags, null, null);
            value = StringFromPtr(v_p);
        }

        string StringFromPtr(byte* ptr)
        {
            //https://github.com/ImGuiNET/ImGui.NET/blob/70a87022f775025b90dbe2194e44983c79de0911/src/ImGui.NET/Util.cs#L11
            int characters = 0;
            while (ptr[characters] != 0)
            {
                characters++;
            }

            return System.Text.Encoding.UTF8.GetString(ptr, characters);
        }
    }
    
    public static unsafe void SeperatorText(string text)
    {
        var t = System.Text.Encoding.UTF8.GetBytes(text);
        fixed (byte* t_p = t)
        {
            Internal.Sokol.ImGUI.igSeparatorText((sbyte*)t_p);
        }
    }

    public static unsafe void DragFloat(string label, ref float value, float speed, float min, float max, string format, SliderFlags flags)
    {
        var t = System.Text.Encoding.UTF8.GetBytes(label);
        var f = System.Text.Encoding.UTF8.GetBytes(format);
        float v = value;
        fixed (byte* t_p = t,fmt_p = f)
        {
            Internal.Sokol.ImGUI.igDragFloat((sbyte*)t_p, &v, speed, min, max, (sbyte*)fmt_p, (int)flags);
            value = v;
        }
    }
    public static unsafe bool SliderFloat(string label, ref float value, float min, float max, string format, SliderFlags flags = SliderFlags.None)
    {
        var t = System.Text.Encoding.UTF8.GetBytes(label);
        var f = System.Text.Encoding.UTF8.GetBytes(format);
        float v = value;
        fixed (byte* t_p = t,fmt_p = f)
        {
            var r = Internal.Sokol.ImGUI.igSliderFloat((sbyte*)t_p, &v, min, max, (sbyte*)fmt_p, (int)flags);
            value = v;
            return r != 0;
        }
    }
    
    public static unsafe bool SliderFloat2(string label, ref float value1, ref float value2, float min, float max, string format, SliderFlags flags)
    {
        var t = System.Text.Encoding.UTF8.GetBytes(label);
        var f = System.Text.Encoding.UTF8.GetBytes(format);
        float[] v_arr = [value1,value2];
        fixed (byte* t_p = t,fmt_p = f)
        {
            fixed (float* vptr = v_arr)
            {
                var r = Internal.Sokol.ImGUI.igSliderFloat2((sbyte*)t_p, vptr, min, max, (sbyte*)fmt_p, (int)flags);
                value1 = v_arr[0];
                value2 = v_arr[1];
                return r != 0;
            }

        }
    }
    
    public static unsafe bool SliderInt(string label, ref int value, int min, int max, string format, SliderFlags flags)
    {
        var t = System.Text.Encoding.UTF8.GetBytes(label);
        var f = System.Text.Encoding.UTF8.GetBytes(format);
        int v = value;
        fixed (byte* t_p = t,fmt_p = f)
        {
            var r = Internal.Sokol.ImGUI.igSliderInt((sbyte*)t_p, &v, min, max, (sbyte*)fmt_p, (int)flags);
            value = v;
            return r != 0;
        }
    }

    public static unsafe bool RadioButton(string label, ref int value, int optionIndex)
    {
        var b = System.Text.Encoding.UTF8.GetBytes(label);
        int v = value;
        fixed (byte* ptr = b)
        {
            var r = Internal.Sokol.ImGUI.igRadioButton_IntPtr((sbyte*)ptr,&v,optionIndex);
            value = v;
            return r != 0;
        }
    }

    public static void Button(string label, Vector2 size, Action OnClick) => Button(label, size.ToImVec2(),OnClick);
    public static unsafe void Button(string label, ImVec2 size, Action OnClick)
    {
        var b = System.Text.Encoding.UTF8.GetBytes(label);
        fixed (byte* ptr = b)
        {
            var r = Internal.Sokol.ImGUI.igButton((sbyte*)ptr,size);
            if (r != 0)
            {
                OnClick?.Invoke();
            }
        }
    }

    public static unsafe bool Combo(string label, IEnumerable<string> items, ref int value)
    {
        var b = System.Text.Encoding.UTF8.GetBytes(label);
        var str_items = System.Text.Encoding.UTF8.GetBytes(String.Join("\0",items));
        int v = value;
        fixed (byte* ptr = b, items_ptr = str_items)
        {
            var r = Internal.Sokol.ImGUI.igCombo_Str((sbyte*)ptr,&v,(sbyte*)items_ptr,4);
            value = v;
            return r != 0;
        }
    }

    public static unsafe bool Color(string label, ref Color c)
    {
        var b = System.Text.Encoding.UTF8.GetBytes(label);
        float[] c_arr = [c.R, c.G, c.B, c.A];
        fixed (byte* ptr = b)
        {
            fixed (float* colPtr = c_arr)
            {
                var r = Internal.Sokol.ImGUI.igColorEdit4((sbyte*)ptr, colPtr,0);
                c.R = c_arr[0];
                c.G = c_arr[1];
                c.B = c_arr[2];
                c.A = c_arr[3];
                return r != 0;
            }
        }
    }

    public static void SameLine()
    {
        Internal.Sokol.ImGUI.igSameLine(0,10);
    }
    public static void Seperator()
    {
        Internal.Sokol.ImGUI.igSeparator();
    }
    public static void BeginMainMenuBar() => Internal.Sokol.ImGUI.igBeginMainMenuBar();
    public static void EndMainMenuBar() => Internal.Sokol.ImGUI.igEndMainMenuBar();

    public static void MainMenu(Action drawMenu)
    {
        BeginMainMenuBar();
        drawMenu?.Invoke();
        EndMainMenuBar();;
    }

    public static Vector2 DefaultWindowSize = new Vector2(300,500); 
    public static Vector2 DefaultWindowPosition = new Vector2(100,100); 
    public static void Window(string name, Action drawWindow, Condition condition = Condition.Once)
    {
        Window(name, DefaultWindowPosition, DefaultWindowSize, drawWindow,condition:condition);
    }
    public static void Window(string name, Vector2 position, Vector2 size,  Action drawWindow, WindowFlags flags = WindowFlags.None, Condition condition = Condition.Always)
    {
        SetNextWindowPosition(position,condition);
        SetNextWindowSize(size,condition);
        Begin(name,flags);
        drawWindow?.Invoke();
        End();
    }
    public static unsafe void Begin(string name, WindowFlags flags)
    {
        var n = System.Text.Encoding.UTF8.GetBytes(name);
        fixed (byte* n_p = n)
        {
            var opened = false;
            Internal.Sokol.ImGUI.igBegin((sbyte*)n_p, &opened, (int)flags);
        }
    }

    public static void End()
    {
        Internal.Sokol.ImGUI.igEnd();
    }
    public static unsafe void ShowStats(string frameRate, string entities, string mouse)
    {
        //ported from the demo
        const float padding = 10.0f;
        var window_flags = WindowFlags.NoDecoration | WindowFlags.NoDocking | WindowFlags.AlwaysAutoResize | WindowFlags.NoSavedSettings | WindowFlags.NoFocusOnAppearing | WindowFlags.NoNav;
        var viewport = Internal.Sokol.ImGUI.igGetMainViewport();
        var work_pos = viewport->WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
        var work_size = viewport->WorkSize;
        ImVec2 window_pos = default;
        ImVec2 window_pos_pivot = default;
        window_pos.x = (work_pos.x + padding);
        window_pos.y = (work_pos.y + padding);
        Internal.Sokol.ImGUI.igSetNextWindowPos(window_pos, (int)Condition.Always, window_pos_pivot);
        Internal.Sokol.ImGUI.igSetNextWindowViewport(viewport->ID);
        window_flags |= WindowFlags.NoMove;
        Internal.Sokol.ImGUI.igSetNextWindowBgAlpha(0.35f);
        Begin("Zinc Stats",window_flags);
        Text(frameRate);
        Text(entities);
        Text(mouse);
        Text("Loaded Scenes:");
        foreach (var i in Engine.MountedScenes)
        {
            var scene = Engine.SceneLookup[i.Key];
            Text($"{scene.Name} {scene.Status}");
        }
        End();
    }
}