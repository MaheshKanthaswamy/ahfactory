using Ftel.Domain.Constants;
using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.FilterAttributes;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.ViewModels;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech)]
    public class EmailController : BaseController
    {
        #region Properties
        [Dependency]
        public IUnitOfWorkManager _UnitOfWorkManager {get;set;}
        //[Dependency]
        //public IMailTemplateService _MailTemplateService { get; set; }
        [Dependency]
        public IMailService _MailService { get; set; }
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
                    Name = LangHelper.Translate("email.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult Update(Guid id)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                var template = _MailTemplateService.Get(id);
                if (template == null)
                    HttpNotFound();

                return View(ToUpdateModel(template));
            }
        }
        #endregion

        #region GRID Grid - AJAX
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                var data = _MailTemplateService.All();
                var models = ToGridModel(data);
                return Json(models.ToDataSourceResult(request, ModelState));
            }
        }
        [HttpPost]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, EmailTemplateUpdateModel model)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var template = _MailTemplateService.Create(model.Name);

                        if(template != null)
                        {
                            UpdateEntity(ref template, model);

                            uow.Commit();
                        }                                               

                        return RedirectToAction("Index");
                    }
                }
                catch(Exception ex)
                {
                    OnError(ex);
                    uow.Rollback();
                }
              
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, EmailTemplateUpdateModel model)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var template = _MailTemplateService.Get(model.EmailTemplateId);

                        if (template == null)
                            HttpNotFound();

                        UpdateEntity(ref template, model);

                        uow.Commit();

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                    uow.Rollback();
                }
            }

            return View(model);
        }
        #endregion

        #region Mappings
        private EmailTemplateUpdateModel ToUpdateModel(MailTemplate entity)
        {
            return new EmailTemplateUpdateModel
            {
                EmailTemplateId = entity.Id,
                Name = entity.Name,
                From = entity.From,
                To = entity.To,
                Bcc = entity.Bcc,
                Cc = entity.Cc,
                Subject = entity.Subject,
                Body = entity.Body 
            };
        }
        private EmailTemplateGridModel ToGridModel(MailTemplate entity)
        {
            return ToGridModel(new[] { entity }).First();
        }
        private IEnumerable<EmailTemplateGridModel> ToGridModel(IEnumerable<MailTemplate> entities)
        {
            return entities.Select(x => new EmailTemplateGridModel
            {
                EmailTemplateId = x.Id,
                Name = x.Name, 
                To = x.To,
                Subject = x.Subject                  
            });
        }
        private void UpdateEntity(ref MailTemplate entity, EmailTemplateUpdateModel model)
        {
            entity.From = model.From;
            entity.To = model.To;
            entity.Bcc = model.Bcc;
            entity.Cc = model.Cc;
            entity.Subject = model.Subject;
            entity.Body = model.Body;            
        }
        #endregion
    }
}