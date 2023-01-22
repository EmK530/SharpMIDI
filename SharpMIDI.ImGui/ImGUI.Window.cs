using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Graphics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using OpenTK.Platform.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace SharpMIDI
{
    public class Window : GameWindow
    {
        ImGuiController _controller;

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
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open MIDI"))
                    {
                        TopLoader.OpenDialog();
                        Task.Run(() => TopLoader.StartLoading());
                    }
                    if (ImGui.MenuItem("Unload MIDI", TopLoader.MIDILoaded))
                    {
                        MessageBox.Show("Not implemented");
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Options"))
                {
                    if (ImGui.BeginMenu("Synth", !Sound.synthLoaded))
                    {
                        if (ImGui.MenuItem("Load KDMAPI"))
                        {
                            Sound.Init(1, "");
                        }
                        if (ImGui.BeginMenu("Load WinMM"))
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
                        if (ImGui.MenuItem("Load XSynth"))
                        {
                            Sound.Init(3, "");
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.MenuItem("Unload Synth",Sound.synthLoaded))
                    {
                        Sound.Close();
                    }
                    if (ImGui.MenuItem("Reload Synth",Sound.synthLoaded))
                    {
                        Sound.Reload();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Help"))
                {
                    if (ImGui.MenuItem("XSynth DLL"))
                    {
                        MessageBox.Show("Trying to make XSynth work? Make sure your DLL is named XSynth.dll and placed in either the program directory or the Windows folder.");
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0,18));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(this.Size.X, this.Size.Y-18));
            if (ImGui.Begin("StatsWindow", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize))
            {
                switch (Sound.engine)
                {
                    case 0:
                        ImGui.Text("Loaded Synth: None");
                        break;
                    case 1:
                        ImGui.Text("Loaded Synth: KDMAPI");
                        break;
                    case 2:
                        ImGui.Text("Loaded Synth: WinMM (" + Sound.lastWinMMDevice + ")");
                        break;
                    case 3:
                        ImGui.Text("Loaded Synth: XSynth");
                        break;
                }
                if (TopLoader.path == "")
                {
                    ImGui.Text("\nLoaded MIDI: None");
                }
                else
                {
                    ImGui.Text("\nLoaded MIDI: " + Path.GetFileName(TopLoader.path));
                }
                if (TopLoader.MIDILoading)
                {
                    ImGui.Text("Status: Loading");
                }
                else if (!TopLoader.MIDILoaded)
                {
                    ImGui.Text("Status: Idle");
                }
                else
                {
                    ImGui.Text("Status: Loaded");
                }
                if (TopLoader.MIDILoaded)
                {
                    ImGui.Text("Memory Usage: " + (GC.GetTotalMemory(false) / 1000000) + " MB");
                }
                ImGui.Text("Notes: " + MIDIData.notes + " / " + MIDIData.maxNotes);
                ImGui.Text("PPQ: " + MIDIData.ppq);
                ImGui.Text("Tracks: " + MIDIData.realTracks);
                ImGui.Text("\nAvg FPS: "+MIDIPlayer.FPS);
                ImGui.Text("Played events: "+Sound.totalEvents+" / "+MIDIData.events);
                ImGui.Text("Tick: "+MIDIPlayer.curTick+" / "+MIDIData.maxTick);
                ImGui.Text("TPS: "+MIDIPlayer.TPS);
                ImGui.Text("BPM: "+MIDIClock.bpm+"");
                bool endDisable = false;
                if (MIDIPlayer.playing || !TopLoader.MIDILoaded)
                {
                    ImGui.BeginDisabled();
                    endDisable = true;
                }
                if (ImGui.Button("Run"))
                {
                    if (Sound.engine != 0)
                    {
                        Sound.totalEvents = 0;
                        MIDIPlayer.playing = true;
                        Task.Run(() => MIDIPlayer.StartPlayback());
                    } else
                    {
                        MessageBox.Show("Please select a synth in Options -> Synth before starting!");
                    }
                }
                if (endDisable)
                {
                    ImGui.EndDisabled();
                }
                string name = "Pause";
                if (!MIDIClock.test.IsRunning && MIDIPlayer.playing)
                {
                    name = "Play";
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
                if (ImGui.Button("Stop"))
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
                if (ImGui.Begin("MIDI Loading...",ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoCollapse|ImGuiWindowFlags.NoMove))
                {
                    ImGui.Text(TopLoader.LoadStatus);
                    ImGui.ProgressBar(TopLoader.LoadProgress,new System.Numerics.Vector2(385f,0f));
                    ImGui.Text("Memory Usage: "+(GC.GetTotalMemory(false)/1000000)+" MB");
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
