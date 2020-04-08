using Ftel.WebSite.Attributes;
using Ftel.WebSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class ArchivedDocumentsGridModel : ModelWithId
    {
        [DisplayNameLocalized]
        public string Name { get; set; }

        [DisplayNameLocalized]
        public DateTime DateCreation { get; set; }

        [DisplayNameLocalized]
        public DateTime? ValidityDate { get; set; }

        [DisplayNameLocalized]
        public string ArchivedBy { get; set; }

        public Guid LastVersionId { get; set; }
    }
}