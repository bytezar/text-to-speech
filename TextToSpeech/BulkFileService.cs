namespace TextToSpeech
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using TextToSpeech.TextToSpeechService;

    //# <text> - comment
    //@$ <integer> - start index.
    //@^ <text> - custom chapter name. <text> should be not white spaces. Applied for current section.
    //@+ <text> - add sub chapter name. <text> should be not white spaces.
    //@@ <text> - replace sub chapter name. <text> should be not white spaces.
    //@-(n) - remove n sub chapter names. It counts minuses. E.g. @--- removes 3 sub chapter names.
    //@--@ <text> - syntactic sugar for: @-- and @@ <text>
    //@# - don't synthesize this section.
    //@! - synthesize only the sections with this markup 

    public class BulkFileService
    {
        private readonly ITextToSpeechService textToSpeechService;

        private class SectionDetails
        {
            public string Text { get; set; }
            public IList<string> Markups { get; set; }

            public int IndexStart { get; set; }
            public string Name { get; set; }

            public string UniqueName { get; set; }

            public bool Ignored { get; set; }
            public bool SynthesizeOnly { get; set; }
        }

        private enum MarkupAction
        {
            Nothing,
            Add,
            Replace,
            CustomName,
            Ignored,
            SynthesizeOnly
        }

        private class MarkupDetails
        {
            public MarkupAction MarkupAction { get; }
            public string Text { get; }

            public int? ToRemove { get; set; }
            public int? Index { get; set; }

            public MarkupDetails(MarkupAction markupAction)
            {
                this.MarkupAction = markupAction;
            }

            public MarkupDetails(MarkupAction markupAction, string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    this.MarkupAction = MarkupAction.Nothing;
                    this.Text = null;
                }
                else
                {
                    this.MarkupAction = markupAction;
                    this.Text = text;
                }
            }
        }
        

        public BulkFileService(ITextToSpeechService textToSpeechService)
        {
            this.textToSpeechService = textToSpeechService;
        }

        public void Run(Options.Options options)
        {
            IList<SectionDetails> content = this.ReadFileContent(options.FilePath);
            content = this.ProcessMarkups(content, options);
            this.ComputeUniqueNames(content, options);
            this.Synthesize(content, options);
        }

        private void Synthesize(IList<SectionDetails> content, Options.Options options)
        {
            bool markupSynthesizeOnlyOccur = content.Any(_ => _.SynthesizeOnly);
            var sectionsToSynthesize = markupSynthesizeOnlyOccur
                ? content.Where(_ => _.SynthesizeOnly).ToList()
                : content.Where(_ => !_.Ignored).ToList();

            string directoryPath = Path.GetDirectoryName(options.FilePath);
            foreach (var section in sectionsToSynthesize)
            {
                string outputPath = Path.Combine(directoryPath!, section.UniqueName + ".mp3");
                this.textToSpeechService.Synthesize(section.Text, outputPath);
            }
        }

        private void ComputeUniqueNames(IList<SectionDetails> content, Options.Options options)
        {
            var usedNames = new HashSet<string>();
            var needIndexSections = new List<SectionDetails>();

            foreach (var sectionsGroup in content.GroupBy(_ => _.Name))
            {
                if (sectionsGroup.Count() == 1 && !string.IsNullOrEmpty(sectionsGroup.Key))
                {
                    var section = sectionsGroup.First();
                    section.UniqueName = section.Name;
                    usedNames.Add(section.Name);
                }
                else
                {
                    needIndexSections.AddRange(sectionsGroup);
                }
            }

            string format = $"D{options.IndexFormat}";
            foreach (var section in needIndexSections)
            {
                int index = section.IndexStart;
                string fileName;

                do
                {
                    fileName = section.Name == string.Empty ? index.ToString(format) 
                        : section.Name + options.IndexSeparator + index.ToString(format);
                    index++;
                } while (usedNames.Contains(fileName));

                section.UniqueName = fileName;
                usedNames.Add(fileName);
            }
        }

        private IList<SectionDetails> ProcessMarkups(IList<SectionDetails> content, Options.Options options)
        {
            int indexStart = 1;
            List<string> chapterNames = new List<string>();

            var result = new List<SectionDetails>();
            foreach (var section in content)
            {
                string customName = null;
                foreach (var markup in section.Markups)
                {
                    var markupDetails = this.ParseMarkupDetails(markup);

                    if (markupDetails.MarkupAction == MarkupAction.Ignored)
                    {
                        section.Ignored = true;
                        break;
                    }

                    if (markupDetails.MarkupAction == MarkupAction.SynthesizeOnly)
                    {
                        section.SynthesizeOnly = true;
                        break;
                    }

                    if (markupDetails.MarkupAction == MarkupAction.CustomName)
                    {
                        customName = markupDetails.Text;
                        break;
                    }

                    if (markupDetails.Index.HasValue)
                    {
                        indexStart = markupDetails.Index.Value;
                        break;
                    }

                    if (markupDetails.ToRemove.HasValue)
                    {
                        for (int i = 0; i < markupDetails.ToRemove.Value; i++)
                        {
                            if (!chapterNames.Any())
                            {
                                break;
                            }

                            chapterNames.RemoveAt(chapterNames.Count - 1);
                        }
                    }

                    if (markupDetails.MarkupAction == MarkupAction.Add || markupDetails.MarkupAction == MarkupAction.Replace)
                    {
                        if (markupDetails.MarkupAction == MarkupAction.Replace && chapterNames.Any())
                        {
                            chapterNames.RemoveAt(chapterNames.Count - 1);
                        }

                        chapterNames.Add(markupDetails.Text);
                    }
                }
                
                if (string.IsNullOrEmpty(section.Text))
                {
                    continue;
                }

                if (section.Ignored && section.SynthesizeOnly)
                {
                    Console.WriteLine("Error. There is section with markup ignored and synthesize only.");
                    throw new HandledException();
                }

                section.IndexStart = indexStart;
                section.Name = customName ?? string.Join(options.ChapterSeparator, chapterNames);

                result.Add(section);
            }

            return result;
        }

        private MarkupDetails ParseMarkupDetails(string markup)
        {
            if (markup.StartsWith("@!"))
            {
                return new MarkupDetails(MarkupAction.SynthesizeOnly);
            }

            if (markup.StartsWith("@#"))
            {
                return new MarkupDetails(MarkupAction.Ignored);
            }

            if (markup.StartsWith("@$"))
            {
                int? startIndex = null;
                if (int.TryParse(markup[2..].Trim(), out int startIndexValue) && startIndexValue >= 0)
                {
                    startIndex = startIndexValue;
                }

                return new MarkupDetails(MarkupAction.Nothing)
                {
                    Index = startIndex,
                };
            }

            if (markup.StartsWith("@^"))
            {
                var text = markup[2..].Trim();
                return new MarkupDetails(MarkupAction.CustomName, text);
            }

            if (markup.StartsWith("@+"))
            {
                var text = markup[2..].Trim();
                return new MarkupDetails(MarkupAction.Add, text);
            }

            int index = 1;
            while (index < markup.Length && markup[index] == '-')
            {
                index++;
            }

            if (index < markup.Length && markup[index] == '@')
            {
                var text = markup[(index+1)..].Trim();
                return new MarkupDetails(MarkupAction.Replace, text)
                {
                    ToRemove = index > 1 ? (int?) (index - 1) : null
                };
            }

            return new MarkupDetails(MarkupAction.Nothing)
            {
                ToRemove = index > 1 ? (int?)(index - 1) : null
            };
        }

        private IList<SectionDetails> ReadFileContent(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            var result = new List<SectionDetails>();

            var textBuilder = new StringBuilder();
            var markups = new List<string>();

            void AddToResultIfNotEmpty()
            {
                if (textBuilder.Length > 0 || markups.Count > 0)
                {
                    result.Add(new SectionDetails()
                    {
                        Text = textBuilder.ToString(),
                        Markups = markups.ToList(),
                    });
                }

                textBuilder.Clear();
                markups.Clear();
            }

            foreach (string line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(trimmedLine))
                {
                    AddToResultIfNotEmpty();
                    continue;
                }

                if (trimmedLine.StartsWith("@"))
                {
                    markups.Add(trimmedLine);
                    continue;
                }

                textBuilder.AppendLine(trimmedLine);
            }

            AddToResultIfNotEmpty();

            return result;
        }
    }
}
