#region

using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.Models;
using System;
using System.Linq;
using System.Web.Mvc;

#endregion

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class RoleLogModel : ModelWithNameAndId
    {
        [DisplayNameLocalized]
        public DateTime Date { get; set; }

        [DisplayNameLocalized("user.giving.role")]
        public Guid UserId { get; set; }
        public string UserName { get; set; }

        [DisplayNameLocalized("user.receiving.role")]
        public Guid UserCibleId { get; set; }
        public string UserCibleName { get; set; }

        [DisplayNameLocalized("role")]
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }

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
                return LangHelper.Translate("role.log." + Type.ToString().ToLower());
            }
        }

        public RoleLogModel(EventLog log)
        {
            var _UserService = DependencyResolver.Current.GetService<IUserService>();
            var _RoleService = DependencyResolver.Current.GetService<IRoleService>();

            Date = log.DateTime;

            var user = _UserService.Get(Guid.Parse(log.AdditionalData1));
            UserId = user.Id;
            UserName = user.Profile.Firstname + " " + user.Profile.Lastname;

            var userCible = _UserService.Get(Guid.Parse(log.AdditionalData3));
            UserCibleId = userCible.Id;
            UserCibleName = userCible.Profile.Firstname + " " + userCible.Profile.Lastname;

            var role = _RoleService.Get(Guid.Parse(log.AdditionalData5));
            RoleId = role.Id;
            RoleName = role.Name;

            Type = log.EventLogType;
            switch (log.EventLogType)
            {
                case EventLogType.ROLE_CHANGE_ADD:
                    Description = string.Format(LangHelper.Translate("role.log.add.role"), UserName, Date, UserCibleName, RoleName);
                    break;
                case EventLogType.ROLE_CHANGE_REMOVE:
                    Description = string.Format(LangHelper.Translate("role.log.remove.role"), UserName, Date, UserCibleName, RoleName);
                    break;
            }
        }
    }
}