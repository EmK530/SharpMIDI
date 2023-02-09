namespace SharpMIDI
{
    public class Lang
    {
        public static LangObj GetLang(string lang)
        {
            LangObj l = new LangObj();
            switch (lang)
            {
                case "English":
                    l.Menus = Lang_English.Menus;
                    l.Other = Lang_English.Other;
                    break;
                case "Swedish":
                    l.Menus = Lang_Swedish.Menus;
                    l.Other = Lang_Swedish.Other;
                    break;
            }
            return l;
        }
    }
    public struct LangObj
    {
        public List<string> Menus = new List<string>(){"?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?"};
        public List<string> Other = new List<string>(){"?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?"};
        public LangObj() {}
    }
    struct Lang_English
    {
        public static List<string> Menus = new List<string>()
        {
            "File",
            "Open MIDI",
            "Unload MIDI",
            "Not implemented",
            "Options",
            "Synth",
            "Load KDMAPI",
            "Load WinMM",
            "Load XSynth",
            "Unload Synth",
            "Reload Synth",
            "Limit FPS",
            "Misc",
            "(placeholder)",
            "Language"
        };
        public static List<string> Other = new List<string>()
        {
            "Loaded Synth",
            "Loaded MIDI",
            "Status: Loading",
            "Status: Idle",
            "Status: Loaded",
            "Memory Usage",
            "Notes",
            "PPQ",
            "Tracks",
            "Average FPS",
            "Played events",
            "Tick",
            "TPS",
            "BPM",
            "Start",
            "Please select a synth in Options -> Synth before starting!",
            "Pause",
            "Play",
            "Stop",
            "MIDI Loading...",
            "Speed"
        };
    }
    struct Lang_Swedish
    {
        public static List<string> Menus = new List<string>()
        {
            "Fil",
            "Öppna MIDI",
            "Ladda ur MIDI",
            "Ej implementerat",
            "Inställningar",
            "Synth",
            "Ladda KDMAPI",
            "Ladda WinMM",
            "Ladda XSynth",
            "Ladda ur Synth",
            "Ladda om Synth",
            "Begränsa FPS",
            "Misc",
            "(platshållare)",
            "Språk"
        };
        public static List<string> Other = new List<string>()
        {
            "Laddad Synth",
            "Laddad MIDI",
            "Status: Laddar",
            "Status: Inaktiv",
            "Status: Laddad",
            "Minnesanvändning",
            "Noter",
            "PPQ",
            "Spår",
            "Genomsnittlig FPS",
            "Spelade händelser",
            "Tick",
            "TPS",
            "BPM",
            "Starta",
            "Snälla välj en synth i Inställningar -> Synth innan du startar!",
            "Pausa",
            "Spela",
            "Stoppa",
            "MIDI Laddar...",
            "Hastighet"
        };
    }
}