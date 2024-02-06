/*
 * Originally adapted from withoutaface/MonoGameImGuiNETexamples which is a port of the C++ Dear IMGUI sample code for the C++ version of Dear ImGui
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using MonoGame.ImGuiNet;
using ImGuiNET;

using Vec2 = System.Numerics.Vector2;
using Vec3 = System.Numerics.Vector3;
using Vec4 = System.Numerics.Vector4;

namespace Monogame.ImGuiNetSamples
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ImGuiRenderer GuiRenderer;

        bool WasResized = false;
        private Model suzanne;

        private Matrix world = Matrix.CreateScale(1.5f, 1.5f, 1.5f) * Matrix.CreateRotationX(-1.5f);
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.1f, 100.0f);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;

            Window.AllowUserResizing = true; // true;
            Window.ClientSizeChanged += delegate { WasResized = true; };
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.Window.Title = "MonoGame & ImGui.NET";

            GuiRenderer = new ImGuiRenderer(this);
            GuiRenderer.RebuildFontAtlas();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            suzanne = Content.Load<Model>("suzanne");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //update logic
            if (WasResized)
            {
                string new_resolution = resolution[select_res];

                int res_width = int.Parse(new_resolution.Split('x')[0]);
                int res_height = int.Parse(new_resolution.Split('x')[1]);

                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width; //1920;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height; //1080;

                graphics.ApplyChanges();

                WasResized = false;
                current_res = select_res;

                debug_log.Add("(" + DateTime.Now.ToShortTimeString() + ") [MonoGame] Changed Resolution to " + resolution[current_res]);
            }

            base.Update(gameTime);
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = false;
                    effect.EnableDefaultLighting();
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Framerate
            float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Depth Buffer
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;
            GraphicsDevice.BlendState = BlendState.Opaque;

            //Draw 3D model
            if (render_model == 1)
            {
                DrawModel(suzanne, world, view, projection);
            }

            base.Draw(gameTime);

            //ImGui Begin
            GuiRenderer.BeginLayout(gameTime);

            #region ImGui
            //Native Demos
            if (show_native_examples)
            {
                DrawImGuiNativeDemos();
            }

            //Style Editor
            if (show_app_style_editor)
            {
                ImGui.Begin("Dear ImGui Style Editor");
                ImGui.ShowStyleEditor();
                ImGui.End();
            }

            //Metrics window
            if (show_app_metrics)
            {
                ImGui.ShowMetricsWindow();
            }

            //About window
            if (show_app_about)
            {
                ImGui.ShowAboutWindow();
            }

            //Overlay
            if (show_app_simple_overlay)
            {
                DrawImGuiOverlay(frameRate);
            }

            //Menu bar
            if (show_app_main_menu_bar)
            {
                DrawImGuiMenuBar();
            }

            //Console
            if (show_app_console)
            {
                DrawImGuiExampleAppConsole();
            }

            //Log
            if (show_app_log)
            {
                DrawImGuiDebugLog();
            }

            //MonoGame
            if (show_monogame_settings)
            {
                DrawMonoGameWindow();
            }

            //Main Demo window
            if (show_main_window)
            {
                DrawDemoWindow();
            }
            #endregion

            //ImGui End
            GuiRenderer.EndLayout();
        }

        #region Demos
        private void DrawImGuiNativeDemos()
        {
            ImGui.ShowDemoWindow();
        }
        #endregion

        #region Overlay

        //OverlayVariables
        float distanceX = 10.0f;
        float distanceY = 10.0f;
        int corner = 0;

        private void DrawImGuiOverlay(float frameRate)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMove;

            if (corner >= 0 && corner < 4)
            {
                //offset X
                if (corner == 1 || corner == 3)
                {
                    distanceX = (float)graphics.PreferredBackBufferWidth - 250.0f;
                }
                else
                {
                    distanceX = 10.0f;
                }
                //offset Y
                if (corner == 2 || corner == 3)
                {
                    distanceY = (float)graphics.PreferredBackBufferHeight - 100.0f;
                }
                else
                {
                    distanceY = 10.0f;
                }
                //offset menubar
                if ((corner == 0 || corner == 1) && show_app_main_menu_bar)
                {
                    distanceY += 20.0f;
                }

                Vec2 windowPosition = new Vec2(distanceX, distanceY);
                ImGui.SetNextWindowPos(windowPosition);
            }

            ImGui.SetNextWindowBgAlpha(0.35f);
            if (ImGui.Begin("Example: Simple overlay", windowFlags))
            {
                ImGui.Text("Simple overlay\nin the corner of the screen\n(right-click to change position)");
                ImGui.Separator();
                if (ImGui.IsMousePosValid())
                {
                    ImGui.Text(string.Format("Mouse Position: ({0},{1})", io.MousePos.X, io.MousePos.Y));
                }
                else
                {
                    ImGui.Text("Mouse Position: <invalid>");
                }

                ImGui.Text(string.Format("Frames per second: {0}", frameRate.ToString()));

                if (ImGui.BeginPopupContextWindow())
                {
                    //if (ImGui.MenuItem("Custom", null, corner == -1)) { corner = -1; }
                    if (ImGui.MenuItem("Top-left", null, corner == 0)) { corner = 0; }
                    if (ImGui.MenuItem("Top-right", null, corner == 1)) { corner = 1; }
                    if (ImGui.MenuItem("Bottom-left", null, corner == 2)) { corner = 2; }
                    if (ImGui.MenuItem("Bottom-right", null, corner == 3)) { corner = 3; }

                    ImGui.EndPopup();
                }
            }

            ImGui.End();
        }
        #endregion

        #region MenuBar
        private void DrawImGuiMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    ShowExampleMenuFile();
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo", "CTRL+Z")) { }
                    if (ImGui.MenuItem("Redo", "CTRL+Y", false, false)) { }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Cut", "CTRL+X")) { }
                    if (ImGui.MenuItem("Copy", "CTRL+C")) { }
                    if (ImGui.MenuItem("Paste", "CTRL+V")) { }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }

        private void ShowExampleMenuFile()
        {
            ImGui.MenuItem("(demo menu)", null, false, false);
            if (ImGui.MenuItem("New")) { }
            if (ImGui.MenuItem("Open", "Ctrl+O")) { }
            if (ImGui.MenuItem("Open Recent"))
            {
                ImGui.MenuItem("fish_hat.c");
                ImGui.MenuItem("fish_hat.inl");
                ImGui.MenuItem("fish_hat.h");
                if (ImGui.MenuItem("More.."))
                {
                    ImGui.MenuItem("Hello");
                    ImGui.MenuItem("Sailor");
                    if (ImGui.BeginMenu("Recurse.."))
                    {
                        ShowExampleMenuFile();
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenu();
            }
            if (ImGui.MenuItem("Save", "Ctrl+S")) { }
            if (ImGui.MenuItem("Save As ..")) { }

            ImGui.Separator();
            if (ImGui.BeginMenu("Options"))
            {
                bool enabled = true;
                ImGui.MenuItem("Enabled", "", enabled);
                ImGui.BeginChild("child", new Vec2(0, 60), ImGuiChildFlags.Border);
                for (int i = 0; i < 10; i++)
                {
                    ImGui.Text(string.Format("Scrolling Text {0}", i));
                }
                ImGui.EndChild();
                float f = 0.5f;
                int n = 0;
                ImGui.SliderFloat("Value", ref f, 0.0f, 1.0f);
                ImGui.InputFloat("Input", ref f, 0.1f);
                ImGui.Combo("Combo", ref n, "Yes\0No\0Maybe\0\0");
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Colors"))
            {
                float sz = ImGui.GetTextLineHeight();
                //ImGui.Text(((int)ImGuiCol.COUNT).ToString()); //Test
                for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
                {
                    string name = ImGui.GetStyleColorName((ImGuiCol)i);
                    Vec2 p = ImGui.GetCursorScreenPos();
                    ImGui.GetWindowDrawList().AddRectFilled(p, new Vec2(p.X + sz, p.Y + sz), ImGui.GetColorU32((ImGuiCol)i));
                    ImGui.Dummy(new Vec2(sz, sz));
                    ImGui.SameLine();
                    ImGui.MenuItem(name);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Options")) //Append!
            {
                bool b = true;
                ImGui.Checkbox("SomeOption", ref b);
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Disabled", false)) { } //Disabled
            if (ImGui.MenuItem("Checked", null, true)) { }
            if (ImGui.MenuItem("Quit", "Alt+F4")) { }
        }
        #endregion

        #region DebugLog
        static List<string> debug_log = new List<string>();
        static bool AutoScroll = true;
        unsafe private struct ImGuiDebugLog
        {
            //ImGuiTextBufferPtr Buf;
            ImGuiTextFilterPtr Filter;
            //ImVector<int> LineOffsets;
            //bool AutoScroll;

            public void DebugLog()
            {
                //AutoScroll = true;
                //Buf = ImGuiNative.ImGuiTextBuffer_ImGuiTextBuffer();
                var FilterPointer = ImGuiNative.ImGuiTextFilter_ImGuiTextFilter(null);
                Filter = new ImGuiTextFilterPtr(FilterPointer);
            }

            public void Clear()
            {
                debug_log.Clear();
            }

            public void AddLog(string text)
            {
                //int old_size = this.Buf.Buf.Size;

                debug_log.Add(text);

            }

            public void Destroy()
            {
                ImGuiNative.ImGuiTextFilter_destroy(Filter.NativePtr);
            }

            public void Draw(string title)
            {
                if (!ImGui.Begin(title))
                {
                    ImGui.End();
                    return;
                }

                //Options menu
                if (ImGui.BeginPopup("Options"))
                {
                    ImGui.Checkbox("Auto-scroll", ref AutoScroll);
                    ImGui.EndPopup();
                }

                //Main window
                if (ImGui.Button("Options"))
                {
                    ImGui.OpenPopup("Options");
                }
                ImGui.SameLine();
                bool clear = ImGui.Button("Clear");
                ImGui.SameLine();
                bool copy = ImGui.Button("Copy");
                ImGui.SameLine();
                Filter.Draw("Filter", -100.0f);

                ImGui.Separator();
                ImGui.BeginChild("scrolling", new Vec2(0, 0), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

                if (clear)
                {
                    Clear();
                }
                if (copy)
                {
                    ImGui.LogToClipboard();
                }

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vec2(0, 0));
                if (Filter.IsActive())
                {
                    foreach (string str in debug_log)
                    {
                        if (Filter.PassFilter(str))
                        {
                            //ImGui.TextUnformatted(str);
                            ImGui.BulletText(str);
                        }

                    }

                }
                else
                {
                    foreach (string str in debug_log)
                    {
                        ImGui.TextUnformatted(str);
                    }
                }
                ImGui.PopStyleVar();

                if (AutoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                {
                    ImGui.SetScrollHereY(1.0f);
                }

                ImGui.EndChild();
                ImGui.End();

            }
        }

        private void DrawImGuiDebugLog()
        {
            ImGuiDebugLog log = new ImGuiDebugLog();

            ImGui.SetNextWindowSize(new Vec2(500, 400), ImGuiCond.FirstUseEver);
            ImGui.Begin("Example: Log");
            if (ImGui.SmallButton("[Debug] Add 5 entries"))
            {
                string[] words = { "Bumfuzzled", "Cattywampus", "Snickersnee", "Abibliophobia", "Absquatulate" };
                foreach (string str in words)
                {
                    log.AddLog("Frame " + ImGui.GetFrameCount() + " [info] Hello, current time is " + ImGui.GetTime() + " here's a word: " + str);
                }
            }
            ImGui.End();

            log.DebugLog();
            log.Draw("Example: Log");
            log.Destroy();
        }
        #endregion

        #region AppConsole
        static List<string> console_log = new List<string>();
        static List<string> console_history = new List<string>();
        static bool AutoScroll_Console = true;
        unsafe private struct ImGuiExampleAppConsole
        {
            ImGuiTextFilterPtr Filter;

            public void ExampleAppConsole()
            {
                var FilterPointer = ImGuiNative.ImGuiTextFilter_ImGuiTextFilter(null);
                Filter = new ImGuiTextFilterPtr(FilterPointer);
            }

            public void ClearLog()
            {
                console_log.Clear();
            }

            public void AddLog(string text)
            {
                console_log.Add(text);
            }

            public void Destroy()
            {
                ImGuiNative.ImGuiTextFilter_destroy(Filter.NativePtr);
            }

            public void Draw(string title)
            {
                ImGui.SetNextWindowSize(new Vec2(520, 600), ImGuiCond.FirstUseEver);
                if (!ImGui.Begin(title))
                {
                    ImGui.End();
                    return;
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem("Close Console"))
                    {
                        show_app_console = false;
                    }
                    ImGui.EndPopup();
                }

                ImGui.TextWrapped(
                "This example implements a console with basic coloring" + //, completion "+//(TAB key) and history (Up/Down keys)
                ". A more elaborate " +
                "implementation may want to store entries along with extra data such as timestamp, emitter, etc.");
                ImGui.TextWrapped("Enter 'HELP' for help.");

                if (ImGui.SmallButton("Add Debug Text"))
                {
                    AddLog(console_log.Count + " some text");
                    AddLog("some more text");
                    AddLog("display very important message here!");
                }
                ImGui.SameLine();
                if (ImGui.SmallButton("Add Debug Error"))
                {
                    AddLog("[error] something went wrong");
                }
                ImGui.SameLine();
                if (ImGui.SmallButton("Clear"))
                {
                    ClearLog();
                }
                ImGui.SameLine();
                bool copy_to_clipboard = ImGui.SmallButton("Copy");
                ImGui.Separator();

                //Options menu
                if (ImGui.BeginPopup("Options"))
                {
                    ImGui.Checkbox("Auto-scroll", ref AutoScroll_Console);
                    ImGui.EndPopup();
                }

                //Filter
                if (ImGui.Button("Options"))
                {
                    ImGui.OpenPopup("Options");
                }
                ImGui.SameLine();
                Filter.Draw("Filter (\"incl,-excl\") (\"error\")", 180.0f);
                ImGui.Separator();

                float footer_height_to_reserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
                ImGui.BeginChild("ScrollingRegion", new Vec2(0, -footer_height_to_reserve), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);
                if (ImGui.BeginPopupContextWindow())
                {
                    if (ImGui.Selectable("Clear"))
                    {
                        ClearLog();
                    }
                    ImGui.EndPopup();
                }

                if (copy_to_clipboard)
                {
                    ImGui.LogToClipboard();
                }

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vec2(4, 1));
                if (Filter.IsActive())
                {
                    foreach (string str in console_log)
                    {
                        if (Filter.PassFilter(str))
                        {
                            Vec4 color;
                            bool has_color = false;
                            if (str.IndexOf("[error]") != -1)
                            {
                                color = new Vec4(1.0f, 0.4f, 0.4f, 1.0f);
                                ImGui.PushStyleColor(ImGuiCol.Text, color);
                                has_color = true;
                            }
                            if (str.IndexOf("# ") == 0)
                            {
                                color = new Vec4(1.0f, 0.8f, 0.6f, 1.0f);
                                ImGui.PushStyleColor(ImGuiCol.Text, color);
                                has_color = true;
                            }
                            ImGui.TextUnformatted(str);
                            if (has_color)
                            {
                                ImGui.PopStyleColor();
                            }
                        }

                    }
                }
                else
                {
                    foreach (string str in console_log)
                    {
                        Vec4 color;
                        bool has_color = false;
                        if (str.IndexOf("[error]") != -1)
                        {
                            color = new Vec4(1.0f, 0.4f, 0.4f, 1.0f);
                            ImGui.PushStyleColor(ImGuiCol.Text, color);
                            has_color = true;
                        }
                        if (str.IndexOf("# ") == 0)
                        {
                            color = new Vec4(1.0f, 0.8f, 0.6f, 1.0f);
                            ImGui.PushStyleColor(ImGuiCol.Text, color);
                            has_color = true;
                        }
                        ImGui.TextUnformatted(str);
                        if (has_color)
                        {
                            ImGui.PopStyleColor();
                        }
                    }
                }
                ImGui.PopStyleVar();

                if (AutoScroll_Console && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                {
                    ImGui.SetScrollHereY(1.0f);
                }

                ImGui.EndChild();
                ImGui.Separator();

                string input_buf = "";
                bool reclaim_focus = false;
                ImGuiInputTextFlags input_text_flags = ImGuiInputTextFlags.EnterReturnsTrue; //| ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.CallbackHistory;
                if (ImGui.InputText("Input", ref input_buf, 250, input_text_flags))
                {
                    ExecCommand(input_buf);
                    reclaim_focus = true;
                }

                ImGui.SetItemDefaultFocus();
                if (reclaim_focus)
                {
                    ImGui.SetKeyboardFocusHere(-1);
                }

                ImGui.End();

            }

            void ExecCommand(string command)
            {
                //History
                if (console_history.Count == 10)
                {
                    console_history.RemoveAt(0);
                }
                console_history.Add(command);

                //Show Input
                AddLog("# " + command);

                //Commands
                if (command == "CLEAR")
                {
                    ClearLog();
                }
                else if (command == "HELP")
                {
                    AddLog("Commands:");
                    AddLog("- CLEAR");
                    AddLog("- HELP");
                    AddLog("- HISTORY");
                }
                else if (command == "HISTORY")
                {
                    int history_pos = 0;
                    foreach (string item in console_history)
                    {
                        AddLog("  " + history_pos.ToString() + ": " + item);
                        history_pos++;
                    }
                }
                else
                {
                    AddLog("Unknown command: '" + command + "'");
                }

            }

        }

        private void DrawImGuiExampleAppConsole()
        {
            string console_window_name = "Example: Console";
            ImGuiExampleAppConsole console = new ImGuiExampleAppConsole();


            //ImGui.Begin(console_window_name);
            //if (ImGui.SmallButton("[Debug] Add 5 entries"))
            //{
            //    string[] words = { "Bumfuzzled", "Cattywampus", "Snickersnee", "Abibliophobia", "Absquatulate" };
            //    foreach (string str in words)
            //    {
            //        console.AddLog("Frame " + ImGui.GetFrameCount() + " [info] Hello, current time is " + ImGui.GetTime() + " here's a word: " + str);
            //    }
            //}
            //ImGui.End();

            console.ExampleAppConsole();
            console.Draw(console_window_name);
            console.Destroy();
        }
        #endregion

        #region DrawDemoWindowVariables
        // Window options
        bool no_titlebar = false;
        bool no_scrollbar = false;
        bool no_menu = false;
        bool no_move = false;
        bool no_resize = false;
        bool no_collapse = false;
        //bool no_close = false;
        bool no_nav = false;
        bool no_background = false;
        bool no_bring_to_front = false;
        // Examples
        bool show_app_main_menu_bar = false;
        bool show_app_log = false;
        bool show_app_simple_overlay = false;
        bool show_native_examples = false;
        static bool show_app_console = false;
        bool show_monogame_settings = true;
        // Tools
        bool show_app_style_editor = false;
        bool show_app_metrics = false;
        bool show_app_about = false;
        #endregion

        private void DrawDemoWindow()
        {

            ImGuiWindowFlags windowFlags = 0;
            if (no_titlebar) { windowFlags |= ImGuiWindowFlags.NoTitleBar; }
            if (no_scrollbar) { windowFlags |= ImGuiWindowFlags.NoScrollbar; }
            if (!no_menu) { windowFlags |= ImGuiWindowFlags.MenuBar; }
            if (no_move) { windowFlags |= ImGuiWindowFlags.NoMove; }
            if (no_resize) { windowFlags |= ImGuiWindowFlags.NoResize; }
            if (no_collapse) { windowFlags |= ImGuiWindowFlags.NoCollapse; }
            if (no_nav) { windowFlags |= ImGuiWindowFlags.NoNav; }
            if (no_background) { windowFlags |= ImGuiWindowFlags.NoBackground; }
            if (no_bring_to_front) { windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus; }
            //if (no_close) { } // p_open = null;
            //bool p_open = true;

            ImGui.SetNextWindowPos(new Vec2(450, 20), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vec2(550, 680), ImGuiCond.FirstUseEver);

            //Main body
            if (!ImGui.Begin("Dear ImGui Demo / Monogame & ImGui.Net", windowFlags))  //ref p_open, windowFlags)) Close button is not changing bool!?
            {
                ImGui.End();
                return;
            }

            ImGui.PushItemWidth(ImGui.GetFontSize() * -12);

            //Menu bar
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Menu"))
                {
                    ShowExampleMenuFile();
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Examples"))
                {
                    ImGui.MenuItem("Main menu bar", null, ref show_app_main_menu_bar);
                    ImGui.MenuItem("Console", null, ref show_app_console);
                    ImGui.MenuItem("Log", null, ref show_app_log);
                    //ImGui.MenuItem("Simple Layout", null, true);
                    //ImGui.MenuItem("Property editor", null, true);
                    //ImGui.MenuItem("Long text display", null, true);
                    //ImGui.MenuItem("Auto-resizing window", null, true);
                    //ImGui.MenuItem("Constrained-resizing window", null, true);
                    ImGui.MenuItem("Simple overlay", null, ref show_app_simple_overlay);
                    //ImGui.MenuItem("Fullscreen window", null, true);
                    //ImGui.MenuItem("Manipulating window titles", null, true);
                    //ImGui.MenuItem("Custom rendering", null, true);
                    //ImGui.MenuItem("Documents", null, true);
                    ImGui.MenuItem("Native ImGui Demos", null, ref show_native_examples);
                    ImGui.MenuItem("MonoGame Settings", null, ref show_monogame_settings);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Tools"))
                {
                    ImGui.MenuItem("Metrics/Debugger", null, ref show_app_metrics);
                    ImGui.MenuItem("Style Editor", null, ref show_app_style_editor);
                    ImGui.MenuItem("About Dear ImGui", null, ref show_app_about);
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            ImGui.Text("dear imgui says hello. (" + ImGui.GetVersion() + ")");
            ImGui.Spacing();

            if (ImGui.CollapsingHeader("Help"))
            {
                ImGui.Text("ABOUT THIS DEMO:");
                ImGui.BulletText("Sections below are demonstrating many aspects of the library.");
                ImGui.BulletText("The \"Examples\" menu above leads to more demo contents.");
                ImGui.BulletText("The \"Tools\" menu above gives access to: About Box, Style Editor,\nand Metrics/Debugger (general purpose Dear ImGui debugging tool).");
                ImGui.Separator();

                ImGui.Text("PROGRAMMER GUIDE:");
                ImGui.BulletText("See the ShowDemoWindow() code in imgui_demo.cpp. <- you are here!");
                ImGui.BulletText("See comments in imgui.cpp.");
                ImGui.BulletText("See example applications in the examples/ folder.");
                ImGui.BulletText("Read the FAQ at http://www.dearimgui.org/faq/");
                ImGui.BulletText("Set 'io.ConfigFlags |= NavEnableKeyboard' for keyboard controls.");
                ImGui.BulletText("Set 'io.ConfigFlags |= NavEnableGamepad' for gamepad controls.");
                ImGui.Separator();

                ImGui.Text("USER GUIDE:");
                ImGui.ShowUserGuide();
            }

            if (ImGui.CollapsingHeader("Configuration"))
            {
                if (ImGui.TreeNode("Style"))
                {
                    HelpMarker("The same contents can be accessed in 'Tools->Style Editor' or by calling the ShowStyleEditor() function.");
                    ImGui.ShowStyleEditor();
                    ImGui.TreePop();
                    ImGui.Separator();
                }

                if (ImGui.TreeNode("Capture/Logging"))
                {
                    HelpMarker("The logging API redirects all text output so you can easily capture the content of a window or a block. Tree nodes can be automatically expanded.\nTry opening any of the contents below in this window and then click one of the \"Log To\" button.");
                    ImGui.LogButtons();

                    HelpMarker("You can also call ImGui.LogText() to output directly to the log without a visual output.");
                    if (ImGui.Button("Copy \"Hello, world!\" to clipboard"))
                    {
                        ImGui.LogToClipboard();
                        ImGui.LogText("Hello, world!");
                        ImGui.LogFinish();
                    }
                    ImGui.TreePop();
                }
            }

            if (ImGui.CollapsingHeader("Window options"))
            {
                //if(ImGui.BeginTable // feature of imgui 1.75?
                ImGui.Checkbox("No titlebar", ref no_titlebar);
                ImGui.Checkbox("No scrollbar", ref no_scrollbar);
                ImGui.Checkbox("No menu", ref no_menu);
                ImGui.Checkbox("No move", ref no_move);
                ImGui.Checkbox("No resize", ref no_resize);
                ImGui.Checkbox("No collapse", ref no_collapse);
                //ImGui.Checkbox("No close", ref no_close);
                ImGui.Checkbox("No nav", ref no_nav);
                ImGui.Checkbox("No background", ref no_background);
                ImGui.Checkbox("No bring to front", ref no_bring_to_front);
            }

            DemoWindowWidgets();
            DemoWindowLayout();
            DemoWindowPopups();
            //ShowDemoWindowTables();
            DemoWindowMisc();

            ImGui.PopItemWidth();
            ImGui.End();
        }

        private void HelpMarker(string desc)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        #region DemoWindowWidgetsVariables
        //Basic Input
        int clicked = 0;
        bool check = true;
        int e = 0;
        int counter = 0;
        int item_current = 0;
        string str0 = "Hello, world!";
        string str1 = "";
        int i0 = 123;
        float f0 = 0.001f;
        double d0 = 999999.00000001;
        float f1 = 1.42f;
        Vec3 vec3 = new Vec3(0.10f, 0.20f, 0.30f);
        //Basic Drag & Slider
        int i1 = 50, i2 = 42;
        float f2 = 1.00f, f3 = 0.0067f;
        int i3 = 0;
        float f4 = 0.123f;
        float angle = 0.0f;
        Vec3 col1 = new Vec3(1.0f, 0.0f, 0.2f);
        Vec4 col2 = new Vec4(0.4f, 0.7f, 0.0f, 0.5f);
        int current_fruit = 1;
        //Trees
        bool base_flags_first_run = true;
        uint base_flags = 0;
        bool align_label_with_current_x_position = false;
        int index_selected = 0;
        //bool test_drag_and_drop = false;
        //Collapsing headers
        bool closable_group = true;
        //Word wrapping
        float wrap_width = 200.0f;
        //Images
        int pressed_count = 0;
        //Combo
        uint flags = 0; //ImGuiComboFlags
        int item_current_idx = 0;
        int item_current_2 = 0;
        int item_current_3 = -1;
        //List boxes
        int item_current_idx_lb = 0;
        //Selectables
        bool[] selection = { false, true, false, false, false };
        int selected = -1;
        bool[] selection_ms = { false, false, false, false, false };
        bool[] selected_rend = { false, false, false };
        bool[] selected_align = { true, false, true, false, true, false, true, false, true };
        //Text input
        bool flags_ti_first_run = true;
        uint flags_ti = 0;
        string buf1 = "";
        string buf2 = "";
        string buf3 = "";
        string buf4 = "";
        string buf5 = "";
        string password = "password123";
        //Tabs
        bool flags_tabs_first_run = true;
        uint tab_bar_flags = 0;
        bool[] opened = { true, true, true, true };
        //Plots Widgets
        bool animate = true;
        float[] values = new float[90];
        int values_offset = 0;
        double refresh_time = 0.0;
        float phase = 0.0f;
        float progress = 0.0f, progress_dir = 1.0f;
        //Color Widgets
        Vec3 color_vec3 = new Vec3(114.0f / 255.0f, 144.0f / 255.0f, 154 / 255.0f);
        Vec4 color_vec4 = new Vec4(114.0f / 255.0f, 144.0f / 255.0f, 154 / 255.0f, 200.0f / 255.0f);
        bool alpha_preview = true;
        bool alpha_half_preview = false;
        bool drag_and_drop = true;
        bool options_menu = true;
        bool hdr = false;
        bool alpha = true;
        bool alpha_bar = true;
        bool side_preview = true;
        bool ref_color = false;
        Vec4 ref_color_v = new Vec4(1.0f, 0.0f, 1.0f, 0.5f);
        int display_mode = 0;
        int picker_mode = 0;
        Vec4 color_hsv = new Vec4(0.23f, 1.0f, 1.0f, 1.0f);
        //Range Widgets
        float begin = 10, end = 90;
        int begin_i = 100, end_i = 1000;
        //Multi component Widgets
        Vec2 vec2f = new Vec2(0.10f, 0.20f);
        Vec3 vec3f = new Vec3(0.10f, 0.20f, 0.30f);
        Vec4 vec4f = new Vec4(0.10f, 0.20f, 0.30f, 0.44f);
        int[] vec4i = { 1, 5, 100, 255 };
        //Vertical Sliders
        float spacing = 4;
        int int_value = 0;
        float[] values_vert = { 0.0f, 0.60f, 0.35f, 0.9f, 0.70f, 0.20f, 0.0f };
        float col_red = 1.0f;
        float col_green = 1.0f;
        float col_blue = 1.0f;
        float[] values2 = { 0.20f, 0.80f, 0.40f, 0.25f };
        #endregion

        private void DemoWindowWidgets()
        {
            if (!ImGui.CollapsingHeader("Widgets"))
            {
                return;
            }

            //Basic
            #region Basic
            if (ImGui.TreeNode("Basic"))
            {

                if (ImGui.Button("Button"))
                {
                    clicked++;
                    if (clicked == 2)
                    {
                        clicked = 0;
                    }
                }
                if (clicked == 1)
                {
                    ImGui.SameLine();
                    ImGui.Text("Thanks for clicking me!");
                }

                ImGui.Checkbox("checkbox", ref check);

                ImGui.RadioButton("radio a", ref e, 0);
                ImGui.SameLine();
                ImGui.RadioButton("radio b", ref e, 1);
                ImGui.SameLine();
                ImGui.RadioButton("radio c", ref e, 2);

                for (int i = 0; i < 7; i++)
                {
                    if (i > 0)
                    {
                        ImGui.SameLine();
                    }
                    ImGui.PushID(i);
                    //ImColorPtr color = new ImColorPtr();
                    //ImGuiNative.ImColor_HSV ?
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vec4(i / 7.0f, 0.6f, 0.6f, 0.6f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vec4(i / 7.0f, 0.7f, 0.7f, 0.7f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vec4(i / 7.0f, 0.8f, 0.8f, 0.8f));
                    ImGui.Button("Click");
                    ImGui.PopStyleColor(3);
                    ImGui.PopID();
                }

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Hold to repeat:");
                ImGui.SameLine();

                float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
                ImGui.PushButtonRepeat(true);
                if (ImGui.ArrowButton("##left", ImGuiDir.Left)) { counter--; }
                ImGui.SameLine(0.0f, spacing);
                if (ImGui.ArrowButton("##right", ImGuiDir.Right)) { counter++; }
                ImGui.PopButtonRepeat();
                ImGui.SameLine();
                ImGui.Text(counter.ToString());

                ImGui.Text("Hover over me");
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("I am a tooltip");
                }

                ImGui.SameLine();
                ImGui.Text("- or me");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("I am a fancy tooltip");
                    float[] arr = { 0.6f, 0.1f, 1.0f, 0.5f, 0.92f, 0.1f, 0.2f };
                    ImGui.PlotLines("Curve", ref arr[0], arr.Length);
                    ImGui.EndTooltip();
                }

                ImGui.Separator();
                ImGui.LabelText("label", "Value");

                //combo box
                string items = "AAAA\0BBBB\0CCCC\0DDDD\0EEEE\0FFFF\0GGGG\0HHHH\0IIIIIII\0JJJJ\0KKKKKKK";
                ImGui.Combo("combo", ref item_current, items, 11);

                //input
                ImGui.InputText("input text", ref str0, 128);
                ImGui.SameLine();
                HelpMarker("USER:\n" +
                "Hold SHIFT or use mouse to select text.\n" +
                "CTRL+Left/Right to word jump.\n" +
                "CTRL+A or double-click to select all.\n" +
                "CTRL+X,CTRL+C,CTRL+V clipboard.\n" +
                "CTRL+Z,CTRL+Y undo/redo.\n" +
                "ESCAPE to revert.\n\n" +
                "PROGRAMMER:\n" +
                "You can use the ImGuiInputTextFlags_CallbackResize facility if you need to wire InputText() " +
                "to a dynamic string type. See misc/cpp/imgui_stdlib.h for an example (this is not demonstrated " +
                "in imgui_demo.cpp).");

                ImGui.InputTextWithHint("input text (w/ hint", "enter text here", ref str1, 10);

                ImGui.InputInt("input int", ref i0);
                ImGui.SameLine();
                HelpMarker("You can apply arithmetic operators +,*,/ on numerical values.\n" +
                            "  e.g. [ 100 ], input \'*2\', result becomes [ 200 ]\n" +
                            "Use +- to subtract.");

                ImGui.InputFloat("input float", ref f0, 0.01f, 1.0f, "%.3f");

                ImGui.InputDouble("input double", ref d0, 0.01f, 1.0f, "%.8f");

                ImGui.InputFloat("input scientific", ref f1, 0.0f, 0.0f, "%e");
                ImGui.SameLine();
                HelpMarker("You can input value using the scientific notation,\n" +
                            "  e.g. \"1e+8\" becomes \"100000000\".");

                ImGui.InputFloat3("input float 3", ref vec3);

                //drag               
                ImGui.DragInt("drag int", ref i1, 1);
                ImGui.SameLine();
                HelpMarker("Click and drag to edit value.\n" +
                "Hold SHIFT/ALT for faster/slower edit.\n" +
                "Double-click or CTRL+click to input value.");

                ImGui.DragInt("drag int 0..100", ref i2, 1, 0, 100, "%d%%");

                ImGui.DragFloat("drag float", ref f2, 0.005f);
                ImGui.DragFloat("drag small float", ref f3, 0.0001f, 0.0f, 0.0f, "%.06f ns");

                ImGui.SliderInt("slider int", ref i3, -1, 3);
                ImGui.SameLine();
                HelpMarker("CTRL+click to input value.");

                ImGui.SliderFloat("slider float", ref f4, 0.0f, 1.0f, "ratio = %.3f");

                ImGui.SliderAngle("slider angle", ref angle);

                //color
                ImGui.ColorEdit3("color 1", ref col1);
                ImGui.SameLine();
                HelpMarker("Click on the color square to open a color picker.\n" +
                "Click and hold to use drag and drop.\n" +
                "Right-click on the color square to show options.\n" +
                "CTRL+click on individual component to input value.\n");

                ImGui.ColorEdit4("color 2", ref col2);

                string[] fruits = { "Apple", "Banana", "Cherry", "Kiwi", "Mango", "Orange", "Pineapple", "Strawberry", "Watermelon" };
                ImGui.ListBox("listbox", ref current_fruit, fruits, fruits.Length);
                ImGui.SameLine();
                HelpMarker("Using the simplified one-liner ListBox API here.\nRefer to the \"List boxes\" section below for an explanation of how to use the more flexible and general BeginListBox/EndListBox API.");

                ImGui.TreePop();
            }
            #endregion

            //Trees
            #region Trees
            if (ImGui.TreeNode("Trees"))
            {
                if (ImGui.TreeNode("Basic trees"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (i == 0)
                        {
                            ImGui.SetNextItemOpen(true, ImGuiCond.Once);
                        }

                        if (ImGui.TreeNode(i.ToString(), "Child " + i.ToString()))
                        {
                            ImGui.Text("blah blah");
                            ImGui.SameLine();
                            if (ImGui.SmallButton("button")) { }
                            ImGui.TreePop();
                        }
                    }
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Advanced, with Selectable nodes"))
                {
                    HelpMarker(
                    "This is a more typical looking tree with selectable nodes.\n" +
                    "Click to select, CTRL+Click to toggle, click on arrows or double-click to open.");

                    if (base_flags_first_run)
                    {
                        ImGuiTreeNodeFlags _base_flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;
                        base_flags = (uint)_base_flags;
                        base_flags_first_run = false;
                    }

                    ImGui.CheckboxFlags("ImGuiTreeNodeFlags_OpenOnArrow", ref base_flags, (uint)ImGuiTreeNodeFlags.OpenOnArrow);
                    ImGui.CheckboxFlags("ImGuiTreeNodeFlags_OpenOnDoubleClick", ref base_flags, (uint)ImGuiTreeNodeFlags.OpenOnDoubleClick);
                    ImGui.Checkbox("Align label with current X position", ref align_label_with_current_x_position);
                    //ImGui.Checkbox("Test tree node as drag source", ref test_drag_and_drop);
                    ImGui.Text("Hello!");
                    if (align_label_with_current_x_position)
                    {
                        ImGui.Unindent(ImGui.GetTreeNodeToLabelSpacing());
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        ImGuiTreeNodeFlags node_flags = (ImGuiTreeNodeFlags)base_flags;
                        if (i == index_selected)
                        {
                            node_flags |= ImGuiTreeNodeFlags.Selected;
                        }

                        if (i < 3)
                        {
                            if (ImGui.TreeNodeEx(i.ToString(), node_flags, "Selectable Node " + i.ToString()))
                            {
                                ImGui.BulletText("Blah blah\nBlah Blah");
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            node_flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
                            ImGui.TreeNodeEx(i.ToString(), node_flags, "Selectable Leaf " + i.ToString());
                        }

                        if (ImGui.IsItemClicked())
                        {
                            index_selected = i;
                        }
                    }


                    ImGui.TreePop();

                }
                ImGui.TreePop();
            }
            #endregion

            //Collapsing Headers
            #region Collapsing Headers & Bullets
            if (ImGui.TreeNode("Collapsing Headers"))
            {
                ImGui.Checkbox("Show 2nd header", ref closable_group);
                if (ImGui.CollapsingHeader("Header", ImGuiTreeNodeFlags.None))
                {
                    ImGui.Text("IsItemHovered: " + ImGui.IsItemHovered());
                    for (int i = 0; i < 5; i++)
                    {
                        ImGui.Text("Some content " + i);
                    }
                }
                if (ImGui.CollapsingHeader("Header with a close button", ref closable_group))
                {
                    ImGui.Text("IsItemHovered: " + ImGui.IsItemHovered());
                    for (int i = 0; i < 5; i++)
                    {
                        ImGui.Text("More content " + i);
                    }
                }
                ImGui.TreePop();
            }

            //Bullets
            if (ImGui.TreeNode("Bullets"))
            {
                ImGui.BulletText("Bullet point 1");
                ImGui.BulletText("Bullet point 2\nOn multiple lines");
                if (ImGui.TreeNode("Tree node"))
                {
                    ImGui.BulletText("Another bullet point");
                    ImGui.TreePop();
                }
                ImGui.Bullet();
                ImGui.Text("Bullet point 3 (two calls)");
                ImGui.Bullet();
                ImGui.SmallButton("Button");
                ImGui.TreePop();
            }
            #endregion

            //Text
            #region Text
            if (ImGui.TreeNode("Text"))
            {
                if (ImGui.TreeNode("Colorful Text"))
                {
                    ImGui.TextColored(new Vec4(1.0f, 0.0f, 1.0f, 1.0f), "Pink");
                    ImGui.TextColored(new Vec4(1.0f, 1.0f, 0.0f, 1.0f), "Yellow");
                    ImGui.TextDisabled("Disabled");
                    ImGui.SameLine();
                    HelpMarker("The TextDisabled color is stored in ImGuiStyle.");
                    ImGui.TreePop();
                }


                if (ImGui.TreeNode("Word Wrapping"))
                {
                    ImGui.TextWrapped("This text should automatically wrap on the edge of the window. The current implementation " +
                    "for text wrapping follows simple rules suitable for English and possibly other languages.");
                    ImGui.Spacing();

                    ImGui.SliderFloat("Wrap width", ref wrap_width, -20, 600, "%.0f");

                    ImDrawListPtr draw_list = ImGui.GetWindowDrawList();
                    for (int n = 0; n < 2; n++)
                    {
                        ImGui.Text("Test paragraph " + n);
                        Vec2 pos = ImGui.GetCursorPos();
                        Vec2 marker_min = new Vec2(pos.X + wrap_width, pos.Y);
                        Vec2 marker_max = new Vec2(pos.X + wrap_width + 10, pos.Y + ImGui.GetTextLineHeight());
                        ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + wrap_width);
                        if (n == 0)
                        {
                            ImGui.Text(string.Format("The lazy dog is a good dog. This paragraph should fit within {0} pixels. Testing a 1 character word. The quick brown fox jumps over the lazy dog.", wrap_width));
                        }
                        else
                        {
                            ImGui.Text("aaaaaaaa bbbbbbbb, c cccccccc,dddddddd. d eeeeeeee   ffffffff. gggggggg!hhhhhhhh");
                        }

                        Vec4 colf = new Vec4(255.0f, 255.0f, 0.0f, 255.0f);
                        uint col = ImGui.ColorConvertFloat4ToU32(colf); //ImGuiNative.igColorConvertFloat4ToU32(colf);

                        draw_list.AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), col);

                        Vec4 colf2 = new Vec4(255.0f, 0.0f, 255.0f, 255.0f);
                        uint col2 = ImGui.ColorConvertFloat4ToU32(colf2); //ImGuiNative.igColorConvertFloat4ToU32(colf2);

                        draw_list.AddRectFilled(marker_min, marker_max, col2);

                        ImGui.PopTextWrapPos();

                    }
                    ImGui.TreePop();
                }
                ImGui.TreePop();
            }
            #endregion

            //Images
            #region Images
            if (ImGui.TreeNode("Images"))
            {
                ImGuiIOPtr io = ImGui.GetIO();
                ImGui.TextWrapped("Below we are displaying the font texture (which is the only texture we have access to in this demo). " +
                "Use the 'ImTextureID' type as storage to pass pointers or identifier to your own texture data. " +
                "Hover the texture for a zoomed view!");

                System.IntPtr my_tex_id = io.Fonts.TexID;
                float my_tex_w = (float)io.Fonts.TexWidth;
                float my_tex_h = (float)io.Fonts.TexHeight;

                ImGui.Text(my_tex_w + "x" + my_tex_h);
                Vec2 pos = ImGui.GetCursorScreenPos();
                Vec2 uv_min = new Vec2(0.0f, 0.0f); // top left
                Vec2 uv_max = new Vec2(1.0f, 1.0f); // lower right
                Vec4 tint_col = new Vec4(1.0f, 1.0f, 1.0f, 1.0f); // no tint
                Vec4 border_col = new Vec4(1.0f, 1.0f, 1.0f, 0.5f); // 50% opaque white

                ImGui.Image(my_tex_id, new Vec2(my_tex_w, my_tex_h), uv_min, uv_max, tint_col, border_col);

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    float region_sz = 32.0f;
                    float region_x = io.MousePos.X - pos.X - region_sz * 0.5f;
                    float region_y = io.MousePos.Y - pos.Y - region_sz * 0.5f;
                    float zoom = 4.0f;
                    if (region_x < 0.0f) { region_x = 0.0f; }
                    else if (region_x > my_tex_w - region_sz) { region_x = my_tex_w - region_sz; }
                    if (region_y < 0.0f) { region_y = 0.0f; }
                    else if (region_y > my_tex_h - region_sz) { region_y = my_tex_h - region_sz; }
                    ImGui.Text(string.Format("Min: ({0},{1})", region_x, region_y));
                    ImGui.Text(string.Format("Max: ({0},{1})", region_x + region_sz, region_y + region_sz));
                    Vec2 uv0 = new Vec2((region_x) / my_tex_w, (region_y) / my_tex_h);
                    Vec2 uv1 = new Vec2((region_x + region_sz) / my_tex_w, (region_y + region_sz) / my_tex_h);
                    ImGui.Image(my_tex_id, new Vec2(region_sz * zoom, region_sz * zoom), uv0, uv1, tint_col, border_col);
                    ImGui.EndTooltip();
                }

                ImGui.TextWrapped("And now some textured buttons..");
                for (int i = 0; i < 8; i++)
                {
                    ImGui.PushID(i);
                    int frame_padding = -1 + i;
                    Vec2 size = new Vec2(32.0f, 32.0f);
                    Vec2 uv0 = new Vec2(0.0f, 0.0f);
                    Vec2 uv1 = new Vec2(32.0f / my_tex_w, 32.0f / my_tex_h);
                    Vec4 bg_col = new Vec4(0.0f, 0.0f, 0.0f, 1.0f); // black background
                    Vec4 tint_col2 = new Vec4(1.0f, 1.0f, 1.0f, 1.0f); // no tint
                    if (ImGui.ImageButton($"textured_button_id_{i}",my_tex_id, size, uv0, uv1, bg_col, tint_col2))
                    {
                        pressed_count += 1;
                    }
                    ImGui.PopID();
                    ImGui.SameLine();
                }
                ImGui.NewLine();
                ImGui.Text(string.Format("Pressed {0} times.", pressed_count));
                ImGui.TreePop();
            }
            #endregion

            //Combo
            #region Combo & List boxes
            if (ImGui.TreeNode("Combo"))
            {
                ImGui.CheckboxFlags("ImGuiComboFlags_PopupAlignLeft", ref flags, (uint)ImGuiComboFlags.PopupAlignLeft);
                ImGui.SameLine();
                HelpMarker("Only makes a difference if the popup is larger than the combo");
                if (ImGui.CheckboxFlags("ImGuiComboFlags_NoArrowButton", ref flags, (uint)ImGuiComboFlags.NoArrowButton))
                {
                    flags &= ~(uint)ImGuiComboFlags.NoPreview; // clear the other flag
                }
                if (ImGui.CheckboxFlags("ImGuiComboFlags_NoPreview", ref flags, (uint)ImGuiComboFlags.NoPreview))
                {
                    flags &= ~(uint)ImGuiComboFlags.NoArrowButton; // clear the other flag
                }

                string[] items = { "AAAA", "BBBB", "CCCC", "DDDD", "EEEE", "FFFF", "GGGG", "HHHH", "IIII", "JJJJ", "KKKK", "LLLLLLL", "MMMM", "OOOOOOO" };
                string combo_label = items[item_current_idx];
                ImGuiComboFlags flags_cf = (ImGuiComboFlags)flags;
                if (ImGui.BeginCombo("combo 1", combo_label, flags_cf))
                {
                    for (int n = 0; n < items.Length; n++)
                    {
                        bool is_selected = (item_current_idx == n);
                        if (ImGui.Selectable(items[n], is_selected))
                        {
                            item_current_idx = n;
                        }

                        if (is_selected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.Combo("combo 2 (one-liner)", ref item_current_2, "aaaa\0bbbb\0cccc\0dddd\0eeee\0\0");

                ImGui.Combo("combo 3 (array)", ref item_current_3, items, items.Length);

                ImGui.TreePop();
            }

            //List boxes
            if (ImGui.TreeNode("List boxes"))
            {
                string[] items = { "AAAA", "BBBB", "CCCC", "DDDD", "EEEE", "FFFF", "GGGG", "HHHH", "IIII", "JJJJ", "KKKK", "LLLLLLL", "MMMM", "OOOOOOO" };
                if (ImGui.ListBox("listbox 1", ref item_current_idx_lb, items, items.Length)) { }

                ImGui.TreePop();
            }
            #endregion

            //Selectables
            #region Selectables
            if (ImGui.TreeNode("Selectables"))
            {
                if (ImGui.TreeNode("Basic"))
                {
                    ImGui.Selectable("1. I am selectable", ref selection[0]);
                    ImGui.Selectable("2. I am selectable", ref selection[1]);
                    ImGui.Text("3. I am not selectable");
                    ImGui.Selectable("4. I am selectable", ref selection[3]);
                    if (ImGui.Selectable("5. I am double clickable", ref selection[4], ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        if (ImGui.IsMouseDoubleClicked(0))
                        {
                            selection[4] = !selection[4];
                        }
                    }
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Selection State: Single Selection"))
                {
                    for (int n = 0; n < 5; n++)
                    {
                        string buf = string.Format("Object {0}", n);
                        if (ImGui.Selectable(buf, selected == n))
                        {
                            selected = n;
                        }
                    }
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Selection State: Multiple Selection"))
                {
                    HelpMarker("Hold CTRL and click to select multiple items.");

                    for (int n = 0; n < 5; n++)
                    {
                        string buf = string.Format("Object {0}", n);
                        if (ImGui.Selectable(buf, selection_ms[n]))
                        {
                            if (!ImGui.GetIO().KeyCtrl)
                            {
                                for (int r = 0; r < selection_ms.Length; r++)
                                {
                                    selection_ms[r] = false;
                                }
                            }
                            selection_ms[n] ^= true;
                        }
                    }
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Rendering more text into the same line"))
                {
                    ImGui.Selectable("main.c", ref selected_rend[0]); ImGui.SameLine(300); ImGui.Text(" 2,345 bytes");
                    ImGui.Selectable("Hello.cpp", ref selected_rend[1]); ImGui.SameLine(300); ImGui.Text("12,345 bytes");
                    ImGui.Selectable("Hello.h", ref selected_rend[2]); ImGui.SameLine(300); ImGui.Text(" 2,345 bytes");
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Alignment"))
                {
                    HelpMarker("By default, Selectables uses style.SelectableTextAlign but it can be overridden on a per-item " +
                        "basis using PushStyleVar(). You'll probably want to always keep your default situation to " +
                        "left-align otherwise it becomes difficult to layout multiple items on a same line");

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            Vec2 alignment = new Vec2((float)x / 2.0f, (float)y / 2.0f);
                            string name = string.Format("({0},{1})", alignment.X, alignment.Y);
                            if (x > 0) { ImGui.SameLine(); }
                            ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, alignment);
                            ImGui.Selectable(name, ref selected_align[3 * y + x], ImGuiSelectableFlags.None, new Vec2(80, 80));
                            ImGui.PopStyleVar();
                        }
                    }
                    ImGui.TreePop();
                }
                ImGui.TreePop();
            }
            #endregion

            //Text Input
            #region Text Input
            if (ImGui.TreeNode("Text Input"))
            {
                if (ImGui.TreeNode("Multi-line Text Input"))
                {
                    string text = "/*\n" +
                    " The Pentium F00F bug, shorthand for F0 0F C7 C8,\n" +
                    " the hexadecimal encoding of one offending instruction,\n" +
                    " more formally, the invalid operand with locked CMPXCHG8B\n" +
                    " instruction bug, is a design flaw in the majority of\n" +
                    " Intel Pentium, Pentium MMX, and Pentium OverDrive\n" +
                    " processors (all in the P5 microarchitecture).\n" +
                    "*/\n\n" +
                    "label:\n" +
                    "\tlock cmpxchg8b eax\n";

                    if (flags_ti_first_run)
                    {
                        ImGuiInputTextFlags _flags = ImGuiInputTextFlags.AllowTabInput;
                        flags_ti = (uint)_flags;
                        flags_ti_first_run = false;
                    }

                    HelpMarker("You can use the ImGuiInputTextFlags_CallbackResize facility if you need to wire InputTextMultiline() to a dynamic string type. See misc/cpp/imgui_stdlib.h for an example. (This is not demonstrated in imgui_demo.cpp because we don't want to include <string> in here)");
                    ImGui.CheckboxFlags("ImGuiInputTextFlags_ReadOnly", ref flags_ti, (uint)ImGuiInputTextFlags.ReadOnly);
                    ImGui.CheckboxFlags("ImGuiInputTextFlags_AllowTabInput", ref flags_ti, (uint)ImGuiInputTextFlags.AllowTabInput);
                    ImGui.CheckboxFlags("ImGuiInputTextFlags_CtrlEnterForNewLine", ref flags_ti, (uint)ImGuiInputTextFlags.CtrlEnterForNewLine);

                    ImGuiInputTextFlags flags_ml = (ImGuiInputTextFlags)flags_ti;
                    ImGui.InputTextMultiline("##source", ref text, (uint)text.Length, new Vec2(0, ImGui.GetTextLineHeight() * 16), flags_ml);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Filtered Text Input"))
                {
                    ImGui.InputText("default", ref buf1, (uint)64);
                    ImGui.InputText("decimal", ref buf2, (uint)64, ImGuiInputTextFlags.CharsDecimal);
                    ImGui.InputText("hexadecimal", ref buf3, (uint)64, ImGuiInputTextFlags.CharsHexadecimal);
                    ImGui.InputText("uppercase", ref buf4, (uint)64, ImGuiInputTextFlags.CharsUppercase);
                    ImGui.InputText("no blank", ref buf5, (uint)64, ImGuiInputTextFlags.CharsNoBlank);
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Password Input"))
                {
                    ImGui.InputText("password", ref password, (uint)64, ImGuiInputTextFlags.Password);
                    ImGui.SameLine();
                    HelpMarker("Display all characters as '*'.\nDisable clipboard cut and copy.\nDisable logging.\n");
                    ImGui.InputTextWithHint("password (w/ hint)", "<password>", ref password, (uint)64, ImGuiInputTextFlags.Password);
                    ImGui.InputText("password (clear)", ref password, (uint)64);
                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }
            #endregion

            //Tabs
            #region Tabs
            if (ImGui.TreeNode("Tabs"))
            {
                if (ImGui.TreeNode("Basic"))
                {
                    ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
                    if (ImGui.BeginTabBar("MyTabBar", tab_bar_flags))
                    {
                        if (ImGui.BeginTabItem("Avocado"))
                        {
                            ImGui.Text("This is the Avocado tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }
                        if (ImGui.BeginTabItem("Broccoli"))
                        {
                            ImGui.Text("This is the Broccoli tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }
                        if (ImGui.BeginTabItem("Cucumber"))
                        {
                            ImGui.Text("This is the Cucumber tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.Separator();
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Advanced & Close Button"))
                {
                    if (flags_tabs_first_run)
                    {
                        ImGuiTabBarFlags _tab_bar_flags = ImGuiTabBarFlags.Reorderable;
                        tab_bar_flags = (uint)_tab_bar_flags;
                        flags_tabs_first_run = false;
                    }

                    ImGui.CheckboxFlags("ImGuiTabBarFlags_Reorderable", ref tab_bar_flags, (uint)ImGuiTabBarFlags.Reorderable);
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_AutoSelectNewTabs", ref tab_bar_flags, (uint)ImGuiTabBarFlags.AutoSelectNewTabs);
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_TabListPopupButton", ref tab_bar_flags, (uint)ImGuiTabBarFlags.TabListPopupButton);
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_NoCloseWithMiddleMouseButton", ref tab_bar_flags, (uint)ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
                    if ((tab_bar_flags & (uint)ImGuiTabBarFlags.FittingPolicyMask) == 0)
                    {
                        tab_bar_flags |= (uint)ImGuiTabBarFlags.FittingPolicyDefault;
                    }
                    if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyResizeDown", ref tab_bar_flags, (uint)ImGuiTabBarFlags.FittingPolicyResizeDown))
                    {
                        tab_bar_flags &= ~((uint)ImGuiTabBarFlags.FittingPolicyMask ^ (uint)ImGuiTabBarFlags.FittingPolicyResizeDown);
                    }
                    if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyScroll", ref tab_bar_flags, (uint)ImGuiTabBarFlags.FittingPolicyScroll))
                    {
                        tab_bar_flags &= ~((uint)ImGuiTabBarFlags.FittingPolicyMask ^ (uint)ImGuiTabBarFlags.FittingPolicyScroll);
                    }

                    string[] names = { "Artichoke", "Beetroot", "Celery", "Daikon" };
                    for (int n = 0; n < opened.Length; n++)
                    {
                        if (n > 0) { ImGui.SameLine(); }
                        ImGui.Checkbox(names[n], ref opened[n]);
                    }

                    ImGuiTabBarFlags tab_bar_flags_tb = (ImGuiTabBarFlags)tab_bar_flags;
                    if (ImGui.BeginTabBar("MyTabBar", tab_bar_flags_tb))
                    {
                        for (int n = 0; n < opened.Length; n++)
                        {
                            if (opened[n] && ImGui.BeginTabItem(names[n], ref opened[n], ImGuiTabItemFlags.None))
                            {
                                ImGui.Text(string.Format("This is the {0} tab!", names[n]));
                                if (n == 1 || n == 3)
                                {
                                    ImGui.Text("I am an odd tab.");
                                }
                                ImGui.EndTabItem();
                            }
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.Separator();
                    ImGui.TreePop();
                }

                ImGui.TreePop();

            }
            #endregion

            //Plots Widgets
            #region Plot Widgets
            if (ImGui.TreeNode("Plots Widgets"))
            {
                ImGui.Checkbox("Animate", ref animate);

                float[] arr = { 0.6f, 0.1f, 1.0f, 0.5f, 0.92f, 0.1f, 0.2f };
                ImGui.PlotLines("Frame Times", ref arr[0], arr.Length);

                if (!animate || refresh_time == 0.0)
                {
                    refresh_time = ImGui.GetTime();
                }
                while (refresh_time < ImGui.GetTime())
                {
                    values[values_offset] = (float)Math.Cos(phase);
                    values_offset = (values_offset + 1) % values.Length;
                    phase += 0.10f * values_offset;
                    refresh_time += 1.0f / 60.0f;
                }

                float average = 0.0f;
                for (int n = 0; n < values.Length; n++)
                {
                    average += values[n];
                }
                average /= (float)values.Length;
                string overlay = string.Format("avg {0}", average);
                ImGui.PlotLines("Lines", ref values[0], values.Length, values_offset, overlay, -1.0f, 1.0f, new Vec2(0, 80.0f));
                ImGui.PlotHistogram("Histogram", ref arr[0], arr.Length, 0, null, 0.0f, 1.0f, new Vec2(0, 80.0f));
                ImGui.Separator();

                if (animate)
                {
                    progress += progress_dir * 0.4f * ImGui.GetIO().DeltaTime;
                    if (progress >= +1.1f) { progress = +1.1f; progress_dir *= -1.0f; }
                    if (progress <= -0.1f) { progress = -0.1f; progress_dir *= -1.0f; }
                }

                ImGui.ProgressBar(progress, new Vec2(0.0f, 0.0f));
                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                ImGui.Text("Progress Bar");

                float progress_saturated = progress; //IM_CLAMP(progress, 0.0f, 1.0f) ?
                string buf = string.Format("{0}/{1}", (int)(progress_saturated * 1753), 1753);
                ImGui.ProgressBar(progress, new Vec2(0.0f, 0.0f), buf);
                ImGui.TreePop();
            }
            #endregion

            //Color Widgets
            #region Color Widgets
            if (ImGui.TreeNode("Color/Picker Widgets"))
            {
                ImGui.Checkbox("With Alpha Preview", ref alpha_preview);
                ImGui.Checkbox("With Half Alpha Preview", ref alpha_half_preview);
                ImGui.Checkbox("With Drag and Drop", ref drag_and_drop);
                ImGui.Checkbox("With Options Menu", ref options_menu); ImGui.SameLine(); HelpMarker("Right-click on the individual color widget to show options.");
                ImGui.Checkbox("With HDR", ref hdr); ImGui.SameLine(); HelpMarker("Currently all this does is to lift the 0..1 limits on dragging widgets.");
                ImGuiColorEditFlags misc_flags = (hdr ? ImGuiColorEditFlags.HDR : 0) | (drag_and_drop ? 0 : ImGuiColorEditFlags.NoDragDrop) | (alpha_half_preview ? ImGuiColorEditFlags.AlphaPreviewHalf : (alpha_preview ? ImGuiColorEditFlags.AlphaPreview : 0)) | (options_menu ? 0 : ImGuiColorEditFlags.NoOptions);

                ImGui.Text("Color widget:");
                ImGui.SameLine(); HelpMarker(
                    "Click on the color square to open a color picker.\n" +
                    "CTRL+click on individual component to input value.\n");
                ImGui.ColorEdit3("MyColor##1", ref color_vec3, misc_flags);

                ImGui.Text("Color widget HSV with Alpha:");
                ImGui.ColorEdit4("MyColor##2", ref color_vec4, ImGuiColorEditFlags.DisplayHSV | misc_flags);

                ImGui.Text("Color widget with Float Display:");
                ImGui.ColorEdit4("MyColor##2f", ref color_vec4, ImGuiColorEditFlags.Float | misc_flags);

                ImGui.Text("Color button with Picker:");
                ImGui.SameLine();
                HelpMarker("With the ImGuiColorEditFlags_NoInputs flag you can hide all the slider/text inputs.\n" +
                    "With the ImGuiColorEditFlags_NoLabel flag you can pass a non-empty label which will only " +
                    "be used for the tooltip and picker popup.");
                ImGui.ColorEdit4("MyColor##3", ref color_vec4, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | misc_flags);

                ImGui.Text("Color button only:");
                ImGui.ColorButton("MyColor##3c", color_vec4, misc_flags, new Vec2(80, 80));

                ImGui.Text("Color picker:");

                ImGui.Checkbox("With Alpha", ref alpha);
                ImGui.Checkbox("With Alpha Bar", ref alpha_bar);
                ImGui.Checkbox("With Side Preview", ref side_preview);
                if (side_preview)
                {
                    ImGui.SameLine();
                    ImGui.Checkbox("With Ref Color", ref ref_color);
                    if (ref_color)
                    {
                        ImGui.SameLine();
                        ImGui.ColorEdit4("##RefColor", ref ref_color_v, ImGuiColorEditFlags.NoInputs | misc_flags);
                    }
                }
                ImGui.Combo("Display Mode", ref display_mode, "Auto/Current\0None\0RGB Only\0HSV Only\0Hex Only\0");
                ImGui.SameLine();
                HelpMarker(
                    "ColorEdit defaults to displaying RGB inputs if you don't specify a display mode, " +
                    "but the user can change it with a right-click.\n\nColorPicker defaults to displaying RGB+HSV+Hex " +
                    "if you don't specify a display mode.\n\nYou can change the defaults using SetColorEditOptions().");
                ImGui.Combo("Picker Mode", ref picker_mode, "Auto/Current\0Hue bar + SV rect\0Hue wheel + SV triangle\0");
                ImGui.SameLine(); HelpMarker("User can right-click the picker to change mode.");
                ImGuiColorEditFlags flags = misc_flags;
                if (!alpha) { flags |= ImGuiColorEditFlags.NoAlpha; }
                if (alpha_bar) { flags |= ImGuiColorEditFlags.AlphaBar; }
                if (!side_preview) { flags |= ImGuiColorEditFlags.NoSidePreview; }
                if (picker_mode == 1) { flags |= ImGuiColorEditFlags.PickerHueBar; }
                if (picker_mode == 2) { flags |= ImGuiColorEditFlags.PickerHueWheel; }
                if (display_mode == 1) { flags |= ImGuiColorEditFlags.NoInputs; }
                if (display_mode == 2) { flags |= ImGuiColorEditFlags.DisplayRGB; }
                if (display_mode == 3) { flags |= ImGuiColorEditFlags.DisplayHSV; }
                if (display_mode == 4) { flags |= ImGuiColorEditFlags.DisplayHex; }
                ImGui.ColorPicker4("MyColor##4", ref color_vec4, flags, ref ref_color_v.X); // ref_color ? ref_color_v.X : null);

                ImGui.Text("Set defaults in code:");
                ImGui.SameLine(); HelpMarker("SetColorEditOptions() is designed to allow you to set boot-time default.\n" +
                    "We don't have Push/Pop functions because you can force options on a per-widget basis if needed," +
                    "and the user can change non-forced ones with the options menu.\nWe don't have a getter to avoid" +
                    "encouraging you to persistently save values that aren't forward-compatible.");
                if (ImGui.Button("Default: Uint8 + HSV + Hue Bar"))
                {
                    ImGui.SetColorEditOptions(ImGuiColorEditFlags.Uint8 | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueBar);
                }
                if (ImGui.Button("Default: Float + HDR + Hue Wheel"))
                {
                    ImGui.SetColorEditOptions(ImGuiColorEditFlags.Float | ImGuiColorEditFlags.HDR | ImGuiColorEditFlags.PickerHueWheel);
                }
                // HSV
                ImGui.Spacing();
                ImGui.Text("HSV encoded colors");
                ImGui.SameLine(); HelpMarker("By default, colors are given to ColorEdit and ColorPicker in RGB, but ImGuiColorEditFlags_InputHSV" +
                    "allows you to store colors as HSV and pass them to ColorEdit and ColorPicker as HSV. This comes with the" +
                    "added benefit that you can manipulate hue values with the picker even when saturation or value are zero.");
                ImGui.Text("Color widget with InputHSV:");
                ImGui.ColorEdit4("HSV shown as RGB##1", ref color_hsv, ImGuiColorEditFlags.DisplayRGB | ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.Float);
                ImGui.ColorEdit4("HSV shown as HSV##1", ref color_hsv, ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.Float);
                ImGui.DragFloat4("Raw HSV values", ref color_hsv, 0.01f, 0.0f, 1.0f);

                ImGui.TreePop();
            }
            #endregion

            //Range & Multi Widgets
            # region Range & Multi Widgets
            if (ImGui.TreeNode("Range Widgets"))
            {
                ImGui.DragFloatRange2("range float", ref begin, ref end, 0.25f, 0.0f, 100.0f, "Min: %.1f %%", "Max: %.1f %%");
                ImGui.DragIntRange2("range int", ref begin_i, ref end_i, 5, 0, 1000, "Min: %d units", "Max: %d units");
                ImGui.DragIntRange2("range int (no bounds)", ref begin_i, ref end_i, 5, 0, 0, "Min: %d units", "Max: %d units");
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Multi-component Widgets"))
            {
                ImGui.InputFloat2("input float2", ref vec2f);
                ImGui.DragFloat2("drag float2", ref vec2f, 0.01f, 0.0f, 1.0f);
                ImGui.SliderFloat2("slider float2", ref vec2f, 0.0f, 1.0f);
                ImGui.InputInt2("input int2", ref vec4i[0]);
                ImGui.DragInt2("drag int2", ref vec4i[0], 1, 0, 255);
                ImGui.SliderInt2("slider int2", ref vec4i[0], 0, 255);
                ImGui.Spacing();

                ImGui.InputFloat3("input float3", ref vec3f);
                ImGui.DragFloat3("drag float3", ref vec3f, 0.01f, 0.0f, 1.0f);
                ImGui.SliderFloat3("slider float3", ref vec3f, 0.0f, 1.0f);
                ImGui.InputInt3("input int3", ref vec4i[0]);
                ImGui.DragInt3("drag int3", ref vec4i[0], 1, 0, 255);
                ImGui.SliderInt3("slider int3", ref vec4i[0], 0, 255);
                ImGui.Spacing();

                ImGui.InputFloat4("input float4", ref vec4f);
                ImGui.DragFloat4("drag float4", ref vec4f);
                ImGui.SliderFloat4("slider float4", ref vec4f, 0.0f, 1.0f);
                ImGui.InputInt4("input int4", ref vec4i[0]);
                ImGui.DragInt4("drag int4", ref vec4i[0], 1, 0, 255);
                ImGui.SliderInt4("slider int4", ref vec4i[0], 0, 255);

                ImGui.TreePop();
            }
            #endregion

            //Vertical Sliders
            #region Vertical Sliders
            if (ImGui.TreeNode("Vertical Sliders"))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vec2(spacing, spacing));

                ImGui.VSliderInt("##int", new Vec2(18, 160), ref int_value, 0, 5);
                ImGui.SameLine();

                ImGui.PushID("set1");
                for (int i = 0; i < 7; i++)
                {
                    if (i > 0)
                    {
                        ImGui.SameLine();
                    }
                    ImGui.PushID(i);

                    ImGui.ColorConvertHSVtoRGB(i / 7.0f, 0.5f, 0.5f, out col_red, out col_green, out col_blue);
                    Vec4 col_slider = new Vec4(col_red, col_green, col_blue, 1.0f);
                    ImGui.ColorConvertHSVtoRGB(i / 7.0f, 0.6f, 0.5f, out col_red, out col_green, out col_blue);
                    Vec4 col_slider_hov = new Vec4(col_red, col_green, col_blue, 1.0f);
                    ImGui.ColorConvertHSVtoRGB(i / 7.0f, 0.7f, 0.5f, out col_red, out col_green, out col_blue);
                    Vec4 col_slider_act = new Vec4(col_red, col_green, col_blue, 1.0f);
                    ImGui.ColorConvertHSVtoRGB(i / 7.0f, 0.9f, 0.9f, out col_red, out col_green, out col_blue);
                    Vec4 col_slider_grab = new Vec4(col_red, col_green, col_blue, 1.0f);

                    ImGui.PushStyleColor(ImGuiCol.FrameBg, col_slider);
                    ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, col_slider_hov);
                    ImGui.PushStyleColor(ImGuiCol.FrameBgActive, col_slider_act);
                    ImGui.PushStyleColor(ImGuiCol.SliderGrab, col_slider_grab);
                    ImGui.VSliderFloat("##v", new Vec2(18, 160), ref values_vert[i], 0.0f, 1.0f, "");
                    if (ImGui.IsAnyItemActive() || ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(values_vert[i].ToString());
                    }
                    ImGui.PopStyleColor(4);
                    ImGui.PopID();
                }
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.PushID("set2");

                int rows = 3;
                Vec2 small_slider_size = new Vec2(18, 50);//(float)(int)((160.0f - (rows - 1) * spacing / rows)));
                for (int nx = 0; nx < 4; nx++)
                {
                    if (nx > 0) { ImGui.SameLine(); }
                    ImGui.BeginGroup();
                    for (int ny = 0; ny < rows; ny++)
                    {
                        ImGui.PushID(nx * rows + ny);
                        ImGui.VSliderFloat("##v", small_slider_size, ref values2[nx], 0.0f, 1.0f, "");
                        if (ImGui.IsItemActive() || ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(values2[nx].ToString());
                        }
                        ImGui.PopID();
                    }
                    ImGui.EndGroup();
                }
                ImGui.PopID();

                ImGui.SameLine();
                ImGui.PushID("set3");
                for (int i = 0; i < 4; i++)
                {
                    if (i > 0)
                    {
                        ImGui.SameLine();
                    }
                    ImGui.PushID(i);
                    ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, 40);
                    ImGui.VSliderFloat("##v", new Vec2(40, 160), ref values_vert[i], 0.0f, 1.0f, "%.2f\nsec");
                    ImGui.PopStyleVar();
                    ImGui.PopID();
                }
                ImGui.PopID();
                ImGui.PopStyleVar();
                ImGui.TreePop();
            }
            #endregion
        }

        #region DemoWindowLayoutVariables
        //Child windows
        bool disable_mouse_wheel = false;
        bool disable_menu = false;
        int offset_x = 0;
        //Widgets Width
        float f = 0.0f;
        bool show_indented_items = true;
        //Basic Horizontal Layout
        bool c1 = false;
        bool c2 = false;
        bool c3 = false;
        bool c4 = false;
        float bf0 = 1.0f;
        float bf1 = 2.0f;
        float bf2 = 3.0f;
        string[] items = { "AAAA", "BBBB", "CCCC", "DDDD" };
        int item = -1;
        int[] bselection = { 0, 1, 2, 3 };
        //Scrolling
        int track_item = 50;
        bool enable_track = true;
        bool enable_extra_decorations = false;
        float scroll_to_off_px = 0.0f;
        float scroll_to_pos_px = 200.0f;
        #endregion

        private void DemoWindowLayout()
        {
            if (!ImGui.CollapsingHeader("Layout & Scrolling"))
            {
                return;
            }

            //Child windows
            #region Child windows
            if (ImGui.TreeNode("Child windows"))
            {
                HelpMarker("Use child windows to begin into a self-contained independent scrolling/clipping regions within a host window.");

                ImGui.Checkbox("Disable Mouse Wheel", ref disable_mouse_wheel);
                ImGui.Checkbox("Disable Menu", ref disable_menu);

                //child 1
                ImGuiWindowFlags window_flags = ImGuiWindowFlags.HorizontalScrollbar;
                if (disable_mouse_wheel)
                {
                    window_flags |= ImGuiWindowFlags.NoScrollWithMouse;
                }
                Vec2 content_region_max;
                ImGui.BeginChild("ChildL", new Vec2(ImGui.GetWindowContentRegionMax().X * 0.5f, 260), ImGuiChildFlags.Border, window_flags);
                for (int i = 0; i < 100; i++)
                {
                    ImGui.Text(string.Format("{0}: scrollable region", i.ToString("D4")));
                }
                ImGui.EndChild();
                ImGui.SameLine();

                //child 2
                ImGuiWindowFlags window_flags_child2 = ImGuiWindowFlags.None;
                if (disable_mouse_wheel)
                {
                    window_flags_child2 |= ImGuiWindowFlags.NoScrollWithMouse;
                }
                if (!disable_menu)
                {
                    window_flags_child2 |= ImGuiWindowFlags.MenuBar;
                }
                ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
                ImGui.BeginChild("ChildR", new Vec2(0, 260), ImGuiChildFlags.Border, window_flags_child2);
                if (!disable_menu && ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("Menu"))
                    {
                        ShowExampleMenuFile();
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenuBar();
                }
                for (int i = 0; i < 100; i++)
                {
                    ImGui.Button(i.ToString("D3"));
                    if ((i % 2) == 0)
                    {
                        ImGui.SameLine();
                    }
                }
                ImGui.EndChild();
                ImGui.PopStyleVar();

                ImGui.Separator();

                ImGui.SetNextItemWidth(100);
                ImGui.DragInt("Offset X", ref offset_x, 1.0f, -1000, 1000);

                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (float)offset_x);
                Vec4 colv4 = new Vec4(255.0f, 0.0f, 0.0f, 100.0f);
                ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.ColorConvertFloat4ToU32(colv4));
                ImGui.BeginChild("Red", new Vec2(200, 100), ImGuiChildFlags.Border, ImGuiWindowFlags.None);
                for (int n = 0; n < 50; n++)
                {
                    ImGui.Text("Some test " + n.ToString());
                }
                ImGui.EndChild();
                bool child_is_hovered = ImGui.IsItemHovered();
                Vec2 child_rect_min = ImGui.GetItemRectMin();
                Vec2 child_rect_max = ImGui.GetItemRectMax();
                ImGui.PopStyleColor();
                ImGui.Text("Hovered: " + child_is_hovered.ToString());
                ImGui.Text(string.Format("Rect of child window is: ({0},{1}) ({2},{3})", child_rect_min.X, child_rect_min.Y, child_rect_max.X, child_rect_max.Y));

                ImGui.TreePop();
            }
            #endregion

            //Widgets Width
            #region Widgets Width
            if (ImGui.TreeNode("Widgets Width"))
            {
                ImGui.Checkbox("Show intended items", ref show_indented_items);

                ImGui.Text("SetNextItemWidth/PushItemWidth(100)");
                ImGui.SameLine(); HelpMarker("Fixed width.");
                ImGui.PushItemWidth(100);
                ImGui.DragFloat("float##1b", ref f);
                if (show_indented_items)
                {
                    ImGui.Indent();
                    ImGui.DragFloat("float (intended)##1b", ref f);
                    ImGui.Unindent();
                }
                ImGui.PopItemWidth();

                ImGui.Text("SetNextItemWidth/PushItemWidth(-100)");
                ImGui.SameLine(); HelpMarker("Align to right edge minus 100");
                ImGui.PushItemWidth(-100);
                ImGui.DragFloat("float##2a", ref f);
                if (show_indented_items)
                {
                    ImGui.Indent();
                    ImGui.DragFloat("float (indented)##2b", ref f);
                    ImGui.Unindent();
                }
                ImGui.PopItemWidth();

                ImGui.Text("SetNextItemWidth/PushItemWidth(GetContentRegionAvail().x * 0.5f)");
                ImGui.SameLine(); HelpMarker("Half of available width.\n(~ right-cursor_pos)\n(works within a column set)");
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
                ImGui.DragFloat("float##3a", ref f);
                if (show_indented_items)
                {
                    ImGui.Indent();
                    ImGui.DragFloat("float (indented)##3b", ref f);
                    ImGui.Unindent();
                }
                ImGui.PopItemWidth();

                ImGui.Text("SetNextItemWidth/PushItemWidth(-GetContentRegionAvail().x * 0.5f)");
                ImGui.SameLine(); HelpMarker("Align to right edge minus half");
                ImGui.PushItemWidth(-ImGui.GetContentRegionAvail().X * 0.5f);
                ImGui.DragFloat("float##4a", ref f);
                if (show_indented_items)
                {
                    ImGui.Indent();
                    ImGui.DragFloat("float (indented)##4b", ref f);
                    ImGui.Unindent();
                }
                ImGui.PopItemWidth();

                ImGui.TreePop();
            }
            #endregion

            //Basic Horizontal Layout
            #region Basic Horizontal Layout
            if (ImGui.TreeNode("Basic Horizontal Layout"))
            {
                ImGui.TextWrapped("(Use ImGui.SameLine() to keep adding items to the right of the preceding item)");

                // Text
                ImGui.Text("Two items: Hello"); ImGui.SameLine();
                ImGui.TextColored(new Vec4(1, 1, 0, 1), "Sailor");

                // Adjust spacing
                ImGui.Text("More spacing: Hello"); ImGui.SameLine(0, 20);
                ImGui.TextColored(new Vec4(1, 1, 0, 1), "Sailor");

                // Button
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Normal buttons"); ImGui.SameLine();
                ImGui.Button("Banana"); ImGui.SameLine();
                ImGui.Button("Apple"); ImGui.SameLine();
                ImGui.Button("Corniflower");

                // Button
                ImGui.Text("Small buttons"); ImGui.SameLine();
                ImGui.SmallButton("Like this one"); ImGui.SameLine();
                ImGui.Text("can fit within a text block.");

                // Aligned to arbitrary position. Easy/cheap column.
                ImGui.Text("Aligned");
                ImGui.SameLine(150); ImGui.Text("x=150");
                ImGui.SameLine(300); ImGui.Text("x=300");
                ImGui.Text("Aligned");
                ImGui.SameLine(150); ImGui.SmallButton("x=150");
                ImGui.SameLine(300); ImGui.SmallButton("x=300");

                // Checkbox
                ImGui.Checkbox("My", ref c1); ImGui.SameLine();
                ImGui.Checkbox("Tailor", ref c2); ImGui.SameLine();
                ImGui.Checkbox("Is", ref c3); ImGui.SameLine();
                ImGui.Checkbox("Rich", ref c4);

                // Various
                ImGui.PushItemWidth(80);
                ImGui.Combo("Combo", ref item, items, items.Length); ImGui.SameLine();
                ImGui.SliderFloat("X", ref bf0, 0.0f, 5.0f); ImGui.SameLine();
                ImGui.SliderFloat("Y", ref bf1, 0.0f, 5.0f); ImGui.SameLine();
                ImGui.SliderFloat("Z", ref bf2, 0.0f, 5.0f);
                ImGui.PopItemWidth();

                ImGui.PushItemWidth(80);
                ImGui.Text("Lists:");
                for (int i = 0; i < 4; i++)
                {
                    if (i > 0) ImGui.SameLine();
                    ImGui.PushID(i);
                    ImGui.ListBox("", ref bselection[i], items, items.Length);
                    ImGui.PopID();
                }
                ImGui.PopItemWidth();

                // Dummy
                Vec2 button_sz = new Vec2(40, 40);
                ImGui.Button("A", button_sz); ImGui.SameLine();
                ImGui.Dummy(button_sz); ImGui.SameLine();
                ImGui.Button("B", button_sz);

                // Manually wrapping
                // (we should eventually provide this as an automatic layout feature, but for now you can do it manually)
                ImGui.Text("Manually wrapping:");
                int buttons_count = 20;
                float window_visible_x2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                for (int n = 0; n < buttons_count; n++)
                {
                    ImGui.PushID(n);
                    ImGui.Button("Box", button_sz);
                    float last_button_x2 = ImGui.GetItemRectMax().X;
                    float next_button_x2 = last_button_x2 + 1.0f + button_sz.X; // Expected position if next button was on same line
                    if (n + 1 < buttons_count && next_button_x2 < window_visible_x2)
                    {
                        ImGui.SameLine();
                    }
                    ImGui.PopID();
                }

                ImGui.TreePop();
            }
            #endregion

            //Groups
            #region Groups
            if (ImGui.TreeNode("Groups"))
            {
                HelpMarker("BeginGroup() basically locks the horizontal position for new line. " +
                "EndGroup() bundles the whole group so that you can use \"item\" functions such as " +
                "IsItemHovered()/IsItemActive() or SameLine() etc. on the whole group.");
                ImGui.BeginGroup();
                {
                    ImGui.BeginGroup();
                    ImGui.Button("AAA");
                    ImGui.SameLine();
                    ImGui.Button("BBB");
                    ImGui.SameLine();
                    ImGui.BeginGroup();
                    ImGui.Button("CCC");
                    ImGui.Button("DDD");
                    ImGui.EndGroup();
                    ImGui.SameLine();
                    ImGui.Button("EEE");
                    ImGui.EndGroup();
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("First group hovered");
                    }
                }
                // Capture the group size and create widgets using the same size
                Vec2 size = ImGui.GetItemRectSize();
                float[] values = { 0.5f, 0.20f, 0.80f, 0.60f, 0.25f };
                ImGui.PlotHistogram("##values", ref values[0], values.Length, 0, null, 0.0f, 1.0f, size);

                ImGui.Button("ACTION", new Vec2((size.X - ImGui.GetStyle().ItemSpacing.X) * 0.5f, size.Y));
                ImGui.SameLine();
                ImGui.Button("REACTION", new Vec2((size.X - ImGui.GetStyle().ItemSpacing.X) * 0.5f, size.Y));
                ImGui.EndGroup();

                // This breaks tree node
                //ImGui.SameLine();
                //ImGui.Button("LEVERAGE\nBUZZWORD", size);
                //ImGui.SameLine();

                ImGui.TreePop();
            }
            #endregion

            //Text Baseline Alignment
            #region Text Baseline Alignment
            if (ImGui.TreeNode("Text Baseline Alignment"))
            {
                ImGui.BulletText("Text baseline:");
                ImGui.SameLine();
                HelpMarker("This is testing the vertical alignment that gets applied on text to keep it aligned with widgets. " +
                    "Lines only composed of text or \"small\" widgets use less vertical space than lines with framed widgets.");
                ImGui.Indent();

                ImGui.Text("KO Blahblah"); ImGui.SameLine();
                ImGui.Button("Some framed item"); ImGui.SameLine();
                HelpMarker("Baseline of button will look misaligned with text..");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("OK Blahblah"); ImGui.SameLine();
                ImGui.Button("Some framed item"); ImGui.SameLine();
                HelpMarker("We call AlignTextToFramePadding() to vertically align the text baseline by +FramePadding.y");

                // SmallButton() uses the same vertical padding as Text
                ImGui.Button("TEST##1"); ImGui.SameLine();
                ImGui.Text("TEST"); ImGui.SameLine();
                ImGui.SmallButton("TEST##2");

                // If your line starts with text, call AlignTextToFramePadding() to align text to upcoming widgets.
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Text aligned to framed item"); ImGui.SameLine();
                ImGui.Button("Item##1"); ImGui.SameLine();
                ImGui.Text("Item"); ImGui.SameLine();
                ImGui.SmallButton("Item##2"); ImGui.SameLine();
                ImGui.Button("Item##3");

                ImGui.Unindent();

                ImGui.Spacing();

                ImGui.BulletText("Multi-line text:");
                ImGui.Indent();
                ImGui.Text("One\nTwo\nThree"); ImGui.SameLine();
                ImGui.Text("Hello\nWorld"); ImGui.SameLine();
                ImGui.Text("Banana");

                ImGui.Text("Banana"); ImGui.SameLine();
                ImGui.Text("Hello\nWorld"); ImGui.SameLine();
                ImGui.Text("One\nTwo\nThree");

                ImGui.Button("HOP##1"); ImGui.SameLine();
                ImGui.Text("Banana"); ImGui.SameLine();
                ImGui.Text("Hello\nWorld"); ImGui.SameLine();
                ImGui.Text("Banana");

                ImGui.Button("HOP##2"); ImGui.SameLine();
                ImGui.Text("Hello\nWorld"); ImGui.SameLine();
                ImGui.Text("Banana");
                ImGui.Unindent();

                ImGui.Spacing();

                ImGui.BulletText("Misc items:");
                ImGui.Indent();

                // SmallButton() sets FramePadding to zero. Text baseline is aligned to match baseline of previous Button.
                ImGui.Button("80x80", new Vec2(80, 80));
                ImGui.SameLine();
                ImGui.Button("50x50", new Vec2(50, 50));
                ImGui.SameLine();
                ImGui.Button("Button()");
                ImGui.SameLine();
                ImGui.SmallButton("SmallButton()");

                // Tree
                float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
                ImGui.Button("Button##1");
                ImGui.SameLine(0.0f, spacing);
                if (ImGui.TreeNode("Node##1"))
                {
                    // Placeholder tree data
                    for (int i = 0; i < 6; i++)
                        ImGui.BulletText(string.Format("Item {0}..", i));
                    ImGui.TreePop();
                }

                // Vertically align text node a bit lower so it'll be vertically centered with upcoming widget.
                // Otherwise you can use SmallButton() (smaller fit).
                ImGui.AlignTextToFramePadding();

                // Common mistake to avoid: if we want to SameLine after TreeNode we need to do it before we add
                // other contents below the node.
                bool node_open = ImGui.TreeNode("Node##2");
                ImGui.SameLine(0.0f, spacing); ImGui.Button("Button##2");
                if (node_open)
                {
                    // Placeholder tree data
                    for (int i = 0; i < 6; i++)
                        ImGui.BulletText(string.Format("Item {0}..", i));
                    ImGui.TreePop();
                }

                // Bullet
                ImGui.Button("Button##3");
                ImGui.SameLine(0.0f, spacing);
                ImGui.BulletText("Bullet text");

                ImGui.AlignTextToFramePadding();
                ImGui.BulletText("Node");
                ImGui.SameLine(0.0f, spacing); ImGui.Button("Button##4");
                ImGui.Unindent();

                ImGui.TreePop();
            }
            #endregion

            //Scrolling
            #region Scrolling
            if (ImGui.TreeNode("Scrolling"))
            {
                //vertical
                HelpMarker("Use SetScrollHereY() or SetScrollFromPosY() to scroll to a given vertical position.");

                ImGui.Checkbox("Decoration", ref enable_extra_decorations);

                ImGui.Checkbox("Track", ref enable_track);
                ImGui.PushItemWidth(100);

                ImGui.SameLine(140); enable_track |= ImGui.DragInt("##item", ref track_item, 0.25f, 0, 99, "Item = %d");

                bool scroll_to_off = ImGui.Button("Scroll Offset");
                ImGui.SameLine(140); scroll_to_off |= ImGui.DragFloat("##off", ref scroll_to_off_px, 1.00f, 0, 100.0f, "+%.0f px");

                bool scroll_to_pos = ImGui.Button("Scroll To Pos");
                ImGui.SameLine(140); scroll_to_pos |= ImGui.DragFloat("##pos", ref scroll_to_pos_px, 1.00f, -10, 100.0f, "X/Y = %.0f px");
                ImGui.PopItemWidth();

                if (scroll_to_off || scroll_to_pos)
                {
                    enable_track = false;
                }

                float child_w = (ImGui.GetContentRegionAvail().X - 4 * 1.0f) / 5;
                if (child_w < 1.0f)
                {
                    child_w = 1.0f;
                }
                ImGui.PushID("##VerticalScrolling");
                for (int i = 0; i < 5; i++)
                {
                    if (i > 0) { ImGui.SameLine(); }
                    ImGui.BeginGroup();
                    string[] names = { "Top", "25%", "Center", "75%", "Bottom" };
                    ImGui.TextUnformatted(names[i]);

                    ImGuiWindowFlags child_flags = enable_extra_decorations ? ImGuiWindowFlags.MenuBar : 0;
                    string child_id = names[i];
                    bool child_is_visible = ImGui.BeginChild(child_id, new Vec2(child_w, 200.0f), ImGuiChildFlags.Border, child_flags);
                    if (ImGui.BeginMenuBar())
                    {
                        ImGui.TextUnformatted("abc");
                        ImGui.EndMenuBar();
                    }
                    if (scroll_to_off)
                    {
                        ImGui.SetScrollY(scroll_to_off_px);
                    }
                    if (scroll_to_pos)
                    {
                        ImGui.SetScrollFromPosY(ImGui.GetCursorStartPos().Y + scroll_to_pos_px, i * 0.25f);
                    }
                    if (child_is_visible)
                    {
                        for (int item = 0; item < 100; item++)
                        {
                            if (enable_track && item == track_item)
                            {
                                ImGui.TextColored(new Vec4(1, 1, 0, 1), "Item " + item);
                                ImGui.SetScrollHereY(i * 0.25f);
                            }
                            else
                            {
                                ImGui.Text("Item " + item);
                            }
                        }
                    }
                    float scroll_y = ImGui.GetScrollY();
                    float scroll_max_y = ImGui.GetScrollMaxY();
                    ImGui.EndChild();
                    ImGui.Text(scroll_y + "/" + scroll_max_y);
                    ImGui.EndGroup();

                }
                ImGui.PopID();

                //horizontal
                ImGui.Spacing();
                HelpMarker("Use SetScrollHereX() or SetScrollFromPosX() to scroll to a given horizontal position.\n\n" +
                    "Because the clipping rectangle of most window hides half worth of WindowPadding on the " +
                    "left/right, using SetScrollFromPosX(+1) will usually result in clipped text whereas the " +
                    "equivalent SetScrollFromPosY(+1) wouldn't.");
                ImGui.PushID("##HorizontalScrolling");
                for (int i = 0; i < 5; i++)
                {
                    float child_height = ImGui.GetTextLineHeight() + 30.0f;
                    ImGuiWindowFlags child_flags = ImGuiWindowFlags.HorizontalScrollbar | (enable_extra_decorations ? ImGuiWindowFlags.AlwaysVerticalScrollbar : 0);
                    string[] names = { "Left", "25%", "Center", "75%", "Right" };
                    string child_id = names[i];
                    bool child_is_visible = ImGui.BeginChild(child_id, new Vec2(-100, child_height), ImGuiChildFlags.Border, child_flags);
                    if (scroll_to_off)
                    {
                        ImGui.SetScrollX(scroll_to_off_px);
                    }
                    if (scroll_to_pos)
                    {
                        ImGui.SetScrollFromPosX(ImGui.GetCursorStartPos().X + scroll_to_pos_px, i * 0.25f);
                    }
                    if (child_is_visible)
                    {
                        for (int item = 0; item < 100; item++)
                        {
                            if (enable_track && item == track_item)
                            {
                                ImGui.TextColored(new Vec4(1, 1, 0, 1), "Item " + item);
                                ImGui.SetScrollHereX(i * 0.25f);
                            }
                            else
                            {
                                ImGui.Text("Item " + item);
                            }
                            ImGui.SameLine();
                        }
                    }
                    float scroll_x = ImGui.GetScrollX();
                    float scroll_max_x = ImGui.GetScrollMaxX();
                    ImGui.EndChild();
                    ImGui.SameLine();
                    ImGui.Text(string.Format("{0}\n{1}/{2}", names[i], scroll_x, scroll_max_x));
                    ImGui.Spacing();
                }

                ImGui.TreePop();
            }
            #endregion

        }

        #region DemoWindowPopupsVariables
        //Popups
        int selected_fish = -1;
        bool[] toggles = { true, false, false, false, false };
        //Context menus
        float value = 0.5f;
        string name = "Label1";
        //Modals
        bool show = false;
        bool show_stacked = false;
        bool dont_ask_me_next_time = false;
        int item_mod = 1;
        Vec4 color = new Vec4(0.4f, 0.7f, 0.0f, 0.5f);
        #endregion

        private void DemoWindowPopups()
        {
            if (!ImGui.CollapsingHeader("Popups & Modal windows"))
            {
                return;
            }

            //Popups
            #region Popups
            if (ImGui.TreeNode("Popups"))
            {
                ImGui.TextWrapped("When a popup is active, it inhibits interacting with windows that are behind the popup. " +
                "Clicking outside the popup closes it.");

                string[] names = { "Bream", "Haddock", "Mackerel", "Pollock", "Tilefish" };

                if (ImGui.Button("Select.."))
                {
                    ImGui.OpenPopup("my_select_popup");
                }
                ImGui.SameLine();
                ImGui.TextUnformatted(selected_fish == -1 ? "<None>" : names[selected_fish]);
                if (ImGui.BeginPopup("my_select_popup"))
                {
                    ImGui.Text("Aquarium");
                    ImGui.Separator();
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (ImGui.Selectable(names[i]))
                        {
                            selected_fish = i;
                        }
                    }
                    ImGui.EndPopup();
                }

                //menu with toggles
                if (ImGui.Button("Toggle.."))
                {
                    ImGui.OpenPopup("my_toggle_popup");
                }
                if (ImGui.BeginPopup("my_toggle_popup"))
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        ImGui.MenuItem(names[i], "", ref toggles[i]);
                    }
                    if (ImGui.BeginMenu("Sub-menu"))
                    {
                        ImGui.MenuItem("Click me");
                        ImGui.EndMenu();
                    }

                    ImGui.Separator();
                    ImGui.Text("Tooltip here");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("I am a tooltip over a popup");
                    }

                    if (ImGui.Button("Stacked Popup"))
                    {
                        ImGui.OpenPopup("another popup");
                    }
                    if (ImGui.BeginPopup("another popup"))
                    {
                        for (int i = 0; i < names.Length; i++)
                        {
                            ImGui.MenuItem(names[i], "", ref toggles[i]);
                        }
                        if (ImGui.BeginMenu("Sub-menu"))
                        {
                            ImGui.MenuItem("Click me");
                            if (ImGui.Button("Stacked Popup"))
                            {
                                ImGui.OpenPopup("another popup");
                            }
                            if (ImGui.BeginPopup("another popup"))
                            {
                                ImGui.Text("I am the last one here.");
                                ImGui.EndPopup();
                            }
                            ImGui.EndMenu();
                        }
                        ImGui.EndPopup();
                    }
                    ImGui.EndPopup();
                }

                if (ImGui.Button("File Menu.."))
                {
                    ImGui.OpenPopup("my_file_popup");
                }
                if (ImGui.BeginPopup("my_file_popup"))
                {
                    ShowExampleMenuFile();
                    ImGui.EndPopup();
                }

                ImGui.TreePop();
            }
            #endregion

            //Context menus
            #region Context menus
            if (ImGui.TreeNode("Context menus"))
            {
                ImGui.Text(string.Format("Value = {0} (<-- right-click here)", value));
                if (ImGui.BeginPopupContextItem("item context menu"))
                {
                    if (ImGui.Selectable("Set to zero")) { value = 0.0f; }
                    if (ImGui.Selectable("Set to PI")) { value = 3.1415f; }
                    ImGui.SetNextItemWidth(-1.0f);
                    ImGui.DragFloat("##Value", ref value, 0.1f, 0.0f, 0.0f);
                    ImGui.EndPopup();
                }

                ImGui.Text("(You can also right-click me to open the same popup as above.)");
                ImGui.OpenPopupOnItemClick("item context menu", ImGuiPopupFlags.MouseButtonRight);

                ImGui.Button(string.Format("Button: {0}###Button", name));
                if (ImGui.BeginPopupContextItem())
                {
                    ImGui.Text("Edit name:");
                    ImGui.InputText("##edit", ref name, 100);
                    if (ImGui.Button("Close"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
                ImGui.SameLine(); ImGui.Text("(<-- right-click here)");

                ImGui.TreePop();
            }
            #endregion

            //Modals
            #region Modals
            if (ImGui.TreeNode("Modals"))
            {
                ImGui.TextWrapped("Modal windows are like popups but the user cannot close them by clicking outside.");

                if (ImGui.Button("Delete.."))
                {
                    ImGui.OpenPopup("Delete?");
                    show = true;
                }

                Vec2 center = new Vec2(400, 400);
                ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vec2(0.5f, 0.5f));

                if (ImGui.BeginPopupModal("Delete?", ref show, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Text("All those beautiful files will be deleted.\nThis operation cannot be undone!\n\n");
                    ImGui.Separator();

                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vec2(0, 0));
                    ImGui.Checkbox("Don't ask me next time", ref dont_ask_me_next_time);
                    ImGui.PopStyleVar();

                    if (ImGui.Button("OK", new Vec2(120, 0))) { ImGui.CloseCurrentPopup(); }
                    ImGui.SetItemDefaultFocus();
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel", new Vec2(120, 0))) { ImGui.CloseCurrentPopup(); }
                    ImGui.EndPopup();
                }

                if (ImGui.Button("Stacked modals.."))
                {
                    ImGui.OpenPopup("Stacked 1");
                    show_stacked = true;
                }
                if (ImGui.BeginPopupModal("Stacked 1", ref show_stacked, ImGuiWindowFlags.MenuBar))
                {
                    if (ImGui.BeginMenuBar())
                    {
                        if (ImGui.BeginMenu("File"))
                        {
                            if (ImGui.MenuItem("Some menu item")) { }
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenuBar();
                    }
                    ImGui.Text("Hello from Stacked The First\nUsing style.Colors[ImGuiCol_ModalWindowDimBg] behind it.");

                    ImGui.Combo("Combo", ref item_mod, "aaaa\0bbbb\0cccc\0dddd\0eeee\0\0");
                    ImGui.ColorEdit4("color", ref color);

                    if (ImGui.Button("Add another modal.."))
                    {
                        ImGui.OpenPopup("Stacked 2");
                    }

                    bool unused_open = true;
                    if (ImGui.BeginPopupModal("Stacked 2", ref unused_open))
                    {
                        ImGui.Text("Hello from Stacked The Second!");
                        if (ImGui.Button("Close"))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                        ImGui.EndPopup();
                    }

                    if (ImGui.Button("Close"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
                ImGui.TreePop();
            }
            #endregion

        }

        #region DemoWindowMiscVariables
        string buf = "hello";
        #endregion

        private void DemoWindowMisc()
        {
            if (ImGui.CollapsingHeader("Inputs, Navigation & Focus"))
            {
                ImGuiIOPtr io = ImGui.GetIO();

                ImGui.Text(string.Format("WantCaptureMouse: {0}", io.WantCaptureMouse));
                ImGui.Text(string.Format("WantCaptureKeyboard: {0}", io.WantCaptureKeyboard));
                ImGui.Text(string.Format("WantTextInput: {0}", io.WantTextInput));
                ImGui.Text(string.Format("WantSetMousePos: {0}", io.WantSetMousePos));
                ImGui.Text(string.Format("NavActive: {0}, NavVisible: {1}", io.NavActive, io.NavVisible));

                //keyboard mouse state
                if (ImGui.TreeNode("Keyboard, Mouse & Navigation State"))
                {
                    if (ImGui.IsMousePosValid())
                    {
                        ImGui.Text(string.Format("Mouse pos: ({0}, {1})", io.MousePos.X, io.MousePos.Y));
                    }
                    else
                    {
                        ImGui.Text("Mouse pos: <INVALID>");
                    }
                    ImGui.Text(string.Format("Mouse delta: ({0}, {1})", io.MouseDelta.X, io.MouseDelta.Y));
                    ImGui.Text("Mouse down:"); for (int i = 0; i < io.MouseDown.Count; i++) if (io.MouseDownDuration[i] >= 0.0f) { ImGui.SameLine(); ImGui.Text(string.Format("{0} ({1} secs)", i, io.MouseDownDuration[i])); }
                    ImGui.Text("Mouse clicked:"); for (int i = 0; i < io.MouseDown.Count; i++) if (ImGui.IsMouseClicked((ImGuiNET.ImGuiMouseButton)i)) { ImGui.SameLine(); ImGui.Text(i.ToString()); }
                    ImGui.Text("Mouse dblclick:"); for (int i = 0; i < io.MouseDown.Count; i++) if (ImGui.IsMouseDoubleClicked((ImGuiNET.ImGuiMouseButton)i)) { ImGui.SameLine(); ImGui.Text(i.ToString()); }
                    ImGui.Text("Mouse released:"); for (int i = 0; i < io.MouseDown.Count; i++) if (ImGui.IsMouseReleased((ImGuiNET.ImGuiMouseButton)i)) { ImGui.SameLine(); ImGui.Text(i.ToString()); }
                    ImGui.Text(string.Format("Mouse wheel: {0}", io.MouseWheel));

// TODO: Not Supported
                    /*
                    ImGui.Text("Keys down:"); for (int i = 0; i < io.KeysDown.Count; i++) if (io.KeysDownDuration[i] >= 0.0f) { ImGui.SameLine(); ImGui.Text(string.Format("{0} ({1}) ({2} secs)", i, i, io.KeysDownDuration[i])); }
                    ImGui.Text("Keys pressed:"); for (int i = 0; i < io.KeysDown.Count; i++) if (ImGui.IsKeyPressed((ImGuiNET.ImGuiKey)i)) { ImGui.SameLine(); ImGui.Text(string.Format("{0} ({1})", i, i)); }
                    ImGui.Text("Keys release:"); for (int i = 0; i < io.KeysDown.Count; i++) if (ImGui.IsKeyReleased((ImGuiNET.ImGuiKey)i)) { ImGui.SameLine(); ImGui.Text(string.Format("{0} ({1})", i, i)); }
                    ImGui.Text(string.Format("Keys mods: {0}{1}{2}{3}", io.KeyCtrl ? "CTRL " : "", io.KeyShift ? "SHIFT " : "", io.KeyAlt ? "ALT " : "", io.KeySuper ? "SUPER " : ""));
                    ImGui.Text("Chars queue:"); for (int i = 0; i < io.InputQueueCharacters.Size; i++) { ushort c = io.InputQueueCharacters[i]; ImGui.SameLine(); ImGui.Text(string.Format("{0} {1}", (c > ' ' && c <= 255) ? (char)c : '?', c)); } // FIXME: Does not show chars as in example

                    ImGui.Text("NavInputs down:"); for (int i = 0; i < io.NavInputs.Count; i++) if (io.NavInputs[i] > 0.0f) { ImGui.SameLine(); ImGui.Text(string.Format("[{0}] {1}", i, io.NavInputs[i])); }
                    ImGui.Text("NavInputs pressed:"); for (int i = 0; i < io.NavInputs.Count; i++) if (io.NavInputsDownDuration[i] == 0.0f) { ImGui.SameLine(); ImGui.Text(string.Format("[{0}]", i)); }
                    ImGui.Text("NavInputs duration:"); for (int i = 0; i < io.NavInputs.Count; i++) if (io.NavInputsDownDuration[i] >= 0.0f) { ImGui.SameLine(); ImGui.Text(string.Format("[{0}] {1}", i, io.NavInputsDownDuration[i])); }
                    */

                    
                    ImGui.Button("Hovering me sets the\nkeyboard capture flag");
                    if (ImGui.IsItemHovered())
                    {
// TODO: Not Supported
                    }
                    ImGui.SameLine();
                    ImGui.Button("Holding me clears the\nthe keyboard capture flag");
                    if (ImGui.IsItemActive())
                    { }
// TODO: Not Supported
//                        ImGui.CaptureKeyboardFromApp(true);
                    }
                    ImGui.SameLine();
                    ImGui.Button("Holding me clears the\nthe keyboard capture flag");
                    if (ImGui.IsItemActive())
                    {
  //                      ImGui.CaptureKeyboardFromApp(false);
                    }
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Tabbing"))
                {
                    ImGui.Text("Use TAB/SHIFT+TAB to cycle through keyboard editable fields.");
                    ImGui.InputText("1", ref buf, 100);
                    ImGui.InputText("2", ref buf, 100);
                    ImGui.InputText("3", ref buf, 100);
// TODO: Not Supported
//                    ImGui.PushAllowKeyboardFocus(false);
                    ImGui.InputText("4 (tab skip)", ref buf, 100);
// TODO: Not Supported
//                    ImGui.PopAllowKeyboardFocus();
                    ImGui.InputText("5", ref buf, 100);
                    ImGui.TreePop();
                }
            }
 

            #region DrawMonoGameWindowVariables
            bool show_main_window = true;
            bool exit_app = false;
            int current_res = 0;
            int select_res = 0;
            int render_model = 1;
            string[] resolution = { "1024x768", "1280x720", "1280x960", "1366x768", "1440x1080", "1680x1050", "1600x1200", "1920x1080" };
            Vec4 monogame_color = new Vec4(231.0f / 255.0f, 60.0f / 255.0f, 0.0f / 255.0f, 200.0f / 255.0f);
            Vec4 monogame_framebg = new Vec4(227.0f / 255.0f, 227.0f / 255.0f, 227.0f / 255.0f, 255.0f / 255.0f);
            Vec4 color_black = new Vec4(0.0f / 255.0f, 0.0f / 0.0f, 0.0f / 255.0f, 200.0f / 255.0f);
            Vec4 color_white = new Vec4(255.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f, 242.0f / 255.0f);
            #endregion

            private void DrawMonoGameWindow()
            {
                ImGui.SetNextWindowSize(new Vec2(300, 220), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new Vec2(10, 140), ImGuiCond.FirstUseEver);

                ImGui.PushStyleColor(ImGuiCol.WindowBg, color_white);
                ImGui.PushStyleColor(ImGuiCol.FrameBg, monogame_framebg);
                ImGui.PushStyleColor(ImGuiCol.PopupBg, monogame_framebg);
                ImGui.PushStyleColor(ImGuiCol.MenuBarBg, monogame_framebg);
                ImGui.PushStyleColor(ImGuiCol.TitleBg, monogame_color);
                ImGui.PushStyleColor(ImGuiCol.TitleBgActive, monogame_color);
                ImGui.PushStyleColor(ImGuiCol.Button, monogame_color);

                if (!ImGui.Begin("MonoGame Settings", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.MenuBar))
                {
                    ImGui.End();
                    return;
                }

                ImGui.PushStyleColor(ImGuiCol.Text, color_black);

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("Options"))
                    {
                        ImGui.MenuItem("Exit App", null, ref exit_app);

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("ImGui"))
                    {
                        ImGui.MenuItem("Show Demo Window", null, ref show_main_window);

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }

                if (ImGui.SmallButton("Close"))
                {
                    show_monogame_settings = false;

                    if (show_main_window == false)
                    {
                        show_main_window = true;
                    }
                }

                ImGui.Separator();

                //ImGui.ColorEdit4("MG", ref monogame_color, ImGuiColorEditFlags.DisplayHSV);
                //ImGui.Text(monogame_color.ToString());

                ImGui.Combo("Window Size", ref select_res, resolution, resolution.Length);

                if (current_res != select_res && WasResized == false)
                {
                    WasResized = true;
                }

                ImGui.PushStyleColor(ImGuiCol.CheckMark, monogame_color);

                ImGui.RadioButton("No render", ref render_model, 0);
                ImGui.SameLine();
                ImGui.RadioButton("Render model", ref render_model, 1);

                ImGui.Separator();

                ImGui.Text("Camera");
                ImGui.PushButtonRepeat(true);

                ImGui.Indent();
                if (ImGui.ArrowButton("up", ImGuiDir.Up)) //ImGui.Button("Up"))
                {
                    world = world * Matrix.CreateRotationX(0.1f);
                }
                ImGui.Unindent();
                if (ImGui.ArrowButton("left", ImGuiDir.Left)) //ImGui.Button("Left"))
                {
                    world = world * Matrix.CreateRotationY(-0.1f);
                }
                ImGui.SameLine();
                ImGui.Text(" ");
                ImGui.SameLine();
                if (ImGui.ArrowButton("right", ImGuiDir.Right)) //ImGui.Button("Right"))
                {
                    world = world * Matrix.CreateRotationY(0.1f);
                }

                ImGui.Indent();
                if (ImGui.ArrowButton("down", ImGuiDir.Down)) //ImGui.Button("Down"))
                {
                    world = world * Matrix.CreateRotationX(-0.1f);
                }
                ImGui.Unindent();
                ImGui.PopButtonRepeat();

                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();

                ImGui.End();

                if (exit_app)
                {
                    this.Exit();
                }
            }
        }
}
