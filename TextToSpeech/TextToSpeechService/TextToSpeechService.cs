namespace TextToSpeech.TextToSpeechService
{
    using System;
    using System.IO;
    using Google.Cloud.TextToSpeech.V1;

    public class TextToSpeechService : ITextToSpeechService
    {
        public string VoiceName { get; }

        public TextToSpeechService(string voiceName)
        {
            VoiceName = voiceName;
        }

        public void Synthesize(string text, string outputPath)
        {
            var voiceNameSections = this.VoiceName.Split('-');
            if (voiceNameSections.Length < 2)
            {
                Console.WriteLine($"Cannot obtain language code from voice name {this.VoiceName}");
                throw new HandledException();
            }

            string languageCode = $"{voiceNameSections[0].ToLower()}-{voiceNameSections[1].ToUpper()}";

            var input = new SynthesisInput { Text = text };
            var audioConfig = new AudioConfig { AudioEncoding = AudioEncoding.Mp3 };
            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = languageCode,
                Name = VoiceName,
            };

            TextToSpeechClient client = TextToSpeechClient.Create();
            SynthesizeSpeechResponse response;
            try
            {
                response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during synthesizing text:\r\n{ex.Message}");
                throw new HandledException();
            }

            try
            {
                using (Stream output = File.Create(outputPath))
                {
                    response.AudioContent.WriteTo(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during saving mp3 file:\r\n{ex.Message}");
                throw new HandledException();
            }
        }
    }
}
