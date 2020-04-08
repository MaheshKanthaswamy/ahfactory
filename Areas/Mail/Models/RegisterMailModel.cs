using Ftel.WebSite.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ftel.WebSite.Areas.Mail.Models
{
    public class RegisterMailModel
    {
        public string UserName { get; set; }
        public string URL { get; set; }
    }
}