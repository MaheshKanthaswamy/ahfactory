using Ftel.WebSite.Models;

namespace Ftel.WebSite.Areas.Mail.Models
{
    public class ForgotPasswordMailModel : ModelWithNameAndId
    {
        public string URL { get; set; }
    }
}