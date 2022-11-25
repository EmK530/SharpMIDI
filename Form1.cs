#pragma warning disable 8622

using SharpMIDI;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SharpMIDI
{
    public partial class Form1 : Form
    {
        public static string toMemoryText(long bytes)
        {
            switch (bytes)
            {
                case var expression when bytes < 1000:
                    return bytes + " B";
                case var expression when bytes < 1000000:
                    return bytes/1000 + " KB";
                default:
                    return bytes / 1000000 + " MB";
            }
        }
        public Form1()
        {
            InitializeComponent();
            List<string> devs = WinMM.GetDevices();
            foreach(string i in devs)
            {
                comboBox1.Items.Add(i);
            }
            Task.Run(() => UpdateMemory());
        }

        void ToggleSynthSettings(bool t)
        {
            comboBox1.Enabled = t;
            radioButton1.Enabled = t;
            radioButton2.Enabled = t;
            radioButton3.Enabled = t;
            button3.Enabled = t;
        }

        private async Task UpdateMemory()
        {
            while(true)
            {
                label7.Text = "Memory Usage: " + toMemoryText(GC.GetTotalMemory(false)) + " (May be inaccurate)";
                label7.Update();
                Thread.Sleep(100);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                button1.Enabled = false;
                openFileDialog.Filter = "MIDI file (*.mid)|*.mid|7-Zip Archive (*.7z)|*.7z|.gz Archive (*.gz)|*.gz|.rar Archive (*.rar)|*.rar|.tar Archive (*.tar)|*.tar|.xz Archive (*.xz)|*.xz|.zip Archive (*.zip)|*.zip";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Starter.SubmitMIDIPath(openFileDialog.FileName);
                    button2.Enabled = true;
                } else
                {
                    button1.Enabled = true;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int soundEngine = 1;
            string winMMdev = (string)comboBox1.SelectedItem;
            if(radioButton2.Checked){soundEngine = 2; }else if(radioButton3.Checked){ soundEngine = 3; }
            Console.WriteLine("Loading sound engine ID " + soundEngine);
            ToggleSynthSettings(false);
            button1.Enabled = Sound.Init(soundEngine,winMMdev) && !Starter.midiLoaded;
            label13.Visible = !button1.Enabled && !Starter.midiLoaded;
            ToggleSynthSettings(true);
        }

        private async Task PlayMIDI()
        {
            await MIDIPlayer.StartPlayback();
            button2.Enabled = true;
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button4.Enabled = false;
            button4.Update();
            button6.Enabled = true;
            button5.Enabled = true;
            button5.Update();
            Task.Run(() => PlayMIDI());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            AllocConsole();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        static bool paused = false;

        private void button6_Click(object sender, EventArgs e)
        {
            if (!paused)
            {
                MIDIClock.Stop();
                button6.Text = "Play";
                button6.Update();
            } else
            {
                MIDIClock.Resume();
                button6.Text = "Pause";
                button6.Update();
            }
            paused = !paused;
            MIDIPlayer.paused = paused;
            button5.Enabled = !paused;
            button5.Update();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            button5.Update();
            button6.Enabled = false;
            button6.Update();
            MIDIPlayer.stopping = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            MIDIClock.throttle = checkBox1.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Starter.midiLoaded)
            {
                label1.Text = "Selected MIDI: (none)";
                label2.Text = "Status: Not Loaded";
                label5.Text = "Notes: ??? / ???";
                label6.Text = "PPQ: ???";
                label10.Text = "Loaded tracks: 0 / ?????";
                label3.Text = "Played events: 0 / 0";
                label12.Text = "FPS \u2248 N/A";
                label14.Text = "Tick: 0";
                label16.Text = "TPS: N/A";
                label17.Text = "BPM: 120";
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                button2.Enabled = false;
                button4.Update();
                button5.Update();
                button6.Update();
                button2.Update();
                MIDIPlayer.ClearEntries();
                MIDILoader.ResetVariables();
                Starter.midiLoaded = false;
                button1.Enabled = true;
                button2.Enabled = false;
            } else
            {
                button1.Enabled = true;
                button2.Enabled = false;
            }
        }
    }
}
