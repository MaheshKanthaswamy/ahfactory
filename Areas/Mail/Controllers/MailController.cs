using Ftel.Domain.Interfaces.Services;
using Ftel.WebSite.Areas.Mail.Models;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.Models;
using Ftel.WebSite.ViewModels;
using Microsoft.Practices.Unity;
using System;
using System.Web.Mvc;
using Ftels.Utilities;
using Kendo.Mvc.Extensions;
using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.Domain.DomainModel;
using System.Collections.Generic;
using System.Linq;

namespace Ftel.WebSite.Areas.Mail.Controllers
{
    public class MailController : BaseController
    {
        [Dependency]
        public IMailService MailService { get; set; }

        [Dependency]
        public IUserService UserService { get; set; }

        [Dependency]
        public IVersionnableDocumentService _VersionnableDocumentService { get; set; }

        [External]
        public ActionResult ForgotPasswordEmail(Guid id)
        {
            var user = UserService.Get(id);
            var model = new ForgotPasswordMailModel
            {
                URL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("ForgotPasswordReset", "User", new { area = "", key = user.ForgotPasswordKey }))
            };

            ViewBag.Headline = LangHelper.Translate("email.forgot.password.headline");
            ViewBag.DeliveryInfo = LangHelper.Translate("email.forgot.password.delivery.info");
            ViewBag.SiteURL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("Index", "Home", new { area = "" }));

            return View(model);
        }

        public ActionResult RegisterUserValidated(Guid id)
        {
            var user = UserService.Get(id);
            var action = Url.Action("LogOn", "User", new { area = "" });

            var model = new ForgotPasswordMailModel
            {
                URL = Helpers.UrlHelper.GenerateExternalUri(action)
            };

            ViewBag.Headline = LangHelper.Translate("email.register.user.validated.title");
            ViewBag.DeliveryInfo = LangHelper.Translate("email.register.user.validated.delivery.info");
            ViewBag.SiteURL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("Index", "Home", new { area = "" }));

            return View(model);
        }

        public ActionResult ContactEmail(ContactMailModel model)
        {
            ViewBag.Headline = LangHelper.Translate("email.support.headline");
            ViewBag.DeliveryInfo = LangHelper.Translate("email.support.footer.delivery.info");
            ViewBag.SiteURL = Helpers.UrlHelper.GenerateInternalUri(Url.Action("Index", "Home", new { area = "" }));

            return View(model);
        }

        public ActionResult Register(Guid userId)
        {
            ViewBag.Headline = LangHelper.Translate("email.register.headline");
            ViewBag.DeliveryInfo = LangHelper.Translate("email.register.footer.delivery.info");
            ViewBag.SiteURL = Helpers.UrlHelper.GenerateInternalUri(Url.Action("Index", "Home", new { area = "" }));

            var user = UserService.Get(userId);
            var action = Url.Action("Edit", "User", new { area = "Admin", Id = user.Id });

            var model = new RegisterMailModel
            {
                UserName = LangHelper.Translate(user.Profile.Sexe.ToString().ToLower().ToCamelCase().CamelCaseToRessourcePropertyName()) + " " + user.Profile.Firstname + " " + user.Profile.Lastname,
                URL = Helpers.UrlHelper.GenerateInternalUri(action)
            };
            return View(model);
        }

        public ActionResult EndDateValidity(Guid userId)
        {
            var user = UserService.Get(userId);

            var action = Url.Action("LogOn", "User", new { area = "" });
            var model = new EndValidityMailModel
            {
                URL = Helpers.UrlHelper.GenerateExternalUri(action),
                ExpirationDate = user.ValidityEndDate,
                Firstname = user.Profile.Firstname,
                Lastname = user.Profile.Lastname
            };

            ViewBag.Headline = LangHelper.Translate("email.end.validity.headline");
            ViewBag.DeliveryInfo = LangHelper.Translate("email.end.validity.delivery.info");
            ViewBag.SiteURL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("Index", "Home", new { area = "" }));

            return View(model);
        }

        public ActionResult Newsletter(Guid userId, IEnumerable<NewsletterDocument> DocsModel = null)
        {
            var user = UserService.Get(userId);
            var docMails = new List<NewsletterDocument>();

            if (DocsModel == null)
            {
                var now = DateTime.Now;
                var documents = _VersionnableDocumentService.GetAllByStatusAndInterval(DocumentStatus.AVAILABLE, ParameterHelper.LastNewsletterDate, now);

                foreach (var doc in documents)
                {
                    var userRanges = user.Ranges;
                    var docRanges = doc.CurrentVersion.Ranges;

                    bool hasRange = userRanges.Select(x => x.Id).Intersect(docRanges.Select(d => d.Id)).Any();
                    if (hasRange)
                    {
                        docMails.Add(new NewsletterDocument
                        {
                            Id = doc.Id,
                            Name = doc.Name,
                            FileName = doc.CurrentDocument.Name,
                            URL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("Details", "Document", new { area = "", id = doc.CurrentDocument.Id })),
                            ImageURL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("GetFile", "Api", new { area = "", id = doc.CurrentDocument.Id, width = 150, height = 150 })),
                            DateCreated = doc.DateCreated,
                            Type = doc.Type,
                            Ranges = doc.CurrentVersion.Ranges.Select(r => new ModelWithNameAndId
                            {
                                Id = r.Id,
                                Name = r.Name
                            })
                        });
                    }
                }
            }
            else
            {
                docMails = DocsModel.ToList();
            }

            var url = user.IsInternalBayer ? Helpers.UrlHelper.GenerateInternalUri(Url.Action("Index", "Home", new { area = "" })) : Helpers.UrlHelper.GenerateExternalUri(Url.Action("LogOn", "User", new { area = "" }));
            var model = new NewsletterModel
            {
                URL = url,
                Firstname = user.Profile.Firstname,
                Lastname = user.Profile.Lastname,
                Docs = docMails
            };

            ViewBag.Headline = LangHelper.Translate("email.newsletter.headline");
            ViewBag.DeliveryInfo = string.Format(LangHelper.Translate("email.newsletter.delivery.info"), " <a href=\""+ Helpers.UrlHelper.GenerateExternalUri(Url.Action("Infos", "User", new { area = "" }))+"\">en cliquant ici</a>");

            if (user.IsInternalBayer)
            {
                ViewBag.SiteURL = Helpers.UrlHelper.GenerateInternalUri(Url.Action("Index", "Home", new { area = "" }));
            } else
            {
                ViewBag.SiteURL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("Index", "Home", new { area = "" }));
            }

            return View(model);
        }
    }
}