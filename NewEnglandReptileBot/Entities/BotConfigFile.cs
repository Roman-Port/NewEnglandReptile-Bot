using System;
using System.Collections.Generic;
using System.Text;

namespace NewEnglandReptileBot.Entities
{
    public class BotConfigFile
    {
        public string access_token; //The token to the bot
        public BotConfigFile_SocialAccount[] social_pages; //Social media pages to check
        public int social_refresh_seconds; //Seconds between each refresh of the social media pages. Don't rate limit us

        public ulong notification_channel; //Channel to send social links
        public BotConfigFile_ApiLoginUsernamePass instagram_auth;
        public BotConfigFile_ApiLoginOAUTH twitter_auth;
        public string youtube_auth;
    }

    public class BotConfigFile_SocialAccount
    {
        public string username;
        public string screen_name;
        public BotConfigFile_SocialAccountType platform;
    }

    public class BotConfigFile_ApiLoginUsernamePass
    {
        public string username;
        public string password;
    }

    public class BotConfigFile_ApiLoginOAUTH
    {
        public string public_key;
        public string private_key;
    }

    public enum BotConfigFile_SocialAccountType
    {
        Twitter = 0,
        Instagram = 1,
        YouTube = 2
    }
}
