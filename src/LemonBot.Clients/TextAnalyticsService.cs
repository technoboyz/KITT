using Azure;
using Azure.AI.TextAnalytics;
using KITT.Shared;
using LemonBot.Clients.Configurations;
using Microsoft.Extensions.Options;

namespace LemonBot.Clients;

public class TextAnalyticsService
{
    private readonly TextAnalyticsOptions _options;

    private readonly AzureKeyCredential _credential;
    private readonly Uri _azureEndpoint;

    private readonly TextAnalyticsClient _client;

    public TextAnalyticsService(IOptions<TextAnalyticsOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _credential = new AzureKeyCredential(_options.ApiKey);
        _azureEndpoint = new Uri(_options.Endpoint);

        _client = new TextAnalyticsClient(_azureEndpoint, _credential);
    }

    public Sentiment Analyze(string input, string language)
    {
        var sentimentAnalyzed = _client.AnalyzeSentiment(input, language);
        return sentimentAnalyzed.Value.Sentiment switch
        {
            TextSentiment.Positive => Sentiment.Positive,
            TextSentiment.Negative => Sentiment.Negative,
            _ => Sentiment.Neutral
        };
    }

    public DetectedLanguage DetectLanguage(string input)
    {
        return _client.DetectLanguage(input);
    }
}
