using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TheBartender.Services;

namespace TheBartender
{
    class Program
    {
        public static string botToken = "NzU0NzI4NzA2NTEzMDQzNTU2.X1494Q.JvYfswZslm_qqW08b-MAwt_TJOs";
        public static string clientId = "754728706513043556";
        public static string clientSecret = "U1gnI-PYGdZPmaahaSjm4Aqvwp20tJ_h";

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // You should dispose a service provider created using ASP.NET
            // when you are finished using it, at the end of your app's lifetime.
            // If you use another dependency injection framework, you should inspect
            // its documentation for the best way to do this.
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                
                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hardcoding.
                await client.LoginAsync(TokenType.Bot, botToken);
                await client.StartAsync();

                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                while(client.ConnectionState != ConnectionState.Connected)
                {
                    await Task.Delay(5000);
                }

                services.GetRequiredService<TwitchService>();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<HttpClient>()

                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                
                .AddSingleton<PictureService>()
                .AddSingleton<TwitchService>()
                .BuildServiceProvider();
        }
    }
}
