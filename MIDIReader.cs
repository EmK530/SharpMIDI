#pragma warning disable 8602
#pragma warning disable 8618

/*
As you might be able to tell by comparison, this is heavily inspired by Zenith
I really just wanted something optimized haha
*/

namespace SharpMIDI
{
    unsafe class MIDIReader
    {
        private static List<long> trackLocations = new List<long>();
        private static List<uint> trackSizes = new List<uint>();
        private static Stream midiStream;
        private static ushort trackCount = 0;
        private static long noteCount = 0;
        public static void LoadPath(string path){
            midiStream = new StreamReader(path).BaseStream;
            Console.WriteLine("Verifying header...");
            uint ppq = VerifyHeader();
            Console.WriteLine("Searching for tracks...");
            while (midiStream.Position < midiStream.Length)
            {
                IndexTrack();
            }
            Console.WriteLine("Found "+trackCount+" tracks.");
            MIDIPlayer.SubmitTrackCount(trackCount);
            Console.WriteLine("Begin event scan.");
            //Parallel.For(0, tracks.Length, (i) =>
            for(ushort i = 0; i < trackCount; i++)
            {
                {
                    MidiTrack temp = new MidiTrack(new BufferByteReader(midiStream,10000,trackLocations[i],trackSizes[i]));
                    Console.WriteLine("Track "+(i+1)+" / "+trackCount+" | Size "+trackSizes[i]);
                    temp.ParseTrackEvents(true);
                    noteCount+=temp.noteCount;
                    temp.Dispose();
                    MIDIPlayer.SubmitTrackForPlayback(i,temp);
                }
            };
            Console.WriteLine("MIDI Loaded!");
            Console.WriteLine("Notes: "+noteCount);
            MIDIPlayer.StartPlayback(ppq,noteCount);
        }
        static void Seek(long offset){
            midiStream.Seek(midiStream.Position+2,SeekOrigin.Begin);
        }
        static uint VerifyHeader()
        {
            FindText("MThd");
            uint size = ReadInt32();
            uint fmt = ReadInt16();
            Seek(2);
            uint ppq = ReadInt16();
            if(size!=6){throw new Exception("Incorrect header size of "+size);}
            if(fmt==2){throw new Exception("MIDI format 2 unsupported");}
            if(ppq<0){throw new Exception("PPQ is negative");}
            return ppq;
        }
        static void IndexTrack()
        {
            FindText("MTrk");
            uint size = ReadInt32();
            trackLocations.Add(midiStream.Position);
            trackSizes.Add(size);
            midiStream.Position += size;
            trackCount++;
        }
        static void FindText(string text)
        {
            foreach (char l in text)
            {
                if(midiStream.ReadByte() != l)
                {
                    throw new Exception("Header issue");
                }
            }
        }
        static uint ReadInt32()
        {
            uint length = 0;
            for (int i = 0; i != 4; i++)
                length = (uint)((length << 8) | (byte)midiStream.ReadByte());
            return length;
        }

        static ushort ReadInt16()
        {
            ushort length = 0;
            for (int i = 0; i != 2; i++)
                length = (ushort)((length << 8) | (byte)midiStream.ReadByte());
            return length;
        }
    }
}