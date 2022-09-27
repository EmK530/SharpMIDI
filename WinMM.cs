using System.Runtime.InteropServices;

namespace SharpMIDI
{
    struct MidiOutCaps
    {
        public UInt16 wMid;
        public UInt16 wPid;
        public UInt32 vDriverVersion;

        [MarshalAs(UnmanagedType.ByValTStr,
            SizeConst = 32)]
        public String szPname;

        public UInt16 wTechnology;
        public UInt16 wVoices;
        public UInt16 wNotes;
        public UInt16 wChannelMask;
        public UInt32 dwSupport;
    }
    class WinMM
    {
        [DllImport("winmm.dll")]
        private static extern int midiOutGetNumDevs();
        [DllImport("winmm.dll")]
        private static extern int midiOutGetDevCaps(Int32 uDeviceID, ref MidiOutCaps lpMidiOutCaps, UInt32 cbMidiOutCaps);
        [DllImport("winmm.dll")]
        static extern uint midiOutOpen(out IntPtr lphMidiOut, uint uDeviceID, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);
        [DllImport("winmm.dll")]
        public static extern UInt32 midiOutClose(IntPtr hMidiOut);
        [DllImport("winmm.dll")]
        public static extern uint midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

        public static (bool,string,string,IntPtr?,MidiOutCaps?) Setup(int? device)
        {
            int id = 0;
            int devices = midiOutGetNumDevs();
            if(device == null){
                if(devices == 0)
                {
                    return (false,"None","No WinMM devices were found!",null,null);
                } else if(devices == 1){
                    MidiOutCaps myCaps = new MidiOutCaps();
                    midiOutGetDevCaps(0, ref myCaps, (UInt32)Marshal.SizeOf(myCaps));
                    IntPtr handle;
                    midiOutOpen(out handle, (uint)0, (IntPtr)0, (IntPtr)0, (uint)0);
                    return (true,myCaps.szPname,"WinMM initialized!",handle,myCaps);
                } else {
                    Console.WriteLine("WinMM Devices:");
                    for(int i = 0; i < devices; i++)
                    {
                        MidiOutCaps caps = new MidiOutCaps();
                        midiOutGetDevCaps(i, ref caps, (UInt32)Marshal.SizeOf(caps));
                        Console.WriteLine((i+1)+": "+caps.szPname);
                    }
                    while(true){
                        Console.Write("Pick device ID: ");
                        string? temp = Console.ReadLine();
                        if(temp == null){Console.WriteLine("\nInvalid input of null");}else{
                            if (int.TryParse(temp, System.Globalization.NumberStyles.HexNumber, null, out int i))
                            {
                                if(i > 0){
                                    id = i-1;
                                    break;
                                } else {
                                    Console.WriteLine("\nCould not parse input");
                                }
                            } else {
                                Console.WriteLine("\nCould not parse input");
                            }
                        }
                    }
                }
            } else {
                id = (int)device;
                if(id >= devices || id < 0){
                    throw new Exception("Selected WinMM device out of range");
                }
            }
            {
                MidiOutCaps caps1 = new MidiOutCaps();
                midiOutGetDevCaps(id, ref caps1, (UInt32)Marshal.SizeOf(caps1));
                IntPtr handle;
                midiOutOpen(out handle, (uint)id, (IntPtr)0, (IntPtr)0, (uint)0);
                return (true,caps1.szPname,"WinMM initialized!",handle,caps1);
            }
        }
    }
}
