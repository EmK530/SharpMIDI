namespace SharpMIDI
{
    internal class Starter
    {
        public static Form1 form = new Form1();

        [STAThread]
        static void Main()
        {
            Form.CheckForIllegalCrossThreadCalls = false;
            ApplicationConfiguration.Initialize();
            Application.Run(form);
        }

        public static async Task SubmitMIDIPath(string str)
        {
            form.label1.Text = "Selected MIDI: " + Path.GetFileName(str);
            form.label2.Text = "Status: Loading";
            form.label1.Update();
            form.label2.Update();
            await MIDILoader.LoadPath(str,(int)form.numericUpDown1.Value);
            return;
        }
    }
}