#pragma warning disable 8602

namespace SharpMIDI
{
    public struct Tempo
    {
        public float pos;
        public int tempo;
    }

    class MIDILoader
    {
        static int pushback = -1;
        private static List<long> trackLocations = new List<long>();
        private static List<uint> trackSizes = new List<uint>();
        static int threshold = 0;
        static Stream? midiStream;
        static uint totalSize = 0;
        public static long totalNotes = 0;
        public static long loadedNotes = 0;
        public static int tks = 0;
        public static uint loadedTracks = 0;
        static uint gcRequirement = 134217728;

        static Stream? midi;

        static void Crash(string test)
        {
            MessageBox.Show(test);
            throw new Exception();
        }

        static async Task LoadArchive(uint ignoreover, int tracklimit)
        {
            await Task.Run(async () =>
            {
                int tk = 0;
                int realtk = 0;
                while (true)
                {
                    bool found = FindText("MTrk");
                    if (found)
                    {
                        bool success = await ParseTrack(tk,ignoreover,realtk);
                        tk++;
                        if (success)
                            realtk++;
                        Starter.form.label10.Text = "Loaded tracks: " + realtk + " / " + MIDILoader.tks;
                        Starter.form.label10.Update();
                        //await Task.Delay(1);
                        if (tracklimit<=tk){
                            Console.WriteLine("Track limit reached, stopping loading.");
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            });
        }

        public static async void LoadPath(string path, int thres, uint ignoreover, int tracklimit)
        {
            threshold = thres;
            midi = File.OpenRead(path);
            (bool, Stream) test = CJCMCG.ArchiveStreamPassthrough(path,midi);
            midi = test.Item2;
            bool find = FindText("MThd");
            if (!find) { Crash("Unexpected EoF searching for MThd"); }
            uint size = ReadInt32();
            uint fmt = ReadInt16();
            uint tracks = ReadInt16();
            tks = (int)tracks;
            uint ppq = ReadInt16();
            //MIDIClock.ppq = ppq;
            MIDIPlayer.ppq = ppq;
            Starter.form.label6.Text = "PPQ: "+ppq;
            Starter.form.label6.Update();
            Starter.form.label10.Text = "Loaded tracks: 0 / " + tracks;
            Starter.form.label10.Update();
            if (size != 6) { Crash("Incorrect header size of " + size); }
            if (fmt == 2) { Crash("MIDI format 2 unsupported"); }
            if (ppq < 0) { Crash("PPQ is negative"); }
            if (test.Item1)
            {
                await LoadArchive(ignoreover,tracklimit);
            } else
            {
                midiStream = new StreamReader(path).BaseStream;
                tks = 0;
                VerifyHeader();
                Console.WriteLine("Indexing MIDI tracks...");
                while (midiStream.Position < midiStream.Length)
                {
                    bool success = IndexTrack();
                    if (!success) { break; }
                }
                MIDIPlayer.SubmitTrackCount(tks);
                midiStream.Position += 1;
                int loops = 0;
                Parallel.For(0, tks, (i) =>
                {
                    {
                        if (trackSizes[(int)i] <= ignoreover && loops <= tracklimit)
                        {
                            int bufSize = 2147483647;
                            if (bufSize > trackSizes[(int)i])
                            {
                                bufSize = (int)trackSizes[(int)i];
                            }
                            FastTrack temp = new FastTrack(new BufferByteReader(midiStream, bufSize, trackLocations[(int)i], trackSizes[(int)i]));
                            Console.WriteLine("Loading track #" + (i + 1) + " | Size " + trackSizes[(int)i]);
                            totalSize += trackSizes[(int)i];
                            temp.ParseTrackEvents((byte)threshold);
                            temp.Dispose();
                            MIDIPlayer.SubmitTrackForPlayback((int)i, temp.track);
                            loops++;
                            Starter.form.label10.Text = "Loaded tracks: " + loops + " / " + MIDILoader.tks;
                            Starter.form.label10.Update();
                            if (totalSize >= gcRequirement)
                            {
                                totalSize = 0;
                                GC.Collect();
                            }
                        } else
                        {
                            Console.WriteLine("Ignoring track #" + (i + 1) + " | Size " + trackSizes[(int)i]);
                        }
                    }
                });
                Starter.form.label10.Text = "Loaded tracks: " + MIDIPlayer.tracks.Length + " / " + MIDILoader.tks;
                Starter.form.label10.Update();
            }
            Starter.form.label2.Text = "Status: Loaded";
            Starter.form.label2.Update();
            GC.Collect();
            Starter.form.button4.Enabled = true;
            Starter.form.button4.Update();
            Console.WriteLine("MIDI Loaded");
            //MIDIPlayer.StartPlayback(ppq);
        }

        static uint VerifyHeader()
        {
            bool success = FindText2("MThd");
            if (success)
            {
                uint size = ReadInt32_v2();
                uint fmt = ReadInt16_v2();
                Seek2(2);
                uint ppq = ReadInt16_v2();
                if (size != 6) { Crash("Incorrect header size of " + size); }
                if (fmt == 2) { Crash("MIDI format 2 unsupported"); }
                if (ppq < 0) { Crash("PPQ is negative"); }
                return ppq;
            }
            else
            {
                Crash("Header issue");
                return 0;
            }
        }

        static bool IndexTrack()
        {
            bool success = FindText2("MTrk");
            if (success)
            {
                uint size = ReadInt32_v2();
                trackLocations.Add(midiStream.Position);
                trackSizes.Add(size);
                midiStream.Position += size;
                tks++;
                return true;
            }
            else
            {
                return false;
            }
        }
        static (uint result, bool overflowed) AddNumbers(uint first, uint second)
        {
            long test = first;
            if (unchecked(first + second) != test + second)
            {
                return (unchecked(first + second), true);
            }
            else
            {
                return (unchecked(first + second), false);
            }
        }
        static async Task<bool> ParseTrack(int tk, uint ignoreover, int realtk)
        {
            MIDITrack track = new MIDITrack();
            List<int[]> skippedNotes = new List<int[]>();
            for (int i = 0; i < 16; i++)
            {
                skippedNotes.Add(new int[256]);
            }
            float trackTime = 0f;
            pushback = -1;
            uint trackSize = ReadInt32();
            if (trackSize > ignoreover)
            {
                Console.WriteLine("Skipping track #" + (tk + 1) + " | Size " + trackSize);
                Seek(trackSize);
                return false;
            } else {
                Console.WriteLine("Loading track #" + (tk + 1) + " | Size " + trackSize);
                byte prevEvent = 0;
                float removedOffset = 0;
                bool trackFinished = false;
                while (!trackFinished)
                {
                    long test = ReadVariableLen();
                    trackTime += test;
                    if (test > 4294967295)
                    {
                        Crash("Variable length offset overflowed the uint variable type, report this to EmK530!");
                    }
                    (uint, bool) addition = AddNumbers((uint)test, (uint)removedOffset);
                    uint timeOptimize = addition.Item1;
                    if (addition.Item2)
                    {
                        //PrintLine("Resolved uint overflow!");
                        track.synthEvents.Add(new SynthEvent()
                        {
                            pos = 4294967295,
                            val = 0
                        });
                    }
                    byte readEvent = (byte)midi.ReadByte();
                    if (readEvent < 0x80)
                    {
                        pushback = readEvent;
                        readEvent = prevEvent;
                    }
                    prevEvent = readEvent;
                    byte trackEvent = (byte)(readEvent & 0b11110000);
                    switch (trackEvent)
                    {
                        case 0b10010000:
                            //Note ON
                            {
                                byte ch = (byte)(readEvent & 0b00001111);
                                byte note = PushbackRead();
                                byte vel = (byte)midi.ReadByte();
                                if (vel != 0)
                                {
                                    track.totalNotes++;
                                    if (vel >= threshold)
                                    {
                                        track.eventAmount++;
                                        track.synthEvents.Add(new SynthEvent()
                                        {
                                            pos = timeOptimize,
                                            val = readEvent | (note << 8) | (vel << 16)
                                        });
                                        track.loadedNotes++;
                                        removedOffset = 0;
                                    }
                                    else
                                    {
                                        skippedNotes[ch][note]++;
                                        removedOffset += test;
                                    }
                                }
                                else
                                {
                                    if (skippedNotes[ch][note] == 0)
                                    {
                                        byte customEvent = (byte)(readEvent - 0b00010000);
                                        track.synthEvents.Add(new SynthEvent()
                                        {
                                            pos = timeOptimize,
                                            val = customEvent | (note << 8) | (vel << 16)
                                        });
                                        track.eventAmount++;
                                        removedOffset = 0;
                                    }
                                    else
                                    {
                                        skippedNotes[ch][note]--;
                                        removedOffset += test;
                                    }
                                }
                            }
                            break;
                        case 0b10000000:
                            //Note OFF
                            {
                                int ch = readEvent & 0b00001111;
                                byte note = PushbackRead();
                                byte vel = (byte)midi.ReadByte();
                                if (skippedNotes[ch][note] == 0)
                                {
                                    track.synthEvents.Add(new SynthEvent()
                                    {
                                        pos = timeOptimize,
                                        val = readEvent | (note << 8) | (vel << 16)
                                    });
                                    track.eventAmount++;
                                    removedOffset = 0;
                                }
                                else
                                {
                                    skippedNotes[ch][note]--;
                                    removedOffset += test;
                                }
                            }
                            break;
                        case 0b10100000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte note = PushbackRead();
                                byte vel = PushbackRead();
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (note << 8) | (vel << 16)
                                });
                                track.eventAmount++;
                                removedOffset = 0;
                            }
                            break;
                        case 0b11000000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte program = PushbackRead();
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (program << 8)
                                });
                                track.eventAmount++;
                                removedOffset = 0;
                            }
                            break;
                        case 0b11010000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte pressure = PushbackRead();
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (pressure << 8)
                                });
                                track.eventAmount++;
                                removedOffset = 0;
                            }
                            break;
                        case 0b11100000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte l = PushbackRead();
                                byte m = PushbackRead();
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (l << 8) | (m << 16)
                                });
                                track.eventAmount++;
                                removedOffset = 0;
                            }
                            break;
                        case 0b10110000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte cc = PushbackRead();
                                byte vv = PushbackRead();
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (cc << 8) | (vv << 16)
                                });
                                track.eventAmount++;
                                removedOffset = 0;
                            }
                            break;
                        default:
                            removedOffset += test;
                            switch (readEvent)
                            {
                                case 0b11110000:
                                    while (PushbackRead() != 0b11110111) ;
                                    break;
                                case 0b11110010:
                                    Seek(2);
                                    break;
                                case 0b11110011:
                                    Seek(1);
                                    break;
                                case 0xFF:
                                    {
                                        readEvent = PushbackRead();
                                        if (readEvent == 81)
                                        {
                                            Seek(1);
                                            int tempo = 0;
                                            for (int i = 0; i != 3; i++)
                                                tempo = (int)((tempo << 8) | PushbackRead());
                                            Tempo tempoEvent = new Tempo();
                                            tempoEvent.pos = trackTime;
                                            tempoEvent.tempo = tempo;
                                            track.tempoAmount++;
                                            lock (track.tempos)
                                            {
                                                track.tempos.Add(tempoEvent);
                                            }
                                        }
                                        else if (readEvent == 0x2F)
                                        {
                                            Seek(1);
                                            trackFinished = true;
                                            break;
                                        }
                                        else
                                        {
                                            Seek(PushbackRead());
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }
                totalNotes += totalNotes;
                loadedNotes += loadedNotes;
                loadedTracks++;
                MIDIPlayer.SubmitTrackForPlayback(realtk, track);
                return true;
            }
        }

        static byte PushbackRead()
        {
            if (pushback != -1)
            {
                byte ret = (byte)pushback;
                pushback = -1;
                return ret;
            }
            return (byte)midi.ReadByte();
        }

        static void Seek2(long bytes)
        {
            midiStream.Seek(midiStream.Position + 2, SeekOrigin.Begin);
        }

        static void Seek(long bytes)
        {
            for(long i = 0; i < bytes; i++)
            {
                midi.ReadByte();
            }
        }

        static long ReadVariableLen()
        {
            byte c;
            int val = 0;
            for (int i = 0; i < 4; i++)
            {
                c = (byte)midi.ReadByte();
                if (c > 0x7F)
                {
                    val = (val << 7) | (c & 0x7F);
                }
                else
                {
                    val = val << 7 | c;
                    return val;
                }
            }
            return val;
        }

        static uint ReadInt32()
        {
            uint length = 0;
            for (int i = 0; i != 4; i++)
                length = (uint)((length << 8) | (byte)midi.ReadByte());
            return length;
        }

        static ushort ReadInt16()
        {
            ushort length = 0;
            for (int i = 0; i != 2; i++)
                length = (ushort)((length << 8) | (byte)midi.ReadByte());
            return length;
        }

        static uint ReadInt32_v2()
        {
            uint length = 0;
            for (int i = 0; i != 4; i++)
                length = (uint)((length << 8) | (byte)midiStream.ReadByte());
            return length;
        }

        static ushort ReadInt16_v2()
        {
            ushort length = 0;
            for (int i = 0; i != 2; i++)
                length = (ushort)((length << 8) | (byte)midiStream.ReadByte());
            return length;
        }

        static bool FindText2(string text)
        {
            foreach (char l in text)
            {
                int test = midiStream.ReadByte();
                if (test != l)
                {
                    if (test == -1)
                    {
                        return false;
                    }
                    else
                    {
                        MessageBox.Show("Could not locate header '"+text+"', attempting to continue.");
                        //Crash("Header issue searching for " + text + " on char " + l.ToString() + ", found " + test + " at pos " + midiStream.Position);
                    }
                }
            }
            return true;
        }

        static bool FindText(string text)
        {
            foreach (char l in text)
            {
                int test = midi.ReadByte();
                if (test != l)
                {
                    if(test == -1){
                        return false;
                    } else {
                        Crash("Header issue searching for " + text);
                    }
                }
            }
            return true;
        }
    }
}
