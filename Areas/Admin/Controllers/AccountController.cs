using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.ViewModels.Account;
using System;
using System.Linq;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	public class AccountController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IUserService _userService;

		public AccountController(IUserService userService)
		{
			this._userService = userService;
		}

        public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginViewModel Login)
		{
			Session.Clear();
			if (ModelState.IsValid)
			{

				var User = _userService.Authenticate(Login.EmailAddress, Login.Password, ref ErrorMessage);
				if (User != null)
				{
					Session["AdminUserID"] = User.ID;
					Session["UserName"] = User.Name;
					Session["Role"] = User.UserRole.RoleName;
					Session["Email"] = User.EmailAddress;
                    Session["ReceiverType"] = User.UserRole.RoleName;


					string Chars = "";
					try
					{

						if (!string.IsNullOrEmpty(User.Name))
						{
							string[] names = User.Name.Split(' ');
							for (int i = 0; i < names.Length; i++)
							{
								char Char;
								Char = names[i].ToUpper().First();
								Chars += Char;
							}
						}
						else
						{
							Chars = "AD";
						}

					}
					catch (Exception)
					{
						Chars = "AD";
					}
					Session["UserNameChar"] = Chars;



					string AccessToken = Guid.NewGuid() + "-" + Convert.ToString(User.ID);
					Session["Access-Token"] = AccessToken;
					Response.Cookies["Admin-Session"]["Access-Token"] = AccessToken;


					string url = "/Admin/Dashboard/Index";
					return Json(new { success = true, url = url, message = ErrorMessage });

				}
				else
				{
					ViewBag.Message = ErrorMessage;
				}
			}
			else
			{
				ErrorMessage = "Please enter email and password first !";
			}
			return Json(new { success = false, message = ErrorMessage });
		}
		[HttpGet]
		public ActionResult ForgetPassword()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ForgetPassword(ForgotPasswordViewModel forgotPasswordViewModel)
		{
			string Message = string.Empty;
			if (ModelState.IsValid)
			{
				var path = Server.MapPath("~/");
				if (_userService.ForgotPassword(forgotPasswordViewModel.EmailAddress, ref Message, path))
				{
					return Json(new { success = true, message = Message });
				}
				else
				{
					ErrorMessage = Message;
				}
			}
			else
			{
				ErrorMessage = "Please fill the form properly ...";
			}


			return Json(new { success = false, message = ErrorMessage });
		}

		public ActionResult ResetPassword()
		{
			var AuthCode = Request.QueryString["auth"];
			if (AuthCode == "" || AuthCode == null)
			{
				ViewBag.ErrorMessage = "Invalid Session!";
			}
			else
			{
				var user = _userService.GetByAuthCode(AuthCode);
				if (user != null)
				{
					if (user.AuthorizationExpiry >= Helpers.TimeZone.GetLocalDateTime())
					{
						Session["UserResetID"] = user.ID;
					}
					else
					{
						ViewBag.ErrorMessage = "Session expired!";
					}
				}
				else
				{
					ViewBag.ErrorMessage = "Invalid Session!";
				}
			}

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
		{
			string Message = string.Empty;

			if (ModelState.IsValid)
			{
				if (Session["UserResetID"] != null)
				{
					Int64 UserId = Convert.ToInt64(Session["UserResetID"]);

					if (_userService.ResetPassword(resetPasswordViewModel.NewPassword, UserId, ref Message))
					{
						Session.Remove("UserResetID");
						string url = "Admin/Dashboard/Index";
						return Json(new { success = true, url = url, message = Message });
					}
					else
					{
						ErrorMessage = Message;
					}
				}
				else
				{
					ErrorMessage = "Session expired!";
				}
			}
			else
			{
				ErrorMessage = "Please fill the form properly ...";
			}
			return Json(new { success = false, message = ErrorMessage });
		}

		[AuthorizeAdmin]
		public ActionResult ChangePassword()
		{
			long userId = Convert.ToInt64(Session["AdminUserID"]);
			var user = _userService.GetUser(userId);

			return View(user);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[AuthorizeAdmin]
		public ActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
		{
			string Message = "Invalid form data";

			if (ModelState.IsValid)
			{

				Int64 UserId = Convert.ToInt64(Session["AdminUserID"]);

				if (_userService.ChangePassword(changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword, UserId, ref Message))
				{
					return Json(new
					{
						success = true,
						message = "Password Changed Successfully"
					});
				}
				else
				{
					ErrorMessage = Message;
				}
			}
			return Json(new { success = false, message = Message });
		}

		[AuthorizeAdmin]
		public ActionResult Logout()
		{
			Session.Remove("AdminUserID");
			Session.Remove("UserName");
			Session.Remove("Role");
			Session.Remove("Email");
			Session.Remove("Access-Token");

			Request.Cookies.Remove("Admin-Session");

			return RedirectToAction("Login");
		}
	}
}