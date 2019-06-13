using DSharpPlus;
using DSharpPlus.Entities;
using InstagramApiSharp.API.Builder;
using NewEnglandReptileBot.Entities;
using NewEnglandReptileBot.SocialWorkers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NewEnglandReptileBot
{
    class Program
    {
        public static BotConfigFile config;
        public static DiscordClient discord;
        public static SocialStreamPointSaved saved_stream;

        public const string FOOTER_TEXT = "Bot by RomanPort#0001";

        static void Main(string[] args)
        {
            Console.WriteLine("Loading the configuration file...");
            config = JsonConvert.DeserializeObject<BotConfigFile>(File.ReadAllText("config.json"));

            Console.WriteLine("Loading the saved stream data...");
            saved_stream = new SocialStreamPointSaved();
            if(File.Exists("saved_stream.json"))
                saved_stream = JsonConvert.DeserializeObject<SocialStreamPointSaved>(File.ReadAllText("saved_stream.json"));

            Console.WriteLine("Starting the Discord bot...");
            RunBot().GetAwaiter().GetResult();
        }

        public static void LogMessage(string topic, string message)
        {
            Console.WriteLine(topic + " - " + message);
        }

        static async Task RunBot()
        {
            LogMessage("DiscordConnection", "Configurating...");
            DiscordConfiguration dc = new DiscordConfiguration
            {
                Token = config.access_token,
                TokenType = TokenType.Bot
            };
            discord = new DiscordClient(dc); //Create client
            discord.Ready += async e =>
            {
                LogMessage("DiscordConnection", "Connected to Discord.");
                await SocialTasks.RefreshSocial();

            };
            discord.MessageCreated += Discord_MessageCreated;
            LogMessage("DiscordConnection", "Connecting to Discord...");

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task Discord_MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            return null;
        }
    }
}
