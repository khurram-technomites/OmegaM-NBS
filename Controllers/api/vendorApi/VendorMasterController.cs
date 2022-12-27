using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NowBuySell.Web.Controllers.api.vendorApi
{
    //[Authorize(Roles = "Vendor")]
    [RoutePrefix("api/v1/vendor")]
    public class VendorMasterController : ApiController
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IFeatureService _featureService;
        private readonly ICarFeatureService _carFeatureService;
        private readonly ICarMakeService _carMakeService;
        private readonly ICarModelService _modelService;
        private readonly IBodyTypeService _bodyTypeService;
        private readonly ICarTagService _carTagService;
        private readonly ITagService _tagService;


        public VendorMasterController(IFeatureService featureService
            , ICarFeatureService carFeatureService
            , ICarMakeService carMakeService
            , ICarModelService modelService
            , IBodyTypeService bodyTypeService
            , ICarTagService carTagService
            , ITagService tagService)
        {
            this._carFeatureService = carFeatureService;
            this._featureService = featureService;
            this._carMakeService = carMakeService;
            this._modelService = modelService;
            this._bodyTypeService = bodyTypeService;
            this._carTagService = carTagService;
            this._tagService = tagService;
        }

        [HttpGet]
        [Route("masterdata")]
        public HttpResponseMessage GetCarMasterData()
        {
            try
            {
                string lang = "en";
                var ImageServer = CustomURL.GetImageServer();

                var features = _featureService.GetAllCarFeature().Select(i => new
                {
                    id = i.ID,
                    name = lang == "ar" ? i.NameAR.ToLower() : i.Name.ToLower(),
                    image = !string.IsNullOrEmpty(i.Image) ? ImageServer + i.Image : "",
                    count = _carFeatureService.GetCount((int)i.ID)
                });

                var makes = _carMakeService.GetCarMake().Select(i => new
                {
                    id = i.ID,
                    //name = lang == "en" ? i.Name.ToLower() : i.NameAR.ToLower()
                    name = lang == "en" ? i.Name : i.NameAR
                });
                var model = _modelService.GetCarModel().Select(i => new
                {
                    id = i.ID,
                    //name = lang == "en" ? i.Name.ToLower() : i.NameAR.ToLower(),
                    name = lang == "en" ? i.Name : i.NameAR,
                    carmakeID = i.CarMake_ID
                });
                var bodyTypes = _bodyTypeService.GetBodyType().Select(i => new
                {
                    id = i.ID,
                    //name = lang == "en" ? i.Name.ToLower() : i.NameAR.ToLower()
                    name = lang == "en" ? i.Name : i.NameAR
                });
                var tag = _tagService.GetTags().Select(i => new
                {
                    id = i.ID,
                    //name = lang == "en" ? i.Name.ToLower() : i.NameAr.ToLower()
                    name = lang == "en" ? i.Name : i.NameAr
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    status = "success",
                    motors = new
                    {
                        features = features,
                        makes = makes,
                        models = model,
                        bodyTypes = bodyTypes,
                        tags = tag,
                        condition = new object[]{
                            new {id=1,name= "1 - 3" } ,
                            new {id=2,name= "4 - 6" } ,
                            new {id=3,name= "7 - 9" } ,
                            new {id=4,name= "10 - 10" } ,
                        },
                        MechanicalCondition = new object[]{
                            
                            new {id=1,name= "Perfect inside" } ,
                            new {id=2,name= "Perfect out" } ,
                            new {id=3,name= "Perfect inside & out" }
                        },
                        fueltype = new object[]{
                            new {id=1,name= "Petrol" } ,
                            new {id=2,name= "Diesel" } ,
                            new {id=3,name= "Hybrid" } ,
                            new {id=4,name= "Electric" }
                        },
                        doors = new object[]{
                            new {id=1,name= "2 Doors" } ,
                            new {id=2,name= "3 Doors" } ,
                            new {id=3,name= "4 Doors" },
                            new {id=4,name= "5+ Doors" }
                        },

                        wheels = new object[]{
                            new {id=1,name= "2 Wheels" } ,
                            new {id=2,name= "3 Wheels" } ,
                            new {id=3,name= "4 Wheels" },
                            new {id=4,name= "6+ Wheels" }
                        },
                        cylinders = new object[]{

                        new {id=1,name= "3"    } ,
                        new {id=2,name= "4"    } ,
                        new {id=3,name= "5"    } ,
                        new {id=4,name= "6"    } ,
                        new {id=5,name= "8"    } ,
                        new {id=6,name= "10"    } ,
                        new {id=7,name= "12"    } ,
                        new {id=8,name= "16"    } ,
                        new {id=9,name= "N/A, Electric"} ,
                        new {id=10,name= "Not Sure"} ,
                        },
                        horsePower = new object[]{
                        new {id=1,name= "Less Than 150 HP"} ,
                        new {id=2,name= "150 - 200 HP"    } ,
                        new {id=3,name= "200 - 300 HP"    } ,
                        new {id=4,name= "300 - 400 HP"    } ,
                        new {id=5,name= "400 - 500 HP"    } ,
                        new {id=6,name= "500 - 600 HP"    } ,
                        new {id=7,name= "600 - 700 HP"    } ,
                        new {id=8,name= "700 - 800 HP"    } ,
                        new {id=9,name= "800 - 900 HP"    } ,
                        new {id=10,name= "900 + HP"        } ,
                        new {id=10,name= "Not Sure"} ,
                        new {id=11,name= "Other"} ,
                          },

                        capacity = new object[]{
                            new {id=1,name= "1 - 2 Persons" } ,
                            new {id=2,name= "2 - 4 Persons" } ,
                            new {id=3,name= "4 - 6 Persons" },
                            new {id=4,name= "6 - 10 Persons" }
                        },
                        steeringSide = new object[]{
                            new {id=1,name= "Left-Hand Side" } ,
                            new {id=2,name= "Right-Hand Side" } ,
                        },

                        Transmission = new object[]{
                            new {id=1,name= "Manual" } ,
                            new {id=2,name= "Automatic" } ,
                            new {id=3,name= "CVT" } ,
                            new {id=4,name= "DCT" } ,
                            },
                        RegionalSpecification = new object[]{
                            new {id=1,name= "GCC Specs" } ,
                            new {id=2,name= "European Specs" },
                            new {id=3,name= "Japanese Specs" },
                            new {id=4,name= "American Specs" },
                            new {id=5,name= "Canadian" },
                            new {id=6,name= "Australian Specs" },
                            new {id=7,name= "Not Sure" },
                            new {id=8,name= "Other Specs" }
                        },
                        TransmissionForFilter = new object[]
                            {
                                new{name = "Manual"},
                                new{name = "Automatic"},
                                new{name = "CVT"},
                                new{name = "DCT"}
                            }
                    }
                });

            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = "failure",
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

    }
}
