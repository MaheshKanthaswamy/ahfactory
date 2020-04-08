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
using Ftel.WebSite.Controllers;
using Ftel.Domain.Constants;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class ResourceValueController : BaseController
    {
        #region Properties
        private readonly ILocalizationManagementService _LocalizationManagementService;
        private readonly ILocalizationService _LocalizationService;
        private readonly IUnitOfWorkManager _UnitOfWorkManager;
        #endregion

        #region Construct
        public ResourceValueController(IUnitOfWorkManager unitOfWorkManager,
            ILocalizationService LocalizationService, ILocalizationManagementService LocalizationManagementService)
        {
            _UnitOfWorkManager = unitOfWorkManager;
            _LocalizationManagementService = LocalizationManagementService;
            _LocalizationService = LocalizationService;
        }
        #endregion

        #region GET
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Ajax - Post - Grid CRUD
        public ActionResult Read([DataSourceRequest] DataSourceRequest request, Guid languageId)
        {
            return Json(ToModel(Read(languageId)).ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, LocaleResourceValueGridModel model)
        {
            return Json(new[] { Update(model) }.ToDataSourceResult(request, ModelState));
        }        
        #endregion

        #region Private Methods
        public LocaleResourceValueGridModel ToModel(LocaleStringResource entity)
        {
            return ToModel(new[] { entity }).First();
        }
        public IEnumerable<LocaleResourceValueGridModel> ToModel(IEnumerable<LocaleStringResource> entities)
        {
            return entities.Select(x => new LocaleResourceValueGridModel
            {
                LocalStringId = x.Id,
                LanguageName = x.Language.Name,
                ResourceKeyId = x.LocaleResourceKey.Id,
                LanguageId = x.Language.Id,
                ResourceKey = x.LocaleResourceKey.Name,
                Value = x.ResourceValue
            });
        }
        private IEnumerable<LocaleStringResource> Read(Guid langageId)
        {
            return _LocalizationManagementService.GetAllValues(langageId);
        }
        public LocaleResourceValueGridModel Update(LocaleResourceValueGridModel model)
        {
            ModelState.Remove("LocaleStringId");
            ModelState.Remove("LanguageId");
            ModelState.Remove("LocaleResourceKeyId");
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _LocalizationManagementService.GetResource(model.LanguageId, model.ResourceKey);
                        if (item != null)
                        {
                            _LocalizationManagementService.UpdateResourceString(model.LanguageId, model.ResourceKey, model.Value);
                            
                            // Save
                            uow.Commit();

                            // Update Cache
                            _LocalizationService.ReloadResourceCacheForLanguage
                            (
                                model.LanguageName,
                                _LocalizationManagementService.ResourceKeysByLanguage(model.LanguageId)
                            );
                        }

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

        protected virtual void OnDeleteError(Exception ex)
        {
            ModelState.AddModelError("", LangHelper.Translate("error.occured"));
        }
        protected virtual void OnUpdateError(Exception ex)
        {
            ModelState.AddModelError("", LangHelper.Translate("error.occured"));
        }
        #endregion
    }
}