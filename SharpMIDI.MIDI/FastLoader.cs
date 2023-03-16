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
            long size = ms.Length;
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
            TopLoader.LoadStatus = "Parsing MIDI...";
            TopLoader.LoadProgress = 0f;
            long loadedSize = 0;
            int tks = MIDIData.realTracks;
            for(int i = 0; i < tks; i++)
            {
                MIDIData.synthEvents.Add(new List<SynthEvent>());
            }
            Parallel.For(0, tks, (i) =>
            //for(int i = 0; i < tks; i++)
            {
                {
                    int bufSize = Window.buffersize;
                    if (bufSize > trackSizes[(int)i])
                    {
                        bufSize = (int)trackSizes[(int)i];
                    }
                    totalSize += trackSizes[(int)i];
                    Console.Write("\nTrack " + (i + 1) + " | Size " + trackSizes[(int)i]);
                    loadedSize += trackSizes[(int)i];
                    FastTrack temp = new FastTrack(new BufferByteReader(ms, bufSize, trackPositions[(int)i], trackSizes[(int)i]));
                    temp.ParseTrack(Window.threshold,i);
                    MIDIData.notes += temp.notes;
                    MIDIData.maxNotes += temp.maxNotes;
                    MIDIData.events += temp.evs;
                    if (temp.trackTime > MIDIData.maxTick)
                    {
                        MIDIData.maxTick = temp.trackTime;
                    }
                    temp.Dispose();
                    TopLoader.LoadProgress = (float)loadedSize / (float)size;
                    if (totalSize > 134217728)
                    {
                        totalSize = 0;
                        Console.Write(" | Calling GC");
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
