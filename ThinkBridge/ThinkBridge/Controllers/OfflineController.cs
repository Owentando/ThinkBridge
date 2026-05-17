using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ThinkBridge.Helpers;
using ThinkBridge.Models;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Student")]
    public class OfflineController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var studentId = (int)Session["UserId"];
            var items = _db.OfflineSavedItems
                .Where(i => i.StudentId == studentId)
                .OrderByDescending(i => i.SavedAt)
                .ToList();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(string itemType, int itemId, string title)
        {
            var studentId = (int)Session["UserId"];

            // Check if already saved
            var existing = _db.OfflineSavedItems.FirstOrDefault(i =>
                i.StudentId == studentId && i.ItemType == itemType && i.ItemId == itemId);

            if (existing != null)
            {
                return Json(new { success = false, message = "Item already saved for offline study." });
            }

            var item = new OfflineSavedItem
            {
                StudentId = studentId,
                ItemType = itemType,
                ItemId = itemId,
                Title = title,
                SavedAt = DateTime.Now
            };

            _db.OfflineSavedItems.Add(item);
            _db.SaveChanges();

            return Json(new { success = true, message = "Saved for offline study!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            var studentId = (int)Session["UserId"];
            var item = _db.OfflineSavedItems.FirstOrDefault(i => i.Id == id && i.StudentId == studentId);
            if (item != null)
            {
                _db.OfflineSavedItems.Remove(item);
                _db.SaveChanges();
                TempData["Success"] = "Item removed from offline collection.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult StudyPage()
        {
            var studentId = (int)Session["UserId"];
            var items = _db.OfflineSavedItems
                .Where(i => i.StudentId == studentId)
                .OrderBy(i => i.ItemType)
                .ThenByDescending(i => i.SavedAt)
                .ToList();

            // Get actual content for each saved item
            var materials = new List<CourseMaterial>();
            var videos = new List<VideoLesson>();
            var notes = new List<PersonalNote>();

            foreach (var item in items)
            {
                switch (item.ItemType)
                {
                    case "Material":
                        var material = _db.CourseMaterials.Find(item.ItemId);
                        if (material != null) materials.Add(material);
                        break;
                    case "Video":
                        var video = _db.VideoLessons.Find(item.ItemId);
                        if (video != null) videos.Add(video);
                        break;
                    case "Note":
                        var note = _db.PersonalNotes.Find(item.ItemId);
                        if (note != null) notes.Add(note);
                        break;
                }
            }

            ViewBag.Materials = materials;
            ViewBag.Videos = videos;
            ViewBag.Notes = notes;
            ViewBag.TotalItems = items.Count;

            return View(items);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}