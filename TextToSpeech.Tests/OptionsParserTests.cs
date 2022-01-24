namespace TextToSpeech.Tests
{
    using System;
    using System.IO;
    using System.Reflection;
    using NUnit.Framework;
    using TextToSpeech.Options;

    public class OptionsParserTests
    {
        private readonly OptionsParser optionsParser = new OptionsParser();
        
        [Test]
        public void WhenNoArguments_ShouldNotDeliverOptions()
        {
            // arrange
            var args = new string[0];

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNull(options);
        }

        [Test]
        public void WhenOnlyPathInArgumentsAndFileExists_ShouldDeliverOptions()
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            var args = new[] { path };

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNotNull(options);
            Assert.AreEqual(path, options.FilePath);
        }

        [Test]
        public void WhenOnlyPathInArgumentsAndFileNotExists_ShouldNotDeliverOptions()
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            path = Path.Join(Path.GetDirectoryName(path), Guid.NewGuid().ToString());

            var args = new[] { path };

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNull(options);
        }
        
        [Test]
        public void WhenArgumentsForBulkFileContainsAllFullNameOptions_ShouldDeliverProperOptions()
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            var voice = "pl-PL-Wavenet-A";
            var indexFormat = "3";
            var indexFormatInteger = 3;
            var indexSeparator = "---";
            var chapterSeparator = "___";

            var args = new[]
            {
                path, 
                "--voice", voice,
                "--index-format", indexFormat,
                "--index-separator", indexSeparator,
                "--chapter-separator", chapterSeparator
            };

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNotNull(options);
            Assert.AreEqual(path, options.FilePath);
            Assert.AreEqual(false, options.IsOneFile);
            Assert.AreEqual(voice, options.VoiceName);
            Assert.AreEqual(indexFormatInteger, options.IndexFormat);
            Assert.AreEqual(indexSeparator, options.IndexSeparator);
            Assert.AreEqual(chapterSeparator, options.ChapterSeparator);
        }
        
        [Test]
        public void WhenArgumentsForBulkFileContainsAllAbbreviationOptions_ShouldDeliverProperOptions()
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            var voice = "pl-PL-Wavenet-A";
            var indexFormat = "3";
            var indexFormatInteger = 3;
            var indexSeparator = "---";
            var chapterSeparator = "___";

            var args = new[]
            {
                path,
                "-V", voice,
                "-F", indexFormat,
                "-S", indexSeparator,
                "-C", chapterSeparator
            };

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNotNull(options);
            Assert.AreEqual(path, options.FilePath);
            Assert.AreEqual(false, options.IsOneFile);
            Assert.AreEqual(voice, options.VoiceName);
            Assert.AreEqual(indexFormatInteger, options.IndexFormat);
            Assert.AreEqual(indexSeparator, options.IndexSeparator);
            Assert.AreEqual(chapterSeparator, options.ChapterSeparator);
        }
        
        [Test]
        public void WhenArgumentsForOneFileContainsAllFullNameOptions_ShouldDeliverProperOptions()
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            var voice = "pl-PL-Wavenet-A";

            var args = new[]
            {
                path, "--one-file",
                "--voice", voice
            };

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNotNull(options);
            Assert.AreEqual(path, options.FilePath);
            Assert.AreEqual(true, options.IsOneFile);
            Assert.AreEqual(voice, options.VoiceName);
        }
        
        [Test]
        public void WhenArgumentsForOneFileContainsAllAbbreviationOptions_ShouldDeliverProperOptions()
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            var voice = "pl-PL-Wavenet-A";

            var args = new[]
            {
                path, "-1",
                "-V", voice
            };

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNotNull(options);
            Assert.AreEqual(path, options.FilePath);
            Assert.AreEqual(true, options.IsOneFile);
            Assert.AreEqual(voice, options.VoiceName);
        }

        [TestCase("unknown")]
        [TestCase("-U")]
        [TestCase("--unknown-option")]
        public void WhenArgumentsContainsUnknownParameter_ShouldNotDeliverOptions(string unknownParameter)
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            var args = new[] { path, unknownParameter, "abc", "-V", "pl-PL-Wavenet-A" };

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNull(options);
        }

        [Test]
        public void WhenArgumentsContainsNoValueForOneParameter_ShouldNotDeliverOptions()
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            var args = new[]  {path, "-V" };
            
            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNull(options);
        }
        
        [Test]
        public void WhenArgumentsContainsTheSameParameterTwice_ShouldNotDeliverOptions()
        {
            // arrange
            var path = Assembly.GetExecutingAssembly().Location;
            var args = new[] { path, "-V", "pl-PL-Wavenet-A", "-V", "pl-PL-Wavenet-A" };

            // act
            var options = this.optionsParser.Parse(args);

            // assert
            Assert.IsNull(options);
        }
    }
}
