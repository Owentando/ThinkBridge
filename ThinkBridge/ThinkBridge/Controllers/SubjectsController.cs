using System;
using System.Linq;
using System.Web.Mvc;
using ThinkBridge.Models;
using ThinkBridge.Helpers;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Admin", "Lecturer", "Student")]
    public class SubjectsController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            return View(subjects);
        }

        public ActionResult Details(int id)
        {
            var subject = _db.Subjects.Find(id);
            if (subject == null)
                return HttpNotFound();

            var topics = _db.Topics.Where(t => t.SubjectId == id).ToList();
            ViewBag.Topics = topics;
            return View(subject);
        }
        [CustomAuthorize("Student", "Lecturer")]
        public ActionResult Create()
        {
            return View(new Subject());
        }

        [HttpPost]
        [CustomAuthorize("Student")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Subject subject)
        {
            if (ModelState.IsValid)
            {
                var adminUser = _db.Users.FirstOrDefault(u => u.Role == "Admin");

                subject.LecturerId = adminUser != null ? adminUser.Id : (int)Session["UserId"];
                subject.CreatedAt = DateTime.Now;

                _db.Subjects.Add(subject);
                _db.SaveChanges();

                TempData["Success"] = "Subject created successfully!";
                return RedirectToAction("Details", new { id = subject.Id });
            }

            return View(subject);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}