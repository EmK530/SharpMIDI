namespace SharpMIDI
{
    class UserInput
    {
        private static string version = "v2.4.0";
        public static bool silent = false;
        static void PrintLine(string str){
            if(!silent){
                Console.WriteLine(str);
            }
        }
        static void Print(string str){
            if(!silent){
                Console.Write(str);
            }
        }
        static void Main(string[] args)
        {
            string? cmdmidipath = null;
            int? cmdsoundengine = null;
            int? cmdwinmmdevice = null;
            byte cmdthreshold = 10;
            int cmdbuffer = 4194240;

            int scanStatus = 0;
            foreach(string i in args){
                if(i.ToLower()=="-s" || i.ToLower()=="-silent")
                {
                    silent = true;
                }else if(i.ToLower()=="-t" || i.ToLower()=="-threshold")
                {
                    scanStatus = 1;
                }else if(i.ToLower()=="-b" || i.ToLower()=="-buffer")
                {
                    scanStatus = 2;
                }else if(i.ToLower()=="-m" || i.ToLower()=="-mid")
                {
                    scanStatus = 3;
                }else if(i.ToLower()=="-se" || i.ToLower()=="-soundengine")
                {
                    scanStatus = 4;
                }else if(i.ToLower()=="-d" || i.ToLower()=="-devid")
                {
                    scanStatus = 5;
                }else if(i.ToLower()=="-h" || i.ToLower()=="-hide")
                {
                    ConsoleView.HideConsole();
                }else if(scanStatus == 1){
                    if(int.TryParse(i, System.Globalization.NumberStyles.Integer, null, out int result)){
                        cmdthreshold = (byte)result;
                    } else {
                        throw new Exception("Could not parse the threshold provided");
                    }
                }else if(scanStatus == 2){
                    if(int.TryParse(i, System.Globalization.NumberStyles.Integer, null, out int result)){
                        cmdbuffer = result;
                    } else {
                        throw new Exception("Could not parse the buffer size provided");
                    }
                }else if(scanStatus == 3){
                    cmdmidipath = i;
                }else if(scanStatus == 4){
                    if(int.TryParse(i, System.Globalization.NumberStyles.Integer, null, out int result)){
                        cmdsoundengine = result;
                    } else {
                        throw new Exception("Could not parse the sound engine ID provided");
                    }
                }else if(scanStatus == 5){
                    if(int.TryParse(i, System.Globalization.NumberStyles.Integer, null, out int result)){
                        cmdwinmmdevice = result;
                    } else {
                        throw new Exception("Could not parse the WinMM device ID provided");
                    }
                }
            }
            #if DEBUG
                PrintLine("SharpMIDI Debug Build of "+version+"\n");
            #else
                PrintLine("SharpMIDI Release "+version+"\n");
            #endif
            if(args.Length==0)
            {
                PrintLine("Initializing sound engine...");
                (bool,string,string) SoundStatus = Sound.Init(null,null);
                if(SoundStatus.Item1==false){
                    throw new Exception("Sound initializing failed with response: '"+SoundStatus.Item3+"'");
                } else {
                    PrintLine(SoundStatus.Item3);
                }
                string? PathInput = null;
                while(true){
                    Print("Setting #1 / #3 | Enter MIDI Path: ");
                    PathInput = Console.ReadLine();
                    if(PathInput == null){PrintLine("\nInvalid input of null");}else{
                        PathInput=PathInput.Replace("& '".ToString(), String.Empty);
                        PathInput=PathInput.Replace('"'.ToString(), String.Empty);
                        if(File.Exists(PathInput)){break;}else{PrintLine("File does not exist");}
                    }
                }
                byte Threshold = 0;
                while(true){
                    Print("Setting #2 / #3 | Pick note threshold (0-127): ");
                    string? temp = Console.ReadLine();
                    if(temp == null){PrintLine("\nInvalid input of null");}else{
                        if (int.TryParse(temp, System.Globalization.NumberStyles.Integer, null, out int i))
                        {
                            Threshold = (byte)i;
                            break;
                        } else {
                            PrintLine("\nCould not parse input");
                        }
                    }
                }
                int maxBuffer = 0;
                while(true){
                    Print("Setting #3 / #3 | Max track buffer (limit 2147483647): ");
                    string? temp = Console.ReadLine();
                    if(temp == null){PrintLine("\nInvalid input of null");}else{
                        if (long.TryParse(temp, System.Globalization.NumberStyles.Integer, null, out long i))
                        {
                            if(i < 8192){
                                PrintLine("Buffer size below 8192 is not recommended.");
                            } else if(i > 2147483647){
                                PrintLine("Buffer size over 2147483647 unsupported");
                            } else {
                                maxBuffer = (int)i;
                                break;
                            }
                        } else {
                            PrintLine("Could not parse input");
                        }
                    }
                }
                MIDIReader.LoadPath(PathInput,Threshold,maxBuffer);
            } else {
                PrintLine("Starting in command line mode.");
                if(cmdmidipath==null&&(cmdsoundengine==null || (cmdsoundengine==2&&cmdwinmmdevice==null))){
                    throw new Exception("A required command line argument is missing");
                }
                PrintLine("Initializing sound engine...");
                (bool,string,string) SoundStatus = Sound.Init(cmdsoundengine,cmdwinmmdevice);
                if(!SoundStatus.Item1){throw new Exception(SoundStatus.Item3);}
                if(!File.Exists(cmdmidipath)){throw new Exception("Provided MIDI path does not exist.");}
                MIDIReader.LoadPath(cmdmidipath,cmdthreshold,cmdbuffer);
            }
        }
    }
}
