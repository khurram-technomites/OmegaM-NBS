using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.VendorPortal.Controllers
{

	[AuthorizeVendor]
	public class UserController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;
        private readonly IVendorUserService _vendorUserService;
		private readonly IVendorUserRoleService _vendorUserRoleService;

		public UserController(IVendorUserService vendorUserService, IVendorUserRoleService vendorUserRoleService)
		{
			this._vendorUserService = vendorUserService;
			this._vendorUserRoleService = vendorUserRoleService;
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
			var vendorUsers = _vendorUserService.GetVendorUsers(VendorID);
			return PartialView(vendorUsers);
		}

		public ActionResult ListReport()
		{
			var vendorUsers = _vendorUserService.GetVendorUsers();
			return View(vendorUsers);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			VendorUser vendorUser = _vendorUserService.GetVendorUser((Int16)id);
			if (vendorUser == null)
			{
				return HttpNotFound();
			}
			return View(vendorUser);
		}

		public ActionResult Create()
		{
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			ViewBag.UserRoleID = new SelectList(_vendorUserRoleService.GetVendorUserRolesForDropDown(VendorID), "value", "text");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(VendorUser vendorUser)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{

				var VendorID = Convert.ToInt64(Session["VendorID"]);
				vendorUser.VendorID = VendorID;
				if (_vendorUserService.CreateVendorUser(vendorUser, ref message, true))
				{
					var role = _vendorUserRoleService.GetVendorUserRole((long)vendorUser.UserRoleID);
					return Json(new
					{
						success = true,
						url = "/Vendor/User/Index",
						message = message,
						data = new
						{
							Date = vendorUser.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
							Name = vendorUser.Name,
							MobileNo = vendorUser.MobileNo,
							EmailAddress = vendorUser.EmailAddress,
							Role = role.Name,
							IsActive = vendorUser.IsActive.HasValue ? vendorUser.IsActive.Value.ToString() : bool.FalseString,
							ID = vendorUser.ID
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
			VendorUser vendorUser = _vendorUserService.GetVendorUser((long)id);
			if (vendorUser == null)
			{
				return HttpNotFound();
			}

			ViewBag.UserRoleID = new SelectList(_vendorUserRoleService.GetVendorUserRolesForDropDown((long)vendorUser.VendorID), "value", "text", vendorUser.UserRoleID);

			TempData["VendorUserID"] = id;
			return View(vendorUser);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(VendorUser vendorUser)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["VendorUserID"] != null && Int64.TryParse(TempData["VendorUserID"].ToString(), out Id) && vendorUser.ID == Id)
				{
					if (_vendorUserService.UpdateVendorUser(ref vendorUser, ref message))
					{
						var role = _vendorUserRoleService.GetVendorUserRole((long)vendorUser.UserRoleID);
						return Json(new
						{
							success = true,
							url = "/Vendor/User/Index",
							message = "User updated successfully ...",
							data = new
							{
								Date = vendorUser.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
								Name = vendorUser.Name,
								MobileNo = vendorUser.MobileNo,
								EmailAddress = vendorUser.EmailAddress,
								Role = role.Name,
								IsActive = vendorUser.IsActive.HasValue ? vendorUser.IsActive.Value.ToString() : bool.FalseString,
								ID = vendorUser.ID
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

		new public ActionResult Profile()
		{
			long id = Convert.ToInt64(Session["VendorUserID"]);

			VendorUser vendorUser = _vendorUserService.GetVendorUser((Int16)id);
			if (vendorUser == null)
			{
				return RedirectPermanent("/Vendor/Dashboard/Index");
			}

			return View(vendorUser);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		new public ActionResult Profile(VendorUser vendorUser)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id = Convert.ToInt64(Session["VendorUserID"]);

				if (vendorUser.ID == Id)
				{
					if (_vendorUserService.UpdateVendorUser(ref vendorUser, ref message))
					{
						ViewBag.SuccessMessage = "Profile updated successfully ...";
						return View();
					}

					ViewBag.ErrorMessage = message;
				}
				else
				{
					ViewBag.ErrorMessage = "Oops! Something went wrong. Please try later.";
				}
			}
			else
			{
				ViewBag.ErrorMessage = "Please fill the form properly ...";
			}
			return View(vendorUser);
		}

		public ActionResult Activate(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var vendorUser = _vendorUserService.GetVendorUser((long)id);
			if (vendorUser == null)
			{
				return HttpNotFound();
			}

			if (!(bool)vendorUser.IsActive)
				vendorUser.IsActive = true;
			else
			{
				vendorUser.IsActive = false;
			}
			string message = string.Empty;
			if (_vendorUserService.UpdateVendorUser(ref vendorUser, ref message))
			{
				var role = _vendorUserRoleService.GetVendorUserRole((long)vendorUser.UserRoleID);
				SuccessMessage = "User " + ((bool)vendorUser.IsActive ? "activated" : "deactivated") + "  successfully ...";
				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						ID = vendorUser.ID,
						Date = vendorUser.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
						Name = vendorUser.Name,
						EmailAddress = vendorUser.EmailAddress,
						MobileNo = vendorUser.MobileNo,
						Role = role.Name,
						IsActive = vendorUser.IsActive.HasValue ? vendorUser.IsActive.Value.ToString() : bool.FalseString
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
			VendorUser vendorUser = _vendorUserService.GetVendorUser((Int16)id);
			if (vendorUser == null)
			{
				return HttpNotFound();
			}
			TempData["VendorUserID"] = id;
			return View(vendorUser);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;

			if (_vendorUserService.DeleteVendorUser((Int16)id, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult UsersReport()
		{
			var VendorID = Convert.ToInt64(Session["VendorID"]);
			var getAllUsers = _vendorUserService.GetVendorUsers(VendorID);
			if (getAllUsers.Count() > 0)
			{
				using (ExcelPackage excel = new ExcelPackage())
				{
					excel.Workbook.Worksheets.Add("UsersReport");

					var headerRow = new List<string[]>()
					{
					new string[] {
						"Creation Date"
						,"Name"
						,"Email"
						,"Mobile"
						,"Role"
						,"Status"
						}
					};

					// Determine the header range (e.g. A1:D1)
					string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

					// Target a worksheet
					var worksheet = excel.Workbook.Worksheets["UsersReport"];

					// Popular header row data
					worksheet.Cells[headerRange].LoadFromArrays(headerRow);

					var cellData = new List<object[]>();

					foreach (var i in getAllUsers)
					{
						cellData.Add(new object[] {
						i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
						,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
						,!string.IsNullOrEmpty(i.EmailAddress) ? i.EmailAddress : "-"
						,!string.IsNullOrEmpty(i.MobileNo) ? i.MobileNo : "-"
						,!string.IsNullOrEmpty(i.VendorUserRole.Name) ? i.VendorUserRole.Name : "-"
						,i.IsActive == true ? "Active" : "InActive"
						});
					}

					worksheet.Cells[2, 1].LoadFromArrays(cellData);

					return File(excel.GetAsByteArray(), "application/msexcel", "Users Report.xlsx");
				}
			}
			return RedirectToAction("Index");
		}
	}
}