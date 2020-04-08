using Ftel.WebSite.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class StatusGridModel
    {
        [DisplayNameLocalized("code")]
        [Required]
        public int Code { get; set; }

        [DisplayNameLocalized("name")]
        [Required]
        public string Name { get; set; }
    }
}