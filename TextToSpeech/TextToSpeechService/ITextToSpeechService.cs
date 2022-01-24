namespace TextToSpeech.TextToSpeechService
{
    public interface ITextToSpeechService
    {
        public void Synthesize(string text, string outputPath);
    }
}
