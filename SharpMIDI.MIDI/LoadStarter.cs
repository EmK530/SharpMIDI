using System.Windows.Forms;

namespace SharpMIDI
{
    class TopLoader
    {
        static OpenFileDialog ofd = new OpenFileDialog();
        public static string path = "";
        public static bool MIDILoaded = false;
        public static bool MIDILoading = false;
        public static string LoadStatus = "Begin loading...";
        public static float LoadProgress = 0f;
        public static DialogResult OpenDialog()
        {
            ofd.Filter = "MIDI file (*.mid)|*.mid|7-Zip Archive (*.7z)|*.7z|gzip Archive (*.gz)|*.gz|rar Archive (*.rar)|*.rar|tar Archive (*.tar)|*.tar|xz Archive (*.xz)|*.xz|zip Archive (*.zip)|*.zip";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            DialogResult test = ofd.ShowDialog();
            if (test == DialogResult.OK)
            {
                path = ofd.FileName;
            }
            return test;
        }
        public static async void StartLoading()
        {
            if (!MIDILoading)
            {
                MIDILoading = true;
                if (path.EndsWith(".mid"))
                {
                    FastLoader.LoadPath(path);
                }
                else
                {
                    ArchiveLoader.LoadPath(path);
                }
            }
        }
    }
}
