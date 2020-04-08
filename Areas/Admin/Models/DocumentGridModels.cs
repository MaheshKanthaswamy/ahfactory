using Ftel.Domain.DomainModel;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ftel.WebSite.Areas.Admin.Models
{
    public class DocumentGridModel
    {
        public Guid Id { get; set; }

        [DisplayNameLocalized("name")]
        public string Name { get; set; }

        [DisplayNameLocalized("contenttype")]
        public string ContentType { get; set; }

        [DisplayNameLocalized("author")]
        public string Author { get; set; }

        [DisplayNameLocalized("size")]
        public int Size { get; set; }

        [DisplayNameLocalized("tags")]
        public string Tags { get; set; }

        [DisplayNameLocalized("date")]
        public DateTime DateCreated { get; set; }
    }

    public class DocumentExceptionGridModel
    {
        public string RangeIds { get; set; }
        public int TypeDocument { get; set; }
        public Guid Id { get; set; }

        public string SearchThumbnailUrl { get; set; }

        [DisplayNameLocalized("name")]
        public string Name { get; set; }

        [DisplayNameLocalized("contenttype")]
        public string ContentType { get; set; }

        [DisplayNameLocalized("author")]
        public string Author { get; set; }

        [DisplayNameLocalized("size")]
        public int Size { get; set; }

        [DisplayNameLocalized("date")]
        public DateTime DateCreated { get; set; }

        [DisplayNameLocalized("ranges")]
        public string Ranges { get; set; }

        public bool IsException { get; set; }

        public IEnumerable<ModelFiltering> Filtering { get; set; }

        public DocumentType? Type { get; set; }
        public string TypeName
        {
            get
            {
                switch (Type)
                {
                    case DocumentType.ADVERT_TOOLS:
                        return LangHelper.Translate("advert.tools");
                    case DocumentType.GUIDELINES:
                        return LangHelper.Translate("guidelines");
                    case DocumentType.IMAGES:
                        return LangHelper.Translate("images");
                    case DocumentType.OBLIGATORY_TEXT:
                        return LangHelper.Translate("obligatory.text");
                    case DocumentType.VIDEOS:
                        return LangHelper.Translate("videos");
                    case DocumentType.UNDEFINED:
                        return LangHelper.Translate("undefined");
                    default:
                        return LangHelper.Translate("unknown.type");
                }
            }
        }
    }
}
