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
        static double last = 0;
        public static bool throttle = true;
        public static void Start()
        {
            test.Start();
            time = 0f;
            last = 0;
            ticklen = (1 / (double)ppq) * (60 / bpm);
        }

        public static void Reset()
        {
            time = 0f;
            last = 0;
        }

        static double GetElapsed()
        {
            double temp = ((double)test.ElapsedTicks / TimeSpan.TicksPerSecond);
            if (throttle)
            {
                if (temp-last > 0.0166666d)
                {
                    temp = last + 0.0166666d;
                    last = temp;
                    return temp;
                }
            }
            last = temp;
            return temp;
        }

        public static void SubmitBPM(double p, double b)
        {
            double remainder = (time - p);
            time = p + (GetElapsed() / ticklen);
            bpm = 60000000 / b;
            Console.WriteLine("New BPM: " + bpm);
            ticklen = (1 / (double)ppq) * (60 / bpm);
            time += remainder;
            test.Restart();
        }

        public static double GetTick()
        {
            return time + (GetElapsed() / ticklen);
        }

        public static void Stop()
        {
            test.Stop();
        }

        public static void Resume()
        {
            test.Start();
        }
    }
}
