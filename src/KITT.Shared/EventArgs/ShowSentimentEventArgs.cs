namespace KITT.Shared.EventArgs
{
    public record ShowSentimentEventArgs
    {
        public string UserName { get; init; } = string.Empty;

        public string Sentence { get; set; } = string.Empty;

        public Sentiment Sentiment { get; set; } = Sentiment.Neutral;
    }
}
