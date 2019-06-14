using DSharpPlus.Entities;
using NewEnglandReptileBot.Entities.Persist;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.Commands
{
    public static class BanCommand
    {
        public static async Task OnCmd(DSharpPlus.EventArgs.MessageCreateEventArgs e, string prefix, string content, DiscordMember member, DiscordPermissionLevel perms, DiscordUserData data)
        {
            //Check if this is prompting for help
            string[] split = content.Split(',');
            if (content.Length == 0 || split.Length < 3)
            {
                await ReturnHelp(e);
                return;
            }

            //Parse the message parts
            string memberString = split[0];
            string timeString = split[1];
            string messageString = content.Substring(2 + memberString.Length + timeString.Length);

            //Find the user
            DiscordUser victim = await BotTools.ParseName(e.Guild, memberString.Trim(' '));

            //If the bot failed to find the user, throw an error
            if (victim == null)
            {
                await ReturnError(e, "Failed to Find User", $"Failed to find the user, ``{memberString.Trim(' ')}``. You can use their ID, their full name, or their first name. You can also mention them.");
                return;
            }

            //Find time
            DateTime time;
            int timeParseErrorCount = 0;
            int timeParseCount = 0;
            if (timeString.Trim(' ').ToLower() != "never")
            {
                time = BotTools.ParseTime(timeString.Trim(' '), out timeParseErrorCount, out timeParseCount);
            } else
            {
                await ReturnError(e, "Cannot Perma Ban", "Please use the ban function built into Discord to permaban a member.");
                return;
            }

            //If the time parse didn't find any, or if there were any errors, stop
            if ((timeParseCount == 0 || timeParseErrorCount > 0))
            {
                await ReturnError(e, "Failed to Parse Time", "There was a problem parsing the time you entered. Do not include commas in the time, make sure you use spaces to separate each part, and make sure you do not use weeks. For help, do not include any arguments and run the command again.");
                return;
            }

            //Limit message to 900 characters
            if (messageString.Length > 900)
            {
                await ReturnError(e, "Reason is Too Long", "Sorry, the reason is limited by Discord to 900 characters.");
                return;
            }

            //Get the user data for the person we just banned and apply.
            DiscordUserData victimUserData = BotTools.GetUserDataById(victim.Id);
            victimUserData.latest_name = $"{victim.Username}#{victim.Discriminator}";
            victimUserData.temp_banned = new DiscordUserDataStatusBanned
            {
                applied_since = DateTime.UtcNow.Ticks,
                catalyst = e.Author.Id,
                expiry = time.Ticks,
                is_applied = true,
                is_automated = false,
                reason = messageString
            };

            //Apply the ban to the account
            await ActionTools.BanTools.DoBanMember(victimUserData, e.Guild, victim, e.Author, messageString, false, time);

            //Save
            victimUserData.Save();

            //Write OK
            await ReturnOk(e, victim, time);
        }

        public static async Task ReturnOk(DSharpPlus.EventArgs.MessageCreateEventArgs e, DiscordUser victim, DateTime time)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "User Temporarily Banned";
            builder.Description = $"{victim.Username}#{victim.Discriminator} ({victim.Id}) has been banned. They will be unbanned in {BotTools.DateTimeOffsetToString(time - DateTime.UtcNow).TrimEnd(' ')}.";
            builder.Color = DiscordColor.Green;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            await e.Message.RespondAsync(embed: builder.Build());
        }

        public static async Task ReturnHelp(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "Temp Ban Help";
            builder.Description = $"``{{member}}, {{time}}, {{message}}``\n\nThe ban command will temporarily ban";
            builder.AddField("Usage", "To temporarily ban a member, type their name followed by comma and the duration. Then, add a comma and include a message. If you would like a ban to never expire, use \"never\" as a time.");
            builder.AddField("Remarks", "Bans **__are not anonymous__** and your username will be included on the direct message sent to the member.");
            builder.AddField("Examples", $"``{Program.config.command_prefix}tempban RomanPort#0001, 2 minutes, Some message here, up to 900 characters. This user can return in 2 minutes.``\n\n``{Program.config.command_prefix}tempban RomanPort#0001, 2 days, This will strike for two days, then expire.``\n\n``{Program.config.command_prefix}tempban RomanPort#0001, 2 days 1 year, You can have more advanced timeframes here, but you cannot include commas.``");
            builder.Color = DiscordColor.Grayple;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            await e.Message.RespondAsync(embed: builder.Build());
        }

        public static async Task ReturnError(DSharpPlus.EventArgs.MessageCreateEventArgs e, string title, string description)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = title;
            builder.Description = description;
            builder.Color = DiscordColor.Red;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            await e.Message.RespondAsync(embed: builder.Build());
        }
    }
}
