﻿using LemonBot.Clients;
using LemonBot.Clients.Configurations;
using LemonBot.Clients.Extensions;
using LemonBot.Commands.Extensions;
using LemonBot.Commands.Options;
using LemonBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.Configure<TwitchBotOptions>(configuration.GetSection("TwitchBot"));
        services.Configure<GithubOptions>(configuration.GetSection("GitHub"));
        services.Configure<TextAnalyticsOptions>(configuration.GetSection("TextAnalytics"));

        services.AddSingleton<TextAnalyticsService>();

        services.AddTwitchClient();
        services.AddBotCommands();
        services.AddHostedService<TwitchBotService>();
    })
    .Build();

await host.RunAsync();