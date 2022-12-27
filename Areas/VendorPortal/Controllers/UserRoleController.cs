using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Areas.Admin.ViewModels;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{
	[AuthorizeVendor]
	public class UserRoleController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IVendorUserRoleService _vendorUserRoleService;
		private readonly IRouteService _routeService;
		private readonly IVendorUserRolePrivilegeService _vendorUserRolePrivilegeService;

		public UserRoleController(IVendorUserRoleService vendorUserRoleService, IRouteService routeService, ISPService spService, IVendorUserRolePrivilegeService vendorUserRolePrivilegeService)
		{
			this._vendorUserRoleService = vendorUserRoleService;
			this._routeService = routeService;
			this._vendorUserRolePrivilegeService = vendorUserRolePrivilegeService;
		}

		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult List()
		{

			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var vendorUserRoles = _vendorUserRoleService.GetVendorUserRolesByVendor(VendorID);
			return PartialView(vendorUserRoles);
		}

		public ActionResult ListReport()
		{
			var vendorUserRoles = _vendorUserRoleService.GetVendorUserRoles();
			return View(vendorUserRoles);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			VendorUserRole vendorUserRole = _vendorUserRoleService.GetVendorUserRole((Int16)id);
			if (vendorUserRole == null)
			{
				return HttpNotFound();
			}
			return View(vendorUserRole);
		}

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(VendorUserRole vendorUserRole)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				var VendorID = Convert.ToInt64(Session["VendorID"]);
				vendorUserRole.VendorID = VendorID;
				if (_vendorUserRoleService.CreateVendorUserRole(vendorUserRole, ref message))
				{
					string directory = Server.MapPath(string.Format("/AuthorizationProvider/Privileges/Vendor/{0}/", VendorID));
					Directory.CreateDirectory(directory);
					TextWriter textWriter = new StreamWriter(directory + vendorUserRole.Name + ".txt");
					textWriter.Close();

					return Json(new
					{
						success = true,
						url = "/Vendor/UserRole/Index",
						message = message,
						data = new
						{
							ID = vendorUserRole.ID,
							Date = vendorUserRole.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							RoleName = vendorUserRole.Name,
							IsActive = vendorUserRole.IsActive.HasValue ? vendorUserRole.IsActive.Value.ToString() : bool.FalseString
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
			VendorUserRole vendorUserRole = _vendorUserRoleService.GetVendorUserRole((long)id);
			if (vendorUserRole == null)
			{
				return HttpNotFound();
			}

			TempData["VendorUserRoleID"] = id;
			return View(vendorUserRole);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(VendorUserRole vendorUserRole)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["VendorUserRoleID"] != null && Int64.TryParse(TempData["VendorUserRoleID"].ToString(), out Id) && vendorUserRole.ID == Id)
				{
					string PrevVendorUserRole = string.Empty;
					if (_vendorUserRoleService.UpdateVendorUserRole(ref vendorUserRole, ref PrevVendorUserRole, ref message))
					{
						try
						{
							var VendorID = Convert.ToInt64(Session["VendorID"]);
							System.IO.File.Move(Server.MapPath("/AuthorizationProvider/Privileges/Vendor/" + VendorID + "/" + PrevVendorUserRole + ".txt"), Server.MapPath("/AuthorizationProvider/Privileges/Vendor/" + VendorID + "/" + vendorUserRole.Name + ".txt"));
							return Json(new
							{
								success = true,
								url = "/Vendor/UserRole/Index",
								message = "User Role updated successfully ...",
								data = new
								{
									ID = vendorUserRole.ID,
									Date = vendorUserRole.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
									RoleName = vendorUserRole.Name,
									IsActive = vendorUserRole.IsActive.HasValue ? vendorUserRole.IsActive.Value.ToString() : bool.FalseString
								}
							});
						}
						catch (Exception)
						{
							return Json(new
							{
								success = true,
								url = "/Vendor/UserRole/Index",
								message = "User Role updated successfully ...",
								data = new
								{
									ID = vendorUserRole.ID,
									Date = vendorUserRole.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
									RoleName = vendorUserRole.Name,
									IsActive = vendorUserRole.IsActive.HasValue ? vendorUserRole.IsActive.Value.ToString() : bool.FalseString
								}
							});
						}

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
			var vendorUserRole = _vendorUserRoleService.GetVendorUserRole((long)id);
			if (vendorUserRole == null)
			{
				return HttpNotFound();
			}

			if (!(bool)vendorUserRole.IsActive)
				vendorUserRole.IsActive = true;
			else
			{
				vendorUserRole.IsActive = false;
			}
			string message = string.Empty;
			if (_vendorUserRoleService.UpdateVendorUserRole(ref vendorUserRole, ref message))
			{
				SuccessMessage = "User Role " + ((bool)vendorUserRole.IsActive ? "activated" : "deactivated") + "  successfully ...";
				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						ID = vendorUserRole.ID,
						Date = vendorUserRole.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						RoleName = vendorUserRole.Name,
						IsActive = vendorUserRole.IsActive.HasValue ? vendorUserRole.IsActive.Value.ToString() : bool.FalseString

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
			VendorUserRole vendorUserRole = _vendorUserRoleService.GetVendorUserRole((Int16)id);
			if (vendorUserRole == null)
			{
				return HttpNotFound();
			}
			TempData["VendorUserRoleID"] = id;
			return View(vendorUserRole);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			string filePath = string.Empty;
			bool softDelete = true;
			if (_vendorUserRoleService.DeleteVendorUserRole((Int16)id, ref message, ref filePath, softDelete))
			{
				if (!softDelete)
				{
					var VendorID = Convert.ToInt64(Session["VendorID"]);
					System.IO.File.Delete(Server.MapPath(filePath.Replace("{VendorID}", VendorID.ToString())));
				}
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Privileges(long id)
		{
			ViewBag.UserRoleId = id;
			ViewBag.RoleName = _vendorUserRoleService.GetVendorUserRole(id).Name;
			var userroles = _vendorUserRolePrivilegeService.GetRoutesWithVendorUserRolePrivileges("Vendor", id);
			return View(userroles);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Update(UserRolePrivilegeFormViewModel userRolePrivileges)
		{
			try
			{
				string message = string.Empty;
				var vendorUserRole = _vendorUserRoleService.GetVendorUserRole(userRolePrivileges.UserRoleId);
				if (vendorUserRole != null)
				{
					if (!vendorUserRole.Name.Equals("Administrator"))
					{
						string UserRole = vendorUserRole.Name;
						if (_vendorUserRolePrivilegeService.DeleteVendorUserRolePrivileges(userRolePrivileges.UserRoleId, ref message))
						{
							var vendorId = Convert.ToInt64(Session["VendorID"]);
							var filePath = Server.MapPath("/AuthorizationProvider/Privileges/Vendor/" + vendorId + "/" + UserRole + ".txt");

							using (StreamWriter writer = new StreamWriter(filePath, false))
							{
								if (userRolePrivileges.Routes != null && userRolePrivileges.Routes.Count > 0)
								{
									foreach (var Route in userRolePrivileges.Routes)
									{
										VendorUserRolePrivilege VendorUserRolePrivilege = new VendorUserRolePrivilege()
										{
											UserRoleID = userRolePrivileges.UserRoleId,
											RouteID = Route.Id
										};
										if (_vendorUserRolePrivilegeService.CreateVendorUserRolePrivilege(VendorUserRolePrivilege, ref message))
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

							message = "User Role Privileges Updated Successfully !";
							return Json(new { success = true, message = message });
						}
						else
						{
							message = "Oops! Something went wrong. Please try later.";
						}
					}
					else
					{
						message = "Administrator privileges cannot be updated!";
					}
				}
				else
				{
					message = "Oops! Invalid user role. Please try later.";
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