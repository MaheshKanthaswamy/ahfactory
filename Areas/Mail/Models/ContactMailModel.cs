using Ftel.WebSite.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ftel.WebSite.Areas.Mail.Models
{
    public class ContactMailModel
    {
        [DisplayNameLocalized("contact.mail.object")]
        [Required]
        public string Object { get; set; }

        [DisplayNameLocalized("contact.mail.question")]
        [UIHint("TextArea")]
        [Required]
        public string Question { get; set; }

        public string Email { get; set; }
    }
}