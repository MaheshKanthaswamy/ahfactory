using Ftel.Domain.DomainModel;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ftel.Domain.Interfaces.Services;
using Ftel.WebSite.Models;
using static Ftel.Domain.DomainModel.MembershipUser;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class UserProfileModel_ReadOnly
    {
        public Guid Id { get; set; }

        [DisplayNameLocalized]
        public string Firstname { get; set; }

        [DisplayNameLocalized]
        public string Lastname { get; set; }

        [DisplayNameLocalized]
        public string Email { get; set; }

        [DisplayNameLocalized]
        public string Login { get; set; }

        [DisplayNameLocalized]
        public DateTime? LastActivityDate { get; set; }

        [DisplayNameLocalized]
        public IEnumerable<ModelWithNameAndId> Roles { get; set; }
    }

    public class UserProfileModel : IValidatableObject
    {
        [Required]
        public Guid Id { get; set; }

        public bool IsInternalBayer { get; set; }

        [DisplayNameLocalized]
        public string Title { get; set; }

        [DisplayNameLocalized]
        public string Firstname { get; set; }

        [DisplayNameLocalized]
        public string Lastname { get; set; }

        [DisplayNameLocalized]
        [DropDownEnumList(typeof(Sexe))]
        public Sexe Civility { get; set; }

        [DisplayNameLocalized]
        public string TypeSexe
        {
            get
            {
                switch(Civility)
                {
                    case Sexe.MR:
                            return LangHelper.Translate("man");
                    case Sexe.MS:
                        return LangHelper.Translate("woman");
                    default:
                        return LangHelper.Translate("undefined");
                }
            }
        }

        [DisplayNameLocalized]
        [DropDownEnumList(typeof(Statut))]
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
        public string Email { get; set; }
        [DisplayNameLocalized]
        public bool Newsletter { get; set; }

        public string RegisteredSociety { get; set; }
        [DisplayNameLocalized]
        [Required]
        [DropDownList("GetSocieties")]
        public ModelWithNameAndId Society { get; set; }

        //public string Society { get; set; }

        [DisplayNameLocalized]
        public DateTime? LastActivityDate { get; set; }

        [DisplayNameLocalized]
        public DateTime EndValidityDate { get; set; }

        [DisplayNameLocalized]
        public Guid[] Ranges { get; set; }

        public IList<string> RangesNames { get; set; }
        public IList<string> Filtering { get; set; }

        public IList<String> FilteringNames { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var _UserService = DependencyResolver.Current.GetService<IUserService>();
            var user = _UserService.GetByEmail(Email);
            if (user != null && user.Id != Id)
                yield return new ValidationResult(LangHelper.Translate("email.already.used"), new List<string> { "Email" });
            if (Statut == Statut.INPROGRESS)
                yield return new ValidationResult(LangHelper.Translate("statut.must.be.choosen"), new List<string> { "Statut" });
            if (Society?.Id == null || Society.Id == Guid.Empty)
                yield return new ValidationResult(LangHelper.Translate("society.must.be.choosen"), new List<string> { "Society.Id" });
        }
    }
}
