namespace TextToSpeech
{
    using System;
    using TextToSpeech.Options;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var optionsParser = new OptionsParser();
                var options = optionsParser.Parse(args);
                if (options == null)
                {
                    return;
                }

                var textToSpeechService =
                    new TextToSpeechService.TextToSpeechService(options.VoiceName);

                if (options.IsOneFile)
                {
                    var service = new OneFileService(textToSpeechService);
                    service.Run(options);
                }
                else
                {
                    var service = new BulkFileService(textToSpeechService);
                    service.Run(options);
                }
            }
            catch (HandledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while executing program:\r\n{ex.Message}");
            }
            
        }
    }
}
