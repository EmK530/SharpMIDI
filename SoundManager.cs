using System.Runtime.InteropServices;

namespace SharpMIDI
{
    class Sound
    {
        private static int engine = 0;
        private static IntPtr? handle;
        static (bool,string,string) KDMAPIPrompt()
        {
            Console.Write("KDMAPI is available! Input y to use it: ");
            string? input = Console.ReadLine();
            if(input != null && input.ToLower() == "y"){
                int loaded = KDMAPI.InitializeKDMAPIStream();
                if(loaded == 1){
                    engine = 1;
                    return (true,"KDMAPI","KDMAPI initialized!");
                } else {
                    Console.WriteLine("KDMAPI init failed, falling back to WinMM...");
                    engine = 2;
                    (bool,string,string,IntPtr?,MidiOutCaps?) result = WinMM.Setup();
                    handle=result.Item4;
                    return (result.Item1,result.Item2,result.Item3);
                    //return (false,"None","KDMAPI failed to initialize");
                }
            } else {
                Console.WriteLine("Loading WinMM...");
                engine = 2;
                (bool,string,string,IntPtr?,MidiOutCaps?) result = WinMM.Setup();
                handle=result.Item4;
                return (result.Item1,result.Item2,result.Item3);
            }
        }
        public static (bool,string,string) Init()
        {
            bool XSynthAvailable = false;
            try
            {
                XSynthAvailable = XSynth.IsKDMAPIAvailable();
            }
            catch(DllNotFoundException)
            {
                Console.WriteLine("Failed to load XSynth.dll, is it in the same directory?");
            }
            bool KDMAPIAvailable = false;
            try
            {
                KDMAPIAvailable = KDMAPI.IsKDMAPIAvailable();
            }
            catch(DllNotFoundException)
            {
                Console.WriteLine("OmniMIDI DllImport failed.");
            }
            if(KDMAPIAvailable || XSynthAvailable){
                if(XSynthAvailable){
                    Console.Write("XSynth is available! Input y to use it: ");
                    string? input = Console.ReadLine();
                    if(input != null && input.ToLower() == "y"){
                        int loaded = XSynth.InitializeKDMAPIStream();
                        if(loaded == 1){
                            engine = 3;
                            return (true,"XSynth","XSynth initialized!");
                        } else {
                            Console.WriteLine("XSynth init failed.");
                            return KDMAPIPrompt();
                        }
                    } else {
                        return KDMAPIPrompt();
                    }
                } else {
                    return KDMAPIPrompt();
                }
            }
            Console.WriteLine("KDMAPI / XSynth not available, falling back to WinMM...");
            engine = 2;
            (bool,string,string,IntPtr?,MidiOutCaps?) result = WinMM.Setup();
            handle=result.Item4;
            return (result.Item1,result.Item2,result.Item3);
            //return (false,"None","KDMAPI is not available");
        }
        public static void Submit(uint ev)
        {
            switch(engine){
                case 0:
                    Console.WriteLine("WARNING: Attempt to submit MIDI event without an engine initialized.");
                    break;
                case 1:
                    KDMAPI.SendDirectData(ev);
                    break;
                case 2:
                    if(handle!=null){
                        WinMM.midiOutShortMsg((IntPtr)handle,ev);
                    } else {
                        Console.WriteLine("Attempt to submit audio event to null WinMM handle");
                    }
                    break;
                case 3:
                    XSynth.SendDirectData(ev);
                    break;
                default:
                    Console.WriteLine("WARNING: Cannot submit MIDI event, unknown engine is initialized.");
                    break;
            }
        }
        public static void Close(){
            switch(engine){
                case 0:
                    Console.WriteLine("WARNING: Attempt to terminate stream when no engine is initialized.");
                    break;
                case 1:
                    KDMAPI.TerminateKDMAPIStream();
                    break;
                case 2:
                    if(handle!=null){
                        WinMM.midiOutClose((IntPtr)handle);
                    } else {
                        Console.WriteLine("Attempt to close null WinMM handle");
                    }
                    break;
                case 3:
                    XSynth.TerminateKDMAPIStream();
                    break;
                default:
                    Console.WriteLine("WARNING: Cannot terminate stream, unknown engine is initialized.");
                    break;
            }
        }
    }
}
