using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(OnlineAuctionProject.Startup))]
namespace OnlineAuctionProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
