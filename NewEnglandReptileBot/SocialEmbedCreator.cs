﻿using DSharpPlus.Entities;
using InstagramApiSharp.Classes.Models;
using NewEnglandReptileBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewEnglandReptileBot
{
    public static class SocialEmbedCreator
    {
        public static DiscordEmbed CreateInstagramEmbed(InstaMedia m, BotConfigFile_SocialAccount user)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = $"Instagram Post by {user.screen_name}";
            if(m.Caption != null)
                builder.Description = m.Caption.Text;
            builder.Url = $"https://www.instagram.com/p/{m.Code}/";

            builder.Color = DiscordColor.Grayple;

            if(m.MediaType == InstaMediaType.Carousel)
            {
                //Use carousel
                builder.ImageUrl = m.Carousel[0].Images[0].Uri;
            } else
            {
                //Use legacy
                builder.ImageUrl = m.Images[0].Uri;
            }

            builder.Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = m.User.ProfilePicture,
                Url = $"https://instagram.com/{m.User.UserName}/",
                Name = $"@{m.User.UserName}"
            };

            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT,
                IconUrl = "https://romanport.com/static/nerd_bot/twitter.png"
            };

            builder.Timestamp = m.TakenAt;

            return builder.Build();
        }

        public static DiscordEmbed CreateTwitterEmbed(TwitterPost post)
        {
            //Try and find the social account for this
            BotConfigFile_SocialAccount account = null;
            var foundAccounts = Program.config.social_pages.Where(x => x.username.ToLower() == post.user.screen_name.ToLower());
            if (foundAccounts.Count() == 1)
                account = foundAccounts.First();
            else
                account = new BotConfigFile_SocialAccount
                {
                    screen_name = post.user.name,
                    platform = BotConfigFile_SocialAccountType.Twitter,
                    username = post.user.screen_name
                }; //Create dummy

            //Build
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            if(post.in_reply_to_status_id == null)
            {
                //Original tweet
                builder.Title = $"Twitter Post by {account.screen_name}";
                builder.Description = post.text;
            } else
            {
                //Reply to a tweet
                builder.Title = $"Twitter Reply by {account.screen_name}";
                builder.Description = $"*(In [reply]({ $"https://twitter.com/{post.in_reply_to_screen_name}/status/{post.in_reply_to_status_id}" }) to @{post.in_reply_to_screen_name})*\n" + post.text;
            }

            builder.Url = $"https://twitter.com/{post.user.screen_name}/status/{post.id.ToString()}";
            builder.Color = new DiscordColor("#48AAE6");
            
            if(post.extended_entities != null)
            {
                if(post.extended_entities.media != null)
                {
                    if(post.extended_entities.media.Length >= 1)
                    {
                        var media = post.extended_entities.media[0];
                        builder.ImageUrl = media.media_url_https;
                    }
                }
            }

            builder.Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = post.user.profile_image_url_https,
                Name = "@"+post.user.screen_name,
                Url = $"https://twitter.com/{post.user.screen_name}"
            };

            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT,
                IconUrl = "https://romanport.com/static/nerd_bot/twitter.png"
            };

            return builder.Build();
        }

        public static DiscordEmbed CreateYouTubeEmbed(YouTubePost post, BotConfigFile_SocialAccount account)
        {
            //Build
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "New Video - " + post.snippet.title;
            builder.Url = $"https://youtube.com/watch?v={post.id.videoId}";
            builder.Color = DiscordColor.Red;
            builder.Description = post.snippet.description;
            builder.ImageUrl = post.snippet.thumbnails.high.url;
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = account.screen_name
            };
            builder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = Program.FOOTER_TEXT,
                IconUrl = "https://romanport.com/static/nerd_bot/twitter.png?v=2"
            };
            builder.Timestamp = post.snippet.publishedAt;
            return builder.Build();
        }
    }
}
