namespace SharpMIDI
{
    public static class Starter
    {
        public static string level = "Beta 1";
        public static string major = "4";
        public static string minor = "0";
        public static string micro = "0";
        public static string name = "SharpMIDI v"+major+"."+minor+"."+micro+" "+level;
        [STAThread]
        public static void Main()
        {
            Window wnd = new Window();
            wnd.Run();
        }
    }
}