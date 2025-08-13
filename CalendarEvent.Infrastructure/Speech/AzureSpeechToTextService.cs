using CalendarEvent.Application.Services;
using Microsoft.Extensions.Logging;

namespace CalendarEvent.Infrastructure.Speech
{
    public class AzureSpeechToTextService : ISpeechToTextService
    {
        private readonly ILogger<AzureSpeechToTextService> _logger;
        private readonly string _subscriptionKey;
        private readonly string _region;
        private readonly HttpClient _httpClient;

        public AzureSpeechToTextService(
            ILogger<AzureSpeechToTextService> logger,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            // Get Azure Speech Service configuration
            _subscriptionKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY") ??
                throw new InvalidOperationException("AZURE_SPEECH_KEY environment variable is required");
            _region = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION") ?? "eastus";
        }

        public async Task<SpeechToTextResult> TranscribeAsync(Stream audioStream, string language = "ru-RU", CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting speech transcription for language: {Language}", language);

                // Convert audio stream to byte array
                var audioData = await ReadStreamToByteArrayAsync(audioStream);
                
                if (audioData.Length == 0)
                {
                    return SpeechToTextResult.Failure("Audio stream is empty");
                }

                // Azure Speech Service REST API endpoint
                var endpoint = $"https://{_region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1";
                var uri = $"{endpoint}?language={language}&format=detailed";

                // Prepare the request
                using var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                request.Headers.Add("Accept", "application/json");
                
                // Set content type based on audio format (assuming OGG from Telegram)
                request.Content = new ByteArrayContent(audioData);
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/ogg");

                _logger.LogDebug("Sending request to Azure Speech Service, audio size: {Size} bytes", audioData.Length);

                // Send the request
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Azure Speech Service returned error: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    return SpeechToTextResult.Failure($"Speech service error: {response.StatusCode}");
                }

                // Parse the response
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Azure Speech Service response: {Response}", responseContent);

                var result = ParseAzureResponse(responseContent);
                
                _logger.LogInformation("Speech transcription completed. Success: {Success}, Text: '{Text}'", 
                    result.IsSuccess, result.TranscribedText);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during speech transcription");
                return SpeechToTextResult.Failure($"Transcription error: {ex.Message}");
            }
        }

        private static async Task<byte[]> ReadStreamToByteArrayAsync(Stream stream)
        {
            if (stream is MemoryStream memoryStream)
            {
                return memoryStream.ToArray();
            }

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        private SpeechToTextResult ParseAzureResponse(string responseContent)
        {
            try
            {
                // Simple JSON parsing for Azure Speech Service response
                // In production, you might want to use System.Text.Json or Newtonsoft.Json
                
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return SpeechToTextResult.Failure("Empty response from speech service");
                }

                // Look for "DisplayText" field in the JSON response
                var displayTextIndex = responseContent.IndexOf("\"DisplayText\":");
                if (displayTextIndex == -1)
                {
                    return SpeechToTextResult.Failure("No DisplayText found in response");
                }

                var startQuote = responseContent.IndexOf('"', displayTextIndex + 14);
                if (startQuote == -1)
                {
                    return SpeechToTextResult.Failure("Invalid DisplayText format");
                }

                var endQuote = responseContent.IndexOf('"', startQuote + 1);
                if (endQuote == -1)
                {
                    return SpeechToTextResult.Failure("Invalid DisplayText format");
                }

                var transcribedText = responseContent.Substring(startQuote + 1, endQuote - startQuote - 1);
                
                if (string.IsNullOrWhiteSpace(transcribedText))
                {
                    return SpeechToTextResult.Failure("No speech detected");
                }

                // Look for confidence score
                var confidence = ExtractConfidence(responseContent);

                return SpeechToTextResult.Success(transcribedText, confidence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Azure Speech Service response");
                return SpeechToTextResult.Failure("Failed to parse speech service response");
            }
        }

        private double ExtractConfidence(string responseContent)
        {
            try
            {
                var confidenceIndex = responseContent.IndexOf("\"Confidence\":");
                if (confidenceIndex == -1) return 0.8; // Default confidence

                var valueStart = responseContent.IndexOf(':', confidenceIndex) + 1;
                var valueEnd = responseContent.IndexOfAny(new[] { ',', '}' }, valueStart);
                
                if (valueEnd == -1) return 0.8;

                var confidenceStr = responseContent.Substring(valueStart, valueEnd - valueStart).Trim();
                
                if (double.TryParse(confidenceStr, out var confidence))
                {
                    return confidence;
                }

                return 0.8; // Default confidence
            }
            catch
            {
                return 0.8; // Default confidence
            }
        }
    }
}
