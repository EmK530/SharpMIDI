namespace SharpMIDI
{
    class MIDIPlayer
    {
        public static MIDITrack[] tracks = new MIDITrack[0];
        public static void SubmitTrackCount(int count)
        {
            tracks = new MIDITrack[count];
        }
        static long totalNotes = 0;
        static long loadedNotes = 0;
        public static uint ppq = 0;
        public static bool paused = false;
        public static void SubmitTrackForPlayback(int index, MIDITrack track)
        {
            if (tracks.Length <= index)
            {
                Array.Resize(ref tracks, tracks.Length + 1);
            }
            Starter.form.label7.Text = "Memory Usage: " + Form1.toMemoryText(GC.GetTotalMemory(false)) + " (May be inaccurate)";
            Starter.form.label7.Update();
            loadedNotes += track.loadedNotes;
            totalNotes += track.totalNotes;
            Starter.form.label5.Text = "Notes: " + loadedNotes + " / " + totalNotes;
            Starter.form.label5.Update();
            tracks[index] = track;
        }
        static double tick()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            double secondsSinceEpoch = t.TotalSeconds;
            return secondsSinceEpoch;
        }
        public static bool stopping = false;
        public static async Task StartPlayback()
        {
            stopping = false;
            double bpm = 120;
            double ticklen = (1 / (double)ppq) * (60 / bpm);
            double clock = 0;
            double timeSinceLastPrint = tick();
            int totalFrames = 0;
            double totalDelay = 0;
            long[] trackPositions = new long[tracks.Length];
            int[] eventProgress = new int[tracks.Length];
            int[] tempoProgress = new int[tracks.Length];
            System.Diagnostics.Stopwatch? watch = System.Diagnostics.Stopwatch.StartNew();
            MIDIClock.Reset();
            MIDIClock.Start();
            while (true)
            {
                if (tick() - timeSinceLastPrint >= 0.01d)
                {
                    Starter.form.label12.Text = "Frametime: " + Math.Round(((double)(totalDelay / TimeSpan.TicksPerSecond) / (double)totalFrames)*1000,5) + " ms";
                    Starter.form.label12.Update();
                    timeSinceLastPrint = tick();
                    totalFrames = 0;
                    totalDelay = 0;
                }
                long watchtime = watch.ElapsedTicks;
                watch.Stop();
                watch = System.Diagnostics.Stopwatch.StartNew();
                double delay = (double)watchtime / TimeSpan.TicksPerSecond;
                totalDelay += watchtime;
                clock = MIDIClock.GetTick();
                int evs = 0;
                int loops = -1;
                foreach (MIDITrack i in tracks)
                {
                    loops++;
                    while (true)
                    {
                        if (tempoProgress[loops] < i.tempoAmount)
                        {
                            Tempo ev = i.tempos[tempoProgress[loops]];
                            evs++;
                            if (ev.pos <= clock)
                            {
                                double lastbpm = bpm;
                                MIDIClock.SubmitBPM(ev.pos,ev.tempo);
                                bpm = 60000000 / (double)ev.tempo;
                                ticklen = (1 / (double)ppq) * (60 / bpm);
                                tempoProgress[loops]++;
                                if (bpm != lastbpm)
                                {
                                    //PrintLine("Tempo event, new BPM: " + bpm);
                                }
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
                    while (true)
                    {
                        if (eventProgress[loops] < i.eventAmount)
                        {
                            SynthEvent ev = i.synthEvents[eventProgress[loops]];
                            evs++;
                            if (trackPositions[loops] + (long)ev.pos <= clock)
                            {
                                eventProgress[loops]++;
                                trackPositions[loops] += (long)ev.pos;
                                if (ev.val != 0)
                                {
                                    Sound.Submit((uint)ev.val);
                                }
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
                    Console.WriteLine("Playback finished...");
                    break;
                }
            }
            MIDIClock.Reset();
            Starter.form.button4.Enabled = true;
            Starter.form.button4.Update();
            Starter.form.button5.Enabled = false;
            Starter.form.button5.Update();
            Starter.form.button6.Enabled = false;
            Starter.form.button6.Update();
            return;
        }
    }
}
