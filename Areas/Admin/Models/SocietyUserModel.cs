using Ftel.Domain.DomainModel;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Helpers;
using System;
using static Ftel.Domain.DomainModel.MembershipUser;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class SocietyUserModel
    {
        public SocietyUserModel()
        {

        }

        public SocietyUserModel(MembershipUser user)
        {
            var profile = user.Profile;

            Id = user.Id;
            Firstname = profile.Firstname;
            Lastname = profile.Lastname;
            Statut = user.StatutUser;
            LastActivityDate = user.LastActivityDate;
        }

        public Guid Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        [DisplayNameLocalized("user")]
        public string Name
        {
            get
            {
                return Firstname + " " + Lastname;
            }
        }

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
        public DateTime? LastActivityDate { get; set; }
    }
}
