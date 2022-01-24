namespace TextToSpeech
{
    using System.IO;
    using TextToSpeech.TextToSpeechService;

    public class OneFileService
    {
        private readonly ITextToSpeechService textToSpeechService;

        public OneFileService(ITextToSpeechService textToSpeechService)
        {
            this.textToSpeechService = textToSpeechService;
        }

        public void Run(Options.Options options)
        {
            string filePath = options.FilePath;
            string text = File.ReadAllText(filePath);

            string outputPath = (filePath.ToLower().EndsWith(".txt")
                ? filePath.Substring(0, filePath.Length - 4)
                : filePath) + ".mp3";

            this.textToSpeechService.Synthesize(text, outputPath);
        }
    }
}
