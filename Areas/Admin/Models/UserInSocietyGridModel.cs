using Ftel.WebSite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class UserInSocietyGridModel
    {
        public string Login { get; set; }
        [DisplayNameLocalized("name")]
        public string FullName { get; set; }
    }
}