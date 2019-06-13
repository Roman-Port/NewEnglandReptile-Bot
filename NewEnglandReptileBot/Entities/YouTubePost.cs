using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NewEnglandReptileBot.Entities
{
    public class YouTubePostContainer
    {
        public string kind;
        public string etag;
        public string nextPageToken;
        public string regionCode;
        public YouTubePost[] items;
    }

    public class YouTubePost
    {
        public string kind;
        public YouTubeVideoId id;
        public YouTubeVideoSnippet snippet;
    }

    public class YouTubeVideoId
    {
        public string kind;
        public string videoId;
    }

    public class YouTubeVideoSnippet
    {
        public DateTime publishedAt;
        public string channelId;
        public string title;
        public string description;
        public YouTubeVideoThumbnailHolder thumbnails;
        public string channelTitle;
    }

    public class YouTubeVideoThumbnailHolder
    {
        [JsonProperty("default")]
        public YouTubeVideoThumbnail _default;

        public YouTubeVideoThumbnail medium;
        public YouTubeVideoThumbnail high;
    }

    public class YouTubeVideoThumbnail
    {
        public string url;
        public int width;
        public int height;
    }
}
