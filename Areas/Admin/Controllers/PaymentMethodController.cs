using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
	[AuthorizeAdmin]
	public class PaymentMethodController : Controller
	{
		string ErrorMessage = string.Empty;
		string SuccessMessage = string.Empty;

		private readonly IPaymentMethodService _paymentmethodService;


		public PaymentMethodController(IPaymentMethodService paymentmethodService)
		{
			this._paymentmethodService = paymentmethodService;
		}

		public ActionResult Index()
		{
			ViewBag.SuccessMessage = TempData["SuccessMessage"];
			ViewBag.ErrorMessage = TempData["ErrorMessage"];
			return View();
		}

		public ActionResult List()
		{
			var paymentmethods = _paymentmethodService.GetPaymentMethods();
			return PartialView(paymentmethods);
		}

		public ActionResult ListReport()
		{
			var paymentmethods = _paymentmethodService.GetPaymentMethods();
			return View(paymentmethods);
		}

		public ActionResult Details(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			PaymentMethod paymentmethod = _paymentmethodService.GetPaymentMethod((Int16)id);
			if (paymentmethod == null)
			{
				return HttpNotFound();
			}
			return View(paymentmethod);
		}

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(PaymentMethod paymentmethod)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				if (_paymentmethodService.CreatePaymentMethod(paymentmethod, ref message))
				{
					return Json(new
					{
						success = true,
						url = "/Admin/PaymentMethod/Index",
						message = message,
						data = new
						{
							ID = paymentmethod.ID,
							Date = paymentmethod.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
							Method = paymentmethod.Method,
							MethodAr = paymentmethod.MethodAr,
							IsActive = paymentmethod.IsActive.HasValue ? paymentmethod.IsActive.Value.ToString() : bool.FalseString
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
			PaymentMethod paymentmethod = _paymentmethodService.GetPaymentMethod((long)id);
			if (paymentmethod == null)
			{
				return HttpNotFound();
			}

			TempData["PaymentMethodID"] = id;
			return View(paymentmethod);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(PaymentMethod paymentmethod)
		{
			string message = string.Empty;
			if (ModelState.IsValid)
			{
				long Id;
				if (TempData["PaymentMethodID"] != null && Int64.TryParse(TempData["PaymentMethodID"].ToString(), out Id) && paymentmethod.ID == Id)
				{
					if (_paymentmethodService.UpdatePaymentMethod(ref paymentmethod, ref message))
					{
						return Json(new
						{
							success = true,
							url = "/Admin/PaymentMethod/Index",
							message = "PaymentMethod updated successfully ...",
							data = new
							{
								ID = paymentmethod.ID,
								Date = paymentmethod.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
								Method = paymentmethod.Method,
								MethodAr = paymentmethod.MethodAr,
								IsActive = paymentmethod.IsActive.HasValue ? paymentmethod.IsActive.Value.ToString() : bool.FalseString

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
			var paymentmethod = _paymentmethodService.GetPaymentMethod((long)id);
			if (paymentmethod == null)
			{
				return HttpNotFound();
			}

			if (!(bool)paymentmethod.IsActive)
				paymentmethod.IsActive = true;
			else
			{
				paymentmethod.IsActive = false;
			}
			string message = string.Empty;
			if (_paymentmethodService.UpdatePaymentMethod(ref paymentmethod, ref message))
			{
				SuccessMessage = "PaymentMethod " + ((bool)paymentmethod.IsActive ? "activated" : "deactivated") + "  successfully ...";
				return Json(new
				{
					success = true,
					message = SuccessMessage,
					data = new
					{
						ID = paymentmethod.ID,
						Date = paymentmethod.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
						Method = paymentmethod.Method,
						MethodAr = paymentmethod.MethodAr,
						IsActive = paymentmethod.IsActive.HasValue ? paymentmethod.IsActive.Value.ToString() : bool.FalseString

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
			PaymentMethod paymentmethod = _paymentmethodService.GetPaymentMethod((Int16)id);
			if (paymentmethod == null)
			{
				return HttpNotFound();
			}
			TempData["PaymentMethodID"] = id;
			return View(paymentmethod);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(long id)
		{
			string message = string.Empty;
			if (_paymentmethodService.DeletePaymentMethod((Int16)id, ref message))
			{
				return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
		}
	}
}