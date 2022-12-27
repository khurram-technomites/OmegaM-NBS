using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Areas.Admin.ViewModels;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class UserRoleController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IUserRoleService _userroleService;
		private readonly IRouteService _routeService;
		private readonly IUserRolePrivilegeService _userRolePrivilegeService;
		private readonly ISPService _spService;

		public UserRoleController(IUserRoleService userroleService, IRouteService routeService, ISPService spService, IUserRolePrivilegeService userRolePrivilegeService)
		{
			this._userroleService = userroleService;
			this._routeService = routeService;
			this._userRolePrivilegeService = userRolePrivilegeService;
			this._spService = spService;
		}

		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult List()
		{
			var userroles = _userroleService.GetUserRoles();
			return PartialView(userroles);
		}

		public ActionResult ListReport()
		{
			var userroles = _userroleService.GetUserRoles();
			return View(userroles);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			UserRole userrole = _userroleService.GetUserRole((Int16)id);
			if (userrole == null)
			{
				return HttpNotFound();
			}
			return View(userrole);
		}

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(UserRole userrole)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_userroleService.CreateUserRole(userrole, ref message))
				{
					TextWriter textWriter = new StreamWriter(Server.MapPath("/AuthorizationProvider/Privileges/Admin/" + userrole.RoleName + ".txt"));
					textWriter.Close();

					return Json(new
					{
						success = true,
						url = "/Admin/UserRole/Index",
						message = message,
						data = new
						{
							ID = userrole.ID,
							Date = userrole.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							RoleName = userrole.RoleName,
							IsActive = userrole.IsActive.HasValue ? userrole.IsActive.Value.ToString() : bool.FalseString
						}
					});
				}
			}
			else
			{
				message = "Please fill the form properly ...";
			}

			return Json(new { success = false, message = message });
		}

		public ActionResult Edit(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			UserRole userrole = _userroleService.GetUserRole((long)id);
			if (userrole == null)
			{
				return HttpNotFound();
			}

			TempData["UserRoleID"] = id;
			return View(userrole);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(UserRole userrole)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["UserRoleID"] != null && Int64.TryParse(TempData["UserRoleID"].ToString(), out Id) && userrole.ID == Id)
				{
					string PrevUserRole = string.Empty;
					if (_userroleService.UpdateUserRole(ref userrole, ref PrevUserRole, ref message))
					{
						System.IO.File.Move(Server.MapPath("/AuthorizationProvider/Privileges/Admin/" + PrevUserRole + ".txt"), Server.MapPath("/AuthorizationProvider/Privileges/Admin/" + userrole.RoleName + ".txt"));
						return Json(new
						{
							success = true,
							url = "/Admin/UserRole/Index",
							message = "UserRole updated successfully ...",
							data = new
							{
								ID = userrole.ID,
								Date = userrole.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
								RoleName = userrole.RoleName,
								IsActive = userrole.IsActive.HasValue ? userrole.IsActive.Value.ToString() : bool.FalseString
							}
						});
					}
				}
				else
				{
					message = "Oops! Something went wrong. Please try later.";
				}
			}
			else
			{
				message = "Please fill the form properly ...";
			}
			return Json(new { success = false, message = message });
		}

		public ActionResult Activate(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var userrole = _userroleService.GetUserRole((long)id);
			if (userrole == null)
			{
				return HttpNotFound();
			}

			if (!(bool)userrole.IsActive)
				userrole.IsActive = true;
			else
			{
				userrole.IsActive = false;
			}
			string message = string.Empty;
			if (_userroleService.UpdateUserRole(ref userrole, ref message))
			{
				SuccessMessage = "UserRole " + ((bool)userrole.IsActive ? "activated" : "deactivated") + "  successfully ...";
				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						ID = userrole.ID,
						Date = userrole.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						RoleName = userrole.RoleName,
						IsActive = userrole.IsActive.HasValue ? userrole.IsActive.Value.ToString() : bool.FalseString

					}
				}, JsonRequestBehavior.AllowGet);
			}
			else
			{
				ErrorMessage = "Oops! Something went wrong. Please try later.";
			}

			return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Delete(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			UserRole userrole = _userroleService.GetUserRole((Int16)id);
			if (userrole == null)
			{
				return HttpNotFound();
			}
			TempData["UserRoleID"] = id;
			return View(userrole);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			string filePath = string.Empty;
			bool softDelete = true;
			if (_userroleService.DeleteUserRole((Int16)id, ref message, ref filePath, softDelete))
			{
				if (!softDelete)
					System.IO.File.Delete(Server.MapPath(filePath));
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Privileges(long id)
		{
			ViewBag.UserRoleId = id;
			ViewBag.RoleName = _userroleService.GetUserRole(id).RoleName;
			var userroles = _spService.GetRoutesWithUserRolePrivileges("Admin", id);
			return View(userroles);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Update(UserRolePrivilegeFormViewModel userRolePrivileges)
		{
			try
			{
				string UserRole = _userroleService.GetUserRole(userRolePrivileges.UserRoleId).RoleName;
				string message = string.Empty;
				if (_userRolePrivilegeService.DeleteUserRolePrivileges(userRolePrivileges.UserRoleId, ref message))
				{
					var filePath = Server.MapPath("/AuthorizationProvider/Privileges/Admin/" + UserRole + ".txt");

					using (StreamWriter writer = new StreamWriter(filePath, false))
					{
						if (userRolePrivileges.Routes != null && userRolePrivileges.Routes.Count > 0)
						{
							foreach (var Route in userRolePrivileges.Routes)
							{
								UserRolePrivilege UserRolePrivilege = new UserRolePrivilege()
								{
									UserRoleID = userRolePrivileges.UserRoleId,
									RouteID = Route.Id
								};
								if (_userRolePrivilegeService.CreateUserRolePrivilege(UserRolePrivilege, ref message))
								{
									writer.WriteLine(Route.Url);
								}
							}
						}
						else
						{
							writer.WriteLine(string.Empty);
						}
					}

					message = "UserRole privileges updated successfully !";
					return Json(new { success = true, message = message });
				}
				else
				{
					message = "Oops! Something went wrong. Please try later.";
				}
				return Json(new { success = false, message = message });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Oops! Something went wrong. Please try later." });
			}
		}
	}
}