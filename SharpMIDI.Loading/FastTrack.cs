#pragma warning disable 8625

namespace SharpMIDI
{
    public struct SynthEvent
    {
        public uint pos;
        public int val;
    }

    public class MIDITrack
    {
        public List<SynthEvent> synthEvents = new List<SynthEvent>();
        public List<Tempo> tempos = new List<Tempo>();
        public long eventAmount = 0;
        public long tempoAmount = 0;
        public long loadedNotes = 0;
        public long totalNotes = 0;
    }

    public unsafe class FastTrack : IDisposable
    {
        public MIDITrack track = new MIDITrack();
        public List<int[]> skippedNotes = new List<int[]>();
        public long trackTime = 0;
        BufferByteReader stupid;
        public FastTrack(BufferByteReader reader)
        {
            stupid = reader;
        }
        byte prevEvent = 0;
        (uint result, bool overflowed) AddNumbers(uint first, uint second)
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
        public void ParseTrackEvents(byte thres)
        {
            for (int i = 0; i < 16; i++)
            {
                skippedNotes.Add(new int[256]);
            }
            float trackTime = 0;
            float removedOffset = 0;
            while (true)
            {
                try
                {
                    //this is huge zenith inspiration lol, if you can't beat 'em, join 'em
                    long test = ReadVariableLen();
                    trackTime += test;
                    if (test > 4294967295)
                    {
                        throw new Exception("Variable length offset overflowed the uint variable type, report this to EmK530!");
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
                    byte readEvent = stupid.ReadFast();
                    if (readEvent < 0x80)
                    {
                        stupid.Pushback = readEvent;
                        readEvent = prevEvent;
                    }
                    prevEvent = readEvent;
                    byte trackEvent = (byte)(readEvent & 0b11110000);
                    switch (trackEvent)
                    {
                        case 0b10010000:
                            {
                                byte ch = (byte)(readEvent & 0b00001111);
                                byte note = stupid.Read();
                                byte vel = stupid.ReadFast();
                                if (vel != 0)
                                {
                                    track.totalNotes++;
                                    if (vel >= thres)
                                    {
                                        track.loadedNotes++;
                                        track.eventAmount++;
                                        track.synthEvents.Add(new SynthEvent()
                                        {
                                            pos = timeOptimize,
                                            val = readEvent | (note << 8) | (vel << 16)
                                        });
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
                                        track.eventAmount++;
                                        track.synthEvents.Add(new SynthEvent()
                                        {
                                            pos = timeOptimize,
                                            val = customEvent | (note << 8) | (vel << 16)
                                        });
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
                            {
                                int ch = readEvent & 0b00001111;
                                byte note = stupid.Read();
                                byte vel = stupid.ReadFast();
                                if (skippedNotes[ch][note] == 0)
                                {
                                    track.eventAmount++;
                                    track.synthEvents.Add(new SynthEvent()
                                    {
                                        pos = timeOptimize,
                                        val = readEvent | (note << 8) | (vel << 16)
                                    });
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
                                byte note = stupid.Read();
                                byte vel = stupid.Read();
                                track.eventAmount++;
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (note << 8) | (vel << 16)
                                });
                                removedOffset = 0;
                            }
                            break;
                        case 0b11000000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte program = stupid.Read();
                                track.eventAmount++;
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (program << 8)
                                });
                                removedOffset = 0;
                            }
                            break;
                        case 0b11010000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte pressure = stupid.Read();
                                track.eventAmount++;
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (pressure << 8)
                                });
                                removedOffset = 0;
                            }
                            break;
                        case 0b11100000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte l = stupid.Read();
                                byte m = stupid.Read();
                                track.eventAmount++;
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (l << 8) | (m << 16)
                                });
                                removedOffset = 0;
                            }
                            break;
                        case 0b10110000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte cc = stupid.Read();
                                byte vv = stupid.Read();
                                track.eventAmount++;
                                track.synthEvents.Add(new SynthEvent()
                                {
                                    pos = timeOptimize,
                                    val = readEvent | (cc << 8) | (vv << 16)
                                });
                                removedOffset = 0;
                            }
                            break;
                        default:
                            removedOffset += test;
                            switch (readEvent)
                            {
                                case 0b11110000:
                                    while (stupid.Read() != 0b11110111) ;
                                    break;
                                case 0b11110010:
                                    stupid.Skip(2);
                                    break;
                                case 0b11110011:
                                    stupid.Skip(1);
                                    break;
                                case 0xFF:
                                    {
                                        readEvent = stupid.Read();
                                        if (readEvent == 81)
                                        {
                                            stupid.Skip(1);
                                            int tempo = 0;
                                            for (int i = 0; i != 3; i++)
                                                tempo = (int)((tempo << 8) | stupid.Read());
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
                                            break;
                                        }
                                        else
                                        {
                                            stupid.Skip(stupid.Read());
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    break;
                }
            }
        }
        long ReadVariableLen()
        {
            byte c;
            int val = 0;
            for (int i = 0; i < 4; i++)
            {
                c = stupid.ReadFast();
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
        public void Dispose()
        {
            stupid.Dispose();
            stupid = null;
        }
    }
}