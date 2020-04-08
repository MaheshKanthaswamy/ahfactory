using Ftel.Domain.DomainModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class EditPageModel
    {
        public Guid Id { get; set; }
        public PageType Type { get; set; }
        public string Content { get; set; }
        public string PageName { get; set; }
    }
}
