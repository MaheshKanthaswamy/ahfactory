using Ftel.WebSite.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class LanguageGridModel
    {
        public Guid LanguageId { get; set; }
        [Required]
        [DisplayNameLocalized("name")]
        public string Name { get; set; }
    }

    public class LocaleResourceKeyGridModel
    {
        public Guid ResourceId { get; set; }
        [Required]
        [DisplayNameLocalized("key")]
        public string Key { get; set; }
        [DisplayNameLocalized("notes")]
        public string Notes { get; set; }
    }

    public class LocaleResourceValueGridModel
    {
        public Guid LocalStringId { get; set; }
        public Guid LanguageId { get; set; }
        public string LanguageName { get; set; }
        public Guid ResourceKeyId { get; set; }
        [DisplayNameLocalized("key")]
        public string ResourceKey { get; set; }
        [Required]
        [DisplayNameLocalized("value")]
        public string Value { get; set; }
    }
}
