using System;
using System.Collections.Generic;
using System.Text;

namespace NewEnglandReptileBot.Entities
{
    /// <summary>
    /// Saves the last post sent. Serialized to disk.
    /// </summary>
    public class SocialStreamPointSaved
    {
        public Dictionary<string, DateTime> instagram_latest_post_time = new Dictionary<string, DateTime>(); //The latest time for each account that we know
        public Dictionary<string, DateTime> youtube_latest_post_time = new Dictionary<string, DateTime>(); //The latest time for each account that we know
        public ulong twitter_latest_refresh_id = 0; //The latest ID in the stream
    }
}
