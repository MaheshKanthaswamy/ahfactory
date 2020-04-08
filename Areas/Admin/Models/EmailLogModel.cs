#region

using System.ComponentModel.DataAnnotations;
using Ftel.WebSite.Attributes;
using System;
using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.WebSite.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.WebSite.Models;
using Kendo.Mvc.Resources;
using Microsoft.Ajax.Utilities;
using UrlHelper = Ftel.WebSite.Helpers.UrlHelper;

#endregion

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class EmailLogModel : ModelWithNameAndId
    {
        [DisplayNameLocalized]
        public DateTime Date { get; set; }
        public ModelWithNameAndId User { get; set; }
        [DisplayNameLocalized("user")]
        public Guid UserId { get; set; }
        public string UserName { get; set; }

        [DisplayNameLocalized]
        public string Description { get; set; }
        [DisplayNameLocalized]
        public EventLogType Type { get; set; }
        [DisplayNameLocalized]
        public DateTime Day
        {
            get
            {
                return new DateTime(Date.Year, Date.Month, Date.Day);
            }
        }
        [DisplayNameLocalized]
        public DateTime Hour
        {
            get
            {
                return Date;
            }
        }

        [DisplayNameLocalized]
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }

        public Guid? AdminId { get; set; }
        public string AdminLogin { get; set; }

        public EmailLogModel(EventLog log)
        {
            var _UserService = DependencyResolver.Current.GetService<IUserService>();
            var _VersionnableDocumentService = DependencyResolver.Current.GetService<IVersionnableDocumentService>();

            Date = log.DateTime;
            var user = _UserService.Get(Guid.Parse(log.AdditionalData1));
            User = new ModelWithNameAndId
            {
                Id = user.Id,
                Name = user.Profile.Firstname + " " + user.Profile.Lastname
            };
            UserId = User.Id;
            UserName = User.Name;
            Type = log.EventLogType;

            Subject = log.AdditionalData3;
            To = log.AdditionalData5;
            From = log.AdditionalData6;
            Cc = log.AdditionalData7;
            Bcc = log.AdditionalData8;

            Guid adminid;
            if (Guid.TryParse(log.AdditionalData9, out adminid)){
                AdminId = adminid;
            }

            AdminLogin = log.AdditionalData10;
			Id = log.Id;
        }
    }
}