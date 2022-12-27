using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.ViewModels.Api.Suggestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
	[Authorize]
	[RoutePrefix("api/v1")]
	public class SuggestionsController : ApiController
	{

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICustomerService _customerService;
		private readonly ICustomerSuggestionService _customerSuggestionService;
		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;

		public SuggestionsController(ICustomerService customerService, ICustomerSuggestionService customerSuggestionService, INotificationService notificationService, INotificationReceiverService notificationReceiverService)
		{
			this._customerService = customerService;
			this._customerSuggestionService = customerSuggestionService;
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
		}

		[HttpPost]
		[Route("suggestions")]
		public HttpResponseMessage Create(SuggestionViewModel suggestionViewModel)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					var customer = _customerService.GetCustomer(customerId);

					string message = string.Empty;
					if (ModelState.IsValid)
					{
						CustomerSuggestion customerSuggestion = new CustomerSuggestion()
						{
							CustomerID = customerId,
							Suggestion = suggestionViewModel.Suggestion,
							Rating= suggestionViewModel.Rating,
							Experience = "",
							Name = customer.Name,
							Email = customer.Email,
							IsShowOnWebsite = false,
						};

						if (_customerSuggestionService.CreateCustomerSuggestion(customerSuggestion, ref message))
						{
							/*Suggestion Notification For Admin*/
							Notification not = new Notification();
							not.Title = "New Suggestion";
							not.TitleAr = "اقتراح جديد";
							not.Description = string.Format("{0} submitted a suggestion", customer.Name);
							not.DescriptionAr = string.Format("{0} قدم اقتراح", customer.Name);
							not.Url = "/Admin/Suggestion/Index";
							not.Module = "Suggestion";
							not.OriginatorType = "System";
							not.RecordID = customerSuggestion.ID;
							if (_notificationService.CreateNotification(not, ref message))
							{
								_notificationReceiverService.NotifyAdminAndVendors(not.ID, "Admin", null);
							}

							return Request.CreateResponse(HttpStatusCode.OK, new
							{
								status = "success",
								message = "Feedback submitted!"
							});
						}
						return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.BadRequest, new
						{
							status = "error",
							message = "Bad request !",
							description = Helpers.ErrorMessages.GetErrorMessages(ModelState)
						});
					}
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}
	}
}
