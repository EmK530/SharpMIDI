namespace SharpMIDI
{
    class MIDIPlayer
    {
        public static double FPS = 0d;
        public static double curTick = 0d;
        public static double TPS = 0d;
        static double tick()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            double secondsSinceEpoch = t.TotalSeconds;
            return secondsSinceEpoch;
        }
        public static bool stopping = false;
        public static bool playing = false;
        public static unsafe async Task StartPlayback()
        {
            stopping = false;
            double bpm = 120;
            double clock = 0;
            double timeSinceLastPrint = tick();
            int totalFrames = 0;
            double totalDelay = 0;
            double recentDelay = 0;
            int[] eventProgress = new int[MIDIData.synthEvents.Count];
            int tempoProgress = 0;
            System.Diagnostics.Stopwatch? watch = System.Diagnostics.Stopwatch.StartNew();
            MIDIClock.Reset();
            Sound.totalEvents = 0;
            MIDIClock.Start();
            fixed (int* eP = eventProgress)
            {
                while (true)
                {
                    clock = MIDIClock.GetTick();
                    if (tick() - timeSinceLastPrint >= 0.01d)
                    {
                        FPS = Math.Round(1/((double)(totalDelay / TimeSpan.TicksPerSecond) / (double)totalFrames), 5);
                        curTick = Math.Round(clock,0);
                        TPS = Math.Round(1/MIDIClock.ticklen,5);
                        timeSinceLastPrint = tick();
                        totalFrames = 0;
                        totalDelay = 0;
                    }
                    long watchtime = watch.ElapsedTicks;
                    watch.Stop();
                    watch = System.Diagnostics.Stopwatch.StartNew();
                    double delay = (double)watchtime / TimeSpan.TicksPerSecond;
                    totalDelay += watchtime;
                    recentDelay = watchtime;
                    int evs = 0;
                    int loops = -1;
                    while (true)
                    {
                        if (tempoProgress < MIDIData.tempos.Count)
                        {
                            Tempo ev = MIDIData.tempos[tempoProgress];
                            evs++;
                            if (ev.pos <= clock)
                            {
                                MIDIClock.SubmitBPM(ev.pos, ev.tempo);
                                bpm = 60000000 / (double)ev.tempo;
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
                    foreach (List<SynthEvent> i in MIDIData.synthEvents)
                    {
                        loops++;
                        while (true)
                        {
                            if (eP[loops] < i.Count)
                            {
                                SynthEvent ev = i[eP[loops]];
                                evs++;
                                if (ev.pos <= clock)
                                {
                                    eP[loops]++;
                                    Sound.Submit((uint)ev.val);
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
                    }
                    totalFrames++;
                    if (evs == 0 || stopping)
                    {
                        if (stopping)
                            Sound.Reload();
                        playing = false;
                        Console.WriteLine("Playback finished...");
                        break;
                    }
                }
                return;
            }
        }
    }
}
