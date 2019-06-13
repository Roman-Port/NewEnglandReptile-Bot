using System;
using System.Collections.Generic;
using System.Text;

namespace NewEnglandReptileBot.Entities
{
    public class TwitterPostContainer
    {
        public TwitterPost[] statuses;
        public TwitterPostContainerMetadata search_metadata;
    }

    public class TwitterPost
    {
        public string created_at;
        public ulong id;
        public string text;
        public bool truncated;
        public TwitterPostEntities extended_entities;
        public TwitterPostUser user;
        public ulong? in_reply_to_status_id;
        public string in_reply_to_screen_name;
    }

    public class TwitterPostEntities
    {
        public TwitterPostMedia[] media;
    }

    public class TwitterPostMedia
    {
        public ulong id;
        public string media_url_https;
        public string type;
    }

    public class TwitterPostUser
    {
        public ulong id;
        public string name;
        public string screen_name;
        public string description;
        public string url;
        public string profile_image_url_https;
    }

    public class TwitterPostContainerMetadata
    {
        public string next_results;
        public string refresh_url;
        public ulong max_id;
    }
}
