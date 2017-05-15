using Platibus.SampleWebApp.Controllers;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Platibus.IIS;

namespace Platibus.SampleWebApp
{
    public class MvcApplication : HttpApplication
    {
	    protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ControllerBuilder.Current.SetControllerFactory(new ControllerFactory());
        }

		private PlatibusHttpModule _platibusHttpModule;

	    public override void Init()
	    {
		    base.Init();

            // Optionally, the HTTP module can also be registered explicitly in the
            // HttpApplication.Init() method
	        if (_platibusHttpModule == null && AppSettings.ExplicitHttpModule.IsEnabled())
	        {
	            _platibusHttpModule = new PlatibusHttpModule();
	            _platibusHttpModule.Init(this);
            }
        }

	    public override void Dispose()
        {
            // In which case it can be shut down explicitly in the Application_Shutdown() method:
            if (_platibusHttpModule != null)
            {
                _platibusHttpModule.Dispose();
                _platibusHttpModule = null;
            }
        }
    }
}