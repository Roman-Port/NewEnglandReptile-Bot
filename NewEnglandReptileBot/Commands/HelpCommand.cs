using DSharpPlus.Entities;
using NewEnglandReptileBot.Entities.Persist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.Commands
{
    public static class HelpCommand
    {
        public static async Task OnCmd(DSharpPlus.EventArgs.MessageCreateEventArgs e, string prefix, string content, DiscordMember member, DiscordPermissionLevel perms, DiscordUserData data)
        {
            //Create an embed with all of the commands this user has access to 
            var cmds = CommandProcessor.commands.Where(x => x.requiredLevel <= perms);
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "Command Help";
            builder.Description = "With any command, run it without any parameters for help.";
            builder.Color = DiscordColor.Grayple;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            foreach(var c in cmds)
            {
                builder.AddField(Program.config.command_prefix + c.prefix, c.info, true);
            }
            await e.Message.RespondAsync(embed: builder.Build());
        }
    }
}
