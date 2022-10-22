using System.Diagnostics;

namespace SharpMIDI
{
    class MIDIClock
    {
        static double time = 0f;
        static double bpm = 120f;
        public static double ppq = 0;
        static double ticklen;
        static Stopwatch test = new Stopwatch();
        public static void Start()
        {
            test.Start();
            ticklen = (1 / (double)ppq) * (60 / bpm);
        }

        public static void SubmitBPM(double b)
        {
            time+=((test.ElapsedTicks / TimeSpan.TicksPerSecond) / ticklen);
            bpm = 60000000 / b;
            ticklen = (1 / (double)ppq) * (60 / bpm);
            test.Restart();
        }

        public static double GetTick()
        {
            return time + ((test.ElapsedTicks / TimeSpan.TicksPerSecond) / ticklen);
        }
    }
}