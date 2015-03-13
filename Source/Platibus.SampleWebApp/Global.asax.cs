using Platibus.IIS;
using Platibus.SampleWebApp.Controllers;
using System.Threading;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Platibus.SampleWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static int _applicationCount;

        protected void Application_Start()
        {
            Interlocked.Increment(ref _applicationCount);

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ControllerBuilder.Current.SetControllerFactory(new ControllerFactory());
        }

        protected void Application_Shutdown()
        {
            if (Interlocked.Decrement(ref _applicationCount) == 0)
            {
                BusManager.Shutdown();
            }
        }
    }
}
