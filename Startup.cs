using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WATHoot2.Startup))]
namespace WATHoot2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
