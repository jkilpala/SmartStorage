using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(jksmartstorageService.Startup))]

namespace jksmartstorageService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}