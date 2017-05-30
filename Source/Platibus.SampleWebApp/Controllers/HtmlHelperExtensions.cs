using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Platibus.SampleWebApp.Controllers
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString NavItemFor(this HtmlHelper html, string text, string actionName, string controllerName, object htmlAttributes = null)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];
            var actionLink = html.ActionLink(text, actionName, controllerName, null, htmlAttributes).ToHtmlString();
            var isActive = controllerName == routeControl && actionName == routeAction;

            return new MvcHtmlString((isActive ? "<li class=\"active\">" : "<li>") + actionLink + "</li>");
        }
    }
}