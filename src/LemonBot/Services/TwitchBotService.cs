using KITT.Shared.EventArgs;
using LemonBot.Clients;
using LemonBot.Commands;
using LemonBot.Commands.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace LemonBot.Services;

public class TwitchBotService : BackgroundService
{
    private readonly ILogger<TwitchBotService> _logger;
    private readonly TextAnalyticsService _analyticsService;
    private readonly TwitchClientProxy _client;

    private readonly BotCommandResolver _commandFactory;

    private HashSet<string> _usersAlreadyJoined;

    private HubConnection _connection;

    public TwitchBotService(TwitchClientProxy client, BotCommandResolver commandFactory, ILogger<TwitchBotService> logger, TextAnalyticsService analyticsService)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/bot")
            .Build();

        _usersAlreadyJoined = new();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection failed: {Message}", ex.Message);
        }

        InitializeTwitchClient();
        ConnectToTwitch();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        UnregisterClientEvents();

        await _connection.StopAsync();
        await base.StopAsync(cancellationToken);
    }

    private void InitializeTwitchClient()
    {
        _client.Initialize();
        RegisterClientEvents();
    }

    private void UnregisterClientEvents()
    {
        _client.RemoveClientEvents(c =>
        {
            c.OnLog -= OnClientLog;
            c.OnConnected -= OnClientConnected;
            c.OnConnectionError -= OnConnectionErrorOccured;
            c.OnChatCommandReceived -= OnChatCommandReceived;
            c.OnMessageReceived -= OnMessageReceived;
        });
    }

    private void RegisterClientEvents()
    {
        _client.ConfigureClientEvents(c =>
        {
            c.OnLog += OnClientLog;
            c.OnConnected += OnClientConnected;
            c.OnConnectionError += OnConnectionErrorOccured;
            c.OnChatCommandReceived += OnChatCommandReceived;
            c.OnMessageReceived += OnMessageReceived;
        });
    }

    private async Task ExecuteCommandByMessage(ChatMessage chatMessage)
    {
        var context = new BotCommandContext
        {
            UserName = chatMessage.Username,
            Message = chatMessage.Message,
            Connection = _connection
        };

        var command = _commandFactory.ResolveByMessage(chatMessage.Message);
        if (command is not null)
        {
            await command.ExecuteAsync(context);
        }
    }

    private async void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        _logger.LogInformation("{UserName} says: {Message}", e.ChatMessage.Username, e.ChatMessage.Message);

        var message = e.ChatMessage.Message;

        var language = _analyticsService.DetectLanguage(message);
        var sentiment = _analyticsService.Analyze(message, language.Iso6391Name);

        await _connection.InvokeAsync(
            "ShowSentiment",
            e.ChatMessage.Username, message, sentiment);

        //Console.WriteLine("<<<<SENTIMENT ANALYSIS>>>>>");
        //Console.WriteLine($"Document sentiment: {sentiment.Sentiment}\n");

        //foreach (var sentence in sentiment.Sentences)
        //{
        //    Console.WriteLine($"\tText: \"{sentence.Text}\"");
        //    Console.WriteLine($"\tSentence sentiment: {sentence.Sentiment}");
        //    Console.WriteLine($"\tPositive score: {sentence.ConfidenceScores.Positive:0.00}");
        //    Console.WriteLine($"\tNegative score: {sentence.ConfidenceScores.Negative:0.00}");
        //    Console.WriteLine($"\tNeutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");
        //}

        //Console.WriteLine("<<<<END SENTIMENT ANALYSIS>>>>>");
    }

    private async void OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {
        await ExecuteCommandByMessage(e.Command.ChatMessage);
    }

    private void OnConnectionErrorOccured(object? sender, OnConnectionErrorArgs e)
    {
        _logger.LogWarning("Error connecting {BotUsername}: {Error}", e.BotUsername, e.Error);
    }

    private void OnClientConnected(object? sender, OnConnectedArgs e)
    {
        _logger.LogInformation("{BotUsername} connected successfully", e.BotUsername);

        _client.Join();
        _client.SendMessage($"Hi everyone, I'm {e.BotUsername}");
    }

    private void OnClientLog(object? sender, OnLogArgs e)
    {
        _logger.LogInformation("[{LogDate}] Bot: {BotUsername}, Data: {Data}", e.DateTime, e.BotUsername, e.Data);
    }

    private void ConnectToTwitch() => _client.Connect();
}
