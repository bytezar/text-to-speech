namespace TextToSpeech.Tests
{
    using System.IO;
    using System.Reflection;
    using NUnit.Framework;
    using TextToSpeech.Options;

    public class BulkFileServiceTests
    {
        private TextToSpeechServiceForTest textToSpeechServiceForTest;
        private BulkFileService bulkFileService;

        [SetUp]
        public void SetUp()
        {
            this.textToSpeechServiceForTest = new TextToSpeechServiceForTest();
            this.bulkFileService = new BulkFileService(this.textToSpeechServiceForTest);
        }

        [TestCase("sample-1.txt", "sample-1__output-1.txt", null, null, " - ")]
        [TestCase("sample-1.txt", "sample-1__output-2.txt", 1, "__", "__")]
        [TestCase("sample-2.txt", "sample-2__output.txt", 3, "-")]
        [TestCase("sample-3.txt", "sample-3__output.txt", 3, "-")]
        public void WhenNoArguments_ShouldNotDeliverOptions(string sampleFile, string outputFile, 
            int? format = null, string indexSeparator = null, string characterSeparator = null)
        {
            // arrange
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            directory = Path.Join(directory, "Samples");

            var outputPath = Path.Join(directory, outputFile);
            var expectedText = File.ReadAllText(outputPath);

            var options = new Options { FilePath = Path.Join(directory, sampleFile) };
            if (format.HasValue)
            {
                options.IndexFormat = format.Value;
            }

            if (indexSeparator != null)
            {
                options.IndexSeparator = indexSeparator;
            }

            if (characterSeparator != null)
            {
                options.ChapterSeparator = characterSeparator;
            }

            // act
            this.bulkFileService.Run(options);
            
            // assert
            var header = GetHeader(options);
            var text = this.textToSpeechServiceForTest.GetSynthesizedFiles();
            var actualText = $"{header}\r\n\r\n{text}";

            Assert.AreEqual(expectedText, actualText);
        }

        private string GetHeader(Options options)
        {
            var defaultOptions = new Options();
            
            var file = Path.GetFileName(options.FilePath);
            var header = $"# Call: TextToSpeech.exe \"{file}\"";
            
            if (options.IndexFormat != defaultOptions.IndexFormat)
            {
                header += $" --index-format {options.IndexFormat}";
            }

            if (options.IndexSeparator != defaultOptions.IndexSeparator)
            {
                header += $" --index-separator \"{options.IndexSeparator}\"";
            }

            if (options.ChapterSeparator != defaultOptions.ChapterSeparator)
            {
                header += $" --chapter-separator \"{options.ChapterSeparator}\"";
            }

            return header;
        }
    }
}
