namespace SharpMIDI
{
    class MIDIPlayer
    {
        private static MIDITrack[] tracks = new MIDITrack[0];
        public static void SubmitTrackCount(int count)
        {
            tracks = new MIDITrack[count];
        }
        static int trackProgress = 0;
        static long totalNotes = 0;
        static long loadedNotes = 0;
        public static void SubmitTrackForPlayback(int index, MIDITrack track)
        {
            if (tracks.Length <= index)
            {
                Array.Resize(ref tracks, tracks.Length + 1);
            }
            trackProgress++;
            Starter.form.label10.Text = "Loaded tracks: " + trackProgress + " / " + MIDILoader.tks;
            Starter.form.label10.Update();
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
        public static async Task StartPlayback(uint ppq)
        {
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
            while (true)
            {
                if (tick() - timeSinceLastPrint >= 1)
                {
                    //PrintLine("FPS: " + (double)totalFrames / totalDelay);
                    timeSinceLastPrint = tick();
                    totalFrames = 0;
                    totalDelay = 0;
                }
                long watchtime = watch.ElapsedTicks;
                watch.Stop();
                watch = System.Diagnostics.Stopwatch.StartNew();
                double delay = (double)watchtime / TimeSpan.TicksPerSecond;
                totalDelay += delay;
                if (delay > 0.1)
                {
                    //PrintLine("Cannot keep up! Fell " + (delay - 0.1) + " seconds behind.");
                    delay = 0.1;
                }
                clock += (delay) / ticklen;
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
                if (delay < 0.001)
                {
                    await Task.Delay(1); // sleep for timing accuracy
                }
                if (evs == 0)
                {
                    //PrintLine("Ran out of events, ending playback...");
                    Sound.Close();
                    return;
                }
            }
        }
    }
}