using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ThinkBridge.Models;
using ThinkBridge.Helpers;
using ThinkBridge.Services;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Student")]
    public class AiSummaryController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly OpenTurnerAiService _aiService = new OpenTurnerAiService();

        public ActionResult Index()
        {
            var studentId = (int)Session["UserId"];
            var summaries = _db.AiSummaries
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();
            return View(summaries);
        }

        public ActionResult Create(int? materialId)
        {
            if (materialId.HasValue)
            {
                var material = _db.CourseMaterials.Find(materialId.Value);
                if (material != null)
                {
                    ViewBag.MaterialTitle = material.Title;
                    ViewBag.MaterialId = material.Id;
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(string title, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                ModelState.AddModelError("", "Please enter text to summarize.");
                return View();
            }

            var studentId = (int)Session["UserId"];
            var summary = await _aiService.SummarizeTextAsync(text);

            var aiSummary = new AiSummary
            {
                StudentId = studentId,
                Title = title ?? "Untitled Summary",
                OriginalText = text,
                SummaryText = summary,
                CreatedAt = DateTime.Now
            };

            _db.AiSummaries.Add(aiSummary);
            _db.SaveChanges();

            TempData["Success"] = "Summary generated successfully!";
            return RedirectToAction("Details", new { id = aiSummary.Id });
        }

        public ActionResult Details(int id)
        {
            var studentId = (int)Session["UserId"];
            var summary = _db.AiSummaries.FirstOrDefault(s => s.Id == id && s.StudentId == studentId);
            if (summary == null)
                return HttpNotFound();
            return View(summary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var studentId = (int)Session["UserId"];
            var summary = _db.AiSummaries.FirstOrDefault(s => s.Id == id && s.StudentId == studentId);
            if (summary != null)
            {
                _db.AiSummaries.Remove(summary);
                _db.SaveChanges();
                TempData["Success"] = "Summary deleted.";
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}