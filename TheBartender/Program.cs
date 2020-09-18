using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TheBartender.Services;

namespace TheBartender
{
    class Program
    {
        public static IConfigurationRoot Configuration;

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
                MySettings mySettings = services.GetRequiredService<MySettings>();
                var client = services.GetRequiredService<DiscordSocketClient>();
                
                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hardcoding.
                await client.LoginAsync(TokenType.Bot, mySettings.BotToken);
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
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddUserSecrets<Program>()
                .Build();
            
            MySettings mySettings = new MySettings();
            Configuration.GetSection("MySettings").Bind(mySettings);

            return new ServiceCollection()
                .AddSingleton(mySettings)
                .AddSingleton<HttpClient>()

                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                
                .AddSingleton<PictureService>()
                .AddSingleton<TwitchService>()
                .BuildServiceProvider();
        }
    }

    public class MySettings
    {
        public string BotToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public ulong Guild { get; set; }
        public ulong AnnouncementChannel { get; set; }
        public ulong PrimaryPerson { get; set; }
    }
}
