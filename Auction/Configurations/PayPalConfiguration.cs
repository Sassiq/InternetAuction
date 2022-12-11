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

        public static Dictionary<string, string> GetConfig() => new Dictionary<string, string>() {
                { "clientId", "AUHD4rAKVwhBOSgvLyoZA2p7Q4R8i9pwjqoQvcJfaNS8-lcHC1gYAcpjBBrBF3ZkRhueSr91wL6uRy6G" },
                { "clientSecret", "EL7_W8C_ROmWeh9Jw5mgI3XtXSnDWhyQzqGRI2HL2NE_e_Xblm2iGbX2EnUScdqnFaIMApflS97Aw-Rk" }
            };

        private static string GetAccessToken() => new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();

        public static APIContext GetAPIContext()
        {
            var apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();

            return apiContext;
        }
    }
}