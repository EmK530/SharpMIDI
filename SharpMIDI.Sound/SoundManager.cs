using System.Runtime.CompilerServices;

namespace SharpMIDI
{
    unsafe class Sound
    {
        private static int engine = 0;
        public static long totalEvents = 0;
        static string lastWinMMDevice = "";
        private static IntPtr? handle;
        static System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        public static Func<uint,uint> sendTo;
        static uint stWinMM(uint ev)
        {
            return WinMM.midiOutShortMsg((IntPtr)handle, ev);
        }
        static uint stXSynth(uint ev)
        {
            return XSynth.SendDirectData(ev);
        }
        static uint stKDMAPI(uint ev)
        {
            return KDMAPI.SendDirectData(ev);
        }
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
                            sendTo = stKDMAPI;
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
                        sendTo = stWinMM;
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
                            sendTo = stXSynth;
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
                    return;
                case 2:
                    (bool, string, string, IntPtr?, MidiOutCaps?) result = WinMM.Setup(lastWinMMDevice);
                    handle = result.Item4;
                    return;
                case 3:
                    XSynth.InitializeKDMAPIStream();
                    return;
            }
        }
        public static void Submit(uint ev)
        {
            sendTo(ev);
            totalEvents++;
        }
        public static void Close(){
            switch(engine){
                case 1:
                    KDMAPI.TerminateKDMAPIStream();
                    return;
                case 2:
                    if(handle!=null){
                        WinMM.midiOutClose((IntPtr)handle);
                    }
                    return;
                case 3:
                    XSynth.TerminateKDMAPIStream();
                    return;
            }
        }
    }
}
