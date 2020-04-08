using Ftel.WebSite.Attributes;
using System;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class UserInRoleGridModel
    {
        public string Login { get; set; }
        [DisplayNameLocalized("name")]
        public string FullName { get; set; }
        public Guid Id { get; set; }
    }
}
