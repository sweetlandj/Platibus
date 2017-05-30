using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
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
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

		private PlatibusHttpModule _platibusHttpModule;

	    public override void Init()
	    {
		    base.Init();

            _platibusHttpModule = new PlatibusHttpModule();
	        _platibusHttpModule.Init(this);
        }

	    public override void Dispose()
        {
            if (_platibusHttpModule != null)
            {
                _platibusHttpModule.Dispose();
                _platibusHttpModule = null;
            }
        }
    }
}