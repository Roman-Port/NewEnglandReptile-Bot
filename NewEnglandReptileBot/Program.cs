using DSharpPlus;
using DSharpPlus.Entities;
using InstagramApiSharp.API.Builder;
using LiteDB;
using NewEnglandReptileBot.ActionTools;
using NewEnglandReptileBot.Entities;
using NewEnglandReptileBot.Entities.Persist;
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
        public static LiteDatabase db;
        public static LiteCollection<DiscordUserData> db_users;

        public const string FOOTER_TEXT = "Bot by RomanPort#0001 • Version 6/13/19";

        static void Main(string[] args)
        {
            Console.WriteLine("Loading the configuration file...");
            config = JsonConvert.DeserializeObject<BotConfigFile>(File.ReadAllText("config.json"));

            Console.WriteLine("Loading the saved stream data...");
            saved_stream = new SocialStreamPointSaved();
            if(File.Exists("saved_stream.json"))
                saved_stream = JsonConvert.DeserializeObject<SocialStreamPointSaved>(File.ReadAllText("saved_stream.json"));

            Console.WriteLine("Opening the database...");
            db = new LiteDatabase("database.db");
            db_users = db.GetCollection<DiscordUserData>("members");

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
                //await SocialTasks.RefreshSocial();

            };
            discord.MessageCreated += Discord_MessageCreated;
            discord.GuildMemberAdded += Discord_GuildMemberAdded;
            LogMessage("DiscordConnection", "Connecting to Discord...");

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task Discord_GuildMemberAdded(DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            //Check if this user has any outstanding bans or mutes
            DiscordUserData data = BotTools.GetUserDataById(e.Member.Id);
            if (data.temp_banned.CheckIfActive())
            {
                await BanTools.OnBannedMemberJoined(data, e.Guild, e.Member);
                return;
            }
        }

        private static async Task Discord_MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            //Check if this a command
            try
            {
                string message = e.Message.Content;
                if (message.StartsWith(config.command_prefix[0]) && !e.Author.IsBot && e.Guild != null)
                {
                    //Get the command
                    message = message.Substring(1);
                    int indexOfBreak = message.IndexOf(' ');
                    if (indexOfBreak != -1)
                    {
                        string prefix = message.Substring(0, indexOfBreak).TrimEnd(' ');
                        message = message.Substring(indexOfBreak).TrimStart(' ');
                        await CommandProcessor.OnCommand(e, prefix, message);
                    } else
                    {
                        await CommandProcessor.OnCommand(e, message, "");
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }

            if(e.Guild != null)
            {
                //Get the member data and update if needed
                DiscordUserData data = BotTools.GetUserDataById(e.Author.Id);
                string name = $"{e.Author.Username}#{e.Author.Discriminator}";
                if(data.latest_name != name)
                {
                    data.latest_name = name;
                    data.Save();
                }
            }
        }
    }
}
