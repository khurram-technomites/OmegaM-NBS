using Newtonsoft.Json;
using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api
{
    [RoutePrefix("api/v1")]
    public class CareerController : ApiController
    {
        private readonly ICandidateService _candidateService;

        public CareerController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpPost]
        [Route("careers")]
        public async Task<HttpResponseMessage> Career(string lang = "en")
        {
            string message = string.Empty, CVpath = string.Empty;

            try
            {
                string ThumbnailfilePath = string.Format("/Assets/AppFiles/CandidateCVs");
                string rootThumbnail = HttpContext.Current.Server.MapPath(ThumbnailfilePath);
                var providerThumbnail = new CustomMultipartFormDataStreamProvider(rootThumbnail);

                if (!Directory.Exists(rootThumbnail))
                {
                    Directory.CreateDirectory(rootThumbnail);
                }

                CustomMultipartFormDataStreamProvider file = await Request.Content.ReadAsMultipartAsync(providerThumbnail);

                if (file.Contents.Count() > 0)
                {
                    List<PathNameList> list = file.filePathList;
                    PathNameList CVImage = list.Where(x => x.Filename != "\"\"").FirstOrDefault();

                    if (CVImage == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = lang == "en" ? "Please upload file first" : ArabicDictionary.Translate("Please upload file first", false) });

                    Candidate Model = JsonConvert.DeserializeObject<Candidate>(providerThumbnail.FormData.GetValues("Career").FirstOrDefault());

                    Model.CreatedOn = NowBuySell.Web.Helpers.TimeZone.GetLocalDateTime();
                    Model.MarkAsRead = false;

                    CVpath = ThumbnailfilePath + "/" + CVImage.LocalPath;
                    Model.FilePath = CVpath.Replace("///", "/");

                    if (_candidateService.CreateCandidate(Model, ref message))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = message });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = lang == "en" ? "Please upload file first" : ArabicDictionary.Translate("Please upload file first", false) });
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false) });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = lang == "en" ? "Oops! Something went wrong. Please try later." : ArabicDictionary.Translate("Oops! Something went wrong. Please try later.", false) });
            }
        }
    }
}

