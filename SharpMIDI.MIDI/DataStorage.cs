namespace SharpMIDI
{
    public struct Tempo
    {
        public float pos;
        public int tempo;
    }
    public struct SynthEvent
    {
        public float pos;
        public uint val;
    }
    class MIDIData
    {
        public static List<Tempo> tempos = new List<Tempo>();
        public static List<List<SynthEvent>> synthEvents = new List<List<SynthEvent>>();
        public static int realTracks = 0;
        public static int format = 0;
        public static int fakeTracks = 0;
        public static int ppq = 0;
        public static float maxTick = 0f;
        public static long notes = 0;
        public static long maxNotes = 0;
        public static long events = 0;
    }
}
