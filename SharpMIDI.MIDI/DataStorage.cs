using System.Runtime.InteropServices;

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
        public Int24 val;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Int24
    {
        [FieldOffset(0)]
        private byte b0;
        [FieldOffset(1)]
        private byte b1;
        [FieldOffset(2)]
        private byte b2;

        [FieldOffset(0)]
        private int value;

        public Int24(int value)
        {
            this.b0 = this.b1 = this.b2 = 0;
            this.value = value;
        }

        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        public static implicit operator Int24(int value)
        {
            return new Int24(value);
        }

        public static implicit operator int(Int24 value)
        {
            return value.value;
        }
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
