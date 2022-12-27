using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.ViewModels.Api.CustomerDocument;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace NowBuySell.Web.Controllers.api
{

	[RoutePrefix("api/v1")]
	public class CustomerDocumentController : ApiController
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ICustomerDocumentService _customerdocumentService;

		public CustomerDocumentController(ICustomerDocumentService customerDocumentService )
		{
			this._customerdocumentService = customerDocumentService;
		}


		[HttpPost]
        [Authorize]
        [Route("customerdocument")]
		public async Task<HttpResponseMessage> CreateDocument()
		{
			try
			{
                var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var claims = identity.Claims;
				long customerId;
                if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
                {
                   
					if (!Request.Content.IsMimeMultipartContent())
					{
						throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
					}
					string message = string.Empty;
					string status = string.Empty;
					string filePath = String.Format("/Assets/AppFiles/CustomerDocument/{0}", customerId.ToString().Replace(" ", "_"));
					string root = HttpContext.Current.Server.MapPath(filePath);
					if (!Directory.Exists(root))
					{
						string dir = filePath;


						if (!Directory.Exists(HttpContext.Current.Server.MapPath(dir)))
						{
							Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir));
						}

						
					}
					
					var provider = new CustomMultipartFormDataStreamProvider(root);

					CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(provider);
					

					CustomerDocumentViewModel customerDocumentViewModel = JsonConvert.DeserializeObject<CustomerDocumentViewModel>(provider.FormData.GetValues("documentdetails").FirstOrDefault());

					var customerIsExist = _customerdocumentService.GetCustomerDocumentByCustomerID(customerId);

					if (customerIsExist.Count() > 0)
					{

						foreach (var item in customerIsExist)
						{
							if (item.Type == customerDocumentViewModel.Type && item.CustomerID == customerId && item.IsGCC == customerDocumentViewModel.IsGCC)
							{
								CustomerDocument documentUpdate = new CustomerDocument()
								{
									ID = item.ID,
									Path = filePath + file.filePath,
								};

								if (_customerdocumentService.UpdateCustomerDocument(ref documentUpdate, ref message))
								{
									return Request.CreateResponse(HttpStatusCode.OK, new
									{
										status = "success",
										message = "Document saved!"
									});
								}

								else
								{
									return Request.CreateResponse(HttpStatusCode.InternalServerError, new
									{
										status = "error",
										message = "Document not saved! "
									});
								}

							}

                           


						}

						CustomerDocument document = new CustomerDocument();

							document.IsGCC = customerDocumentViewModel.IsGCC;
							document.CustomerID = customerId;
							document.Type = customerDocumentViewModel.Type;
							document.Path = filePath + file.filePath;
						
						

						if (_customerdocumentService.CreateCustomerDocument(ref document, ref message))
						{
							return Request.CreateResponse(HttpStatusCode.OK, new
							{
								status = "success",
								message = "Document saved!"
							});

						}
						else
						{
							return Request.CreateResponse(HttpStatusCode.InternalServerError, new
							{
								status = "error",
								message = "Failed to upload "
							});
						}


					}

					else 
							{
								CustomerDocument document = new CustomerDocument()
								{
									IsGCC = customerDocumentViewModel.IsGCC,
									CustomerID = customerId,
									Type = customerDocumentViewModel.Type,
									Path = filePath + file.filePath,
								};

								if (_customerdocumentService.CreateCustomerDocument(ref document, ref message))
								{
									return Request.CreateResponse(HttpStatusCode.OK, new
									{
										status = "success",
										message = "Document saved!"
									});

								}
								else
								{
									return Request.CreateResponse(HttpStatusCode.InternalServerError, new
									{
										status = "error",
										message = "Failed to upload "
									});
								}

							}


					}
					return Request.CreateResponse(HttpStatusCode.InternalServerError, new
					{
						status = "error",
						message = "Failed to upload "
					});


				

			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
				//Logs.Write(ex);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, new { status = "failure", message = "Oops! Something went wrong. Please try later." });
			}
		}

		[HttpGet]
		[Route("customerdocument-delete/{id}")]
		public HttpResponseMessage Delete(long id)
		{
			try
			{
				
					var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
					var claims = identity.Claims;
					long customerId;
					if (claims.Count() == 4 && Int64.TryParse(claims.LastOrDefault().Value, out customerId))
					{
						string status = string.Empty;
						string message = string.Empty;
						if (_customerdocumentService.DeleteCustomerDocument(id, ref message))
						{
							return Request.CreateResponse(HttpStatusCode.OK, new
							{
								status = "success",
								message = "Document deleted successfully"
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
