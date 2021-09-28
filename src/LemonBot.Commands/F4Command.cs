using LemonBot.Commands.Attributes;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace LemonBot.Commands
{
    [BotCommand("!f4", Comparison = CommandComparison.Equal, HelpText = "F4, Basito")]
    public class F4Command : IBotCommand
    {
        public async Task ExecuteAsync(BotCommandContext context)
        {
            if (context.Connection.State == HubConnectionState.Disconnected)
            {
                await context.Connection.StartAsync();
            }

            await context.Connection.InvokeAsync(
                "SendOverlay",
                "");
        }
    }
}
