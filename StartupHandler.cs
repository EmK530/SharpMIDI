namespace SharpMIDI
{
    class UserInput
    {
        private static string version = "v2.2.0";
        static void Main(string[] args)
        {
            #if DEBUG
                Console.WriteLine("SharpMIDI Debug Build of "+version+"\n");
            #else
                Console.WriteLine("SharpMIDI Release "+version+"\n");
            #endif
            Console.WriteLine("Initializing sound engine...");
            (bool,string,string) SoundStatus = Sound.Init();
            if(SoundStatus.Item1==false){
                throw new Exception("Sound initializing failed with response: '"+SoundStatus.Item3+"'");
            } else {
                Console.WriteLine(SoundStatus.Item3);
            }
            string? PathInput = null;
            while(true){
                Console.Write("Enter MIDI Path: ");
                PathInput = Console.ReadLine();
                if(PathInput == null){Console.WriteLine("\nInvalid input of null");}else{
                    PathInput=PathInput.Replace("& '".ToString(), String.Empty);
                    PathInput=PathInput.Replace('"'.ToString(), String.Empty);
                    if(File.Exists(PathInput)){break;}else{Console.WriteLine("File does not exist");}
                }
            }
            byte Threshold = 0;
            while(true){
                Console.Write("Pick note threshold (0-127): ");
                string? temp = Console.ReadLine();
                if(temp == null){Console.WriteLine("\nInvalid input of null");}else{
                    if (int.TryParse(temp, System.Globalization.NumberStyles.HexNumber, null, out int i))
                    {
                        Threshold = (byte)i;
                        break;
                    } else {
                        Console.WriteLine("\nCould not parse input");
                    }
                }
            }
            MIDIReader.LoadPath(PathInput,Threshold);
        }
    }
}
