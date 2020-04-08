using Ftel.Domain.DomainModel;
using Ftel.Domain.Interfaces.Services;
using Ftel.Domain.Interfaces.UnitOfWork;
using Ftel.Utilities.Infrastructure;
using Ftel.WebSite.Areas.Admin.Models;
using Ftel.WebSite.Controllers;
using Ftel.WebSite.FilterAttributes;
using Ftel.WebSite.Helpers;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Ftel.Domain.Constants;
using Ftel.WebSite.ViewModels;
using AHDigitalFactory.Domain.Interfaces.Services;
using Ftel.WebSite.Areas.Mail;
using Ftel.WebSite.Attributes;
using Ftel.WebSite.Models;
using RangeModel = Ftel.WebSite.ViewModels.RangeModel;
using Microsoft.Practices.Unity;
using ImageProcessor.Imaging;

namespace Ftel.WebSite.Areas.Admin.Controllers
{
    [RequireRoleFilter(AppConstants.Roles.AdminTech, AppConstants.Roles.AdminData)]
    public class UserController : BaseController
    {
        [Dependency]
        public IEventLogService _EventLogService { get; set; }

        #region Properties
        private readonly IUserService _UserService;
        private readonly IDocumentService _DocumentService;
        private readonly IDocumentExceptionService _DocumentExceptionService;
        private readonly IVersionnableDocumentService _VersionnableDocumentService;
        private readonly IRoleService _RoleService;
        private readonly IRangeService _RangeService;
        private readonly ISocietyService _SocietyService;
        private readonly IUnitOfWorkManager _UnitOfWorkManager;
        private readonly IMailService _MailService;
        #endregion

        #region Construct
        public UserController(IUnitOfWorkManager unitOfWorkManager,
            IUserService UserService, IRoleService RoleService, IRangeService RangeService, ISocietyService SocietyService, IDocumentExceptionService DocumentExceptionService, IVersionnableDocumentService VersionnableDocumentService, IMailService MailService, IDocumentService DocumentService)
        {
            _MailService = MailService;
            _UnitOfWorkManager = unitOfWorkManager;
            _UserService = UserService;
            _RoleService = RoleService;
            _RangeService = RangeService;
            _SocietyService = SocietyService;
            _DocumentService = DocumentService;
            _DocumentExceptionService = DocumentExceptionService;
            _VersionnableDocumentService = VersionnableDocumentService;
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
                    Name = LangHelper.Translate("title.administration.clients")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            ViewBag.Menu = "Droit";
            return View();
        }

        public ActionResult Bayer()
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
                    Name = LangHelper.Translate("title.administration.users.and.roles")
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            return View();
        }

        #endregion

        #region Ajax - Post - Grid CRUD

        public ActionResult Read([DataSourceRequest] DataSourceRequest request, bool isBayer)
        {
            var users = _UserService.GetAll().Where(c => c.IsInternalBayer == isBayer);
            return Json(ToModel(users).ToDataSourceResult(request, ModelState));
        }

        public ActionResult ReadDocumentException([DataSourceRequest] DataSourceRequest request, Guid userId)
        {
            var searchModel = new SearchDocumentModel();
            var data = _VersionnableDocumentService.SearchDocuments(searchModel.DocumentType, searchModel.RangeId, searchModel.Text, GlobalVariables.CanViewArchivedFile, GlobalVariables.UserId, GlobalVariables.IsAdmin)
                .Where(o => o.Status.HasValue && (int)o.Status.Value != 999)
                .OrderBy(o => o.Name);
            
            var exceptions = _DocumentExceptionService.GetAllByUser(userId).ToDictionary(o => o.UserId + "_" + o.Id, o => o);

            var docs = data.Select(o => new DocumentException
            {
                Document = o,
                UserId = exceptions.ContainsKey(userId + "_" + o.Id) ? userId : Guid.Empty,
            });
            return Json(ToModel(docs).ToDataSourceResult(request, ModelState));
        }

        public ActionResult DocumentException(Guid docId, Guid userId, int index)
        {
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if(index == 1)
                    {
                        var user = _UserService.Get(userId);
                        var item = new DocumentException
                        {
                            Id = docId,
                            UserId = userId,
                        };
                        _DocumentExceptionService.Create(item);
                    }else
                    {
                        var item = _DocumentExceptionService.Get(docId, userId);
                        if(item != null)
                            _DocumentExceptionService.Delete(item);
                    }
                    
                    uow.Commit();

                    return Json(true);
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
                return Json(false);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateOrUpdate([DataSourceRequest] DataSourceRequest request, UserGridModel model)
        {
            return Json(new[] { CreateOrUpdate(model) }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, UserGridModel model)
        {
            return Json(new[] { Delete(model) }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, UserGridModel model)
        {
            return Json(new[] { Update(model) }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Private Methods
        public MembershipUser ToEntity(UserGridModel from, MembershipUser to)
        {
            to.Login = from.Login;

            if (!string.IsNullOrWhiteSpace(from.Password))
            {
                to.Password = from.Password;
                _UserService.UpdatePassword(to, to.Password);
            }

            // Suppression des roles qui ne sont plus dedans
            var willberemoved = to.Roles.Where(i => !from.Roles.Select(t => t.Id).Contains(i.Id)).ToList();
            foreach (var mod in willberemoved)
            {
                to.Roles.Remove(mod);
            }

            // Ajout des nouveaux roles
            foreach (var role in from.Roles)
            {
                if (to.Roles.All(i => i.Id != role.Id))
                {
                    to.Roles.Add(_RoleService.Get(role.Id));
                }
            }

            if (to.Profile == null)
                to.Profile = new Profile();

            to.Profile.Firstname = from.Firstname;
            to.Profile.Lastname = from.Lastname;

            return to;
        }

        public UserGridModel ToModel(MembershipUser entity)
        {
            return ToModel(new[] { entity }).First();
        }

        public IEnumerable<UserGridModel> ToModel(IEnumerable<MembershipUser> entities)
        {
            return entities.Select(x => new UserGridModel
            {
                Id = x.Id,
                Login = x.Login,
                Firstname = x.Profile != null ? x.Profile.Firstname : "",
                Lastname = x.Profile != null ? x.Profile.Lastname : "",
                SocietyId = x.Profile.Society.FirstOrDefault() == null ? (Guid?)null : x.Profile.Society.First().Id,
                SocietyName = x.Profile.Society.FirstOrDefault() == null ? "" : x.Profile.Society.First().Name,
                LastLoginDate = x.LastLoginDate,
                Statut = x.StatutUser,
                Roles = x.Roles == null
                    ? null
                    : x.Roles.Select(g => new Role
                    {
                        Name = g.Name,
                        Id = g.Id
                    }),
                Filtering = x.Filtering == null ? null : DoSomething(x.Filtering)
            });
        }

        public IEnumerable<DocumentExceptionGridModel> ToModel(IEnumerable<DocumentException> entities)
        {
            return entities.Select(x => new DocumentExceptionGridModel
            {
                RangeIds = string.Join(", ",x.Document.Ranges.Select(j => j.Id)),
                Id = x.Document.Id,
                Author = x.Document.Author,
                ContentType = x.Document.ContentType,
                DateCreated = x.Document.DateCreated,
                Name = x.Document.VersionName,
                Size = x.Document.Size,
                IsException = x.UserId != Guid.Empty,
                TypeDocument = (int?)x.Document.Type ?? 999,
                Type = x.Document.Type,
                SearchThumbnailUrl = DocumentHelper.IsImage(x.Document.ContentType)
                   ? Helpers.UrlHelper.GetFromContext().Action("GetFile", "Api", new { area = "", id = x.Document.Id, animationprocessmode = AnimationProcessMode.First })
                   : x.Document.ThumbnailId.HasValue
                       ? Helpers.UrlHelper.GetFromContext().Action("GetFile", "Api", new { area = "", id = x.Document.ThumbnailId, animationprocessmode = AnimationProcessMode.First })
                       : Helpers.UrlHelper.GetFromContext().Content(ParameterHelper.DefaultThumbnailPdfUrl + "?animationprocessmode=First"),
                Ranges = x.Document.Ranges == null ? "" : string.Join(", ", x.Document.Ranges.Select(r => r.Name))
            });
        }

        public IEnumerable<ModelFiltering> DoSomething(String Filtering)
        {
            IList<ModelFiltering> Filter = new List<ModelFiltering>();
            foreach( String item in Filtering.Split(';'))
            {
                DocumentType type;
                Enum.TryParse(item, out type);
                switch (type)
                {
                    case DocumentType.ADVERT_TOOLS:
                        Filter.Add(new ModelFiltering { Name = LangHelper.Translate("advert.tools"), Value = item });
                        break;
                    case DocumentType.GUIDELINES:
                        Filter.Add(new ModelFiltering { Name = LangHelper.Translate("guidelines"), Value = item });
                        break;
                    case DocumentType.IMAGES:
                        Filter.Add(new ModelFiltering { Name = LangHelper.Translate("images"), Value = item });
                        break;
                    case DocumentType.OBLIGATORY_TEXT:
                        Filter.Add(new ModelFiltering { Name = LangHelper.Translate("obligatory.text"), Value = item });
                        break;
                    case DocumentType.VIDEOS:
                        Filter.Add(new ModelFiltering { Name = LangHelper.Translate("videos"), Value = item });
                        break;
                }
            }
            return Filter;
        }


        public UserGridModel CreateOrUpdate(UserGridModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _UserService.Get(model.Login) ?? _UserService.Create(new MembershipUser
                        {
                            Id = GuidComb.GenerateComb(),
                            Login = model.Login,
                            Password = model.Password,
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

        public UserGridModel Delete(UserGridModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _UserService.Get(model.Login);
                        _UserService.Delete(item);
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

        public UserGridModel Update(UserGridModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                using (var uow = _UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var item = _UserService.Get(model.Login);
                        item.Filtering = null;
                        if (model.Filtering != null)
                        {
                           item.Filtering = String.Join(";", model.Filtering.Select(x =>x.Value));
                        }
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
        protected virtual void OnDeleteError(Exception ex)
        {
            ModelState.AddModelError("", LangHelper.Translate("error.occured"));
        }

        protected virtual void OnUpdateError(Exception ex)
        {
            ModelState.AddModelError("", LangHelper.Translate("error.occured"));
        }
        #endregion

        public ActionResult Edit(Guid Id)
        {
            var user = _UserService.Get(Id);
            //if (user.StatutUser == MembershipUser.Statut.REFUSED)
            //    return RedirectToAction("Infos", "User", new {area = "Admin", id = Id});
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
                    Link = Url.Action("Index", "User", new { area = "Admin" }),
                    Name = LangHelper.Translate("user.management")
                },
                new BreadcrumbModel
                {
                    Enabled = true,
                    Name = user.Login,
                    Link = Url.Action("Infos", "User", new { area = "Admin", id = user.Id })
                },
                new BreadcrumbModel
                {
                    Enabled = false,
                    Name = LangHelper.Translate("edit"),
                    Link = "#"
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            var model = new UserProfileModel
            {
                Id = user.Profile.MembershipUserId,
                Firstname = user.Profile.Firstname,
                Lastname = user.Profile.Lastname,
                Title = user.Profile.Title,
                Civility = user.Profile.Sexe,
                Email = user.Profile.Email,
                Society = user.Profile.Society.FirstOrDefault()?.ToModelWithNameAndId(),
                RegisteredSociety = user.Profile.SocietyRegister,
                EndValidityDate = user.ValidityEndDate,
                Ranges = user.Ranges.Select(r => r.Id).ToArray(),
                RangesNames = user.Ranges.Select(r => r.Name).ToArray(),
                Filtering = user.Filtering == null ? null : user.Filtering.Split(';'),
                FilteringNames = user.Filtering == null ? new List<string>() : DoSomething(user.Filtering).Select(x => x.Name).ToList(),
                Statut = user.StatutUser,
                IsInternalBayer = user.IsInternalBayer,
                LastActivityDate = user.LastActivityDate
            };

            return View(model);
        }

        public ActionResult ReadSocieties()
        {
            var data = _SocietyService.GetAll().ToModelWithNameAndId().ToList().OrderBy(r => r.Name);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadRanges()
        {
            var data = _RangeService.GetAll().ToModelWithNameAndId().ToList().OrderBy(r => r.Name);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadRoles()
        {
            var data = _RoleService.GetAll().ToModelWithNameAndId().ToList().OrderBy(r => r.Name);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAjax]
        public ActionResult Edit(UserProfileModel model)
        {
            var user = _UserService.Get(model.Id);
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
                    Link = Url.Action("Index", "User", new { area = "Admin" }),
                    Name = LangHelper.Translate("user.management")
                },
                new BreadcrumbModel
                {
                    Enabled = false,
                    Name = user.Login,
                    Link = "#"
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion

            if (!ModelState.IsValid)
                return View(model);
            var sendmail = user.StatutUser == MembershipUser.Statut.INPROGRESS && model.Statut == MembershipUser.Statut.ALLOWED;
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                user.Profile.Lastname = model.Lastname;
                user.Profile.Firstname = model.Firstname;
                user.Profile.Email = model.Email;
                user.Login = model.Email;
                user.Profile.Sexe = model.Civility;
                user.Profile.Title = model.Title;
                user.StatutUser = model.Statut;
                user.ValidityEndDate = model.EndValidityDate;

                //Société
                if (user.Profile.Society == null)
                    user.Profile.Society = new List<Society>();
                user.Profile.Society.Clear();
                if (model.Society != null)
                {
                    var r = _SocietyService.Get(model.Society.Id);
                    user.Profile.Society.Add(r);
                }

                //Gammes
                if (user.Ranges == null)
                    user.Ranges = new List<Range>();
                user.Ranges.Clear();
                if (model.Ranges != null)
                {
                    foreach (var range in model.Ranges)
                    {
                        var r = _RangeService.Get(range);
                        user.Ranges.Add(r);
                    }
                }

                user.Filtering = null;
                if (model.Filtering != null)
                {
                    IList<string> filter = new List<string>();
                    foreach (var filtering in model.Filtering)
                    {
                        filter.Add(filtering);
                    }
                    user.Filtering = String.Join(";", filter);
                }

                if (sendmail)
                {
                    var subject = LangHelper.Translate("email.register.user.validated.title");
                    var body = WebPartHelper.WebPart("RegisterUserValidated", "Mail", MailAreaRegistration.AREA_NAME, false, user.Id);
                    var to = user.Profile.Email;
                    var from = AppConstants.DefaultFromAddress.Address;
                    var cc = "";
                    var bcc = "";

                    _MailService.SendMail(subject, body, new[] { to }, from);
                    
                    var admin = _UserService.Get(GlobalVariables.UserId);
                    _EventLogService.AddLine(EventLogType.EMAIL_VALIDATE_REGISTER, user.Id.ToString(), user.Login, subject, body, to, from, cc, bcc, admin.Id.ToString(), admin.Login);
                }
                if (model.Statut != MembershipUser.Statut.INPROGRESS)
                {
                    var visitor = _RoleService.Get("Visitor");
                    if(user.Roles.All(r => r.Id != visitor.Id))
                        user.Roles.Add(visitor);
                }
                uow.Commit();
            }
            return Message("user.edited", urlButton: Url.Action("Edit", "User", new { Id = user.Id }));
        }

        public ActionResult ForgotPassword(string email)
        {
            var user = _UserService.Get(email);
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                _UserService.GenerateForgotPasswordKey(email);
                uow.Commit();
            }

            var html = WebPartHelper.WebPart("ForgotPasswordEmail", "Mail", MailAreaRegistration.AREA_NAME, false, user.Id);
            _MailService.SendMail(LangHelper.Translate("email.forgot.password.headline"), html, new[] { user.Profile.Email }, "AHDigitalFactory@bayer.fr");

            return Message("user.password.reinit", urlButton: Url.Action("Edit", "User", new { Id = user.Id }));
        }

        public ActionResult Refuse(Guid id, string ReturnURL = "")
        {
            var user = _UserService.Get(id);
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                user.StatutUser = MembershipUser.Statut.REFUSED;
                uow.Commit();
            }

            var urlButton = string.IsNullOrWhiteSpace(ReturnURL) ? Url.Action("Index", "User", new { area = "Admin" }) : ReturnURL;

            return Message("user.refused", urlButton: urlButton);
        }

        public ActionResult Accept(Guid id, string ReturnURL = "")
        {
            var user = _UserService.Get(id);
            using (var uow = _UnitOfWorkManager.NewUnitOfWork())
            {
                user.StatutUser = MembershipUser.Statut.ALLOWED;
                uow.Commit();
            }

            var urlButton = string.IsNullOrWhiteSpace(ReturnURL) ? Url.Action("Index", "User", new { area = "Admin" }) : ReturnURL;

            return Message("user.accepted", urlButton: urlButton);
        }

        public ActionResult Infos(Guid id)
        {
            var user = _UserService.Get(id);

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
                    Link = Url.Action("Index", "User", new { area = "Admin" }),
                    Name = LangHelper.Translate("user.management")
                },
                new BreadcrumbModel
                {
                    Enabled = false,
                    Link = "#",
                    Name = user.Profile.Email
                }
            };

            ViewBag.Breadcrumb = breadcrumb;

            #endregion


            if (user.IsInternalBayer)
            {
                var model_readonly = new UserProfileModel_ReadOnly
                {
                    Id = user.Id,
                    Firstname = user.Profile.Firstname,
                    Lastname = user.Profile.Lastname,
                    Email = user.Profile.Email,
                    LastActivityDate = user.LastActivityDate,
                    Login = user.Login,
                    Roles = user.Roles.Select(r => new ModelWithNameAndId
                    {
                        Id = r.Id,
                        Name = r.Name
                    })
                };

                return View("_ReadOnlyInfos", model_readonly);
            }

            var model = new UserProfileModel
            {
                Id = user.Profile.MembershipUserId,
                Firstname = user.Profile.Firstname,
                Lastname = user.Profile.Lastname,
                Title = user.Profile.Title,
                Civility = user.Profile.Sexe,
                Email = user.Profile.Email,
                Society = user.Profile.Society.FirstOrDefault()?.ToModelWithNameAndId(),
                RegisteredSociety = user.Profile.SocietyRegister,
                EndValidityDate = user.ValidityEndDate,
                Ranges = user.Ranges.Select(r => r.Id).ToArray(),
                RangesNames = user.Ranges.Select(r => r.Name).ToArray(),
                Filtering = user.Filtering == null ? null : user.Filtering.Split(';'),
                FilteringNames = user.Filtering == null ? new List<string>() : DoSomething(user.Filtering).Select(x =>x.Name).ToList(),
                Statut = user.StatutUser,
                LastActivityDate = user.LastActivityDate,
                Newsletter = user.Profile.Newsletter
            };

            return View(model);
        }

        public ActionResult Logs(Guid Id)
        {
            return View(Id);
        }

        public ActionResult ReadLogs([DataSourceRequest]DataSourceRequest request, Guid id)
        {
            var userlogs = _EventLogService.GetLogsByUserId(id);
            var model = userlogs.ToList().Select(d => new UserLogModel(d));

            return Json(model.OrderByDescending(m => m.Date).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}