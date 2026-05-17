using System;
using System.Collections.Generic;
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
    public class VideoLessonsController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly OpenTurnerAiService _aiService = new OpenTurnerAiService();

        public ActionResult Index(int? subjectId, int? topicId)
        {
            var videos = _db.VideoLessons.AsQueryable();

            if (subjectId.HasValue)
                videos = videos.Where(v => v.SubjectId == subjectId);

            if (topicId.HasValue)
                videos = videos.Where(v => v.TopicId == topicId);

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
                ? "online learning tutorial"
                : searchText.Trim();

            ViewBag.DynamicVideoResources = new List<ResourceLinkViewModel>
    {
        new ResourceLinkViewModel
        {
            Title = "YouTube Tutorial Search",
            Url = "https://www.youtube.com/results?search_query=" +
                  HttpUtility.UrlEncode(searchText + " tutorial"),
            Type = "YouTube"
        },
        new ResourceLinkViewModel
        {
            Title = "YouTube Full Course Search",
            Url = "https://www.youtube.com/results?search_query=" +
                  HttpUtility.UrlEncode(searchText + " full course"),
            Type = "Full Course"
        },
        new ResourceLinkViewModel
        {
            Title = "YouTube Beginner Explanation",
            Url = "https://www.youtube.com/results?search_query=" +
                  HttpUtility.UrlEncode(searchText + " explained for beginners"),
            Type = "Beginner"
        },
        new ResourceLinkViewModel
        {
            Title = "YouTube Exam Revision Videos",
            Url = "https://www.youtube.com/results?search_query=" +
                  HttpUtility.UrlEncode(searchText + " exam revision practice"),
            Type = "Revision"
        }
    };

            return View(videos.OrderByDescending(v => v.CreatedAt).ToList());
        }

        public ActionResult Watch(int id)
        {
            var video = _db.VideoLessons.Find(id);
            if (video == null)
                return HttpNotFound();

            ViewBag.Subject = _db.Subjects.Find(video.SubjectId);
            ViewBag.Topic = _db.Topics.Find(video.TopicId);

            // Track progress
            var studentId = (int)Session["UserId"];
            var existingProgress = _db.ProgressRecords.FirstOrDefault(p =>
                p.StudentId == studentId &&
                p.ActivityType == "Lesson" &&
                p.ActivityName == video.Title);

            if (existingProgress == null)
            {
                _db.ProgressRecords.Add(new ProgressRecord
                {
                    StudentId = studentId,
                    SubjectId = video.SubjectId,
                    TopicId = video.TopicId,
                    ActivityType = "Lesson",
                    ActivityName = video.Title,
                    CompletedAt = DateTime.Now
                });
                _db.SaveChanges();
            }

            return View(video);
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
        public ActionResult Create(VideoLesson video)
        {
            if (ModelState.IsValid)
            {
                video.CreatedAt = DateTime.Now;
                _db.VideoLessons.Add(video);
                _db.SaveChanges();
                TempData["Success"] = "Video lesson added successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            ViewBag.Topics = _db.Topics.OrderBy(t => t.Name).ToList();
            return View(video);
        }

        [HttpPost]
        [CustomAuthorize("Student")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateAiNotes(int id)
        {
            var video = _db.VideoLessons.Find(id);
            if (video == null)
                return Json(new { success = false, message = "Video not found." });

            var topic = _db.Topics.Find(video.TopicId);
            var notes = await _aiService.GenerateVideoNotesAsync(
                video.Title,
                video.Description,
                topic?.Name ?? "General"
            );

            // Save to AI summaries
            var summary = new AiSummary
            {
                StudentId = (int)Session["UserId"],
                Title = $"AI Notes: {video.Title}",
                OriginalText = $"Video: {video.Title}\nDescription: {video.Description}",
                SummaryText = notes,
                CreatedAt = DateTime.Now
            };
            _db.AiSummaries.Add(summary);
            _db.SaveChanges();

            return Json(new { success = true, notes = notes });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}