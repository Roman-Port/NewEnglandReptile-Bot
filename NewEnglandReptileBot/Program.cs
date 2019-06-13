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

        static void Main(string[] args)
        {
            Console.WriteLine("Loading the configuration file...");
            config = JsonConvert.DeserializeObject<BotConfigFile>(File.ReadAllText("config.json"));

            Console.WriteLine("Starting the Discord bot...");
            RunBot().GetAwaiter().GetResult();
        }

        static async Task RunTwitterTest(DiscordChannel c)
        {
            try
            {
                TwitterPost[] p = await TwitterApi.FetchNewPosts();
                foreach (var post in p)
                {
                    await c.SendMessageAsync(embed: SocialEmbedCreator.CreateTwitterEmbed(post));
                    //return;
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        static async Task RunTest(DiscordChannel c)
        {
            
        }

        static void LogMessage(string topic, string message)
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

                var channel = await discord.GetChannelAsync(config.notification_channel);
                //await RunTest(channel);
                await RunTwitterTest(channel);
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
