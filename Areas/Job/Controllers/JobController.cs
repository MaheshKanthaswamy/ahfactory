#region

using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.Domain.Constants;
using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.WebSite.Areas.Mail;
using Ftel.WebSite.Areas.Mail.Models;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

#endregion

namespace Ftel.WebSite.Areas.Job.Controllers
{
    public class JobController : BaseController
    {
        [Dependency]
        public IUserService _UserService { get; set; }

        [Dependency]
        public IDocumentService _DocumentService { get; set; }

        [Dependency]
        public IDocumentExceptionService _DocumentExceptionService { get; set; }

        [Dependency]
        public IVersionnableDocumentService _VersionnableDocumentService { get; set; }

        [Dependency]
        public IMailService _MailService { get; set; }

        [Dependency]
        public IEventLogService _EventLogService { get; set; }

        [Dependency]
        public IUnitOfWorkManager _UnitOfWorkManager { get; set; }

        [Dependency]
        public IParameterService _ParameterService { get; set; }

        public ActionResult CheckDocumentValidity()
        {
            var documents = _VersionnableDocumentService.GetAllByStatus(DocumentStatus.AVAILABLE);
            var documentArchived = new List<Guid>();

            var now = DateTime.Now;
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                foreach (var doc in documents)
                {
                    if (doc.ValidityDate < now)
                    {
                        var toupdate = _DocumentService.Get(doc.Id);
                        toupdate.Status = DocumentStatus.ARCHIVED;

                        documentArchived.Add(doc.Id);
                    }
                }

                uow.Commit();
            }

            return Json(documentArchived, JsonRequestBehavior.AllowGet);
        }

        private void SendCheckUserMail(Guid UserId)
        {
            var user = _UserService.Get(UserId);

            var subject = LangHelper.Translate("account.is.running.out");
            var body = WebPartHelper.WebPart("EndDateValidity", "Mail", MailAreaRegistration.AREA_NAME, false, user.Id);
#if DEBUG
            var to = "jlemonsu@ftel.fr";
#else
            var to = user.Profile.Email;
#endif
            var from = AppConstants.DefaultFromAddress.Address;

            _MailService.SendMail(subject, body, to.Split(';'), from);

            _EventLogService.AddLine(EventLogType.EMAIL_ACCOUNT_VALIDITY, user.Id.ToString(), user.Login, subject, body, to, from);
        }
        public ActionResult CheckUserValidity()
        {
            var now = DateTime.Now;
            var users = _UserService.GetAllNoBayer();
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {

                foreach (var user in users)
                {
                    var validityEndDate = user.ValidityEndDate;

                    // 1 mois
                    if (now >= validityEndDate.AddMonths(-1))
                    {
                        if (user.UserAccountStatus == MembershipUser.AccountStatus.CLEAR)
                        {
                            SendCheckUserMail(user.Id);
                            user.UserAccountStatus = MembershipUser.AccountStatus.ONE_MONTH_REMAINING;
                        }

                        // 3 semaines
                        if (now >= validityEndDate.AddDays(-(3 * 7)))
                        {
                            if (user.UserAccountStatus == MembershipUser.AccountStatus.ONE_MONTH_REMAINING)
                            {
                                SendCheckUserMail(user.Id);
                                user.UserAccountStatus = MembershipUser.AccountStatus.THREE_WEEKS_REMAINING;
                            }

                            // 2 semaines
                            if (now >= validityEndDate.AddDays(-(2 * 7)))
                            {
                                if (user.UserAccountStatus == MembershipUser.AccountStatus.THREE_WEEKS_REMAINING)
                                {
                                    SendCheckUserMail(user.Id);
                                    user.UserAccountStatus = MembershipUser.AccountStatus.TWO_WEEKS_REMAINING;
                                }

                                // 1 semaine
                                if (now >= validityEndDate.AddDays(-(1 * 7)))
                                {
                                    if (user.UserAccountStatus == MembershipUser.AccountStatus.TWO_WEEKS_REMAINING)
                                    {
                                        SendCheckUserMail(user.Id);
                                        user.UserAccountStatus = MembershipUser.AccountStatus.ONE_WEEKS_REMAINING;
                                    }

                                    // Compte deviens invalide
                                    if (now >= validityEndDate)
                                    {
                                        if (user.UserAccountStatus == MembershipUser.AccountStatus.ONE_WEEKS_REMAINING)
                                        {
                                            user.StatutUser = MembershipUser.Statut.REFUSED;
                                            user.UserAccountStatus = MembershipUser.AccountStatus.CLOSED;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                uow.Commit();
            }

            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        private void SendNewsletterMail(MembershipUser User, List<VersionnableDocument> Docs)
        {
            var DocsModel = Docs.Select(d => new NewsletterDocument
            {
                Id = d.Id,
                Name = d.Name,
                FileName = d.CurrentDocument.Name,
                URL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("Details", "Document", new { area = "", id = d.CurrentDocument.Id })),
                ImageURL = Helpers.UrlHelper.GenerateExternalUri(Url.Action("GetFile", "Api", new { area = "", id = d.CurrentDocument.Id, width = 150, height = 150 })),
                DateCreated = d.DateCreated,
                Type = d.Type,
                Ranges = d.CurrentVersion.Ranges.Select(r => new ModelWithNameAndId
                {
                    Id = r.Id,
                    Name = r.Name
                })
            });

            var subject = LangHelper.Translate("email.newsletter.subject");
            var body = WebPartHelper.WebPart("Newsletter", "Mail", MailAreaRegistration.AREA_NAME, false, User.Id, DocsModel);
//#if DEBUG
//            var to = "jlemonsu@ftel.fr";
//#else
//            var to = User.Profile.Email;
//#endif
            var to = User.Profile.Email;
            var from = AppConstants.DefaultFromAddress.Address;

            _MailService.SendMail(subject, body, to.Split(';'), from);

            _EventLogService.AddLine(EventLogType.EMAIL_NEWSLETTER, User.Id.ToString(), User.Login, subject, body, to, from);
        }
        public ActionResult Newsletter()
        {
            var now = DateTime.Now;
            var users = _UserService.GetAllSuscribedToNewsletter();
            var documents = _VersionnableDocumentService.GetAllByStatusAndInterval(DocumentStatus.AVAILABLE, ParameterHelper.LastNewsletterDate, now);

            var docMails = new List<VersionnableDocument>();
            var userDocMails = new Dictionary<MembershipUser, List<VersionnableDocument>>();


            foreach (var user in users)
            {
                docMails.Clear();
                var exceptions = _DocumentExceptionService.GetAllByUser(user.Id).Select(o => o.Id);
                var newDocs = documents.Where(o => exceptions.Contains(o.Id));
                IList<DocumentType> docType = new List<DocumentType>();
                foreach (string filter in "0;1;2;3;4".Split(';'))
                {
                    DocumentType outDocType;
                    if (Enum.TryParse<DocumentType>(filter, out outDocType))
                        docType.Add(outDocType);
                }
                newDocs = newDocs.Where(d => docType.Contains(d.CurrentVersion.VersionnableDocument.Type));

                foreach (var doc in newDocs)
                {
                    var userRanges = user.Ranges.Select(r => r.Id);
                    var docRanges = doc.CurrentVersion.Ranges.Select(r => r.Id);

                    bool hasRange = userRanges.Intersect(docRanges).Any();
                    if (hasRange)
                    {
                        docMails.Add(doc);
                    }
                }

                userDocMails.Add(user, docMails.ToList());
            }

            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                foreach (var userDoc in userDocMails)
                {
                    if (userDoc.Value.Any())
                    {
                        SendNewsletterMail(userDoc.Key, userDoc.Value);
                    }
                }

                var param = _ParameterService.GetByName(AppConstants.Params.LAST_NEWSLETTER_DATE);
                param.StringValue = now.ToString();

                uow.Commit();
            }

            return Json("ok", JsonRequestBehavior.AllowGet);
        }
    }
}