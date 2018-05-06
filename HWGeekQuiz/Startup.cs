using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HWGeekQuiz.Startup))]
namespace HWGeekQuiz
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
