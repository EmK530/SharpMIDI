#pragma warning disable 8622

using SharpMIDI;
using System;
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
            System.Windows.Forms.Timer tmr = new System.Windows.Forms.Timer();
            tmr.Interval = 100;
            tmr.Tick += UpdateMemory;
            tmr.Start();
        }

        void ToggleSynthSettings(bool t)
        {
            comboBox1.Enabled = t;
            radioButton1.Enabled = t;
            radioButton2.Enabled = t;
            radioButton3.Enabled = t;
            button3.Enabled = t;
        }

        private void UpdateMemory(object sender, EventArgs e)
        {
            label7.Text = "Memory Usage: " + Form1.toMemoryText(GC.GetTotalMemory(false)) + " (May be inaccurate)";
            label7.Update();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                button1.Enabled = false;
                openFileDialog.Filter = "MIDI file (*.mid)|*.mid|.7z Archive (*.7z)|*.7z|.gz Archive (*.gz)|*.gz|.rar Archive (*.rar)|*.rar|.tar Archive (*.tar)|*.tar|.xz Archive (*.xz)|*.xz|.zip Archive (*.zip)|*.zip";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    await Starter.SubmitMIDIPath(openFileDialog.FileName);
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
            ToggleSynthSettings(false);
            button1.Enabled = Sound.Init(soundEngine,winMMdev);
            label13.Visible = !button1.Enabled;
            ToggleSynthSettings(true);
        }
    }
}