using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ThinkBridge.Helpers;
using ThinkBridge.Models;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Student")]
    public class StudyRoomsController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var rooms = _db.StudyRooms
                .Include(r => r.CreatedByStudent)
                .Include(r => r.Subject)
                .Include(r => r.Topic)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(rooms);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Subjects = new SelectList(_db.Subjects.OrderBy(s => s.Name).ToList(), "Id", "Name");
            ViewBag.Topics = new SelectList(_db.Topics.OrderBy(t => t.Name).ToList(), "Id", "Name");

            return View(new StudyRoom());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudyRoom room)
        {
            ViewBag.Subjects = new SelectList(_db.Subjects.OrderBy(s => s.Name).ToList(), "Id", "Name");
            ViewBag.Topics = new SelectList(_db.Topics.OrderBy(t => t.Name).ToList(), "Id", "Name");

            if (!ModelState.IsValid)
                return View(room);

            if (!room.SubjectId.HasValue)
            {
                ModelState.AddModelError("SubjectId", "Please select a subject for this study room.");
                return View(room);
            }

            var existingLiveRoom = _db.StudyRooms
                .Include(r => r.Subject)
                .FirstOrDefault(r =>
                    r.IsLive &&
                    r.SubjectId == room.SubjectId.Value);

            if (existingLiveRoom != null)
            {
                TempData["Error"] =
                    "There is already an active study room for this subject. Please join the existing room instead.";

                return RedirectToAction("Join", new { id = existingLiveRoom.Id });
            }

            int studentId = Convert.ToInt32(Session["UserId"]);

            string roomName = "ThinkBridge-StudyRoom-" + Guid.NewGuid().ToString("N").Substring(0, 10);

            room.CreatedByStudentId = studentId;
            room.MeetingUrl = "https://meet.jit.si/" + roomName;
            room.IsLive = true;
            room.CreatedAt = DateTime.Now;

            _db.StudyRooms.Add(room);
            _db.SaveChanges();

            var creator = _db.Users.Find(studentId);
            var creatorName = creator != null ? creator.FullName : "A student";

            var joinUrl = Url.Action(
                "Join",
                "StudyRooms",
                new { id = room.Id },
                Request.Url.Scheme
            );

            var students = _db.Users
                .Where(u => u.Role == "Student" && u.IsActive && u.Id != studentId)
                .ToList();

            foreach (var student in students)
            {
                if (!string.IsNullOrWhiteSpace(student.Email))
                {
                    string subject = "ThinkBridge Study Room Started: " + room.Title;

                    string body =
                        creatorName + " has started a live study room on ThinkBridge.\n\n" +
                        "Room: " + room.Title + "\n" +
                        "Description: " + room.Description + "\n\n" +
                        "Join the session here:\n" +
                        joinUrl + "\n\n" +
                        "This session is for peer learning, revision, and group discussion.";

                    EmailHelper.SendEmail(student.Email, subject, body);
                }
            }

            TempData["Success"] = "Study room started successfully. Email notifications were sent to other students.";
            return RedirectToAction("Join", new { id = room.Id });
        }

        public ActionResult Join(int id)
        {
            var room = _db.StudyRooms
                .Include(r => r.CreatedByStudent)
                .Include(r => r.Subject)
                .Include(r => r.Topic)
                .FirstOrDefault(r => r.Id == id);

            if (room == null)
                return HttpNotFound();

            ViewBag.RoomUrl = room.MeetingUrl;
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult End(int id)
        {
            var room = _db.StudyRooms.Find(id);
            if (room == null)
                return HttpNotFound();

            room.IsLive = false;
            room.EndedAt = DateTime.Now;
            _db.SaveChanges();

            TempData["Success"] = "Study room ended successfully.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();

            base.Dispose(disposing);
        }
    }
}