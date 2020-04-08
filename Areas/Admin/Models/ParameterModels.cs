using Ftel.WebSite.Attributes;
using Ftel.WebSite.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class ParametersGridModel
    {
        [Required]
        [DisplayNameLocalized("value")]
        public string Value { get; set; }

        [Required]
        [DisplayNameLocalized("key")]
        public string Name { get; set; }

        [DisplayNameLocalized("name")]
        public string NameFormat
        {
            get
            {
                return LangHelper.Translate(Name);
            }
        }
    }

}
