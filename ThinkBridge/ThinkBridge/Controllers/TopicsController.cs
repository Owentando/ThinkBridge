using System;
using System.Linq;
using System.Web.Mvc;
using ThinkBridge.Models;
using ThinkBridge.Helpers;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Admin", "Lecturer", "Student")]
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index(int? subjectId)
        {
            var topics = subjectId.HasValue
                ? _db.Topics.Where(t => t.SubjectId == subjectId).ToList()
                : _db.Topics.ToList();

            ViewBag.Subjects = _db.Subjects.ToList();
            ViewBag.SelectedSubjectId = subjectId;
            return View(topics);
        }

        [CustomAuthorize("Lecturer")]
        public ActionResult Create(int subjectId)
        {
            var subject = _db.Subjects.Find(subjectId);
            if (subject == null)
                return HttpNotFound();

            ViewBag.SubjectName = subject.Name;
            return View(new Topic { SubjectId = subjectId });
        }

        [HttpPost]
        [CustomAuthorize("Lecturer")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Topic topic)
        {
            if (ModelState.IsValid)
            {
                topic.CreatedAt = DateTime.Now;
                _db.Topics.Add(topic);
                _db.SaveChanges();
                TempData["Success"] = "Topic created successfully!";
                return RedirectToAction("Details", "Subjects", new { id = topic.SubjectId });
            }
            var subject = _db.Subjects.Find(topic.SubjectId);
            ViewBag.SubjectName = subject?.Name;
            return View(topic);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}