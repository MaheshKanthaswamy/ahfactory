using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.Domain.DomainModel;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.WebSite.Helpers;
using Ftel.WebSite.ViewModels;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    public class EmailLogsController : Controller
    {
        [Dependency]
        public IEventLogService _EventLogService { get; set; }

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
                    Name = LangHelper.Translate("email.logs")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }

        public ActionResult ReadLogs([DataSourceRequest]DataSourceRequest request, EventLogType Type)
        {
            var societylogs = _EventLogService.GetByType(Type);
            var model = societylogs.ToList().Select(d => new EmailLogModel(d));

            return Json(model.OrderByDescending(m => m.Date).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

		public ActionResult PopupEmail(Guid LogId)
		{
			return View(model: LogId);
		}

		public ActionResult SeeEmail(Guid LogId)
		{
			var log = _EventLogService.Get(LogId);
			var body = log.AdditionalData4;

			return View(model:body);
		}
    }
}