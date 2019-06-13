using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes.Models;
using NewEnglandReptileBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.SocialWorkers
{
    public class InstagramApi
    {
        public static async Task<List<Tuple<InstaMedia, BotConfigFile_SocialAccount>>> GetNewPosts(SocialStreamPointSaved saved)
        {
            //Login and create the API wrapper
            var api = InstaApiBuilder.CreateBuilder().SetUser(new InstagramApiSharp.Classes.UserSessionData
            {
                UserName = Program.config.instagram_auth.username,
                Password = Program.config.instagram_auth.password
            }).Build();
            var logInResult = await api.LoginAsync();

            //Find new posts
            List<Tuple<InstaMedia, BotConfigFile_SocialAccount>> newPosts = new List<Tuple<InstaMedia, BotConfigFile_SocialAccount>>();
            foreach(var account in Program.config.social_pages)
            {
                if (account.platform != BotConfigFile_SocialAccountType.Instagram)
                    continue;

                //Fetch posts
                var userMedias = await api.UserProcessor.GetUserMediaAsync(account.username, InstagramApiSharp.PaginationParameters.MaxPagesToLoad(1));

                //Get the saved data from the stream, if we have it
                DateTime lastTime = DateTime.MinValue;
                if (saved.instagram_latest_post_time.ContainsKey(account.username))
                    lastTime = saved.instagram_latest_post_time[account.username];
                DateTime checkTime = lastTime.AddSeconds(10); //Rounding protection. Not sure if it's actually needed.

                //We know that the latest posts are first, so go through until we get the last post that was sent.
                for(int i = 0; i<userMedias.Value.Count; i+=1)
                {
                    var media = userMedias.Value[i];
                    if(media.TakenAt > checkTime)
                    {
                        newPosts.Add(new Tuple<InstaMedia, BotConfigFile_SocialAccount>(media, account));
                    }
                }

                //Update the last time
                if(userMedias.Value.Count >= 1)
                {
                    if (saved.instagram_latest_post_time.ContainsKey(account.username))
                        saved.instagram_latest_post_time[account.username] = userMedias.Value[0].TakenAt;
                    else
                        saved.instagram_latest_post_time.Add(account.username, userMedias.Value[0].TakenAt);
                }
            }

            return newPosts;
        }
    }
}
