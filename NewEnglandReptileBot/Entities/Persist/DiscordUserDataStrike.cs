using System;
using System.Collections.Generic;
using System.Text;

namespace NewEnglandReptileBot.Entities.Persist
{
    public class DiscordUserDataStrike
    {
        /// <summary>
        /// The time this strike was added.
        /// </summary>
        public long time { get; set; }

        /// <summary>
        /// The time this will expire, if ever. If null, this will never expire. Strikes will never be removed from the data for historical reasons, but will not appear to users.
        /// </summary>
        public long? expire_time { get; set; }

        /// <summary>
        /// The user ID of the member who striked this person.
        /// </summary>
        public ulong striker { get; set; }

        /// <summary>
        /// Message provided by the member who striked this user.
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// Returns true if active, false if not
        /// </summary>
        /// <returns></returns>
        public bool GetIsActive()
        {
            if (expire_time != null)
                return DateTime.UtcNow < GetExpiry();
            return true;
        }

        public DateTime GetTime()
        {
            return new DateTime(time);
        }

        public DateTime? GetExpiry()
        {
            if (!expire_time.HasValue)
                return null;
            return new DateTime(expire_time.Value);
        }
    }
}
