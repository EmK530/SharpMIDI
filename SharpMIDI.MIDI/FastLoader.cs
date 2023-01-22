using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace SharpMIDI
{
    class FastLoader
    {
        static uint totalSize = 0;
        static Stream ms;
        static List<long> trackPositions = new List<long>();
        static List<uint> trackSizes = new List<uint>();
        public static void LoadPath(string path)
        {
            TopLoader.LoadStatus = "Indexing tracks...";
            ms = new StreamReader(path).BaseStream;
            MIDIData.realTracks = 0;
            ms.Seek(8, SeekOrigin.Begin);
            MIDIData.format = ms.ReadByte() * 256 + ms.ReadByte();
            MIDIData.fakeTracks = ms.ReadByte() * 256 + ms.ReadByte();
            MIDIData.ppq = ms.ReadByte() * 256 + ms.ReadByte();
            while (ms.Position < ms.Length)
            {
                bool success = IndexTrack();
                if (!success) { break; }
                MIDIData.realTracks++;
                TopLoader.LoadProgress = (float)MIDIData.realTracks / (float)MIDIData.fakeTracks;
            }
            Console.WriteLine("Indexed " + MIDIData.realTracks + " tracks.");
            TopLoader.LoadStatus = "Parsing tracks...";
            TopLoader.LoadProgress = 0f;
            int LoadedTracks = 0;
            int tks = MIDIData.realTracks;
            for(int i = 0; i < tks; i++)
            {
                MIDIData.synthEvents.Add(new List<SynthEvent>());
            }
            Parallel.For(0, tks, (i) =>
            {
                {
                    int bufSize = 2000000000;
                    if (bufSize > trackSizes[(int)i])
                    {
                        bufSize = (int)trackSizes[(int)i];
                    }
                    totalSize += trackSizes[(int)i];
                    Console.WriteLine("Track " + (i + 1) + " | Size " + trackSizes[(int)i]);
                    FastTrack temp = new FastTrack(new BufferByteReader(ms, bufSize, trackPositions[(int)i], trackSizes[(int)i]));
                    temp.ParseTrack(0,i);
                    temp.Dispose();
                    LoadedTracks++;
                    TopLoader.LoadProgress = (float)LoadedTracks / (float)MIDIData.realTracks;
                    if (totalSize > 134217728)
                    {
                        totalSize = 0;
                        GC.Collect();
                    }
                }
            });
            ms.Close();
            GC.Collect();
            TopLoader.MIDILoaded = true;
            TopLoader.MIDILoading = false;
        }
        static bool FindText(string text)
        {
            foreach (char l in text)
            {
                int test = ms.ReadByte();
                if (test != l)
                {
                    return false;
                }
            }
            return true;
        }
        static bool IndexTrack()
        {
            bool success = FindText("MTrk");
            if (success)
            {
                uint size = ReadInt32();
                trackPositions.Add(ms.Position);
                trackSizes.Add(size);
                ms.Position += size;
                return true;
            }
            else
            {
                return false;
            }
        }
        static uint ReadInt32()
        {
            uint length = 0;
            for (int i = 0; i != 4; i++)
                length = (uint)((length << 8) | (byte)ms.ReadByte());
            return length;
        }
    }
}