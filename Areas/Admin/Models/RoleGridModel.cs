using Ftel.WebSite.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class RoleGridModel
    {
        public Guid Role_ID { get; set; }

        [DisplayNameLocalized]
        [Required]
        public string Name { get; set; }

        [DisplayNameLocalized]
        public string Description { get; set; }
    }
}