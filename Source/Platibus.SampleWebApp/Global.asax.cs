using Platibus.IIS;
using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Platibus.Diagnostics;
using Platibus.SampleWebApp.Controllers;

namespace Platibus.SampleWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private PlatibusHttpModule _httpModule;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        public override void Init()
        {
            if (!SampleWebAppSetting.OwinMiddleware.IsEnabled())
            {
                // OWIN middleware is not enabled, so fall back to the HTTP 
                // module configuration
                DiagnosticService.DefaultInstance.AddSink(DiagnosticEventLog.SingletonInstance);
                _httpModule = new PlatibusHttpModule();
                _httpModule.Init(this);
            }
        }

        protected void Application_End()
        {
            _httpModule?.Dispose();
        }
    }
}
