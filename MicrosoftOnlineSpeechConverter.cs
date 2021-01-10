using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
namespace VoiceControl
{

    internal  class ContosoAudioStream : PullAudioInputStreamCallback
    {
        MemoryStream stream;
        public MemoryStream Stream
        {
            get => stream;
            set
            {
                stream = value;
                stream.Position = 0;
            }
        }

        public override int Read(byte[] dataBuffer, uint size)
        {
            return stream.Read(dataBuffer, 0, (int)size);
        }
    }



    public class MicrosoftOnlineSpeechConverter : ISpeechToTextConverter
    {
        SpeechRecognizer recognizer;
        ContosoAudioStream cas;
        ISetting<string> subscriptionKeys;
        ISetting<string> region;
        public MicrosoftOnlineSpeechConverter(ISettings settings)
        {
            subscriptionKeys=settings.Create("subscriptionKeys", "Please insert Key");
            region = settings.Create("region", "Please insert region");
        }
        public void Configure(string language)
        {
            cas = new ContosoAudioStream();
            var audioConfig = AudioConfig.FromStreamInput(cas);
            var sourceLanguageConfig = SourceLanguageConfig.FromLanguage(language);
            var config = SpeechConfig.FromSubscription(subscriptionKeys.Value, region.Value);
            recognizer = new SpeechRecognizer(config, sourceLanguageConfig, audioConfig);
        }

        public void Dispose()
        {
            recognizer?.Dispose();
            recognizer = null;
        }

        public async Task<string> RecognizeSpeechAsync(MemoryStream stream)
        {
            cas.Stream = stream;
            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"We recognized: {result.Text}");
                return result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                }
            }
            return "";
        }
    }
}

