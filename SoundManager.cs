namespace SharpMIDI
{
    class Sound
    {
        private static int engine = 0;
        public static (bool,string,string) Init()
        {
            if(KDMAPI.IsKDMAPIAvailable()){
                int loaded = KDMAPI.InitializeKDMAPIStream();
                if(loaded == 1){
                    engine = 1;
                    return (true,"KDMAPI","KDMAPI initialized!");
                } else {
                    return (false,"None","KDMAPI failed to initialize");
                }
            } else {
                return (false,"None","KDMAPI is not available");
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
                default:
                    Console.WriteLine("WARNING: Cannot terminate stream, unknown engine is initialized.");
                    break;
            }
        }
    }
}