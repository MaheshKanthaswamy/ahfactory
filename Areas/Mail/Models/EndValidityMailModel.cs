using Ftel.WebSite.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ftel.WebSite.Areas.Mail.Models
{
    public class EndValidityMailModel
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string URL { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}