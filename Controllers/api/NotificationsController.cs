using NowBuySell.Data;
using NowBuySell.Service;
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
	public class NotificationsController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly INotificationService _notificationService;
		private readonly INotificationReceiverService _notificationReceiverService;

		public NotificationsController(INotificationService notificationService, INotificationReceiverService notificationReceiverService)
		{
			this._notificationService = notificationService;
			this._notificationReceiverService = notificationReceiverService;
		}

		[HttpGet]
		[Route("{lang}/notifications/offset/{offset}")]
		public HttpResponseMessage GetNotifications(string lang, long offset)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				Int64 customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
					if (ReceiverType == "Vendor")
					{
						customerId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
					}
					var NewNotificationCount = _notificationReceiverService.GetNewNotificationCount(customerId, ReceiverType);
					if (NewNotificationCount > 0)
					{
						var Notifications = _notificationService.GetNewNotifications(customerId, ReceiverType, offset, lang);
						_notificationReceiverService.MarkNotificationsAsDelivered(customerId, ReceiverType);

						foreach (var notification in Notifications)
							notification.Date = GetDate(Convert.ToDateTime(notification.Date));

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							newNotifications = NewNotificationCount,
							notifications = Notifications
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							newNotifications = 0,
							notifications = new List<SP_GetNewNotifications_Result>()
						});
					}

				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpGet]
		[Route("{lang}/notifications")]
		public HttpResponseMessage GetNotifications(string lang, int pg = 10)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				Int64 customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    if (ReceiverType == "Vendor")
                    {
						customerId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
					}

					var NewNotificationCount = _notificationReceiverService.GetNewNotificationCount(customerId, ReceiverType);
					var Notifications = _notificationService.GetNotifications(customerId, ReceiverType, pg, lang);
					_notificationReceiverService.MarkNotificationsAsDelivered(customerId, ReceiverType);

					foreach (var notification in Notifications)
						notification.Date = GetDate(Convert.ToDateTime(notification.Date));

					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						newNotifications = NewNotificationCount,
						notifications = Notifications
					});
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpGet]
		[Route("notifications/count")]
		public HttpResponseMessage GetNewNotificationsCount()
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				Int64 customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
					if (ReceiverType == "Vendor")
					{
						customerId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
					}
					var NewNotificationCount = _notificationReceiverService.GetNewNotificationCount(customerId, ReceiverType);

					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						newNotifications = NewNotificationCount
					});
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpPut]
		[Route("notifications/read")]
		public HttpResponseMessage NotificationsReadAll()
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				Int64 customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
					if (ReceiverType == "Vendor")
					{
						customerId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
					}
					_notificationReceiverService.MarkNotificationsAsRead(customerId, ReceiverType);
					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						message = "All notification read successfully !"
					});
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpPut]
		[Route("notifications/{id}/read")]
		public HttpResponseMessage NotificationsIsRead(long id)
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				Int64 customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					_notificationReceiverService.MarkNotificationAsRead(id);
					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						message = "Notification read successfully !"
					});
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpPut]
		[Route("notifications/seen")]
		public HttpResponseMessage NotificationsIsSeen()
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				Int64 customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					string ReceiverType = claims.Where(y => y.Type == ClaimTypes.Role).FirstOrDefault().Value;
					if (ReceiverType == "Vendor")
					{
						customerId = Int64.Parse(claims.Where(y => y.Type == ClaimTypes.Name).FirstOrDefault().Value);
					}
					_notificationReceiverService.MarkNotificationsAsSeen(customerId, ReceiverType);
					return Request.CreateResponse(HttpStatusCode.OK, new
					{
						status = "success",
						message = "Notification seen successfully !"
					});
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new { status = "error", message = "Session invalid or expired !" });
				}
			}
			catch
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		private string GetDate(DateTime? dateTime)
		{
			if (!dateTime.HasValue)
				return "";

			string Month = dateTime.Value.ToString("MMM");

			return dateTime.Value.Day + " " + Month + " " + dateTime.Value.Year;
		}
	}
}
