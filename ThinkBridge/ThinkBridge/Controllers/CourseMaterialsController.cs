using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ThinkBridge.Helpers;
using ThinkBridge.Models;
using ThinkBridge.Models.ViewModels;
using ThinkBridge.Services;
namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Admin", "Lecturer", "Student")]
    public class CourseMaterialsController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly WebResourceSearchService _webSearch = new WebResourceSearchService();
        public ActionResult Index(int? subjectId, int? topicId)
        {
            var materials = _db.CourseMaterials.AsQueryable();

            if (subjectId.HasValue)
                materials = materials.Where(m => m.SubjectId == subjectId);

            if (topicId.HasValue)
                materials = materials.Where(m => m.TopicId == topicId);

            var subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            var topics = _db.Topics.OrderBy(t => t.Name).ToList();

            ViewBag.Subjects = subjects;
            ViewBag.Topics = topics;
            ViewBag.SelectedSubjectId = subjectId;
            ViewBag.SelectedTopicId = topicId;

            var selectedSubject = subjectId.HasValue
                ? subjects.FirstOrDefault(s => s.Id == subjectId.Value)
                : null;

            var selectedTopic = topicId.HasValue
                ? topics.FirstOrDefault(t => t.Id == topicId.Value)
                : null;

            var searchText = "";

            if (selectedSubject != null)
                searchText += selectedSubject.Name + " ";

            if (selectedTopic != null)
                searchText += selectedTopic.Name + " ";

            searchText = string.IsNullOrWhiteSpace(searchText)
                ? "online learning study notes"
                : searchText.Trim();

            ViewBag.DynamicResources = new List<ResourceLinkViewModel>
    {
        new ResourceLinkViewModel
        {
            Title = "Search Tutorials and Notes",
            Url = "https://www.google.com/search?q=" +
                  HttpUtility.UrlEncode(searchText + " tutorial notes pdf article"),
            Type = "PDF/Tutorial"
        },
        new ResourceLinkViewModel
        {
            Title = "Search Academic Articles",
            Url = "https://scholar.google.com/scholar?q=" +
                  HttpUtility.UrlEncode(searchText),
            Type = "Research"
        },
        new ResourceLinkViewModel
        {
            Title = "Search YouTube Videos",
            Url = "https://www.youtube.com/results?search_query=" +
                  HttpUtility.UrlEncode(searchText + " tutorial"),
            Type = "Video"
        },
        new ResourceLinkViewModel
        {
            Title = "Search GitHub Projects",
            Url = "https://github.com/search?q=" +
                  HttpUtility.UrlEncode(searchText),
            Type = "Code"
        }
    };

            return View(materials.OrderByDescending(m => m.UploadDate).ToList());
        }
        public ActionResult Resources(string topic)
        {
            topic = topic ?? "Data Structures";

            var resources = new List<ResourceLinkViewModel>
    {
        new ResourceLinkViewModel
        {
            Title = "Search Tutorials",
            Url = "https://www.google.com/search?q=" +
                  HttpUtility.UrlEncode(topic + " tutorial pdf"),
            Type = "PDF/Tutorial"
        },

        new ResourceLinkViewModel
        {
            Title = "Search Academic Articles",
            Url = "https://scholar.google.com/scholar?q=" +
                  HttpUtility.UrlEncode(topic),
            Type = "Research"
        },

        new ResourceLinkViewModel
        {
            Title = "Search YouTube Videos",
            Url = "https://www.youtube.com/results?search_query=" +
                  HttpUtility.UrlEncode(topic),
            Type = "Video"
        },

        new ResourceLinkViewModel
        {
            Title = "Search GitHub Projects",
            Url = "https://github.com/search?q=" +
                  HttpUtility.UrlEncode(topic),
            Type = "Code"
        }
    };

            return View(resources);
        }
        public ActionResult Details(int id)
        {
            var material = _db.CourseMaterials.Find(id);
            if (material == null)
                return HttpNotFound();

            ViewBag.Subject = _db.Subjects.Find(material.SubjectId);
            ViewBag.Topic = _db.Topics.Find(material.TopicId);
            return View(material);
        }

        [CustomAuthorize("Student")]
        public ActionResult Create()
        {
            ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            ViewBag.Topics = _db.Topics.OrderBy(t => t.Name).ToList();
            return View();
        }

        [HttpPost]
        [CustomAuthorize("Student")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourseMaterial material, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(fileName).ToLower();
                var allowedExtensions = new[] { ".pdf", ".docx", ".txt", ".doc" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Only PDF, DOCX, and TXT files are allowed.");
                    ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
                    ViewBag.Topics = _db.Topics.OrderBy(t => t.Name).ToList();
                    return View(material);
                }

                var uniqueFileName = Guid.NewGuid() + extension;
                var uploadsPath = Server.MapPath("~/Uploads/CourseMaterials");

                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var path = Path.Combine(uploadsPath, uniqueFileName);
                file.SaveAs(path);

                material.FilePath = "/Uploads/CourseMaterials/" + uniqueFileName;
                material.FileType = extension.TrimStart('.').ToUpper();
                material.UploadDate = DateTime.Now;

                _db.CourseMaterials.Add(material);
                _db.SaveChanges();

                TempData["Success"] = "Material uploaded successfully!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Please select a file to upload.");
            ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            ViewBag.Topics = _db.Topics.OrderBy(t => t.Name).ToList();
            return View(material);
        }

        public ActionResult Download(int id)
        {
            var material = _db.CourseMaterials.Find(id);
            if (material == null)
                return HttpNotFound();

            var path = Server.MapPath(material.FilePath);
            if (!System.IO.File.Exists(path))
                return HttpNotFound();

            var mimeType = "application/octet-stream";
            if (material.FileType == "PDF") mimeType = "application/pdf";
            else if (material.FileType == "DOCX" || material.FileType == "DOC") mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if (material.FileType == "TXT") mimeType = "text/plain";

            return File(path, mimeType, material.Title + Path.GetExtension(path));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}