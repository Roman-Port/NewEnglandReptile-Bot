using DSharpPlus.Entities;
using InstagramApiSharp.Classes.Models;
using NewEnglandReptileBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NewEnglandReptileBot.SocialWorkers
{
    /// <summary>
    /// Overall container of all things social
    /// </summary>
    public static class SocialTasks
    {
        public static async Task RefreshSocial()
        {
            //Log
            Program.LogMessage("social-worker", "Refreshing posts...");

            try
            {
                //Grab all and wait
                SocialStreamPointSaved stream = Program.saved_stream;
                Task<TwitterPost[]> twitterTask = TwitterApi.FetchNewPosts(stream);
                //Task<List<Tuple<InstaMedia, BotConfigFile_SocialAccount>>> instagramTask = InstagramApi.GetNewPosts(stream);
                Task<List<Tuple<YouTubePost, BotConfigFile_SocialAccount>>> youtubeTask = YouTubeApi.FetchNewVideos(stream);

                //Wait for all to complete
                Task.WaitAll(twitterTask/*, instagramTask*/, youtubeTask);

                //Now, process all of these and send them to Discord
                DiscordChannel channel = await Program.discord.GetChannelAsync(Program.config.notification_channel);
                foreach(var post in twitterTask.Result)
                {
                    await channel.SendMessageAsync(embed: SocialEmbedCreator.CreateTwitterEmbed(post));
                }
                /*foreach (var post in instagramTask.Result)
                {
                    await channel.SendMessageAsync(embed: SocialEmbedCreator.CreateInstagramEmbed(post.Item1, post.Item2));
                }*/
                foreach(var post in youtubeTask.Result)
                {
                    await channel.SendMessageAsync(embed: SocialEmbedCreator.CreateYouTubeEmbed(post.Item1, post.Item2));
                }
                
                //Save stream
                File.WriteAllText("saved_stream.json", JsonConvert.SerializeObject(stream));
            } catch (Exception ex)
            {
                //Log
                Program.LogMessage("social-worker-error", ex.Message+ex.StackTrace);
            }
        }
    }
}
