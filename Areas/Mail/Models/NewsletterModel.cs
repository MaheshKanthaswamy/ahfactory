using Ftel.Domain.DomainModel;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.Models;
using System;
using System.Collections.Generic;

namespace Ftel.WebSite.Areas.Mail.Models
{
    public class NewsletterModel
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string URL { get; set; }
        public IEnumerable<NewsletterDocument> Docs { get; set; }
    }

    public class NewsletterDocument
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string ImageURL { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<ModelWithNameAndId> Ranges { get; set; }
        public DocumentType Type { get; set; }
        public string FileName { get; set; }

        public string Extension
        {
            get
            {
                var split = FileName.Split('.');
                return "." + split[split.Length - 1];
            }
        }

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