using DSharpPlus.Entities;
using NewEnglandReptileBot.Entities.Persist;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.ActionTools
{
    public static class BanTools
    {
        public static async Task OnBannedMemberJoined(DiscordUserData data, DiscordGuild server, DiscordMember member)
        {
            //Gather data
            DiscordUserDataStatusBanned status = data.temp_banned;

            //Send them a message telling them why they were banned, if they have any strikes, ect.
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "You are banned temporarily from " + server.Name + ".";
            if (status.is_automated)
            {
                builder.Description = "You cannot join this server. You were banned for having too many strikes. Your active strikes are listed below.";
                var strikes = data.GetActiveStrikes();
                for(int i = 0; i<strikes.Length; i+=1)
                {
                    DiscordUserDataStrike strike = strikes[i];
                    string message = $"*Added {BotTools.DateTimeToString(strike.GetTime())}*\n";
                    if (strike.expire_time.HasValue)
                        message += $"*__Expires {BotTools.DateTimeToString(strike.GetExpiry().Value)}__*\n";
                    else
                        message += "*__Never expires__*\n";
                    builder.AddField($"Strike {i+1} from {await BotTools.GetRemoteUsername(strike.striker)}", message+strike.message);
                }
            }
            else
            {
                DiscordUser user = await Program.discord.GetUserAsync(status.catalyst);
                builder.Description = $"You cannot join this server. You were banned by a member.";
                builder.AddField("Banned By", $"{user.Username}#{user.Discriminator}");
                builder.AddField("Reason", status.reason);
            }
            builder.AddField("Expires", $"This ban expires on {BotTools.DateTimeToString((DateTime)status.GetExpiry())}. You may rejoin after this time.");
            builder.AddField("Expires In", BotTools.DateTimeOffsetToString((DateTime)status.GetExpiry() - DateTime.UtcNow));
            builder.AddField("Added", $"You were banned on {BotTools.DateTimeToString(status.GetAppliedSince())}");
            builder.Color = DiscordColor.Red;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            DiscordEmbed embed = builder.Build();

            //DM this to the person
            await BotTools.SendMemberMsg(member.Id, embed);

            //Kick them from this server
            await server.RemoveMemberAsync(member, "Automated temp ban system.");
        }

        public static async Task DoBanMember(DiscordUserData data, DiscordGuild server, DiscordUser victim, DiscordUser cataylist, string reason, bool is_automated, DateTime expire_time)
        {
            //Apply the ban to the account
            DiscordUserDataStatusBanned status = new DiscordUserDataStatusBanned
            {
                applied_since = DateTime.UtcNow.Ticks,
                expiry = expire_time.Ticks,
                is_applied = true,
                is_automated = is_automated,
                reason = reason
            };
            if (!is_automated)
                status.catalyst = cataylist.Id;
            data.temp_banned = status;

            //Notify the user
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "You have been temporarily banned from " + server.Name + ".";
            if (is_automated)
                builder.Description = "You were removed from this server. You have had too many strikes applied to your account.";
            else
                builder.Description = "You were removed from this server. You were removed by a member.";
            if(!is_automated)
            {
                builder.AddField("Banned By", $"{cataylist.Username}#{cataylist.Discriminator}");
                builder.AddField("Reason", reason);
            }
            builder.AddField("Expires", BotTools.DateTimeToString(expire_time));
            builder.AddField("Expires In", BotTools.DateTimeOffsetToString(expire_time - DateTime.UtcNow));
            builder.Color = DiscordColor.Red;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            DiscordEmbed embed = builder.Build();

            //Send
            await BotTools.SendMemberMsg(victim.Id, embed);

            //Kick
            DiscordMember victimMember = await server.GetMemberAsync(victim.Id);
            await server.RemoveMemberAsync(victimMember, "Automated temporary ban system.");

            //Save
            data.Save();
        }

        public static async Task NotifyPermaBan(DiscordGuild server, DiscordUser victim, DiscordUser cataylist, string reason, bool is_automated)
        {
            //Notify the user
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "You have been banned from " + server.Name + ".";
            if (is_automated)
                builder.Description = "You were removed from this server. You have had too many strikes applied to your account.";
            else
                builder.Description = "You were removed from this server. You were removed by a member.";
            if (!is_automated)
            {
                builder.AddField("Banned By", $"{cataylist.Username}#{cataylist.Discriminator}");
                builder.AddField("Reason", reason);
            }
            builder.AddField("Recourse", "You may appeal a ban by writing an email to "+Program.config.contact+". Please be thoughtful and descriptive.");
            builder.Color = DiscordColor.Red;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            DiscordEmbed embed = builder.Build();

            //Send
            await BotTools.SendMemberMsg(victim.Id, embed);
        }
    }
}
