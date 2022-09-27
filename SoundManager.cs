using System.Runtime.InteropServices;

namespace SharpMIDI
{
    class Sound
    {
        private static int engine = 0;
        private static IntPtr? handle;
        static (bool,string,string) KDMAPIPrompt(bool automatic)
        {
            if(!automatic)
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
                        (bool,string,string,IntPtr?,MidiOutCaps?) result = WinMM.Setup(null);
                        handle=result.Item4;
                        return (result.Item1,result.Item2,result.Item3);
                        //return (false,"None","KDMAPI failed to initialize");
                    }
                } else {
                    Console.WriteLine("Loading WinMM...");
                    engine = 2;
                    (bool,string,string,IntPtr?,MidiOutCaps?) result = WinMM.Setup(null);
                    handle=result.Item4;
                    return (result.Item1,result.Item2,result.Item3);
                }
            } else {
                int loaded = KDMAPI.InitializeKDMAPIStream();
                if(loaded == 1){
                    return (true,"KDMAPI","KDMAPI initialized!");
                } else {
                    throw new Exception("KDMAPI failed to initialize");
                }
            }
        }
        public static (bool,string,string) Init(int? a, int? b)
        {
            if(a==null){
                bool XSynthAvailable = false;
                if(Environment.Is64BitProcess)
                {
                    try
                    {
                        XSynthAvailable = XSynth.IsKDMAPIAvailable();
                    }
                    catch(DllNotFoundException)
                    {
                        Console.WriteLine("Failed to load XSynth.dll, is it in the same directory?");
                    }
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
                                return KDMAPIPrompt(false);
                            }
                        } else {
                            return KDMAPIPrompt(false);
                        }
                    } else {
                        return KDMAPIPrompt(false);
                    }
                }
                Console.WriteLine("KDMAPI / XSynth not available, falling back to WinMM...");
                engine = 2;
                (bool,string,string,IntPtr?,MidiOutCaps?) result = WinMM.Setup(null);
                handle=result.Item4;
                return (result.Item1,result.Item2,result.Item3);
            } else {
                engine = (int)a;
                switch(a)
                {
                    case 1:
                        bool KDMAPIAvailable = false;
                        try
                        {
                            KDMAPIAvailable = KDMAPI.IsKDMAPIAvailable();
                        }
                        catch(DllNotFoundException)
                        {
                            throw new Exception("OmniMIDI DllImport failed.");
                        }
                        if(KDMAPIAvailable){
                            return KDMAPIPrompt(true);
                        } else {
                            throw new Exception("KDMAPI is not available.");
                        }
                    case 2:
                        if(b==null){
                            throw new Exception("No WinMM device was specified.");
                        }
                        Console.WriteLine("Loading WinMM...");
                        (bool,string,string,IntPtr?,MidiOutCaps?) result = WinMM.Setup((int)b);
                        handle=result.Item4;
                        return (result.Item1,result.Item2,result.Item3);
                    case 3:
                        bool XSynthAvailable = false;
                        try
                        {
                            if(Environment.Is64BitProcess)
                            {
                                XSynthAvailable = KDMAPI.IsKDMAPIAvailable();
                            } else {
                                throw new Exception("XSynth is not available on the x86 build");
                            }
                        }
                        catch(DllNotFoundException)
                        {
                            throw new Exception("XSynth.dll DllImport failed.");
                        }
                        if(XSynthAvailable){
                            int loaded = XSynth.InitializeKDMAPIStream();
                            if(loaded == 1){
                                return (true,"XSynth","XSynth initialized!");
                            } else {
                                Console.WriteLine("XSynth init failed.");
                                return KDMAPIPrompt(false);
                            }
                        } else {
                            throw new Exception("XSynth is not available.");
                        }
                    default:
                        throw new Exception("Unknown audio engine ID.");
                }
            }
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
