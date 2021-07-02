using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(QuizR.Startup))]
namespace QuizR
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
