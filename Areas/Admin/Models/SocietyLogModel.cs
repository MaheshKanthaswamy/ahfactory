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
using Kendo.Mvc.Extensions;
using Kendo.Mvc.Resources;
using Microsoft.Ajax.Utilities;
using UrlHelper = Ftel.WebSite.Helpers.UrlHelper;

#endregion

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class SocietyLogModel : ModelWithNameAndId
    {
        [DisplayNameLocalized]
        public Guid VersionId { get; set; }
        [DisplayNameLocalized]
        public DateTime Date { get; set; }
        public ModelWithNameAndId User { get; set; }
        [DisplayNameLocalized("user")]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        [DisplayNameLocalized]
        public int? Version { get; set; }
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
        public string Action
        {
            get
            {
                return LangHelper.Translate("log." + Type.ToString().ToLower());
            }
        }

        public SocietyLogModel(EventLog log)
        {
            var _UserService = DependencyResolver.Current.GetService<IUserService>();

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
            switch (log.EventLogType)
            {
                case EventLogType.FILE_CONSULTATION:
                    Description = string.Format(LangHelper.Translate("document.log.file.consultation.description"), User, Date);
                    Id = Guid.Parse(log.AdditionalData5);
                    VersionId = Guid.Parse(log.AdditionalData3);
                    Version = int.Parse(log.AdditionalData7).AsNullable();
                    break;
                case EventLogType.FILE_DOWNLOAD:
                    Description = string.Format(LangHelper.Translate("document.log.file.download.description"), User, Date);
                    Id = Guid.Parse(log.AdditionalData5);
                    VersionId = Guid.Parse(log.AdditionalData3);
                    Version = int.Parse(log.AdditionalData7).AsNullable();
                    break;
                case EventLogType.SOURCE_DOWNLOAD:
                    Description = string.Format(LangHelper.Translate("document.log.source.download.description"), User, Date);
                    Id = Guid.Parse(log.AdditionalData4);
                    Version = null;
                    break;
            }
            Name = log.AdditionalData8;
        }
    }
}