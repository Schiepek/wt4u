using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(wt4u.Startup))]
namespace wt4u
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
