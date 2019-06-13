using InstagramApiSharp.API.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.SocialWorkers
{
    public class InstagramApi
    {
        public static async Task GetNewPosts()
        {
            //Login and create the API wrapper
            var api = InstaApiBuilder.CreateBuilder().SetUser(new InstagramApiSharp.Classes.UserSessionData
            {
                UserName = Program.config.instagram_auth.username,
                Password = Program.config.instagram_auth.password
            }).Build();
            var logInResult = await api.LoginAsync();

            var userMedias = await api.UserProcessor.GetUserMediaAsync("reptiles_by_sainz", InstagramApiSharp.PaginationParameters.MaxPagesToLoad(1));
            foreach (var u in userMedias.Value)
            {
                await c.SendMessageAsync(embed: SocialEmbedCreator.CreateInstagramEmbed(u, Program.config.social_pages[0]));
                return;
            }
        }
    }
}
