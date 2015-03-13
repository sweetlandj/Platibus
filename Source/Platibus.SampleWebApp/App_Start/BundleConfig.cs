using System.Web.Optimization;

namespace Platibus.SampleWebApp
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include("~/Scripts/jquery-2.1.3.min.js", "~/Scripts/bootstrap.min.js"));

            bundles.Add(new StyleBundle("~/Content/bootstrap")
                .Include("~/Content/bootstrap.min.css", "~/Content/bootstrap-theme.min.css"));
        }
    }
}