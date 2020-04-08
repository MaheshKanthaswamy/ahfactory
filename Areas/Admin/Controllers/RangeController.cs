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
using Microsoft.Practices.Unity;
using Ftel.WebSite.ViewModels;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class RangeController : GridController<Range, RangeGridModel>
    {
        [Dependency]
        public IRangeService _RangeService { get; set; }
        public RangeController(IRangeService RangeService) : base(RangeService)
        {
            _RangeService = RangeService;
        }


        public override IEnumerable<RangeGridModel> ToModel(IEnumerable<Range> entities)
        {
            return entities.Select(x => new RangeGridModel
            {
                Id = x.Id,
                Name = x.Name
            });
        }

        public override Range ToEntity(RangeGridModel from, Range to)
        {
            to.Name = from.Name;
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
                    Name = LangHelper.Translate("range.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }
    }
}