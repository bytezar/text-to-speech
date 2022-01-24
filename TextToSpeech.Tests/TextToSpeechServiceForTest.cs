namespace TextToSpeech.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using TextToSpeech.TextToSpeechService;

    public class TextToSpeechServiceForTest : ITextToSpeechService
    {
        private readonly IList<(string filePath, string text)> synthesizedFiles = new List<(string filePath, string text)>();
        
        public void Synthesize(string text, string outputPath)
        {
            string file = Path.GetFileName(outputPath);
            this.synthesizedFiles.Add((file, text));
        }

        public string GetSynthesizedFiles()
        {
            return string.Join("\r\n", this.synthesizedFiles.Select(_ => $"***** {_.filePath} *****\r\n{_.text}"));
        }

        public void Clear()
        {
            this.synthesizedFiles.Clear();
        }
    }
}
