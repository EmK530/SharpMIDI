namespace SharpMIDI
{
    class FastTrack : IDisposable
    {
        BufferByteReader br;
        List<SynthEvent> events;
        public uint notes = 0;
        public uint maxNotes = 0;
        public uint evs = 0;
        public float trackTime = 0;
        public FastTrack(BufferByteReader brr)
        {
            br = brr;
        }
        public void ParseTrack(int threshold,int tk)
        {
            events = MIDIData.synthEvents[tk];
            long ReadVariableLen()
            {
                byte c;
                int val = 0;
                for (int i = 0; i < 4; i++)
                {
                    c = br.ReadFast();
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
            List<int[]> heldNotes = new List<int[]>();
            for (int i = 0; i < 16; i++)
            {
                heldNotes.Add(new int[256]);
            }
            byte prevEvent = 0;
            while (true)
            {
                try
                {
                    trackTime += ReadVariableLen();
                    byte readEvent = br.ReadFast();
                    if (readEvent < 0x80)
                    {
                        br.Pushback = readEvent;
                        readEvent = prevEvent;
                    }
                    prevEvent = readEvent;
                    byte trackEvent = (byte)(readEvent & 0b11110000);
                    switch (trackEvent)
                    {
                        case 0b10010000:
                            {
                                byte ch = (byte)(readEvent & 0b00001111);
                                byte note = br.Read();
                                byte vel = br.ReadFast();
                                if (vel != 0)
                                {
                                    unchecked { maxNotes++; };
                                    if (vel >= threshold)
                                    {
                                        unchecked { notes++; };
                                        events.Add(new SynthEvent()
                                        {
                                            pos = trackTime,
                                            val = (Int24)(readEvent | (note << 8) | (vel << 16))
                                        });
                                        unchecked { evs++; };
                                        unchecked { heldNotes[ch][note]++; };
                                    }
                                }
                                else
                                {
                                    if (heldNotes[ch][note] != 0)
                                    {
                                        events.Add(new SynthEvent()
                                        {
                                            pos = trackTime,
                                            val = (Int24)(readEvent | (note << 8) | (vel << 16))
                                        });
                                        unchecked { evs++; };
                                        unchecked { heldNotes[ch][note]--; };
                                    }
                                }
                            }
                            break;
                        case 0b10000000:
                            {
                                byte ch = (byte)(readEvent & 0b00001111);
                                byte note = br.Read();
                                byte vel = br.ReadFast();
                                if (heldNotes[ch][note] != 0)
                                {
                                    events.Add(new SynthEvent()
                                    {
                                        pos = trackTime,
                                        val = (Int24)(readEvent | (note << 8) | (vel << 16))
                                    });
                                    unchecked { evs++; };
                                    unchecked { heldNotes[ch][note]--; };
                                }
                            }
                            break;
                        case 0b10100000:
                            {
                                byte note = br.Read();
                                byte vel = br.Read();
                                events.Add(new SynthEvent()
                                {
                                    pos = trackTime,
                                    val = (Int24)(readEvent | (note << 8) | (vel << 16))
                                });
                                unchecked { evs++; };
                            }
                            break;
                        case 0b11000000:
                            {
                                byte program = br.Read();
                                events.Add(new SynthEvent()
                                {
                                    pos = trackTime,
                                    val = (Int24)(readEvent | (program << 8))
                                });
                                unchecked { evs++; };
                            }
                            break;
                        case 0b11010000:
                            {
                                byte pressure = br.Read();
                                events.Add(new SynthEvent()
                                {
                                    pos = trackTime,
                                    val = (Int24)(readEvent | (pressure << 8))
                                });
                                unchecked { evs++; };
                            }
                            break;
                        case 0b11100000:
                            {
                                byte l = br.Read();
                                byte m = br.Read();
                                events.Add(new SynthEvent()
                                {
                                    pos = trackTime,
                                    val = (Int24)(readEvent | (l << 8) | (m << 16))
                                });
                                unchecked { evs++; };
                            }
                            break;
                        case 0b10110000:
                            {
                                byte cc = br.Read();
                                byte vv = br.Read();
                                events.Add(new SynthEvent()
                                {
                                    pos = trackTime,
                                    val = (Int24)(readEvent | (cc << 8) | (vv << 16))
                                });
                                unchecked { evs++; };
                            }
                            break;
                        default:
                            switch (readEvent)
                            {
                                case 0b11110000:
                                    while (br.Read() != 0b11110111) ;
                                    break;
                                case 0b11110010:
                                    br.Skip(2);
                                    break;
                                case 0b11110011:
                                    br.Skip(1);
                                    break;
                                case 0xFF:
                                    {
                                        readEvent = br.Read();
                                        if (readEvent == 81)
                                        {
                                            br.Skip(1);
                                            int tempo = 0;
                                            for (int i = 0; i != 3; i++)
                                                tempo = (int)((tempo << 8) | br.Read());
                                            Tempo tempoEvent = new Tempo();
                                            tempoEvent.pos = trackTime;
                                            tempoEvent.tempo = tempo;
                                            MIDIData.tempos.Add(tempoEvent);
                                        }
                                        else if (readEvent == 0x2F)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            br.Skip(br.Read());
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
                    events.TrimExcess();
                    break;
                }
            }
        }
        public void Dispose()
        {
            br.Dispose();
            br = null;
        }
    }
}
