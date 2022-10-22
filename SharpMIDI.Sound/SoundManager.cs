using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SharpMIDI
{
    class Sound
    {
        private static int engine = 0;
        private static IntPtr? handle;
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
