using Ftel.WebSite.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class EmailTemplateGridModel
    {
        public Guid EmailTemplateId { get; set; }

        [DisplayNameLocalized("name")]
        public string Name { get; set; }

        [DisplayNameLocalized("to")]
        public string To { get; set; }

        [DisplayNameLocalized("subject")]
        public string Subject { get; set; }

        [DisplayNameLocalized("language")]
        public string LanguageCode { get; set; }
    }

    public class EmailTemplateUpdateModel
    {
        public Guid EmailTemplateId { get; set; }

        [Required]
        [DisplayNameLocalized("name")]
        public string Name { get; set; }
        [DisplayNameLocalized("from")]
        public string From { get; set; }
        [DisplayNameLocalized("to")]
        public string To { get; set; }
        [DisplayNameLocalized("cc")]
        public string Cc { get; set; }
        [DisplayNameLocalized("bcc")]
        public string Bcc { get; set; }
        [DisplayNameLocalized("subject")]
        public string Subject { get; set; }
        [DisplayNameLocalized("body")]
        [AllowHtml]
        public string Body { get; set; }
    }
}