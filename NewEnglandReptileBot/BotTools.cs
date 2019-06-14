using DSharpPlus.Entities;
using NewEnglandReptileBot.Entities.Persist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot
{
    public static class BotTools
    {
        public static DiscordUserData GetUserDataById(ulong id)
        {
            //Check if this is on the database.
            var collection = Program.db_users;
            var matches = collection.FindOne(x => x._id == id);
            if (matches != null)
                return matches;

            //We'll need to create one for this user.
            matches = new DiscordUserData
            {
                _id = id,
                firstSeenDate = DateTime.UtcNow.Ticks,
                latest_name = "invalid"
            };
            collection.Insert(matches);
            return matches;
        }

        public static async Task<bool> SendMemberMsg(ulong id, string message)
        {
            try
            {
                var user = await Program.discord.GetUserAsync(id);
                var dm = await Program.discord.CreateDmAsync(user);
                await dm.SendMessageAsync(message);
                return true;
            } catch
            {
                return false;
            }
        }

        public static async Task<bool> SendMemberMsg(ulong id, DiscordEmbed message)
        {
            try
            {
                var user = await Program.discord.GetUserAsync(id);
                var dm = await Program.discord.CreateDmAsync(user);
                await dm.SendMessageAsync(embed: message);
                return true;
            } catch
            {
                return false;
            }
        }

        public static string DateTimeToString(DateTime time)
        {
            return $"{time.ToShortDateString()} at {time.ToLongTimeString()} UTC";
        }

        public static string DateTimeOffsetToString(TimeSpan offset)
        {
            string output = "";
            output += OffsetHelper("day", offset.Days);
            output += OffsetHelper("hour", offset.Hours);
            output += OffsetHelper("minute", offset.Minutes, false);
            return output;
        }

        private static string OffsetHelper(string label, int value, bool allowNone = true)
        {
            if (value == 1)
                return $"{value} {label} ";
            else if (value == 0 && allowNone)
                return "";
            else
                return $"{value} {label}s ";
        }

        public static async Task<string> GetRemoteUsername(ulong id)
        {
            try
            {
                DiscordUser user = await Program.discord.GetUserAsync(id);
                return $"{user.Username}#{user.Discriminator}";
            } catch
            {
                return "(INVALID USER)";
            }
        }

        public static async Task<DiscordUser> ParseName(DiscordGuild server, string text)
        {
            //Parse a name typed in. First, check if it is tagging a member
            DiscordUser user;
            if(text.StartsWith("<@") && text.EndsWith(">"))
            {
                string id = text.Substring(2, text.Length - 3);
                if(ulong.TryParse(id, out ulong result))
                {
                    user = await TryGetUser(result);
                    if (user != null)
                        return user;
                }
            }
            
            //Check if it contains a #. If it does, parse it
            if(text.Contains('#'))
            {
                string name = text.Substring(0, text.IndexOf('#')).Trim('#');
                string discrim = text.Substring(text.IndexOf('#')).Trim('#');
                if(int.TryParse(discrim, out int descrimInt) && discrim.Length == 4)
                {
                    //Try and find
                    user = await FindUserByName(server, name, discrim);
                    if (user != null)
                        return user;
                }
            }

            //Check if this is just the ID. That'd be easy.
            if (ulong.TryParse(text, out ulong result2))
            {
                user = await TryGetUser(result2);
                if (user != null)
                    return user;
            }

            //Great. Fallback and see if this is maybe just their first name
            user = await FindUserByName(server, text, null);
            return user;
        }

        public static async Task<DiscordUser> ParseOfflineName(string text)
        {
            //Find
            DiscordUser user;

            //Check if this is just the ID. That'd be easy.
            if (ulong.TryParse(text, out ulong result))
            {
                user = await TryGetUser(result);
                if (user != null)
                    return user;
            }

            //Find all members with this
            text = text.ToLower();
            var foundUsers = Program.db_users.Find(x => x.latest_name.ToLower() == text);
            if (foundUsers.Count() == 1)
            {
                ulong id = foundUsers.First()._id;
                return await TryGetUser(id);
            }
            else
                return null;
        }

        private static async Task<DiscordUser> TryGetUser(ulong id)
        {
            try
            {
                DiscordUser user = await Program.discord.GetUserAsync(id);
                return user;
            } catch
            {
                return null;
            }
        }

        public static async Task<DiscordUser> FindUserByName(DiscordGuild server, string name, string discrim)
        {
            var members = await server.GetAllMembersAsync();

            if(discrim == null)
            {
                //Try searching without discrim
                var matches = members.Where(x => x.Username.ToLower() == name.ToLower());
                if (matches.Count() == 1)
                    return matches.First();
                else
                    return null;
            } else
            {
                //Try searching with discrim
                var matches = members.Where(x => x.Username.ToLower() == name.ToLower() && x.Discriminator == discrim.ToString());
                if (matches.Count() == 1)
                    return matches.First();
                else
                    return null;
            }
            
        }

        public static DateTime ParseTime(string msg, out int errorCount, out int registeredCount)
        {
            //Everything will be in pairs. Split by space
            DateTime result = DateTime.UtcNow;
            errorCount = 0;
            registeredCount = 0;
            string[] data = msg.Split(' ');
            for(int i = 0; i<data.Length; i+=1)
            {
                if (i >= data.Length-1 || i % 2 == 1)
                    continue; //Skip the last or the middle

                //This should be a number
                if (!int.TryParse(data[i], out int value))
                {
                    errorCount++;
                    continue;
                }

                //Read the previous and try to determine what it was
                string key = data[i + 1].TrimEnd('s').ToLower();
                registeredCount++;
                if (key == "second")
                    result = result.AddSeconds(value);
                else if (key == "minute")
                    result = result.AddMinutes(value);
                else if (key == "hour")
                    result = result.AddHours(value);
                else if (key == "day")
                    result = result.AddDays(value);
                else if (key == "month")
                    result = result.AddMonths(value);
                else if (key == "year")
                    result = result.AddYears(value);
                else
                {
                    registeredCount--;
                    errorCount++;
                }
            }
            return result;
        }
    }
}
