using System.Diagnostics;

namespace SharpMIDI
{
    class MIDIClock
    {
        static double time = 0f;
        public static double bpm = 120d;
        public static double ticklen;
        public static Stopwatch test = new Stopwatch();
        static double last = 0;
        public static bool throttle = true;
        static double timeLost = 0;
        public static void Start()
        {
            test.Start();
            ticklen = (1 / (double)MIDIData.ppq) * (60 / bpm);
        }

        public static void Reset()
        {
            time = 0f;
            last = 0;
            timeLost = 0f;
            test.Reset();
        }

        static double GetElapsed()
        {
            double temp = ((double)test.ElapsedTicks / TimeSpan.TicksPerSecond);
            if (throttle)
            {
                if (temp-last > 0.0166666d)
                {
                    timeLost += (temp - last) - 0.0166666d;
                    last = temp;
                    return temp-timeLost;
                }
            }
            last = temp;
            return temp-timeLost;
        }

        public static void SubmitBPM(double p, double b)
        {
            double remainder = (time - p);
            time = p + (GetElapsed() / ticklen);
            bpm = 60000000 / b;
            timeLost = 0d;
            Console.WriteLine("New BPM: " + bpm);
            ticklen = (1 / (double)MIDIData.ppq) * (60 / bpm);
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
