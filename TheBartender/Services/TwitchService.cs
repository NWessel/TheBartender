using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;

namespace TheBartender.Services
{
    public class TwitchService
    {
        private readonly DiscordSocketClient _discord;
        private TwitchAPI client;
        private List<string> quotes;
        private MySettings _mySettings;

        public TwitchService(MySettings mySettings, DiscordSocketClient discord)
        {
            _mySettings = mySettings;
            _discord = discord;
            client = new TwitchAPI();
            client.Settings.ClientId = mySettings.ClientId;
            client.Settings.Secret = mySettings.ClientSecret;
            client.Settings.Scopes = new List<TwitchLib.Api.Core.Enums.AuthScopes>() { TwitchLib.Api.Core.Enums.AuthScopes.Channel_Stream };

            InitQuotes();
            StartTask();
        }

        private void InitQuotes()
        {
            string mentionEveryone = _discord.GetGuild(_mySettings.Guild).EveryoneRole.Mention;
            string mentionJon = _discord.GetGuild(_mySettings.Guild).GetUser(_mySettings.PrimaryPerson).Mention;
            string streamUrl = "http://twitch.tv/JonJLevesque";
            quotes = new List<string>()
            {
                $"Hey! {mentionEveryone} - Wake up! Get a fresh drink! The Stream just went Live!! {streamUrl}",
                $"Its time to shake off that hangover and get into the chat {mentionEveryone}! The Stream just went live and {mentionJon} is about to get his ass kicked in something... come point and laugh! {streamUrl}"
            };
        }

        private void StartTask()
        {
            Task.Run(async () =>
            {
                bool streamIsLive = false;
                var channel = _discord.GetChannel(_mySettings.AnnouncementChannel) as SocketTextChannel; // Gets the channel to send the message in
                
                while (true)
                {

                    var streams = await client.Helix.Streams.GetStreamsAsync(userLogins: new List<string>() { "jonjlevesque" });
                    Console.WriteLine(streams);
                    if(streams.Streams.Length == 0)
                    {
                        streamIsLive = false;
                    }
                    else
                    {
                        if (!streamIsLive)
                        {
                            await channel.SendMessageAsync(GetRandomQuote()); //Welcomes the new user
                            streamIsLive = true;
                        }
                    }
                    await Task.Delay(10000);
                }
            });
        }

        private string GetRandomQuote()
        {
            Random rand = new Random();
            int number = rand.Next(quotes.Count);
            return quotes[number];
        }
    }
}
