#pragma warning disable 8625

namespace SharpMIDI
{
    public class Note
    {
        public double start;
        public double end;
        public byte channel;
        public byte key;
        public byte vel;
        public int track;
    }

    public struct SynthEvent
    {
        public double pos;
        public int val;
    }

    public class Tempo
    {
        public long pos;
        public int tempo;
    }

    public unsafe class MidiTrack : IDisposable
    {
        public List<SynthEvent> synthEvents = new List<SynthEvent>();
        public List<int[]> skippedNotes = new List<int[]>();
        public long eventAmount = 0;
        public long tempoAmount = 0;
        public List<Tempo> tempos = new List<Tempo>();
        public double trackTime = 0;
        public long noteCount = 0;
        BufferByteReader stupid;
        public MidiTrack(BufferByteReader reader)
        {
            stupid=reader;
        }
        byte prevEvent = 0;
        public void ParseTrackEvents(bool runGC, byte thres)
        {
            for(int i = 0; i < 16; i++)
            {
                skippedNotes.Add(new int[256]);
            }
            trackTime = 0;
            while(true)
            {
                try
                {
                    //this is huge zenith inspiration lol, if you can't beat 'em, join 'em
                    trackTime+=ReadVariableLen();
                    double time = trackTime;
                    byte readEvent = stupid.ReadFast();
                    if (readEvent < 0x80)
                    {
                        stupid.Pushback = readEvent;
                        readEvent = prevEvent;
                    }
                    prevEvent = readEvent;
                    byte trackEvent = (byte)(readEvent & 0b11110000);
                    switch(trackEvent)
                    {
                        case 0b10010000:
                            {
                                byte ch = (byte)(readEvent & 0b00001111);
                                byte note = stupid.Read();
                                byte vel = stupid.ReadFast();
                                if(vel!=0){
                                    if(vel >= thres)
                                    {
                                        noteCount++;
                                        eventAmount++;
                                        synthEvents.Add(new SynthEvent()
                                        {
                                            pos = time,
                                            val = readEvent | (note << 8) | (vel << 16)
                                        });
                                    } else {
                                        skippedNotes[ch][note]++;
                                    }
                                } else {
                                    if(skippedNotes[ch][note] == 0)
                                    {
                                        byte customEvent = (byte)(readEvent-0b00010000);
                                        eventAmount++;
                                        synthEvents.Add(new SynthEvent()
                                        {
                                            pos = time,
                                            val = customEvent | (note << 8) | (vel << 16)
                                        });
                                    } else {
                                        skippedNotes[ch][note]--;
                                    }
                                }
                            }
                            break;
                        case 0b10000000:
                            {
                                int ch = readEvent & 0b00001111;
                                byte note = stupid.Read();
                                byte vel = stupid.ReadFast();
                                if(skippedNotes[ch][note] == 0)
                                {
                                    eventAmount++;
                                    synthEvents.Add(new SynthEvent()
                                    {
                                        pos = time,
                                        val = readEvent | (note << 8) | (vel << 16)
                                    });
                                } else {
                                    skippedNotes[ch][note]--;
                                }
                            }
                            break;
                        case 0b10100000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte note = stupid.Read();
                                byte vel = stupid.Read();
                                eventAmount++;
                                synthEvents.Add(new SynthEvent()
                                {
                                    pos = time,
                                    val = readEvent | (note << 8) | (vel << 16)
                                });
                            }
                            break;
                        case 0b11000000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte program = stupid.Read();
                                eventAmount++;
                                synthEvents.Add(new SynthEvent()
                                {
                                    pos = time,
                                    val = readEvent | (program << 8)
                                });
                            }
                            break;
                        case 0b11010000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte pressure = stupid.Read();
                                eventAmount++;
                                synthEvents.Add(new SynthEvent()
                                {
                                    pos = time,
                                    val = readEvent | (pressure << 8)
                                });
                            }
                            break;
                        case 0b11100000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte l = stupid.Read();
                                byte m = stupid.Read();
                                eventAmount++;
                                synthEvents.Add(new SynthEvent()
                                {
                                    pos = time,
                                    val = readEvent | (l << 8) | (m << 16)
                                });
                            }
                            break;
                        case 0b10110000:
                            {
                                int channel = readEvent & 0b00001111;
                                byte cc = stupid.Read();
                                byte vv = stupid.Read();
                                eventAmount++;
                                synthEvents.Add(new SynthEvent()
                                {
                                    pos = time,
                                    val = readEvent | (cc << 8) | (vv << 16)
                                });
                            }
                            break;
                        default:
                            switch(readEvent)
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
                                        if(readEvent == 81){
                                            stupid.Skip(1);
                                            int tempo = 0;
                                            for (int i = 0; i != 3; i++)
                                                tempo = (int)((tempo << 8) | stupid.Read());
                                            Tempo tempoEvent = new Tempo();
                                            tempoEvent.pos = (long)time;
                                            tempoEvent.tempo = tempo;
                                            tempoAmount++;
                                            lock (tempos)
                                            {
                                                tempos.Add(tempoEvent);
                                            }
                                        }
                                        else if(readEvent == 0x2F){
                                            break;
                                        } else {
                                            stupid.Skip(stupid.Read());
                                        }
                                    }
                                    break;
                                default:
                                    //Console.WriteLine("Unrecognized event: "+readEvent);
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
            if(runGC){GC.Collect();}
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
        }
    }
}
