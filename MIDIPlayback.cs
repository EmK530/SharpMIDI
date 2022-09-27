namespace SharpMIDI
{
    unsafe class MIDIPlayer
    {
        private static MidiTrack[] tracks = new MidiTrack[0];
        static void PrintLine(string str){
            if(!UserInput.silent){
                Console.WriteLine(str);
            }
        }
        static void Print(string str){
            if(!UserInput.silent){
                Console.Write(str);
            }
        }
        public static void SubmitTrackCount(int count)
        {
            tracks = new MidiTrack[count];
        }
        public static void SubmitTrackForPlayback(int index, MidiTrack track)
        {
            tracks[index] = track;
        }
        static double tick(){
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            double secondsSinceEpoch = t.TotalSeconds;
            return secondsSinceEpoch;
        }
        public static void StartPlayback(uint ppq,long notes)
        {
            PrintLine("Now playing...");
            double bpm = 120;
            double ticklen = (1/(double)ppq)*(60/bpm);
            double clock = 0;
            double timeSinceLastPrint = tick();
            int totalFrames = 0;
            double totalDelay = 0;
            long[] trackPositions = new long[tracks.Length];
            int[] eventProgress = new int[tracks.Length];
            int[] tempoProgress = new int[tracks.Length];
            System.Diagnostics.Stopwatch? watch = System.Diagnostics.Stopwatch.StartNew();
            while(true){
                if(tick()-timeSinceLastPrint >= 1){
                    PrintLine("FPS: "+(double)totalFrames/totalDelay);
                    timeSinceLastPrint = tick();
                    totalFrames = 0;
                    totalDelay = 0;
                }
                long watchtime = watch.ElapsedTicks;
                watch.Stop();
                watch = System.Diagnostics.Stopwatch.StartNew();
                double delay = (double)watchtime / TimeSpan.TicksPerSecond;
                if(delay > 0.1){
                    PrintLine("Cannot keep up! Fell "+(delay-0.1)+" seconds behind.");
                    delay = 0.1;
                }
                totalDelay+=delay;
                clock+=(delay)/ticklen;
                int evs = 0;
                int loops = -1;
                foreach(MidiTrack i in tracks){
                    loops++;
                    while(true)
                    {
                        if(tempoProgress[loops] < i.tempoAmount){
                            Tempo ev = i.tempos[tempoProgress[loops]];
                            evs++;
                            if(ev.pos <= clock){
                                double lastbpm = bpm;
                                bpm=60000000/(double)ev.tempo;
                                ticklen=(1/(double)ppq)*(60/bpm);
                                tempoProgress[loops]++;
                                if(bpm != lastbpm){
                                    PrintLine("Tempo event, new BPM: "+bpm);
                                }
                            } else {
                                break;
                            }
                        } else {
                            break;
                        }
                    }
                    while(true)
                    {
                        if(eventProgress[loops] < i.eventAmount){
                            SynthEvent ev = i.synthEvents[eventProgress[loops]];
                            evs++;
                            if(trackPositions[loops]+(long)ev.pos <= clock){
                                eventProgress[loops]++;
                                trackPositions[loops]+=(long)ev.pos;
                                if(ev.val != 0){
                                    Sound.Submit((uint)ev.val);
                                }
                            } else {
                                break;
                            }
                        } else {
                            break;
                        }
                    }
                }
                totalFrames++;
                System.Threading.Thread.Sleep(1);
                if(evs==0){
                    PrintLine("Ran out of events, ending playback...");
                    Sound.Close();
                    return;
                }
            }
        }
    }
}
