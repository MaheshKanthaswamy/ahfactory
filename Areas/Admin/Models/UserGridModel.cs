using Ftel.Domain.DomainModel;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Ftel.Domain.DomainModel.MembershipUser;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class UserGridModel
    {
        [Required]
        public Guid Id { get; set; }

        [DisplayNameLocalized("email")]
        [Required]
        public string Login { get; set; }

        [DisplayNameLocalized]
        public string Firstname { get; set; }

        [DisplayNameLocalized]
        public string Lastname { get; set; }

        [DisplayNameLocalized]
        public string Password { get; set; }

        [DisplayNameLocalized]
        public DateTime? LastLoginDate { get; set; }

        [DisplayNameLocalized]
        public string SocietyName { get; set; }

        [DisplayNameLocalized("society")]
        public Guid? SocietyId { get; set; }

        [DisplayNameLocalized]
        [UIHint("RoleList")]
        public IEnumerable<Role> Roles { get; set; }

        [DisplayNameLocalized]
        public Statut Statut { get; set; }

        [DisplayNameLocalized]
        public string TypeStatut
        {
            get
            {
                switch (Statut)
                {
                    case Statut.ALLOWED:
                        return LangHelper.Translate("allowed");
                    case Statut.INPROGRESS:
                        return LangHelper.Translate("inprogress");
                    case Statut.REFUSED:
                        return LangHelper.Translate("refused");
                    default:
                        return null;
                }
            }
        }

        [DisplayNameLocalized]
        [UIHint("MultiSelect")]
        public IEnumerable<ModelFiltering> Filtering { get; set; }
    }

    public class ModelFiltering
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}

