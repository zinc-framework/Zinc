using Zinc.Internal.Sokol;
using System.Numerics;

namespace Zinc.Core.ImGUI;

public static class ImGUIHelper
{
    public static class Wrappers
    {
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
        public static void SetNextWindowPosition(Vector2 p,ImGuiCond_ condition = ImGuiCond_.ImGuiCond_Always)
        {
            SetNextWindowPosition(p.X,p.Y,condition,0,0);
        }
        public static void SetNextWindowPosition(float x, float y, Internal.Sokol.ImGuiCond_ condition, float pivot_x, float pivot_y)
        {
            Internal.Sokol.ImGUI.igSetNextWindowPos(new(){x = x, y=y}, (int)condition, new ImVec2(){x = pivot_x,y=pivot_y});
        }
        public static void SetNextWindowSize(Vector2 size, ImGuiCond_ condition = ImGuiCond_.ImGuiCond_Always) => SetNextWindowSize(size.X,size.Y,condition);
        public static unsafe void SetNextWindowSize(float width, float height, ImGuiCond_ condition = ImGuiCond_.ImGuiCond_Always)
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
        
        public static unsafe void SeperatorText(string text)
        {
            var t = System.Text.Encoding.UTF8.GetBytes(text);
            fixed (byte* t_p = t)
            {
                Internal.Sokol.ImGUI.igSeparatorText((sbyte*)t_p);
            }
        }

        public static unsafe void DragFloat(string label, ref float value, float speed, float min, float max, string format, ImGuiSliderFlags_ flags)
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
        public static unsafe bool SliderFloat(string label, ref float value, float min, float max, string format, ImGuiSliderFlags_ flags)
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
        
        public static unsafe bool SliderFloat2(string label, ref float value1, ref float value2, float min, float max, string format, ImGuiSliderFlags_ flags)
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
        
        public static unsafe bool SliderInt(string label, ref int value, int min, int max, string format, ImGuiSliderFlags_ flags)
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
        public static void Window(string name, Action drawWindow, ImGuiCond_ condition = ImGuiCond_.ImGuiCond_Once)
        {
            Window(name, DefaultWindowPosition, DefaultWindowSize, drawWindow,condition:condition);
        }
        public static void Window(string name, Vector2 position, Vector2 size,  Action drawWindow, ImGuiWindowFlags_ flags = ImGuiWindowFlags_.ImGuiWindowFlags_None, ImGuiCond_ condition = ImGuiCond_.ImGuiCond_Always)
        {
            SetNextWindowPosition(position,condition);
            SetNextWindowSize(size,condition);
            Begin(name,flags);
            drawWindow?.Invoke();
            End();
        }
        public static unsafe void Begin(string name, ImGuiWindowFlags_ flags)
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
            var window_flags = ImGuiWindowFlags_.ImGuiWindowFlags_NoDecoration | ImGuiWindowFlags_.ImGuiWindowFlags_NoDocking | ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_.ImGuiWindowFlags_NoFocusOnAppearing | ImGuiWindowFlags_.ImGuiWindowFlags_NoNav;
            var viewport = Internal.Sokol.ImGUI.igGetMainViewport();
            var work_pos = viewport->WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
            var work_size = viewport->WorkSize;
            ImVec2 window_pos = default;
            ImVec2 window_pos_pivot = default;
            window_pos.x = (work_pos.x + padding);
            window_pos.y = (work_pos.y + padding);
            Internal.Sokol.ImGUI.igSetNextWindowPos(window_pos, (int)Internal.Sokol.ImGuiCond_.ImGuiCond_Always, window_pos_pivot);
            Internal.Sokol.ImGUI.igSetNextWindowViewport(viewport->ID);
            window_flags |= ImGuiWindowFlags_.ImGuiWindowFlags_NoMove;
            Internal.Sokol.ImGUI.igSetNextWindowBgAlpha(0.35f);
            Begin("Dinghy Stats",window_flags);
            Text(frameRate);
            Text(entities);
            Text(mouse);
            End();
        }
    }
}