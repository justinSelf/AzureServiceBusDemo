using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AsciiIt.Web.Startup))]
namespace AsciiIt.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
