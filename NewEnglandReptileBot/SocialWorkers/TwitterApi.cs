using NewEnglandReptileBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.SocialWorkers
{
    public class TwitterApi
    {
        public static async Task<TwitterPostContainer> SearchPosts(string query)
        {
            //First, obtain a token.
            TwitterOauthTokenResponse token;
            using (HttpClient hc = new HttpClient())
            {
                string authChal = $"{Program.config.twitter_auth.public_key}:{Program.config.twitter_auth.private_key}";
                hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authChal)));
                HttpContent content = new StringContent("grant_type=client_credentials"); //Just gonna cheat
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var tokenResponse = await hc.PostAsync("https://api.twitter.com/oauth2/token", content);

                if (!tokenResponse.IsSuccessStatusCode)
                    throw new Exception("Failed to obtain token.");
                string reply = await tokenResponse.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<TwitterOauthTokenResponse>(reply);
            }

            //Now, request new posts
            TwitterPostContainer response;
            using (WebClient hc = new WebClient())
            {
                hc.Headers.Add("Authorization", "Bearer "+token.access_token);
                string reply = hc.DownloadString("https://api.twitter.com/1.1/search/tweets.json"+query);
                response = JsonConvert.DeserializeObject<TwitterPostContainer>(reply);
            }
            return response;
        }

        public static async Task<TwitterPost[]> FetchNewPosts(SocialStreamPointSaved saved)
        {
            //Create a query. First, find all users that we'd like to use
            string usersQuery = "";
            foreach(var account in Program.config.social_pages)
            {
                if(account.platform == BotConfigFile_SocialAccountType.Twitter)
                {
                    if (usersQuery.Length != 0)
                        usersQuery += " OR ";
                    usersQuery += "from:"+account.username;
                }
            }

            //Combine this with the last post to produce a full query
            string finalQuery = $"?since_id={saved.twitter_latest_refresh_id}&q={System.Web.HttpUtility.UrlEncode(usersQuery)}";

            //Use the Twitter API
            TwitterPostContainer container = await SearchPosts(finalQuery);
            saved.twitter_latest_refresh_id = container.search_metadata.max_id;

            return container.statuses;
        }
    }
}
