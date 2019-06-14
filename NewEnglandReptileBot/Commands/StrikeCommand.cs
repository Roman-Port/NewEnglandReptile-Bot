using DSharpPlus.Entities;
using NewEnglandReptileBot.Entities.Persist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.Commands
{
    public static class StrikeCommand
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
            if(victim == null)
            {
                await ReturnError(e, "Failed to Find User", $"Failed to find the user, ``{memberString.Trim(' ')}``. You can use their ID, their full name, or their first name. You can also mention them.");
                return;
            }

            //Find time
            bool isForever = true;
            DateTime? time;
            int timeParseErrorCount = 0;
            int timeParseCount = 0;
            if (timeString.Trim(' ').ToLower() != "never")
            {
                time = BotTools.ParseTime(timeString.Trim(' '), out timeParseErrorCount, out timeParseCount);
                isForever = false;
            } else
            {
                time = null;
            }

            //If the time parse didn't find any, or if there were any errors, stop
            if(time != null && (timeParseCount == 0 || timeParseErrorCount > 0))
            {
                await ReturnError(e, "Failed to Parse Time", "There was a problem parsing the time you entered. Do not include commas in the time, make sure you use spaces to separate each part, and make sure you do not use weeks. For help, do not include any arguments and run the command again.");
                return;
            }

            //Limit message to 900 characters
            if(messageString.Length > 900)
            {
                await ReturnError(e, "Reason is Too Long", "Sorry, the reason is limited by Discord to 900 characters.");
                return;
            }

            //Awesome. Now, create the strike
            DiscordUserDataStrike strike = new DiscordUserDataStrike
            {
                time = DateTime.UtcNow.Ticks,
                striker = e.Author.Id,
                message = messageString
            };
            if (time.HasValue)
                strike.expire_time = time.Value.Ticks;

            //Get the user data for the person we just struck and apply.
            DiscordUserData victimUserData = BotTools.GetUserDataById(victim.Id);
            victimUserData.strikes.Add(strike);

            //Send a notification of this to the person
            await NotifyStrike(victim, member, strike, e.Guild, victimUserData);

            //Check if we need to issue a ban for this member
            var activeStrikes = victimUserData.GetActiveStrikes();
            if(activeStrikes.Length >= Program.config.ban_strikes)
            {
                //Ban time! Find when the latest ban expire happens
                DateTime latest_expire = DateTime.MaxValue;
                bool does_expire = false;
                foreach(var a in activeStrikes)
                {
                    if(a.expire_time.HasValue)
                    {
                        does_expire = true;
                        if (a.GetExpiry().Value < latest_expire)
                            latest_expire = a.GetExpiry().Value;
                    }
                }
                if(does_expire)
                {
                    //Temp ban
                    await ActionTools.BanTools.DoBanMember(victimUserData, e.Guild, victim, e.Author, "You are banned for having too many strikes.", true, latest_expire);
                } else
                {
                    //Send message
                    await ActionTools.BanTools.NotifyPermaBan(e.Guild, victim, e.Author, "You are banned for having too many strikes.", true);
                    
                    //Ban from the server
                    DiscordMember victimMember = await e.Guild.GetMemberAsync(victim.Id);
                    await e.Guild.BanMemberAsync(victimMember, 0, "Temporary ban system.");
                }
            }

            //Save
            victimUserData.Save();

            //Return OK
            await ReturnOk(e, victim, activeStrikes.Length);
        }

        public static async Task ReturnOk(DSharpPlus.EventArgs.MessageCreateEventArgs e, DiscordUser victim, int strikeCount)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "User Striked";
            builder.Description = $"{victim.Username}#{victim.Discriminator} ({victim.Id}) has been striked. This is strike number {strikeCount}.";
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
            builder.Title = "Strike Help";
            builder.Description = $"``{{member}}, {{time}}, {{message}}``\n\nThe strike command will save a message about a member, direct message them, and will ban them temporarily if they receive more than {Program.config.ban_strikes} strikes.";
            builder.AddField("Usage", "To strike a member, type their name followed by comma and the duration. Then, add a comma and include a message. If you would like a ban to never expire, use \"never\" as a time.");
            builder.AddField("Remarks", "Strikes **__are not anonymous__** and your username will be included on the direct message sent to the member.");
            builder.AddField("Examples", $"``{Program.config.command_prefix}strike RomanPort#0001, never, Some message here, up to 900 characters.``\n``{Program.config.command_prefix}strike RomanPort#0001, 2 days, This will strike for two days, then expire.``\n``{Program.config.command_prefix}strike RomanPort#0001, 2 days 1 year, You can have more advanced timeframes here, but you cannot include commas.``");
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

        public static async Task NotifyStrike(DiscordUser victim, DiscordUser striker, DiscordUserDataStrike strike, DiscordGuild guild, DiscordUserData data)
        {
            //Notify a user that they've been struck
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "You have been struck in " + guild.Name + "!";
            builder.Description = $"{striker.Username}#{striker.Discriminator} struck you. You now have {data.GetActiveStrikes().Length} strikes. When you hit {Program.config.ban_strikes} strikes, you will be **banned** from {guild.Name}.";
            if (strike.expire_time.HasValue)
            {
                builder.AddField("Expires", BotTools.DateTimeToString(strike.GetExpiry().Value));
            } else
            {
                builder.AddField("Expires", "*Never*");
            }
            builder.AddField("Striked By", $"{striker.Username}#{striker.Discriminator}");
            builder.AddField("Added", BotTools.DateTimeToString(strike.GetTime()));
            builder.AddField("Message", strike.message);
            builder.Color = DiscordColor.Yellow;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            await BotTools.SendMemberMsg(victim.Id, builder.Build());
        }
    }
}
