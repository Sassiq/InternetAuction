using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OnlineAuctionProject.Startup))]
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
