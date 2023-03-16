using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace SharpMIDI
{
    public class Window : GameWindow
    {
        ImGuiController _controller;
        public static int threshold = 10;
        public static int buffersize = 536862720;
        public Window() : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i(640,480), APIVersion = new Version(3, 3) })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            Title = Starter.name+" - OpenGL " + GL.GetString(StringName.Version);

            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }

        static LangObj langObj = Lang.GetLang("English");

        static List<string> winMMDevices = WinMM.GetDevices();

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _controller.Update(this, (float)e.Time);

            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            //ImGui.ShowDemoWindow();
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu(langObj.Menus[0]))
                {
                    if (ImGui.MenuItem(langObj.Menus[1], !TopLoader.MIDILoaded))
                    {
                        if (TopLoader.OpenDialog() == DialogResult.OK)
                        {
                            Task.Run(() => TopLoader.StartLoading());
                        }
                    }
                    if (ImGui.MenuItem(langObj.Menus[2], TopLoader.MIDILoaded))
                    {
                        MessageBox.Show(langObj.Menus[3]);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu(langObj.Menus[4]))
                {
                    if (ImGui.BeginMenu(langObj.Menus[5]))
                    {
                        if (ImGui.MenuItem(langObj.Menus[6], !Sound.synthLoaded))
                        {
                            Sound.Init(1, "");
                        }
                        if (ImGui.BeginMenu(langObj.Menus[7], !Sound.synthLoaded))
                        {
                            foreach (string i in winMMDevices)
                            {
                                if (ImGui.MenuItem(i))
                                {
                                    Sound.Init(2, i);
                                }
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.MenuItem(langObj.Menus[8], !Sound.synthLoaded))
                        {
                            Sound.Init(3, "");
                        }
                        if (ImGui.MenuItem(langObj.Menus[9], Sound.synthLoaded && !MIDIClock.test.IsRunning && !MIDIPlayer.playing))
                        {
                            Sound.Close();
                        }
                        if (ImGui.MenuItem(langObj.Menus[10], Sound.synthLoaded && !MIDIClock.test.IsRunning && !MIDIPlayer.playing))
                        {
                            Sound.Reload();
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu(langObj.Menus[19]))
                    {
                        ImGui.SliderInt(langObj.Menus[20], ref threshold, 0, 127);
                        ImGui.SliderInt(langObj.Menus[21], ref buffersize, 8192, 2147483647);
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu(langObj.Menus[15]))
                    {
                        ImGui.Checkbox(langObj.Menus[11], ref MIDIPlayer.limitFPS);
                        ImGui.Checkbox(langObj.Menus[18], ref MIDIPlayer.accurateLimit);
                        ImGui.InputInt(langObj.Menus[17], ref MIDIPlayer.targetFPS);
                        if(MIDIPlayer.targetFPS <= 0)
                        {
                            MIDIPlayer.targetFPS = 1;
                        }
                        ImGui.Checkbox(langObj.Menus[16], ref MIDIClock.throttle);
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu(langObj.Menus[14]))
                {
                    if (ImGui.MenuItem("English"))
                    {
                        langObj = Lang.GetLang("English");
                    }
                    if (ImGui.MenuItem("Svenska"))
                    {
                        langObj = Lang.GetLang("Swedish");
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu(langObj.Menus[12]))
                {
                    if (ImGui.MenuItem(langObj.Menus[13]))
                    {
                        
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0,18));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(this.Size.X, this.Size.Y-18));
            if (ImGui.Begin("StatsWindow", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoNavFocus))
            {
                switch (Sound.engine)
                {
                    case 0:
                        ImGui.Text(langObj.Other[0]+": None");
                        break;
                    case 1:
                        ImGui.Text(langObj.Other[0] + ": KDMAPI");
                        break;
                    case 2:
                        ImGui.Text(langObj.Other[0] + ": WinMM (" + Sound.lastWinMMDevice + ")");
                        break;
                    case 3:
                        ImGui.Text(langObj.Other[0] + ": XSynth");
                        break;
                }
                if (TopLoader.path == "")
                {
                    ImGui.Text("\n" + langObj.Other[1] +": None");
                }
                else
                {
                    ImGui.Text("\n" + langObj.Other[1] + ": " + Path.GetFileName(TopLoader.path));
                }
                if (TopLoader.MIDILoading)
                {
                    ImGui.Text(langObj.Other[2]);
                }
                else if (!TopLoader.MIDILoaded)
                {
                    ImGui.Text(langObj.Other[3]);
                }
                else
                {
                    ImGui.Text(langObj.Other[4]);
                }
                if (TopLoader.MIDILoaded)
                {
                    ImGui.Text(langObj.Other[5]+": " + (GC.GetTotalMemory(false) / 1000000) + " MB");
                }
                ImGui.Text(langObj.Other[6]+": " + MIDIData.notes + " / " + MIDIData.maxNotes);
                ImGui.Text(langObj.Other[7] + ": " + MIDIData.ppq);
                ImGui.Text(langObj.Other[8] + ": " + MIDIData.realTracks);
                ImGui.Text("\n"+ langObj.Other[9] + ": "+MIDIPlayer.FPS);
                ImGui.Text(langObj.Other[10] + ": " + Sound.totalEvents+" / "+MIDIData.events);
                ImGui.Text(langObj.Other[11] + ": " + MIDIPlayer.curTick+" / "+MIDIData.maxTick);
                ImGui.Text(langObj.Other[12] + ": " + MIDIPlayer.TPS);
                ImGui.Text(langObj.Other[13] + ": " + MIDIClock.bpm+"");
                bool endDisable = false;
                ImGui.SliderFloat(langObj.Other[20], ref MIDIClock.speed,0.5f, 2f);
                if (MIDIPlayer.playing || !TopLoader.MIDILoaded)
                {
                    ImGui.BeginDisabled();
                    endDisable = true;
                }
                if (ImGui.Button(langObj.Other[14]))
                {
                    if (Sound.engine != 0)
                    {
                        Sound.totalEvents = 0;
                        MIDIPlayer.playing = true;
                        Task.Run(() => MIDIPlayer.StartPlayback());
                    } else
                    {
                        MessageBox.Show(langObj.Other[15]);
                    }
                }
                if (endDisable)
                {
                    ImGui.EndDisabled();
                }
                string name = langObj.Other[16];
                if (!MIDIClock.test.IsRunning && MIDIPlayer.playing)
                {
                    name = langObj.Other[17];
                }
                bool endDisable3 = false;
                if (MIDIPlayer.stopping || !TopLoader.MIDILoaded)
                {
                    ImGui.BeginDisabled();
                    endDisable3 = true;
                }
                if (ImGui.Button(name))
                {
                    if (!MIDIClock.test.IsRunning)
                    {
                        MIDIClock.test.Start();
                    } else
                    {
                        MIDIClock.test.Stop();
                    }
                }
                if (endDisable3)
                {
                    ImGui.EndDisabled();
                }
                bool endDisable2 = false;
                if (MIDIPlayer.stopping || !TopLoader.MIDILoaded || !MIDIPlayer.playing)
                {
                    ImGui.BeginDisabled();
                    endDisable2 = true;
                }
                if (ImGui.Button(langObj.Other[18]))
                {
                    MIDIPlayer.stopping = true;
                }
                if (endDisable2)
                {
                    ImGui.EndDisabled();
                }
                ImGui.End();
            }
            if (TopLoader.MIDILoading)
            {
                ImGui.SetNextWindowPos(new System.Numerics.Vector2(120, 140));
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200));
                if (ImGui.Begin(langObj.Other[19], ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoCollapse|ImGuiWindowFlags.NoMove))
                {
                    ImGui.Text(TopLoader.LoadStatus);
                    ImGui.ProgressBar(TopLoader.LoadProgress,new System.Numerics.Vector2(385f,0f));
                    ImGui.Text(langObj.Other[5]+": " +(GC.GetTotalMemory(false)/1000000)+" MB");
                    ImGui.End();
                }
            }

            _controller.Render();

            ImGuiController.CheckGLError("End of frame");

            SwapBuffers();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);


            _controller.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _controller.MouseScroll(e.Offset);
        }
    }
}
