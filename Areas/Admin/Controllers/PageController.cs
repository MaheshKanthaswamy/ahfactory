using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.Domain.Constants;
using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.Models;
using Ftel.WebSite.ViewModels;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    public class PageController : BaseController
    {
        [Dependency]
        public IPageService _PageService { get; set; }

        [Dependency]
        public ILocalizationManagementService _LocalizationManagementService { get; set; }

        [Dependency]
        public IUnitOfWorkManager _UnitOfWorkManager { get; set; }

        public ActionResult EditPage(PageType PageType, string PageName)
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
                    Name = PageName
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            var page = _PageService.GetByType(PageType).FirstOrDefault();
            var model = new EditPageModel
            {
                Id = page == null ? Guid.Empty : page.Id,
                Content = page == null ? "" : page.Content,
                PageName = PageName,
                Type = PageType
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAjax]
        public ActionResult EditPage(EditPageModel model)
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
                    Name = model.PageName
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            if (ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    Page page;
                    if (model.Id == Guid.Empty)
                    {
                        // Page qui n'existe pas encore en base
                        page = new Page
                        {
                            Type = model.Type,
                            Content = HttpUtility.HtmlDecode(model.Content),
                            Lang = _LocalizationManagementService.GetLanguageByName(AppConstants.DefaultLanguage)
                        };

                        _PageService.Create(page);
                    }
                    else
                    {
                        page = _PageService.Get(model.Id);
                        page.Content = HttpUtility.HtmlDecode(model.Content);
                    }

                    uow.Commit();

                    return RedirectToAction("EditPage", new { PageType = page.Type, PageName = model.PageName });
                }
            }

            return View(model);
        }
    }
}