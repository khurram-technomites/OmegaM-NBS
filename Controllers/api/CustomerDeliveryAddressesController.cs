using NowBuySell.Data;
using NowBuySell.Service;
using System;
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
	public class CustomerDeliveryAddressesController : ApiController
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICustomerService _customerService;
		private readonly ICustomerDeliveryAddressService _customerDeliveryAddressService;

		public CustomerDeliveryAddressesController(ICustomerService customerService, ICustomerDeliveryAddressService customerDeliveryAddressService)
		{
			this._customerService = customerService;
			this._customerDeliveryAddressService = customerDeliveryAddressService;
		}

		[HttpGet]
		[Route("deliveryaddress")]
		public HttpResponseMessage GetAll()
		{
			try
			{
				var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
				var claims = identity.Claims;
				long customerId;
				if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
				{
					Customer customer = _customerService.GetCustomer((long)customerId);
					if (customer != null)
					{
						var DeliveryAddresses = _customerDeliveryAddressService.GetCustomerDeliveryAddresses(customerId);
						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = "success",
							deliveryAddresses = DeliveryAddresses.Select(i => new
							{
								i.ID,
								i.Type,
								country = i.Country != null ? new { id = i.Country.ID, name = i.Country.Name } : null,
								state = i.City != null ? new { id = i.City.ID, name = i.City.Name } : null,
								area = i.Area != null ? new { id = i.Area.ID, name = i.Area.Name } : null,
								IsDefault = i.IsDefault,
								address = i.Address,
							})
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.Unauthorized, new
						{
							status = "error",
							message = "Session invalid or expired !"
						});
					}
				}
				else
				{
					return Request.CreateResponse(HttpStatusCode.Unauthorized, new
					{
						status = "error",
						message = "Session invalid or expired !"
					});
				}
			}
			catch (Exception ex)
			{
				//log.Error("Error", ex);

				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpPost]
		[Route("deliveryaddress")]
		public HttpResponseMessage Create(CustomerDeliveryAddress deliveryAddress)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
					var claims = identity.Claims;
					long customerId;
					if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
					{
						string status = string.Empty;
						string message = string.Empty;
						deliveryAddress.CustomerID = customerId;
						if (_customerDeliveryAddressService.CreateCustomerDeliveryAddress(ref deliveryAddress, ref message))
						{

							if (deliveryAddress.IsDefault.HasValue && deliveryAddress.IsDefault.Value)
								_customerDeliveryAddressService.UpdateIsDefaultCustomerDeliveryAddress(deliveryAddress.ID, customerId, ref message);
							return Request.CreateResponse(HttpStatusCode.OK, new
							{
								status = "success",
								message = "Delivery address added!"
							});
						}

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = status,
							message = message
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.Unauthorized, new
						{
							status = "error",
							message = "Session invalid or expired !"
						});
					}
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
			catch (Exception ex)
			{
				//log.Error("Error", ex);

				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpPut]
		[Route("deliveryaddress/{id}")]
		public HttpResponseMessage Update(long id, CustomerDeliveryAddress deliveryAddress)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
					var claims = identity.Claims;
					long customerId;
					if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
					{
						string status = string.Empty;
						string message = string.Empty;
						deliveryAddress.ID = id;
						deliveryAddress.CustomerID = customerId;
						if (_customerDeliveryAddressService.UpdateCustomerDeliveryAddress(ref deliveryAddress, ref message))
						{
							if (deliveryAddress.IsDefault.HasValue && deliveryAddress.IsDefault.Value)
								_customerDeliveryAddressService.UpdateIsDefaultCustomerDeliveryAddress(deliveryAddress.ID, customerId, ref message);
							return Request.CreateResponse(HttpStatusCode.OK, new
							{
								status = "success",
								message = "Delivery address updated!"
							});
						}

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = status,
							message = message
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.Unauthorized, new
						{
							status = "error",
							message = "Session invalid or expired !"
						});
					}
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
			catch (Exception ex)
			{
				//log.Error("Error", ex);

				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}

		[HttpDelete]
		[Route("deliveryaddress/{id}")]
		public HttpResponseMessage Delete(long id)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
					var claims = identity.Claims;
					long customerId;
					if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
					{
						string status = string.Empty;
						string message = string.Empty;
						if (_customerDeliveryAddressService.DeleteCustomerDeliveryAddress(id, ref message))
						{
							return Request.CreateResponse(HttpStatusCode.OK, new
							{
								status = "success",
								message = "Delivery address deleted!"
							});
						}

						return Request.CreateResponse(HttpStatusCode.OK, new
						{
							status = status,
							message = message
						});
					}
					else
					{
						return Request.CreateResponse(HttpStatusCode.Unauthorized, new
						{
							status = "error",
							message = "Session invalid or expired !"
						});
					}
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
			catch (Exception ex)
			{
				//log.Error("Error", ex);

				return Request.CreateResponse(HttpStatusCode.InternalServerError, new
				{
					status = "failure",
					message = "Oops! Something went wrong. Please try later."
				});
			}
		}
	}
}
