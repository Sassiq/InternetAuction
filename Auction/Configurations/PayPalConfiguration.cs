using PayPal.Api;
using System.Collections.Generic;

namespace OnlineAuctionProject.Configurations
{
    internal class PayPalConfiguration
    {
        public readonly static string ClientId;
        public readonly static string ClientSecret;

        static PayPalConfiguration()
        {
            var config = GetConfig();
            ClientId = config["clientId"];
            ClientSecret = config["clientSecret"];
        }

        public static Dictionary<string, string> GetConfig() => ConfigManager.Instance.GetProperties();

        public static APIContext GetAPIContext()
        {
            var apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();

            return apiContext;
        }

        private static string GetAccessToken() => new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
    }
}