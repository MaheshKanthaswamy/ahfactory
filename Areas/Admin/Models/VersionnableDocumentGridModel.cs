using Ftel.WebSite.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ftel.WebSite.Models;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class VersionnableDocumentGridModel : ModelWithNameAndId
    {
        [DisplayNameLocalized]
        public string Ranges { get; set; }
        [DisplayNameLocalized]
        public string Category { get; set; }
        [DisplayNameLocalized]
        public DateTime? DocumentEndValidityDate { get; set; }
        [DisplayNameLocalized]
        public Guid LastVersionId { get; set; }
    }
    public class VersionnableDocumentDetailGridModel : ModelWithNameAndId
    {
        [DisplayNameLocalized]
        public int Version { get; set; }
        [DisplayNameLocalized]
        public DateTime VersionStart { get; set; }
    }
}
