using Ftel.Domain.DomainModel;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class SocietyGridModel : ModelWithId
    {
        [DisplayNameLocalized]
        public string SocietyName { get; set; }

        [DisplayNameLocalized]
        public DateTime DateCreation { get; set; }

        [DisplayNameLocalized]
        public int ClientCount { get; set; }

        [DisplayNameLocalized]
        public DateTime LastActivityDate { get; set; }

        [DisplayNameLocalized]
        [UIHint("UsersList")]
        public IEnumerable<MembershipUser> Users { get; set; }
    }
}