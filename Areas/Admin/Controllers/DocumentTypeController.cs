using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.FilterAttributes;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Web;
using System.IO;
using Ftel.Domain.Constants;
using Ftel.WebSite.ViewModels;
using Ftel.WebSite.Helpers;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class DocumentTypeController : BaseController
    {
        #region Properties
        public readonly IDocumentService _DocumentService;
        public readonly IUnitOfWorkManager _UnitOfWorkManager;
        #endregion

        #region Construct
        public DocumentTypeController(IUnitOfWorkManager unitOfWorkManager,
            IDocumentService DocumentService)
        {
            _UnitOfWorkManager = unitOfWorkManager;
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
                    Link = Url.Action("Admin", "Page", new { area = "" }),
                    Name = LangHelper.Translate("admin")
                },
                new BreadcrumbModel
                {
                    Enabled = false,
                    Link = "#",
                    Name = LangHelper.Translate("document.type.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }
        #endregion

        #region POST
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> files)
        {
            if (files != null && files.Any())
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {

                        foreach (var file in files)
                        {
                            byte[] content = new byte[] {};
                            using (var ms = new MemoryStream())
                            {
                                file.InputStream.CopyTo(ms);
                                content = ms.ToArray();
                            }
                            
                            _DocumentService.AddDocument(content,  
                                file.FileName,
                                User.Identity.Name
                            );
                        }

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

            return RedirectToAction("Index");
        }
        #endregion

        #region Ajax - Post - Grid CRUD
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(ToModel(Read()).ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, DocumentGridModel model)
        {
            return Json(new[] { Delete(model) }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Private Methods
        public DocumentGridModel ToModel(DocumentDetail entity)
        {
            return ToModel(new[] { entity }).First();
        }
        public IEnumerable<DocumentGridModel> ToModel(IEnumerable<DocumentDetail> entities)
        {
            return entities.Select(x => new DocumentGridModel
            {
                Id = x.Id,
                Author = x.Author,
                Name = x.Name,
                ContentType = x.ContentType,
                Size = x.Size,
                Tags = x.Tags,
                DateCreated = x.DateCreated
            });
        }
        private IEnumerable<DocumentDetail> Read()
        {
            var docs = _DocumentService.GetDocuments();
            return docs;
        }
        public DocumentGridModel Delete(DocumentGridModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        _DocumentService.RemoveDocumentById(model.Id);
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
            return model;
        }
        #endregion
    }
}