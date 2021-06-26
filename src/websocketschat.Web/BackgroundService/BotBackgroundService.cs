using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using websocketschat.Bot.Interfaces;

namespace websocketschat.Web.BackgroundService
{
    public class BotBackgroundService : IHostedService
    {
        IBotManager _bot;
        public BotBackgroundService(IBotManager bot)
        {
            _bot = bot;
        }
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _bot.RegisterBotAsync("http://79.143.31.13/api/accounts/register");
            _bot.AuthBotAsync("http://79.143.31.13/api/accounts/token",
                "http://79.143.31.13/chat/negotiate?negotiateVersion=1",
                "http://79.143.31.13/chat");

            try
            {
                _bot.OnAsync();
            }
            catch
            {
                StartAsync(stoppingToken);
            }

            return Task.CompletedTask;
        }
        

        public Task StopAsync(CancellationToken cancellationToken)
        {
           _bot.DisconnectAsync();
           return Task.CompletedTask;
        }
    }
}
