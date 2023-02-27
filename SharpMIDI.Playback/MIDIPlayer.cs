namespace SharpMIDI
{
    class MIDIPlayer
    {
        public static double FPS = 0d;
        public static double curTick = 0d;
        public static double TPS = 0d;
        public static int targetFPS = 1000;
        public static bool limitFPS = false;
        public static bool accurateLimit = false;
        public static bool stopping = false;
        public static bool playing = false;
        public static unsafe async Task StartPlayback()
        {
            stopping = false;
            long clock = 0;
            int totalFrames = 0;
            bool[] trackFinished = new bool[MIDIData.synthEvents.Count];
            int tempoProgress = 0;
            System.Diagnostics.Stopwatch? watch = System.Diagnostics.Stopwatch.StartNew();
            System.Diagnostics.Stopwatch? watch2 = System.Diagnostics.Stopwatch.StartNew();
            MIDIClock.Reset();
            Sound.totalEvents = 0;
            MIDIClock.Start();
            uint[] diff = new uint[MIDIData.synthEvents.Count];
            List<IEnumerator<SynthEvent>> enums = new List<IEnumerator<SynthEvent>>();
            foreach (List<SynthEvent> i in MIDIData.synthEvents)
            {
                IEnumerator<SynthEvent> temp = i.GetEnumerator();
                enums.Add(temp);
            }
            fixed (bool* tF = trackFinished)
            {
                while (true)
                {
                    if (limitFPS)
                    {
                        watch2.Restart();
                        float WasteOfCPU = 0f;
                        while ((double)watch2.ElapsedTicks / (double)TimeSpan.TicksPerSecond < 1d / (double)targetFPS)
                        {
                            if (accurateLimit)
                            {
                                WasteOfCPU++;
                            } else
                            {
                                Thread.Sleep(1);
                            }
                        }
                    }
                    long newClock = (long)MIDIClock.GetTick();
                    if (watch.ElapsedTicks > 333333)
                    {
                        FPS = Math.Round(1 / (((double)watch.ElapsedTicks / (double)TimeSpan.TicksPerSecond) / (double)totalFrames), 5);
                        curTick = clock;
                        totalFrames = 0;
                        watch.Restart();
                    }
                    int evs = 0;
                    int loops = -1;
                    totalFrames++;
                    if (newClock != clock)
                    {
                        clock = newClock;
                        while (true)
                        {
                            if (tempoProgress < MIDIData.tempos.Count)
                            {
                                Tempo ev = MIDIData.tempos[tempoProgress];
                                evs++;
                                if (ev.pos <= clock)
                                {
                                    MIDIClock.SubmitBPM(ev.pos, ev.tempo);
                                    TPS = Math.Round(1 / MIDIClock.ticklen, 5);
                                    tempoProgress++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        foreach (IEnumerator<SynthEvent> i in enums)
                        {
                            loops++;
                            if (!tF[loops])
                            {
                                evs++;
                                if (i.Current.pos <= clock)
                                {
                                    Sound.Submit(i.Current.val);
                                    while (true)
                                    {
                                        if (i.MoveNext())
                                        {
                                            if (i.Current.pos <= clock)
                                            {
                                                Sound.Submit(i.Current.val);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            tF[loops] = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    } else
                    {
                        evs++;
                        /*
                        if (limitFPS)
                        {
                            Thread.Sleep(1);
                        }
                        */
                    }
                    if (evs == 0 || stopping)
                    {
                        if (stopping)
                            Sound.Reload();
                        playing = false;
                        MIDIClock.Stop();
                        Console.WriteLine("Playback finished...");
                        break;
                    }
                }
                return;
            }
        }
    }
}
