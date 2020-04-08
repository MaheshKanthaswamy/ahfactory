using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.Domain.Constants;
using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.Utilities.Infrastructure;
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
using System.Web;
using System.Web.Mvc;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class SocietyController : GridController<Society, SocietyGridModel>
    {
        [Dependency]
        public IEventLogService _EventLogService { get; set; }

        #region Properties
        private readonly IUserService _UserService;
        private readonly ISocietyService _SocietyService;
        #endregion

        #region Construct
        public SocietyController(IUserService UserService, ISocietyService SocietyService) : base(SocietyService)
        {
            _UserService = UserService;
            _SocietyService = SocietyService;
        }
        #endregion

        public override IEnumerable<SocietyGridModel> ToModel(IEnumerable<Society> entities)
        {
            return entities.Select(x => new SocietyGridModel
            {
                Id = x.Id,
                DateCreation = x.CreationDate,
                LastActivityDate = x.LastActivityDate,
                SocietyName = x.Name,
                ClientCount = x.Users.Count
            });
        }

        public override Society ToEntity(SocietyGridModel from, Society to)
        {
            to.Name = from.SocietyName;
            to.CreationDate = from.DateCreation;
            to.LastActivityDate = from.LastActivityDate;
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
                    Name = LangHelper.Translate("society.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }

        public ActionResult AddUserToSociety(string login, Guid societyId)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var user = _UserService.AddUserIfNotExist(login);
                    uow.SaveChanges();

                    var society = _SocietyService.Get(societyId);
                    if (user != null && society != null)
                    {
                        if (society.Users.Where(c => c.MembershipUser.Login.ToLower().Equals(user.Login.ToLower())).SingleOrDefault() == null)
                        {
                            _SocietyService.AddUserInSociety(user.Login, society.Name);
                            uow.Commit();
                            return Json(true);
                        }
                        else
                        {
                            return Json("User '" + user.Login + "' already exist in the '" + society.Name + "' society.");
                        }
                    }
                    return Json(false);
                }
                catch (Exception ex)
                {
                    uow.Rollback();

                    return Json(ex.Message);
                }
            }
        }

        public ActionResult DeleteUserInSociety([DataSourceRequest] DataSourceRequest request, UserInRoleGridModel model, Guid? SocietyId)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if (SocietyId.HasValue)
                    {
                        var society = _SocietyService.Get(SocietyId.Value);
                        _SocietyService.RemoveUserFromSociety(model.Login, society.Name);
                        uow.Commit();
                    }
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
            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult SocietyInfos(Guid Id)
        {
            var society = _SocietyService.Get(Id);

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
                    Enabled = true,
                    Link = Url.Action("Index", "Society", new { area = "Admin" }), //A rectifier
                    Name = LangHelper.Translate("society.management")
                },
                new BreadcrumbModel
                {
                    Enabled = false,
                    Name = society.Name,
                    Link = "#"
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            var model = new SocietyGridModel
            {
                Id = society.Id,
                SocietyName = society.Name,
                DateCreation = society.CreationDate,
                ClientCount = society.Users.Count,
                LastActivityDate = society.LastActivityDate
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult SocietyInfos(SocietyGridModel model)
        {
            var society = _SocietyService.Get(model.Id);

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
                    Enabled = true,
                    Link = Url.Action("Index", "Society", new { area = "Admin" }), //A rectifier
                    Name = LangHelper.Translate("society.management")
                },
                new BreadcrumbModel
                {
                    Enabled = false,
                    Name = society.Name,
                    Link = "#"
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            if (ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {

                    society.Name = model.SocietyName;

                    uow.Commit();

                    return Message("society.updated", urlButton: Url.Action("SocietyInfos", "Society", new { area = "Admin", Id = society.Id }));
                }
            }

            return View(model);
        }

        public ActionResult Logs(Guid id)
        {
            return View(id);
        }

        public ActionResult ReadLogs([DataSourceRequest]DataSourceRequest request, Guid id)
        {
            var societylogs = _EventLogService.GetLogsBySocietyId(id);
            var model = societylogs.ToList().Select(d => new SocietyLogModel(d));

            return Json(model.OrderByDescending(m => m.Date).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Users(Guid id)
        {
            return View(id);
        }

        public ActionResult ReadUsers([DataSourceRequest]DataSourceRequest request, Guid id)
        {
            var users = _UserService.GetBySocietyId(id);
            var model = users.ToList().Select(d => new SocietyUserModel(d));

            return Json(model.OrderBy(u => u.Firstname).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RefuseAllUsers(Guid id, string ReturnURL = "")
        {
            var society = _SocietyService.Get(id);
            var users = society.Users;

            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                foreach (var u in users)
                {
                    var user = _UserService.Get(u.MembershipUserId);
                    user.StatutUser = MembershipUser.Statut.REFUSED;
                }
                uow.Commit();
            }

            var urlButton = string.IsNullOrWhiteSpace(ReturnURL) ? Url.Action("SocietyInfos", "Society", new { area = "Admin", Id = id }) : ReturnURL;
            return Message("users.refused", urlButton: urlButton);
        }

        public ActionResult AcceptAllUsers(Guid id, string ReturnURL = "")
        {
            var society = _SocietyService.Get(id);
            var users = society.Users;

            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                foreach (var u in users)
                {
                    var user = _UserService.Get(u.MembershipUserId);
                    user.StatutUser = MembershipUser.Statut.ALLOWED;
                }
                uow.Commit();
            }

            var urlButton = string.IsNullOrWhiteSpace(ReturnURL) ? Url.Action("SocietyInfos", "Society", new { area = "Admin", Id = id }) : ReturnURL;
            return Message("users.accepted", urlButton: urlButton);
        }
    }
}