namespace SharpMIDI
{
    unsafe class MIDIPlayer
    {
        private static MidiTrack[] tracks = new MidiTrack[0];
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
            Console.WriteLine("Now playing...");
            double start = tick();
            double bpm = 120;
            double ticklen = (1/(double)ppq)*(60/bpm);
            double clock = 0;
            double timeSinceLastPrint = tick();
            int totalFrames = 0;
            double totalDelay = 0;
            int[] eventProgress = new int[tracks.Length];
            int[] tempoProgress = new int[tracks.Length];
            while(true){
                if(tick()-timeSinceLastPrint >= 1){
                    Console.WriteLine("FPS: "+totalFrames/totalDelay);
                    timeSinceLastPrint = tick();
                    totalFrames = 0;
                    totalDelay = 0;
                }
                double delay = tick()-start;
                totalDelay+=delay;
                start=tick();
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
                                bpm=60000000/(double)ev.tempo;
                                ticklen=(1/(double)ppq)*(60/bpm);
                                tempoProgress[loops]++;
                                Console.WriteLine("Tempo event, new BPM: "+bpm);
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
                            if(ev.pos <= clock){
                                eventProgress[loops]++;
                                Sound.Submit((uint)ev.val);
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
                    Console.WriteLine("Ran out of events, ending playback...");
                    Sound.Close();
                    return;
                }
            }
        }
    }
}