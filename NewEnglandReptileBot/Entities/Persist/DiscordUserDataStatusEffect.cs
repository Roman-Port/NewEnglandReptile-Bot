using System;
using System.Collections.Generic;
using System.Text;

namespace NewEnglandReptileBot.Entities.Persist
{
    public class DiscordUserDataStatusEffect
    {
        /// <summary>
        /// Is this active?
        /// </summary>
        public bool is_applied { get; set; }

        /// <summary>
        /// The time since this was applied, if it was applied. 
        /// </summary>
        public long applied_since { get; set; }

        /// <summary>
        /// The time this expires, if any
        /// </summary>
        public long? expiry { get; set; }

        /// <summary>
        /// The reason.
        /// </summary>
        public string reason { get; set; }

        /// <summary>
        /// The member who banned this person.
        /// </summary>
        public ulong catalyst { get; set; }

        /// <summary>
        /// Returns false if this is no longer active, or true if it is.
        /// </summary>
        /// <returns></returns>
        public bool CheckIfActive()
        {
            if (!is_applied)
                return false;
            if (expiry != null)
                return DateTime.UtcNow < GetExpiry();
            return true;
        }

        public DateTime GetAppliedSince()
        {
            return new DateTime(applied_since);
        }

        public DateTime? GetExpiry()
        {
            if (!expiry.HasValue)
                return null;
            return new DateTime(expiry.Value);
        }
    }
}
