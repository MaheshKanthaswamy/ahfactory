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
    public class UserLogModel : ModelWithNameAndId
    {
        [DisplayNameLocalized]
        public DateTime Date { get; set; }
        [DisplayNameLocalized]
        public ModelWithNameAndId User { get; set; }
        [DisplayNameLocalized]
        public int Version { get; set; }
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

        public UserLogModel(EventLog log)
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
            Type = log.EventLogType;
            switch (log.EventLogType)
            {
                case EventLogType.FILE_CONSULTATION:
                    Description = string.Format(LangHelper.Translate("document.log.file.consultation.description"), User, Date);
                    Id = Guid.Parse(log.AdditionalData5);
                    break;
                case EventLogType.FILE_DOWNLOAD:
                    Description = string.Format(LangHelper.Translate("document.log.file.download.description"), User, Date);
                    Id = Guid.Parse(log.AdditionalData5);
                    break;
                case EventLogType.SOURCE_DOWNLOAD:
                    Description = string.Format(LangHelper.Translate("document.log.source.download.description"), User, Date);
                    Id = Guid.Parse(log.AdditionalData4);
                    break;
            }

            if (Id != Guid.Empty)
            {
                // A revoir pour stocker le numéro de version !
                var versionnableDocument = _VersionnableDocumentService.Get(Id);
                var version =
                    versionnableDocument.Versions.Single(
                        v =>
                            v.Document.DateCreated ==
                            versionnableDocument.Versions.Where(vv => vv.Document.DateCreated < Date).Max(vv => vv.Document.DateCreated));
                Version =
                    versionnableDocument.Versions.OrderBy(v => v.DateUpdated)
                        .ToList()
                        .FindIndex(v => v.Id == version.Id) + 1;
                Name = version.Name;
            }
        }
    }
}