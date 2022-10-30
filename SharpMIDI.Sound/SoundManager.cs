namespace SharpMIDI
{
    class Sound
    {
        private static int engine = 0;
        public static long totalEvents = 0;
        static string lastWinMMDevice = "";
        private static IntPtr? handle;
        static System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        public static bool Init(int synth, string winMMdev)
        {
            Close();
            switch (synth)
            {
                case 1:
                    bool KDMAPIAvailable = false;
                    try { KDMAPIAvailable = KDMAPI.IsKDMAPIAvailable(); } catch (DllNotFoundException) { }
                    if (KDMAPIAvailable)
                    {
                        int loaded = KDMAPI.InitializeKDMAPIStream();
                        if (loaded == 1)
                        {
                            engine = 1;
                            return true;
                        }
                        else { return false; }
                    }
                    else { MessageBox.Show("KDMAPI is not available."); return false; }
                case 2:
                    (bool, string, string, IntPtr?, MidiOutCaps?) result = WinMM.Setup(winMMdev);
                    if (!result.Item1)
                    {
                        MessageBox.Show(result.Item3);
                        return false;
                    }
                    else
                    {
                        engine = 2;
                        handle = result.Item4;
                        lastWinMMDevice = winMMdev;
                        return true;
                    }
                case 3:
                    bool XSynthAvailable = false;
                    try { XSynthAvailable = XSynth.IsKDMAPIAvailable(); } catch (DllNotFoundException) { }
                    if (XSynthAvailable)
                    {
                        int loaded = XSynth.InitializeKDMAPIStream();
                        if (loaded == 1)
                        {
                            engine = 3;
                            return true;
                        }
                        else { MessageBox.Show("KDMAPI is not available."); return false; }
                    }
                    else { return false; }
                default:
                    return false;
            }
        }
        public static void Reload()
        {
            Close();
            switch (engine)
            {
                case 1:
                    KDMAPI.InitializeKDMAPIStream();
                    break;
                case 2:
                    (bool, string, string, IntPtr?, MidiOutCaps?) result = WinMM.Setup(lastWinMMDevice);
                    handle = result.Item4;
                    break;
                case 3:
                    XSynth.InitializeKDMAPIStream();
                    break;
            }
        }
        public static void Submit(uint ev)
        {
            switch(engine){
                case 1:
                    KDMAPI.SendDirectData(ev);
                    break;
                case 2:
                    if(handle!=null){
                        WinMM.midiOutShortMsg((IntPtr)handle,ev);
                    }
                    break;
                case 3:
                    XSynth.SendDirectData(ev);
                    break;
            }
            totalEvents++;
            if((double)watch.ElapsedTicks / TimeSpan.TicksPerSecond > 0.0166666d)
            {
                Starter.form.label3.Text = "Played events: " + totalEvents;
                Starter.form.label3.Update();
                watch.Restart();
            }
        }
        public static void Close(){
            switch(engine){
                case 1:
                    KDMAPI.TerminateKDMAPIStream();
                    break;
                case 2:
                    if(handle!=null){
                        WinMM.midiOutClose((IntPtr)handle);
                    }
                    break;
                case 3:
                    XSynth.TerminateKDMAPIStream();
                    break;
            }
        }
    }
}