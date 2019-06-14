using DSharpPlus.Entities;
using NewEnglandReptileBot.Commands;
using NewEnglandReptileBot.Entities.Persist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot
{
    public delegate Task CommandCallback(DSharpPlus.EventArgs.MessageCreateEventArgs e, string prefix, string content, DiscordMember member, DiscordPermissionLevel perms, DiscordUserData data);

    public class RegisteredCommand
    {
        public CommandCallback callback;
        public DiscordPermissionLevel requiredLevel;
        public string prefix;
        public string info;

        public RegisteredCommand(string prefix, DiscordPermissionLevel level, CommandCallback callback, string info)
        {
            this.prefix = prefix;
            this.requiredLevel = level;
            this.callback = callback;
            this.info = info;
        }
    }

    public static class CommandProcessor
    {
        public static List<RegisteredCommand> commands = new List<RegisteredCommand>
        {
            new RegisteredCommand("help", DiscordPermissionLevel.Default, HelpCommand.OnCmd, "Shows help for all commands this bot has to offer."),
            new RegisteredCommand("tempban", DiscordPermissionLevel.Moderator, BanCommand.OnCmd, "Temporarily bans a member for a specified amount of time."),
            new RegisteredCommand("strike", DiscordPermissionLevel.Moderator, StrikeCommand.OnCmd, "Strikes a member. Too many strikes and they're banned."),
            new RegisteredCommand("unban", DiscordPermissionLevel.Moderator, UnbanCommand.OnCmd, "Unbans a member.")
        };

        public static async Task OnCommand(DSharpPlus.EventArgs.MessageCreateEventArgs e, string prefix, string content)
        {
            //Get Discord member and user data
            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            DiscordUserData data = BotTools.GetUserDataById(e.Author.Id);
            data.latest_name = $"{e.Author.Username}#{e.Author.Discriminator}";

            //Identify the permission level of this member
            DiscordPermissionLevel perms = DiscordPermissionLevel.Default;
            if (member.Roles.Where(x => x.Id == Program.config.muted_role).Count() > 0)
                perms = DiscordPermissionLevel.Moderator;
            if (member.Roles.Where(x => x.Id == Program.config.admin_role).Count() > 0)
                perms = DiscordPermissionLevel.Admin;

            //Now, find a command we can use.
            foreach(var c in commands)
            {
                if (c.prefix.ToLower() != prefix.ToLower())
                    continue;
                if(c.requiredLevel > perms)
                {
                    //Tell the user that they're not allowed to do that.
                    await OnCommandAuthFailed(e, c);
                    return;
                }
                try
                {
                    await c.callback(e, prefix, content, member, perms, data);
                } catch (Exception ex)
                {
                    //Failed!
                    Program.LogMessage("cmd-error", ex.Message + ex.StackTrace);
                    await OnCommandExceptionFailed(e, c, ex);
                }
            }
        }

        public static async Task OnCommandAuthFailed(DSharpPlus.EventArgs.MessageCreateEventArgs e, RegisteredCommand command)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = ":warning: Not Authorized";
            builder.Description = $"Sorry, you're not allowed to use that command. You must be a {command.requiredLevel.ToString().ToLower()} to do this.";
            builder.Color = DiscordColor.Yellow;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            await e.Message.RespondAsync(embed: builder.Build());
        }

        public static async Task OnCommandExceptionFailed(DSharpPlus.EventArgs.MessageCreateEventArgs e, RegisteredCommand command, Exception ex)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = ":exclamation: Command Failed";
            builder.Description = $"There was an internal error while processing this command. Sorry, try again.";
            builder.AddField("Exception Name", ex.Message);
            builder.AddField("Exception Stack Trace", "```"+ex.StackTrace+"```");
            builder.Color = DiscordColor.Red;
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT
            };
            await e.Message.RespondAsync(embed: builder.Build());
        }
    }
}
