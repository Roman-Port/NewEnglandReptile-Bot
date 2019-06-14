using DSharpPlus.Entities;
using NewEnglandReptileBot.Entities.Persist;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.Commands
{
    public static class UnbanCommand
    {
        public static async Task OnCmd(DSharpPlus.EventArgs.MessageCreateEventArgs e, string prefix, string content, DiscordMember member, DiscordPermissionLevel perms, DiscordUserData data)
        {
            if(content.Length == 0)
            {
                await ReturnHelp(e);
                return;
            }

            //Do it 

            //Find the user
            DiscordUser victim = await BotTools.ParseOfflineName(content.Trim(' '));

            //If not found, complain
            if(victim == null)
            {
                DiscordEmbedBuilder badBuilder = new DiscordEmbedBuilder();
                badBuilder.Title = "User Not Found";
                badBuilder.Description = $"\"{content.Trim(' ')}\" was not found. Try their ID.";
                badBuilder.Color = DiscordColor.Yellow;
                badBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = Program.FOOTER_TEXT
                };
                await e.Message.RespondAsync(embed: badBuilder.Build());
                return;
            }

            //Get user
            DiscordUserData victimData = BotTools.GetUserDataById(victim.Id);

            //Set as not active
            victimData.temp_banned.is_applied = false;

            //Save
            victimData.Save();

            //Write OK
            DiscordEmbedBuilder okBuilder = new DiscordEmbedBuilder();
            okBuilder.Title = "User Unbanned";
            okBuilder.Description = $"{victim.Username}#{victim.Discriminator} ({victim.Id}) was unbanned.";
            okBuilder.Color = DiscordColor.Green;
            okBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            await e.Message.RespondAsync(embed: okBuilder.Build());
        }

        public static async Task ReturnHelp(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "Unban Help";
            builder.Description = $"``{{member}}``\n\nThe command will unban this member. A message will be sent to them.";
            builder.AddField("Usage", "To unban a member, tag them or type in their name. Make sure to include the discriminator.");
            builder.AddField("Remarks", "If this user changed their username, you'll need to use their old username. This is due to limitations of Discord.");
            builder.AddField("Examples", $"``{Program.config.command_prefix}unban RomanPort#0001``");
            builder.Color = DiscordColor.Grayple;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            await e.Message.RespondAsync(embed: builder.Build());
        }
    }
}
