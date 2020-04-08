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
using Ftel.WebSite.Models;
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
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class RoleController : BaseController
    {
        #region Properties
        public readonly IRoleService _RoleService;
        public readonly IUserService _UserService;
        public readonly IUnitOfWorkManager _UnitOfWorkManager;

        [Dependency]
        public IEventLogService _EventLogService { get; set; }
        #endregion

        #region Construct
        public RoleController(IUnitOfWorkManager unitOfWorkManager,
            IRoleService GroupService, IUserService UserService)
        {
            _UnitOfWorkManager = unitOfWorkManager;
            _RoleService = GroupService;
            _UserService = UserService;
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
                    Name = LangHelper.Translate("role.management")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }

        public ActionResult Logs()
        {
            return View();
        }

        #endregion

        #region Ajax
        [HttpPost]
        public ActionResult AddUserToRole(string login, Guid roleId)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var user = _UserService.AddUserIfNotExist(login);
                    uow.SaveChanges();

                    var role = _RoleService.Get(roleId);
                    if (user != null && role != null)
                    {
                        if (role.Users.Where(c =>c.Login.ToLower().Equals(user.Login.ToLower())).SingleOrDefault()==null)
                        //if (!_RoleService.IsInRole(user.Login, role.Name))
                        {
                            _RoleService.AddUserInRole(user.Login, role.Name);
                            uow.SaveChanges();

                            var currentUser = _UserService.Get(GlobalVariables.UserId);
                            _EventLogService.AddLine(EventLogType.ROLE_CHANGE_ADD, currentUser.Id.ToString(), currentUser.Login, user.Id.ToString(), user.Login, role.Id.ToString());

                            uow.Commit();
                            return Json(true);
                        }
                        else
                        {
                            return Json("User '" + user.Login + "' already exist in the '" + role.Name + "' role.");
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
        #endregion

        #region Ajax - Post - Grid CRUD
        #region Role Grid
        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(ToModel(Read()).ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateOrUpdate([DataSourceRequest] DataSourceRequest request, RoleGridModel model)
        {
            return Json(new[] { CreateOrUpdate(model) }.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, RoleGridModel model)
        {
            return Json(new[] { Delete(model) }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region UserInRole Grid
        public ActionResult ReadUsersInRole([DataSourceRequest] DataSourceRequest request, Guid? RoleId)
        {
            var data = RoleId.HasValue ? _RoleService.Get(RoleId.Value).Users.Where(u => u.IsInternalBayer) : new List<MembershipUser>();           
            return Json(ToModel(data).ToDataSourceResult(request, ModelState));
        }

        public ActionResult DeleteUserInRole([DataSourceRequest] DataSourceRequest request, UserInRoleGridModel model, Guid? RoleId)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if (RoleId.HasValue)
                    {
                        var role = _RoleService.Get(RoleId.Value);
                        _RoleService.RemoveUserFromRole(model.Login, role.Name);

                        var currentUser = _UserService.Get(GlobalVariables.UserId);
                        _EventLogService.AddLine(EventLogType.ROLE_CHANGE_REMOVE, currentUser.Id.ToString(), currentUser.Login, model.Id.ToString(), model.Login, RoleId.Value.ToString());

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

        public ActionResult ReadLogs([DataSourceRequest]DataSourceRequest request)
        {
            var rolelogs = _EventLogService.GetRoleLogs();
            var model = rolelogs.ToList().Select(d => new RoleLogModel(d));

            return Json(model.OrderByDescending(m => m.Date).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region AutoComplete
        public ActionResult ReadUsers([DataSourceRequest] DataSourceRequest request, string Text)
        {
            var data = _UserService.SearchByLastname(Text)
                .Select(y => new { UserName = y.Login, FullName = y.Fullname })
                .ToArray();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadUsersAndGroups([DataSourceRequest] DataSourceRequest request, string Text)
        {
            var data = _UserService.SearchByUsersAndGroups(Text)
                .Select(y => new { UserName = y.Login, FullName = y.Fullname })
                .ToArray();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        #region Private Methods
        public IEnumerable<RoleGridModel> ToModel(IEnumerable<Role> entities)
        {
            return entities.Select(x => new RoleGridModel
            {
                Role_ID = x.Id,
                Name = x.Name,
                Description = x.Description
            });
        }
        public IEnumerable<UserInRoleGridModel> ToModel(IEnumerable<MembershipUser> entities)
        {
            return entities.Select(x => new UserInRoleGridModel
            {
                Login = x.Login,
                FullName = x.Profile.Fullname,
                Id = x.Id
            });
        }
        public Role ToEntity(RoleGridModel from, Role to)
        {
            to.Id = from.Role_ID;
            to.Name = from.Name;
            to.Description = from.Description;
            return to;
        }
        public RoleGridModel ToModel(Role entity)
        {
            return ToModel(new[] { entity }).First();
        }
        private IEnumerable<Role> Read()
        {
            return _RoleService.GetAll();
        }
        public RoleGridModel CreateOrUpdate(RoleGridModel model)
        {
            ModelState.Remove("Role_ID");
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = model.Role_ID == Guid.Empty ?
                            _RoleService.Create(new Role { Id = model.Role_ID = GuidComb.GenerateComb() }) :
                            _RoleService.Get(model.Role_ID);
                          
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
        public RoleGridModel Delete(RoleGridModel model)
        {
            ModelState.Remove("Role_ID");
            //model = BeforeDelete(model);
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _RoleService.Get(model.Role_ID);
                        _RoleService.Delete(item);
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
        public RoleGridModel BeforeDelete(RoleGridModel model)
        {
            var role = _RoleService.Get(model.Role_ID);
            if (role.Users != null && role.Users.Any())
            {
                ModelState.AddModelError("", LangHelper.Translate("cant.delete.site.model.is.attached"));
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