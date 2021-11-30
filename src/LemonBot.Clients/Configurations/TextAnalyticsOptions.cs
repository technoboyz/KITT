namespace LemonBot.Clients.Configurations;

public record TextAnalyticsOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;
}
