using NowBuySell.Data;
using NowBuySell.Service;
using NowBuySell.Web.AuthorizationProvider;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml;

namespace NowBuySell.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class TagsController : Controller
    {
        string ErrorMessage = string.Empty;
        string SuccessMessage = string.Empty;
        private readonly ITagService _tagsService;

        public TagsController(ITagService tagsService)
        {
            this._tagsService = tagsService;

        }
      
        // GET: Admin/Tags
        public ActionResult Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.ExcelUploadErrorMessage = TempData["ExcelUploadErrorMessage"];
            return View();
        }
        public ActionResult List()
        {
            var tags = _tagsService.GetTags();
            return PartialView(tags);

        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Tag data)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_tagsService.CreateTag( ref data, ref message))
                {
                    return Json(new
                    {
                        success = true,
                        url = "/Admin/Tags/Index",
                        message = message,
                        data = new
                        {
                            Date = data.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                            Name = data.Name,
                            IsActive = data.IsActive.HasValue ? data.IsActive.Value.ToString() : bool.FalseString,
                            ID = data.ID
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
            Tag tag = _tagsService.GetTag((long)id);
            if (tag == null)
            {
                return HttpNotFound();
            }

            TempData["tagID"] = id;
            return View(tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tag tag)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                long Id;
                if (TempData["tagID"] != null && Int64.TryParse(TempData["tagID"].ToString(), out Id) && tag.ID == Id)
                {
                    if (_tagsService.UpdateTag(ref tag, ref message))
                    {
                        return Json(new
                        {
                            success = true,
                            url = "/Admin/Brands/Index",
                            message = "Tag updated successfully ...",
                            data = new
                            {
                                Date = tag.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                                Name = tag.Name,
                                IsActive = tag.IsActive.HasValue ? tag.IsActive.Value.ToString() : bool.FalseString,
                                ID = tag.ID
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
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tag = _tagsService.GetTag((Int16)id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            TempData["tagID"] = id;
            return View(tag);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            string message = string.Empty;
            if (_tagsService.DeleteTag((Int16)id, ref message))
            {
                return Json(new { success = true, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Activate(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var tag = _tagsService.GetTag((long)id);
            if (tag == null)
            {
                return HttpNotFound();
            }

            if (!(bool)tag.IsActive)
                tag.IsActive = true;
            else
            {
                tag.IsActive = false;
            }
            string message = string.Empty;
            if (_tagsService.UpdateTag(ref tag, ref message))
            {
                SuccessMessage = "Tag " + ((bool)tag.IsActive ? "activated" : "deactivated") + "  successfully ...";
                return Json(new
                {
                    success = true,
                    message = SuccessMessage,
                    data = new
                    {
                        Date = tag.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt"),
                        Name = tag.Name,
                        IsActive = tag.IsActive.HasValue ? tag.IsActive.Value.ToString() : bool.FalseString,
                        ID = tag.ID
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ErrorMessage = "Oops! Something went wrong. Please try later.";
            }

            return Json(new { success = false, message = ErrorMessage }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tag = _tagsService.GetTag((Int16)id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TagsReport()
        {
            var getAllTags = _tagsService.GetTags().ToList();
            if (getAllTags.Count() > 0)
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("TagsReport");

                    var headerRow = new List<string[]>()
                    {
                    new string[] {
                        "Creation Date"
                        ,"Name"
                        ,"Status"
                        }
                    };

                    // Determine the header range (e.g. A1:D1)
                    string headerRange = "A1:" + char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    // Target a worksheet
                    var worksheet = excel.Workbook.Worksheets["TagsReport"];

                    // Popular header row data
                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    var cellData = new List<object[]>();

                    foreach (var i in getAllTags)
                    {
                        cellData.Add(new object[] {
                        i.CreatedOn.HasValue ? i.CreatedOn.Value.ToString("dd MMM yyyy, h:mm tt") : "-"
                        ,!string.IsNullOrEmpty(i.Name) ? i.Name : "-"
                        ,i.IsActive == true ? "Active" : "InActive"
                        });
                    }

                    worksheet.Cells[2, 1].LoadFromArrays(cellData);

                    return File(excel.GetAsByteArray(), "application/msexcel", "Tags Report.xlsx");
                }
            }
            return RedirectToAction("Index");
        }

    }
}