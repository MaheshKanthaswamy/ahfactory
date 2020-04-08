using System.Web.Mvc;

namespace Ftel.WebSite.Areas.Mail
{
    public class MailAreaRegistration : AreaRegistration 
    {
        public const string AREA_NAME = "Mail";
        public override string AreaName => AREA_NAME;

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Mail_default",
                "Mail/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}