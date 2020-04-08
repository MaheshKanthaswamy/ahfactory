using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.WebSite.FilterAttributes;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Ftel.WebSite.Helpers;
using Ftel.Domain.Interfaces.Services;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.Domain.DomainModel;
using Ftel.Utilities.Infrastructure;
using Ftel.WebSite.Controllers;
using Ftel.Domain.Constants;
using Microsoft.Practices.Unity;
using OfficeOpenXml;
using System.Web;
using Ftel.WebSite.ViewModels;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class LanguageController : BaseController
    {
        #region Properties
        [Dependency]
        public ILocalizationManagementService _LocalizationManagementService { get; set; }
        [Dependency]
        public IUnitOfWorkManager _UnitOfWorkManager { get; set; }

        private static IDictionary<int, string> LangCodes
        {
            get
            {
                return new Dictionary<int, string>
                {
                    { 2, "fr-FR" },
                };
            }
        }
        #endregion

        #region GET
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
                    Name = LangHelper.Translate("language.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }
        #endregion

        #region POST
        [HttpPost]
        public ActionResult ImportResources(HttpPostedFileBase file)
        {
            if (file != null)
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    int startRow = 2;
                    int startColumn = 2;
                    int keyColumn = 1;
                    int maxColumn = LangCodes.Keys.Count;

                    for (int row = startRow; worksheet.Cells[row, 1].Value != null; row++)
                    {
                        using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                        {
                            try
                            {
                                var key = worksheet.Cells[row, keyColumn].Value.ToString();
                                for (int column = startColumn; column < (maxColumn + startColumn); column++)
                                {
                                    if (worksheet.Cells[row, column].Value != null)
                                    {
                                        var traduction = worksheet.Cells[row, column].Value.ToString();
                                        var code = LangCodes[column];
                                        var language = _LocalizationManagementService.GetLanguageByName(code);

                                        if (language != null)
                                        {
                                            // Ajouter la clé si elle n'existe pas
                                            var rsxKey = _LocalizationManagementService.GetResourceKey(key);
                                            if (rsxKey == null)
                                                rsxKey = _LocalizationManagementService.Add(new LocaleResourceKey { DateAdded = DateTime.Now, Id = Guid.NewGuid(), Name = key.Trim() });

                                            var resource = _LocalizationManagementService.GetResource(language.Id, key);
                                            if (resource != null && !string.IsNullOrEmpty(traduction))
                                                resource.ResourceValue = traduction;
                                            else
                                            {
                                                resource = new LocaleStringResource
                                                {
                                                    Id = Guid.NewGuid(),
                                                    Language = language,
                                                    LocaleResourceKey = rsxKey,
                                                    ResourceValue = traduction
                                                };

                                                rsxKey.LocaleStringResources.Add(resource);
                                            }
                                        }
                                        uow.SaveChanges();
                                    }
                                }

                                uow.Commit();
                            }
                            catch (Exception e)
                            {
                                uow.Rollback();
                            }
                        }
                    }
                }
            }

            LoadLocaleResourceCache();

            return RedirectToAction("Index");
        }
        #endregion

        #region Ajax - Post - Grid CRUD
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(ToModel(Read()).ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateOrUpdate([DataSourceRequest] DataSourceRequest request, LanguageGridModel model)
        {
            return Json(new[] { CreateOrUpdate(model) }.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, LanguageGridModel model)
        {
            return Json(new[] { Delete(model) }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Mappings
        public Language ToEntity(LanguageGridModel from, Language to)
        {
            to.Id = from.LanguageId;
            to.Name = from.Name;

            return to;
        }
        public LanguageGridModel ToModel(Language entity)
        {
            return ToModel(new[] { entity }).First();
        }
        public IEnumerable<LanguageGridModel> ToModel(IEnumerable<Language> entities)
        {
            return entities.Select(x => new LanguageGridModel
            {
                LanguageId = x.Id,
                Name = x.Name
            });
        }
        private IEnumerable<Language> Read()
        {
            return _LocalizationManagementService.AllLanguages.OrderBy(x => x.Name);
        }
        public LanguageGridModel CreateOrUpdate(LanguageGridModel model)
        {
            ModelState.Remove("LanguageId");
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _LocalizationManagementService.Get(model.LanguageId);
                        if (item == null)
                        {
                            item = new Language { Id = model.LanguageId = GuidComb.GenerateComb(), Name = model.Name };
                            _LocalizationManagementService.Add(item);
                        }
                        // Properties copy
                        item = ToEntity(model, item);

                        // Save
                        uow.Commit();

                        return ToModel(item);
                    }
                    catch (Exception ex)
                    {
                        uow.Rollback();

                        OnDeleteError(ex);

                        if (System.Web.HttpContext.Current.Request.IsLocal)
                        {
                            throw;
                        }
                    }
                }
            }
            return model;
        }
        public LanguageGridModel Delete(LanguageGridModel model)
        {
            ModelState.Remove("LanguageId");
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _LocalizationManagementService.Get(model.LanguageId);
                        _LocalizationManagementService.Delete(item);
                        uow.Commit();
                    }
                    catch (Exception ex)
                    {
                        uow.Rollback();

                        OnUpdateError(ex);

                        if (System.Web.HttpContext.Current.Request.IsLocal)
                        {
                            throw;
                        }
                    }
                }
            }
            return model;
        }

        protected virtual void OnDeleteError(Exception ex)
        {
            ModelState.AddModelError("", LangHelper.Translate("error.occured"));
        }
        protected virtual void OnUpdateError(Exception ex)
        {
            ModelState.AddModelError("", LangHelper.Translate("error.occured"));
        }
        #endregion

        #region Helpers
        private void LoadLocaleResourceCache()
        {
            var LocalizationManagementService = DependencyResolver.Current.GetService<ILocalizationManagementService>();
            var LocalizationService = DependencyResolver.Current.GetService<ILocalizationService>();

            foreach (var language in LocalizationManagementService.AllLanguages)
                LocalizationService.ReloadResourceCacheForLanguage(language.Name, LocalizationManagementService.ResourceKeysByLanguage(language.Id));
        }
        #endregion
    }
}