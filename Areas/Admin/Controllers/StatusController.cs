using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.FilterAttributes;
using Ftel.WebSite.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Ftel.Domain.Constants;
using Ftel.Domain.DomainModel.Framework;
using Ftel.WebSite.ViewModels;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class StatusController : BaseController
    {
        #region Properties
        private readonly IStatusService _StatusService;
        private readonly IUnitOfWorkManager _UnitOfWorkManager;
        #endregion

        #region Construct
        public StatusController(IUnitOfWorkManager unitOfWorkManager,
            IStatusService StatusService)
        {
            _UnitOfWorkManager = unitOfWorkManager;
            _StatusService = StatusService;
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
                    Name = LangHelper.Translate("status.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }
        #endregion

        #region Ajax - Post - Grid CRUD
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(ToModel(Read()).ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateOrUpdate([DataSourceRequest] DataSourceRequest request, StatusGridModel model)
        {
            return Json(new[] { CreateOrUpdate(model) }.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, StatusGridModel model)
        {
            return Json(new[] { Delete(model) }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Private Methods
        public Status ToEntity(StatusGridModel from, Status to)
        {
            to.Name = from.Name;
            return to;
        }

        public StatusGridModel ToModel(Status entity)
        {
            return ToModel(new[] { entity }).First();
        }
        public IEnumerable<StatusGridModel> ToModel(IEnumerable<Status> entities)
        {
            return entities.Select(x => new StatusGridModel
            {
                Code = x.Code,
                Name = x.Name,
            });
        }

        private IEnumerable<Status> Read()
        {
            return _StatusService.All();
        }
        public StatusGridModel CreateOrUpdate(StatusGridModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        // Create or update the item
                        var item = _StatusService.AddOrUpdate(model.Code, model.Name);
                        
                        // Save
                        uow.Commit();

                        // Transfert back value
                        model = ToModel(item);

                        return model;
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
        public StatusGridModel Delete(StatusGridModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        _StatusService.Delete(model.Code);
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
            ModelState.AddModelError("", LangHelper.Translate("an.error.occured"));
        }
        protected virtual void OnUpdateError(Exception ex)
        {
            ModelState.AddModelError("", LangHelper.Translate("an.error.occured"));
        }
        #endregion
    }
}