using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewEnglandReptileBot.Entities.Persist
{
    public class DiscordUserData
    {
        /// <summary>
        /// ID of this user from Discord.
        /// </summary>
        public ulong _id { get; set; }

        /// <summary>
        /// Name and discriminator
        /// </summary>
        public string latest_name { get; set; }

        /// <summary>
        /// The date this member was first seen on this server.
        /// </summary>
        public long firstSeenDate { get; set; }

        /// <summary>
        /// The mute status
        /// </summary>
        public DiscordUserDataStatusEffect muted { get; set; } = new DiscordUserDataStatusEffect();

        /// <summary>
        /// Temp ban status
        /// </summary>
        public DiscordUserDataStatusBanned temp_banned { get; set; } = new DiscordUserDataStatusBanned();

        /// <summary>
        /// Strikes placed on this member
        /// </summary>
        public List<DiscordUserDataStrike> strikes { get; set; } = new List<DiscordUserDataStrike>();

        public DateTime GetFirstSeenDate()
        {
            return new DateTime(firstSeenDate);
        }

        public DiscordUserDataStrike[] GetActiveStrikes()
        {
            return strikes.Where(x => x.GetIsActive()).ToArray();
        }

        public void Save()
        {
            Program.db_users.Update(this);
        }
    }
}
