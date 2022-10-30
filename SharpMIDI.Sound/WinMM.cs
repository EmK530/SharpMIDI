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

        public static List<String> GetDevices()
        {
            List<String> list = new List<String>();
            int devices = midiOutGetNumDevs();
            for (uint i = 0; i < devices; i++)
            {
                MidiOutCaps caps = new MidiOutCaps();
                midiOutGetDevCaps((int)i, ref caps, (UInt32)Marshal.SizeOf(caps));
                list.Add(caps.szPname);
            }
            return list;
        }

        public static (bool,string,string,IntPtr?,MidiOutCaps?) Setup(string device)
        {
            int devices = midiOutGetNumDevs();
            if (devices == 0)
            {
                return (false, "None", "No WinMM devices were found!", null, null);
            }
            else
            {
                MidiOutCaps myCaps = new MidiOutCaps();
                midiOutGetDevCaps(0, ref myCaps, (UInt32)Marshal.SizeOf(myCaps));
                IntPtr handle;
                for (uint i = 0; i < devices; i++)
                {
                    MidiOutCaps caps = new MidiOutCaps();
                    midiOutGetDevCaps((int)i, ref caps, (UInt32)Marshal.SizeOf(caps));
                    if (device == caps.szPname)
                    {
                        midiOutOpen(out handle, i, (IntPtr)0, (IntPtr)0, (uint)0);
                        return (true, myCaps.szPname, "WinMM initialized!", handle, myCaps);
                    }
                }
                return (false, "None", "Could not find the specified WinMM device.", null, null);
            }
        }
    }
}
