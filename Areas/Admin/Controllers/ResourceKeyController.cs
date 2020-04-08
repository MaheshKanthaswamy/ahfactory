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

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class ResourceKeyController : BaseController
    {
        #region Properties
        private readonly ILocalizationManagementService _LocalizationManagementService;
        private readonly IUnitOfWorkManager _UnitOfWorkManager;
        #endregion

        #region Construct
        public ResourceKeyController(IUnitOfWorkManager unitOfWorkManager,
            ILocalizationService LocalizationService, ILocalizationManagementService LocalizationManagementService)
        {
            _UnitOfWorkManager = unitOfWorkManager;
            _LocalizationManagementService = LocalizationManagementService;
        }
        #endregion

        #region GET
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Ajax - Post - Grid CRUD
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(ToModel(Read()).ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateOrUpdate([DataSourceRequest] DataSourceRequest request, LocaleResourceKeyGridModel model)
        {
            return Json(new[] { CreateOrUpdate(model) }.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, LocaleResourceKeyGridModel model)
        {
            return Json(new[] { Delete(model) }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Private Methods
        public LocaleResourceKey ToEntity(LocaleResourceKeyGridModel from, LocaleResourceKey to)
        {
            to.Id = from.ResourceId;
            to.Name = from.Key;
            to.Notes = from.Notes;

            return to;
        }
        public LocaleResourceKeyGridModel ToModel(LocaleResourceKey entity)
        {
            return ToModel(new[] { entity }).First();
        }
        public IEnumerable<LocaleResourceKeyGridModel> ToModel(IEnumerable<LocaleResourceKey> entities)
        {
            return entities.Select(x => new LocaleResourceKeyGridModel
            {
                ResourceId = x.Id,
                Key = x.Name,
                Notes = x.Notes
            });
        }
        private IEnumerable<LocaleResourceKey> Read()
        {
            return _LocalizationManagementService.GetAllResourceKeys();
        }
        public LocaleResourceKeyGridModel CreateOrUpdate(LocaleResourceKeyGridModel model)
        {
            ModelState.Remove("ResourceId");
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _LocalizationManagementService.GetResourceKey(model.ResourceId) ?? 
                            _LocalizationManagementService.Add(new LocaleResourceKey
                        {
                            Id = model.ResourceId = GuidComb.GenerateComb(),
                            Name = model.Key
                        });

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
        public LocaleResourceKeyGridModel Delete(LocaleResourceKeyGridModel model)
        {
            ModelState.Remove("ResourceId");
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _LocalizationManagementService.GetResourceKey(model.ResourceId);
                        _LocalizationManagementService.DeleteLocaleResourceKey(item);
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
    }
}