using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class UserController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly IUserService _userService;
        private readonly IUserRoleService _userRoleService;

        public UserController(IUserService userService, IUserRoleService userRoleService)
        {
            this._userService = userService;
            this._userRoleService = userRoleService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var users = _userService.GetUsers();
            return PartialView(users);
        }

        public ActionResult ListReport()
        {
            var users = _userService.GetUsers();
            return View(users);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userService.GetUser((Int16)id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        public ActionResult Create()
        {
            ViewBag.UserRoleID = new SelectList(_userRoleService.GetUserRolesForDropDown(), "value", "text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_userService.CreateUser(user, ref message, true))
                {
                    var role = _userRoleService.GetUserRole((long)user.UserRoleID);
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/User/Index",
                        message = message,
                        data = new
                        {
                            Date = user.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = user.Name,
                            MobileNo = user.MobileNo,
                            EmailAddress = user.EmailAddress,
                            Role = role.RoleName,
                            IsActive = user.IsActive.HasValue ? user.IsActive.Value.ToString() : bool.FalseString,
                            ID = user.ID
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
            User user = _userService.GetUser((long)id);
            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserRoleID = new SelectList(_userRoleService.GetUserRolesForDropDown(), "value", "text", user.UserRoleID);

            TempData["AdminUserID"] = id;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["AdminUserID"] != null && Int64.TryParse(TempData["AdminUserID"].ToString(), out Id) && user.ID == Id)
                {
                    if (_userService.UpdateUser(ref user, ref message))
                    {
                        var role = _userRoleService.GetUserRole((long)user.UserRoleID);
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/User/Index",
                            message = "User updated successfully ...",
                            data = new
                            {
                                Date = user.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                Name = user.Name,
                                MobileNo = user.MobileNo,
                                EmailAddress = user.EmailAddress,
                                Role = role.RoleName,
                                IsActive = user.IsActive.HasValue ? user.IsActive.Value.ToString() : bool.FalseString,
                                ID = user.ID
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
            long id = Convert.ToInt64(Session["AdminUserID"]);

            User user = _userService.GetUser((Int16)id);
            if (user == null)
            {
                return RedirectPermanent("/Admin/Dashboard/Index");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        new public ActionResult Profile(User user)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id = Convert.ToInt64(Session["AdminUserID"]);

                if (user.ID == Id)
                {
                    if (_userService.UpdateUser(ref user, ref message))
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
            return View(user);
        }


        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = _userService.GetUser((long)id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (!(bool)user.IsActive)
                user.IsActive = true;
            else
            {
                user.IsActive = false;
            }
            string message = string.Empty;
            if (_userService.UpdateUser(ref user, ref message))
            {
                var role = _userRoleService.GetUserRole((long)user.UserRoleID);
                SuccessMessage = "User " + ((bool)user.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = user.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = user.Name,
                        MobileNo = user.MobileNo,
                        EmailAddress = user.EmailAddress,
                        Role = role.RoleName,
                        IsActive = user.IsActive.HasValue ? user.IsActive.Value.ToString() : bool.FalseString,
                        ID = user.ID
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
            User user = _userService.GetUser((Int16)id);
            if (user == null)
            {
                return HttpNotFound();
            }
            TempData["AdminUserID"] = id;
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;

            if (_userService.DeleteUser((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UsersReport()
        {
            var getAllUsers = _userService.GetUsers().ToList();
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
                        ,!string.IsNullOrEmpty(i.UserRole.RoleName) ? i.UserRole.RoleName : "-"
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