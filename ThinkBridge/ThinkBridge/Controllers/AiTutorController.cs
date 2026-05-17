using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ThinkBridge.Models;
using ThinkBridge.ViewModels;
using ThinkBridge.Helpers;
using ThinkBridge.Services;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Student")]
    public class AiTutorController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly OpenTurnerAiService _aiService = new OpenTurnerAiService();

        public ActionResult Index(int? subjectId, int? topicId)
        {
            var studentId = (int)Session["UserId"];
            var messages = _db.ChatMessages
                .Where(m => m.StudentId == studentId)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            ViewBag.Topics = topicId.HasValue ? _db.Topics.Where(t => t.SubjectId == subjectId).ToList() : _db.Topics.ToList();
            ViewBag.SelectedSubjectId = subjectId;
            ViewBag.SelectedTopicId = topicId;

            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Ask(AiTutorAskViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Please enter a question." });

            var studentId = (int)Session["UserId"];
            var subjectName = model.SubjectId.HasValue ? _db.Subjects.Find(model.SubjectId.Value)?.Name : null;
            var topicName = model.TopicId.HasValue ? _db.Topics.Find(model.TopicId.Value)?.Name : null;

            var response = await _aiService.ChatAsync(model.Question, subjectName, topicName);

            var chatMessage = new ChatMessage
            {
                StudentId = studentId,
                SubjectId = model.SubjectId,
                TopicId = model.TopicId,
                Message = model.Question,
                Response = response,
                IsFromAi = true,
                CreatedAt = DateTime.Now
            };
            _db.ChatMessages.Add(chatMessage);
            _db.SaveChanges();

            return Json(new { success = true, response = response, timestamp = DateTime.Now.ToString("g") });
        }

        public ActionResult History()
        {
            var studentId = (int)Session["UserId"];
            var messages = _db.ChatMessages
                .Where(m => m.StudentId == studentId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new AiTutorMessageViewModel
                {
                    Id = m.Id,
                    Message = m.Message,
                    Response = m.Response,
                    IsFromAi = m.IsFromAi,
                    CreatedAt = m.CreatedAt,
                    SubjectName = m.Subject != null ? m.Subject.Name : "General"
                })
                .ToList();

            return View(messages);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}