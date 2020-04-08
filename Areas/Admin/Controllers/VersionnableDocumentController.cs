using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.WebSite.FilterAttributes;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AHDigitalFactory.Domain.Interfaces.Services;
using Kendo.Mvc.Extensions;
using Ftel.WebSite.Helpers;
using Ftel.Domain.Interfaces.Services;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.Domain.DomainModel;
using Ftel.Utilities.Infrastructure;
using Ftel.WebSite.Controllers;
using Ftel.Domain.Constants;
using Ftel.WebSite.Models;
using Microsoft.Practices.Unity;
using Ftel.WebSite.ViewModels;
using Ftels.Utilities;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class VersionnableDocumentController : GridController<VersionnableDocument, VersionnableDocumentGridModel>
    {
        [Dependency]
        public IVersionnableDocumentService _VersionnableDocumentService { get; set; }
        public VersionnableDocumentController(IVersionnableDocumentService VersionnableDocumentService) : base(VersionnableDocumentService)
        {
            _VersionnableDocumentService = VersionnableDocumentService;
        }


        public override IEnumerable<VersionnableDocumentGridModel> ToModel(IEnumerable<VersionnableDocument> entities)
        {
            return entities.Select(x => new VersionnableDocumentGridModel
            {
                Id = x.Id,
                Name = x.Name,
                Ranges = string.Join(", ", x.CurrentVersion.Ranges.OrderBy(r => r.Name).Select(r => r.Name)),
                DocumentEndValidityDate = x.ValidityDate,
                Category = LangHelper.Translate(x.Type.ToString().ToLower().ToCamelCase().CamelCaseToRessourcePropertyName()),
                LastVersionId = x.CurrentVersion.Document.Id
            });
        }

        protected override IEnumerable<VersionnableDocument> Read()
        {
            return _VersionnableDocumentService.GetAll().Where(v => v.Status == DocumentStatus.AVAILABLE);
        }

        public override VersionnableDocument ToEntity(VersionnableDocumentGridModel from, VersionnableDocument to)
        {
            to.CurrentVersion.Name = from.Name;
            return to;
        }
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
                    Enabled = false,
                    Link = "#",
                    Name = LangHelper.Translate("document.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }


        public virtual ActionResult ReadDetails([DataSourceRequest] DataSourceRequest request, Guid id)
        {
            var versionnable = _VersionnableDocumentService.Get(id);
            var versions = versionnable.Versions.OrderBy(v => v.DateUpdated)
                .ToList().Select(v => new VersionnableDocumentDetailGridModel
                {
                    Id = v.Id,
                    Name = v.Name,
                    Version = versionnable.Versions.OrderBy(vv => vv.DateUpdated).ToList().FindIndex(vv => vv.Id == v.Id) + 1,
                    VersionStart = v.Document.DateCreated
                });

            var model = versions;
            return Json(model.ToDataSourceResult(request, ModelState));
        }

    }
}