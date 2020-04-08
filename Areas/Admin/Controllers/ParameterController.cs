using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.FilterAttributes;
using Ftel.WebSite.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Ftel.Domain.Constants;
using Microsoft.Practices.Unity;
using Ftel.WebSite.ViewModels;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class ParameterController : BaseController
    {
        #region Properties
        private readonly IParameterService _ParameterService;
        private readonly IUnitOfWorkManager _UnitOfWorkManager;
        public readonly IDocumentService _DocumentService;
        #endregion

        #region Construct

        public ParameterController(IUnitOfWorkManager unitOfWorkManager,
            IParameterService ParameterService, IDocumentService DocumentService)
        {
            _UnitOfWorkManager = unitOfWorkManager;
            _ParameterService = ParameterService;
            _DocumentService = DocumentService;
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
                    Link = Url.Action("Admin", "Page", new {area = ""}),
                    Name = LangHelper.Translate("admin")
                },
                new BreadcrumbModel
                {
                    Enabled = false,
                    Link = "#",
                    Name = LangHelper.Translate("parameter.management")
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
            return Json(ToModel(Read()).Where(m => m.Name != AppConstants.Params.THUMBNAIL_DEFAULT_URL).ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateOrUpdate([DataSourceRequest] DataSourceRequest request, ParametersGridModel model)
        {
            return Json(new[] {CreateOrUpdate(model)}.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, ParametersGridModel model)
        {
            return Json(new[] {Delete(model)}.ToDataSourceResult(request, ModelState));
        }

        #endregion

        #region Private Methods

        public Parameter ToEntity(ParametersGridModel from, Parameter to)
        {
            to.Name = from.Name;
            to.StringValue = from.Value;
            return to;
        }

        public ParametersGridModel ToModel(Parameter entity)
        {
            return ToModel(new[] {entity}).First();
        }

        public IEnumerable<ParametersGridModel> ToModel(IEnumerable<Parameter> entities)
        {
            return entities.Select(x => new ParametersGridModel
            {
                Name = x.Name,
                Value = x.StringValue
            });
        }

        private IEnumerable<Parameter> Read()
        {
            return _ParameterService.GetAll();
        }

        public ParametersGridModel CreateOrUpdate(ParametersGridModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        Guid item = _ParameterService.AddOrUpdate(ParameterType.ADMINISTRATION, model.Name, model.Value);
                        // Save
                        uow.Commit();

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

        public ParametersGridModel Delete(ParametersGridModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        _ParameterService.DeleteByName(model.Name);
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

        public ActionResult DefaultThumbnailImage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DefaultThumbnailImage(HttpPostedFileBase File)
        {
            if (File != null)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        byte[] content = new byte[] {};
                        using (var ms = new MemoryStream())
                        {
                            File.InputStream.CopyTo(ms);
                            content = ms.ToArray();
                        }

                        var doc = _DocumentService.AddDocument(
                            content,
                            File.FileName,
                            User.Identity.Name,
                            Guid.NewGuid()
                        );
                        uow.SaveChanges();

                        var param = _ParameterService.GetByName(AppConstants.Params.THUMBNAIL_DEFAULT_URL);
                        param.StringValue = Url.Action("GetFile", "Api", new {area = "", id = doc.Id});
                        uow.Commit();
                    }
                    catch (Exception ex)
                    {
                        uow.Rollback();

                        if (System.Web.HttpContext.Current.Request.IsLocal)
                        {
                            throw ex;
                        }
                    }
                }
            }
            return Message("default.thumbnail.updated", urlButton:Url.Action("Index", "Parameter", "Admin"));
        }
    }
}