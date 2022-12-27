using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using NowBuySell.Web.Helpers;
using NowBuySell.Web.Helpers.Routing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CategoryController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;

        private readonly ICategoryService _categoryService;


        public CategoryController(ICategoryService categoryService)
        {
            this._categoryService = categoryService;
        }

        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult List()
        {
            var categories = _categoryService.GetCarCategories();
            return PartialView(categories);
        }

        public ActionResult ListReport()
        {
            var categories = _categoryService.GetCarCategories();
            return View(categories);
        }

        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = _categoryService.GetCategory((Int16)id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        public ActionResult Create()
        {
            ViewBag.ParentCategoryID = new SelectList(_categoryService.GetCarCategoriesForDropDown(0), "value", "text");
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(string CategoryName, string CategoryNameAr, string Description, int Position, string DescriptionAR, string Slug, bool IsDefault, string For, string type, string PropertyType)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Category/"), CategoryName.Replace(" ", "_"), "/Image");

                    string absolutePath = Server.MapPath("~");
                    string relativePathImage = string.Format("/Assets/AppFiles/Images/Category/{0}/", CategoryName.Replace(" ", "_"));
                    string relativePathIcon = string.Format("/Assets/AppFiles/Images/Category/{0}/Icon/", CategoryName.Replace(" ", "_"));
                    Category category = new Category();
                    category.CategoryName = CategoryName;
                    category.CategoryNameAr = CategoryNameAr;
                    //category.ParentCategoryID = ParentCategoryID;
                    category.Slug = Slug;
                    category.Description = Description;
                    category.DescriptionAR = DescriptionAR;
                    category.IsDefault = IsDefault;
                    category.Position = Position;
                    category.Module = For;
                    category.PropertyType = PropertyType;
                    category.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePathImage, "Image", ref message, "Image");
                    category.Icon = Uploader.UploadImage(Request.Files, absolutePath, relativePathIcon, "Icon", ref message, "Icon");

                    switch (type)
                    {
                        case "0":
                            category.ForSale = false;
                            category.ForRent = false;
                            break;
                        case "1":
                            category.ForSale = false;
                            category.ForRent = true;
                            break;
                        case "2":
                            category.ForSale = true;
                            category.ForRent = false;
                            break;
                        case "3":
                            category.ForSale = true;
                            category.ForRent = true;
                            break;
                    }

                    if (_categoryService.CreateCategory(category, ref message))
                    {
                        var Parent = category.ParentCategoryID.HasValue ? _categoryService.GetCategory((long)category.ParentCategoryID) : null;
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/Category/Index",
                            message = message,
                            data = new
                            {
                                ID = category.ID,
                                Date = category.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                //Parent = Parent != null ? (Parent.CategoryName) : "",
                                Position = category.Position,
                                Category = category.Icon + '|' + category.Image + '|' + category.CategoryName,
                                Type = category.ForSale.Value.ToString() + '|' + category.ForRent.Value.ToString(),
                                //IsParentCategoryDeleted = category.IsParentCategoryDeleted.HasValue ? category.IsParentCategoryDeleted.Value.ToString() : bool.FalseString,
                                IsActive = category.IsActive.HasValue ? category.IsActive.Value.ToString() : bool.FalseString
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
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = _categoryService.GetCategory((long)id);
            if (category == null)
            {
                return HttpNotFound();
            }

            TempData["CategoryID"] = id;

            ViewBag.ParentCategoryID = new SelectList(_categoryService.GetCarCategoriesForDropDown(id), "value", "text", category.ParentCategoryID);
            return View(category);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit(long id, string CategoryName, string CategoryNameAr, int Position, string Description, string DescriptionAR, string Slug, bool IsDefault, string For, string type)
        {
            try
            {
                string message = string.Empty;
                if (ModelState.IsValid)
                {
                    long Id;
                    if (TempData["CategoryID"] != null && Int64.TryParse(TempData["CategoryID"].ToString(), out Id) && id == Id)
                    {

                        Category category = _categoryService.GetCategory(id);
                        category.CategoryName = CategoryName;
                        category.CategoryNameAr = CategoryNameAr;
                        //category.ParentCategoryID = ParentCategoryID;
                        category.Slug = Slug;
                        category.Description = Description;
                        category.DescriptionAR = DescriptionAR;
                        category.IsDefault = IsDefault;
                        category.IsParentCategoryDeleted = false;
                        category.Position = Position;
                        //category.Module = For;

                        switch (type)
                        {
                            case "0":
                                category.ForSale = false;
                                category.ForRent = false;
                                break;
                            case "1":
                                category.ForSale = false;
                                category.ForRent = true;
                                break;
                            case "2":
                                category.ForSale = true;
                                category.ForRent = false;
                                break;
                            case "3":
                                category.ForSale = true;
                                category.ForRent = true;
                                break;
                        }

                        if (Request.Files["Image"] != null)
                        {
                            string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Category/"), CategoryName.Replace(" ", "_"), "/Image");

                            string absolutePath = Server.MapPath("~");
                            string relativePath = string.Format("/Assets/AppFiles/Images/Category/{0}/", CategoryName.Replace(" ", "_"));
                            category.Image = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Image", ref message, "Image");
                        }
                        if (Request.Files["Icon"] != null)
                        {
                            string FilePath = string.Format("{0}{1}{2}", Server.MapPath("~/Assets/AppFiles/Images/Category/"), CategoryName.Replace(" ", "_"), "/Image");

                            string absolutePath = Server.MapPath("~");
                            string relativePath = string.Format("/Assets/AppFiles/Images/Category/{0}/", CategoryName.Replace(" ", "_"));
                            category.Icon = Uploader.UploadImage(Request.Files, absolutePath, relativePath, "Icon", ref message, "Icon");
                        }

                        if (_categoryService.UpdateCategory(ref category, ref message, false))
                        {
                            //var Parent = ParentCategoryID.HasValue ? _categoryService.GetCategory((long)category.ParentCategoryID) : null;
                            return Json(new
                            {
                                success = true,
                                url = "/Admin/Category/Index",
                                message = "Category updated successfully ...",
                                data = new
                                {
                                    ID = category.ID,
                                    Date = category.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                                    //Parent = Parent != null ? (Parent.CategoryName) : "",
                                    Position = category.Position,
                                    Category = category.Icon + '|' + category.Image + "|" + category.CategoryName,
                                    //IsParentCategoryDeleted = category.IsParentCategoryDeleted.HasValue ? category.IsParentCategoryDeleted.Value.ToString() : bool.FalseString,
                                    IsActive = category.IsActive.HasValue ? category.IsActive.Value.ToString() : bool.FalseString
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
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Oops! Something went wrong. Please try later."
                });
            }
        }

        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var category = _categoryService.GetCategory((long)id);
            if (category == null)
            {
                return HttpNotFound();
            }

            if (!(bool)category.IsActive)
                category.IsActive = true;
            else
            {
                category.IsActive = false;
            }
            string message = string.Empty;
            if (_categoryService.UpdateCategory(ref category, ref message))
            {
                //bool hasChilds = false;
                //if (category.Category1.Count() > 0)
                //{
                //	hasChilds = true;
                //	_categoryService.UpdateDeletedCategoryChilds(category.ID);
                //}
                //else
                //{
                //	hasChilds = false;
                //}

                SuccessMessage = "Category " + ((bool)category.IsActive ? "activated" : "deactivated") + "  successfully ...";
                //var Parent = category.ParentCategoryID.HasValue ? _categoryService.GetCategory((long)category.ParentCategoryID) : null;
                return Json(new
                {
                    success = true,
                    //hadChilds = hasChilds,
                    message = SuccessMessage,
                    data = new
                    {
                        ID = category.ID,
                        Date = category.CreatedOn.Value.ToString("dd MMM yyyy, h: mm tt"),
                        //Parent = Parent != null ? (Parent.CategoryName) : "",
                        Position = category.Position,
                        Category = category.Icon + "|" + category.Image + "|" + category.CategoryName,
                        Icon = category.Icon,
                        //IsParentCategoryDeleted = category.IsParentCategoryDeleted.HasValue ? category.IsParentCategoryDeleted.Value.ToString() : bool.FalseString,
                        IsActive = category.IsActive.HasValue ? category.IsActive.Value.ToString() : bool.FalseString

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
            Category category = _categoryService.GetCategory((Int16)id);
            if (category == null)
            {
                return HttpNotFound();
            }
            TempData["CategoryID"] = id;
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            bool hasChilds = false;

            if (_categoryService.DeleteCategory((Int16)id, ref message, ref hasChilds))
            {
                return Json(new
                {
                    success = true,
                    message = message,
                    hadChilds = hasChilds
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoriesReport()
        {
            string ImageServer = CustomURL.GetImageServer();
            var getAllCatagories = _categoryService.GetCarCategories().ToList();
            if (getAllCatagories.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("CategoriesReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Name"
                        ,"NameAr"
                        ,"Description"
                        ,"DescriptionAR"
                        ,"Position"
                        ,"Slug"
                        ,"Image"
                        ,"Icon"
                        ,"Show on Header"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["CategoriesReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    if (getAllCatagories.Count != 0)
                        getAllCatagories = getAllCatagories.OrderByDescending(x => x.ID).ToList();

                    foreach (var i in getAllCatagories)
                    {
                        //string parentCategory = "-";
                        //if (i.ParentCategoryID != null)
                        //	parentCategory = getAllCatagories.SingleOrDefault(x => x.ID == i.ParentCategoryID)?.CategoryName;

                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.CategoryName) ? i.CategoryName : "-"
                        ,!string.IsNullOrEmpty(i.CategoryNameAr) ? i.CategoryNameAr : "-"
                        ,!string.IsNullOrEmpty(i.Description) ? i.Description : "-"
                        ,!string.IsNullOrEmpty(i.DescriptionAR) ? i.DescriptionAR : "-"
                        ,i.Position.HasValue ? i.Position.Value : 0
                        ,!string.IsNullOrEmpty(i.Slug) ? i.Slug : "-"
                        ,!string.IsNullOrEmpty(i.Image) ? (ImageServer + i.Image) : "-"
                        ,!string.IsNullOrEmpty(i.Icon) ? (ImageServer + i.Icon) : "-"
						//,parentCategory
						,i.IsDefault == true ? "Yes" : "No"
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Categories Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

    }
}