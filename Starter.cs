namespace SharpMIDI
{
    internal class Starter
    {
        public static Form1 form = new Form1();
        public static bool midiLoaded = false;

        [STAThread]
        static void Main()
        {
            Form.CheckForIllegalCrossThreadCalls = false;
            ApplicationConfiguration.Initialize();
            Application.Run(form);
        }

        public static void SubmitMIDIPath(string str)
        {
            Console.WriteLine("Loading MIDI file: " + str);
            midiLoaded = true;
            form.label1.Text = "Selected MIDI: " + Path.GetFileName(str);
            form.label2.Text = "Status: Loading";
            form.label1.Update();
            form.label2.Update();
            MIDILoader.LoadPath(str,(int)form.numericUpDown1.Value,(uint)form.numericUpDown2.Value,(int)form.numericUpDown3.Value);
            return;
        }
    }
}
