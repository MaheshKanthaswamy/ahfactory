using System.Web.Mvc;
using System.Web.Routing;

namespace Ftel.WebSite.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        private const string DefaultRouteUrl = "/{controller}/{action}/{id}";
        private const string ControllersNamespace = "Ftel.WebSite.Areas.Admin.Controllers";

        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Admin_localized",
                AreaName + "/{culture}" + DefaultRouteUrl,
                new { culture = "fr", controller = "User", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { ControllersNamespace },
                constraints: new { culture = "[a-z]{2}-[a-z]{2}" }
            );

            context.MapRoute(
                "Admin_default",
                AreaName + DefaultRouteUrl,
                new { controller = "User", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { ControllersNamespace }
            );
        }
    }
}