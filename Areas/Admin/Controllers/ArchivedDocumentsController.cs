using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    public class ArchivedDocumentsController : GridController<VersionnableDocument, ArchivedDocumentsGridModel>
    {
        #region Properties
        private readonly IUserService _UserService;
        private readonly ISocietyService _SocietyService;
        private readonly IVersionnableDocumentService _VersionnableDocumentService;
        #endregion

        #region Construct
        public ArchivedDocumentsController(IUserService UserService, ISocietyService SocietyService, IVersionnableDocumentService VersionnableDocumentService):base(VersionnableDocumentService)
        {
            _UserService = UserService;
            _SocietyService = SocietyService;
            _VersionnableDocumentService = VersionnableDocumentService;
        }
        #endregion

        public override IEnumerable<ArchivedDocumentsGridModel> ToModel(IEnumerable<VersionnableDocument> entities)
        {
            return entities.Select(x => new ArchivedDocumentsGridModel
            {
                Id = x.Id,
                Name = x.CurrentDocument.Name,
                DateCreation = x.DateCreated,
                //ArchivedBy = x.ArchivedBy,
                ValidityDate = x.ValidityDate,
                LastVersionId = x.CurrentVersion.Document.Id
            });
        }
        
        public override VersionnableDocument ToEntity(ArchivedDocumentsGridModel from, VersionnableDocument to)
        {
            to.CurrentDocument.Name = from.Name;
            to.CurrentDocument.DateCreated = from.DateCreation;
            //to.ArchivedBy = from.ArchivedBy;
            to.ValidityDate = from.ValidityDate;
            return to;
        }
        protected override IEnumerable<VersionnableDocument> Read()
        {
            return _VersionnableDocumentService.GetAllArchived();
        }

        // GET: Admin/ArchivedDocuments
        public ActionResult Index()
        {
            #region breadcrumb

            var breadcrumb = new List<BreadcrumbModel>()
            {
                new BreadcrumbModel
                {
                    Enabled = true,
                    Link = Url.Action("Admin", "Page", new { area = "" }),
                    Name = LangHelper.Translate("admin")
                },
                new BreadcrumbModel
                {
                    Enabled = true,
                    Link = Url.Action("Index", "VersionnableDocument", new { area = "Admin" }),
                    Name = LangHelper.Translate("document.management")
                },
                new BreadcrumbModel
                {
                    Enabled = false,
                    Link = "#",
                    Name = LangHelper.Translate("archived.documents.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }
    }
}