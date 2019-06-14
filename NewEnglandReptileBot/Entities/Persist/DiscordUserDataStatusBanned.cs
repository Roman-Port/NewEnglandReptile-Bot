using System;
using System.Collections.Generic;
using System.Text;

namespace NewEnglandReptileBot.Entities.Persist
{
    public class DiscordUserDataStatusBanned : DiscordUserDataStatusEffect
    {
        /// <summary>
        /// If this is true, this member was banned for too many strikes. If not, the reason is still valid.
        /// </summary>
        public bool is_automated { get; set; }
    }
}
