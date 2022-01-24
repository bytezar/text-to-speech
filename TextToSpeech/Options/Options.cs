namespace TextToSpeech.Options
{
    public class Options
    {
        public string FilePath { get; set; }
        

        [ConsoleParameter("one-file", "1", true)]
        public bool IsOneFile { get; set; }


        [ConsoleParameter("voice", "V")]
        public string VoiceName { get; set; } = "en-US-Wavenet-D";
        

        [ConsoleParameter("index-format", "F")]
        public int IndexFormat { get; set; } = 2;

        [ConsoleParameter("index-separator", "S")]
        public string IndexSeparator { get; set; } = "_";

        [ConsoleParameter("chapter-separator", "C")]
        public string ChapterSeparator { get; set; } = "_";
    }
}
