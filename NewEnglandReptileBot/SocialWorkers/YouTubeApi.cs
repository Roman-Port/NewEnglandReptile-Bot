using NewEnglandReptileBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.SocialWorkers
{
    public static class YouTubeApi
    {
        public static async Task<List<Tuple<YouTubePost, BotConfigFile_SocialAccount>>> FetchNewVideos(SocialStreamPointSaved saved)
        {
            //Loop through all social accounts and find their latest videos
            List<Tuple<YouTubePost, BotConfigFile_SocialAccount>> newPosts = new List<Tuple<YouTubePost, BotConfigFile_SocialAccount>>();
            foreach(var account in Program.config.social_pages)
            {
                if (account.platform != BotConfigFile_SocialAccountType.YouTube)
                    continue;
                
                //Get all posts
                YouTubePost[] posts = await FetchChannel(account);

                //Get the saved data from the stream, if we have it
                DateTime lastTime = DateTime.MinValue;
                if (saved.youtube_latest_post_time.ContainsKey(account.username))
                    lastTime = saved.youtube_latest_post_time[account.username];
                DateTime checkTime = lastTime.AddSeconds(10); //Rounding protection. Not sure if it's actually needed.

                //Knowing that the latest posts start at the beginning, loop through and add new posts
                foreach (var p in posts)
                {
                    if (p.snippet.publishedAt > checkTime)
                        newPosts.Add(new Tuple<YouTubePost, BotConfigFile_SocialAccount>(p, account));
                }

                //Update the last time
                if (posts.Length >= 1)
                {
                    if (saved.youtube_latest_post_time.ContainsKey(account.username))
                        saved.youtube_latest_post_time[account.username] = posts[0].snippet.publishedAt;
                    else
                        saved.youtube_latest_post_time.Add(account.username, posts[0].snippet.publishedAt);
                }
            }

            return newPosts;
        }

        public static async Task<YouTubePost[]> FetchChannel(BotConfigFile_SocialAccount account)
        {
            //The YouTube API is simple. Thank god. This is how everything should be.
            string url = $"https://www.googleapis.com/youtube/v3/search?key={Program.config.youtube_auth}&channelId={account.username}&part=snippet,id&order=date&maxResults=20";
            YouTubePostContainer response;
            using (WebClient hc = new WebClient())
            {
                string reply = hc.DownloadString(url);
                response = JsonConvert.DeserializeObject<YouTubePostContainer>(reply);
            }

            //Now, just filter this to videos
            List<YouTubePost> posts = new List<YouTubePost>();
            foreach(var p in response.items)
            {
                if (p.kind == "youtube#searchResult" && p.id.kind == "youtube#video")
                    posts.Add(p);
            }
            return posts.ToArray();
        }
    }
}
