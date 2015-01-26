using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Pluribus.IIS;

namespace Pluribus.SampleWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AppDomain.CurrentDomain.DomainUnload += (sender, args) => BusManager.Shutdown();
        }
    }
}
