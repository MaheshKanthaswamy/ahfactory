using Ftel.Domain.DomainModel;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class UserInfoModel : ModelWithId, IValidatableObject
    {
        [Required(ErrorMessage = "Le champ {0} est requis")]
        [DisplayNameLocalized]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Le champ {0} est requis")]
        [DisplayNameLocalized]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "Le champ {0} est requis")]
        [DisplayNameLocalized]
        public string Email { get; set; }

        [DisplayNameLocalized]
        public string Password { get; set; }

        [DisplayNameLocalized]
        public string VerifPassword { get; set; }

        [Required(ErrorMessage = "Le champ {0} est requis")]
        [DisplayNameLocalized("civility")]
        public Sexe Sexe { get; set; }

        [DisplayNameLocalized]
        public string Function { get; set; }

        [DisplayNameLocalized]
        public string Society { get; set; }

        [DisplayNameLocalized("document.ranges")]
        public IEnumerable<RangeModel> Ranges { get; set; }

        public bool Newsletter { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VerifPassword != Password)
                yield return new ValidationResult(LangHelper.Translate("verif.password.not.the.same"), new List<string> { "VerifPassword" });
        }
    }
}