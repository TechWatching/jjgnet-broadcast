using LinqToTwitter;

namespace JosephGuadagno.Broadcasting.Twitter
{
    public static class Constants
    {
        public static class Settings
        {
            public const string StorageAccount = "AzureWebJobsStorage";
            public const string TwitterApiKey = "Twitter-Api-Key";
            public const string TwitterApiSecret = "Twitter-Api-Secret";
            public const string TwitterAccessToken = "Twitter-Access-Token";
            public const string TwitterAccessTokenSecret = "Twitter-Access-Token-Secret";
            public const string FeedUrl = "Feed-Url";
            public const string BitlyToken = "Bitly-Token";
            public const string BitlyAPIRootUri = "Bitly-APIRootUri";
        }

        public static class Queues
        {
            public const string TwitterTweetsToSend = "twitter-tweets-to-send";
        }

        public static class Tables
        {
            public const string Configuration  = "Configuration";
        }
    }
}