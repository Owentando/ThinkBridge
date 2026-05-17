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
    public class StudyPlannerController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly OpenTurnerAiService _aiService = new OpenTurnerAiService();

        public ActionResult Index()
        {
            var studentId = (int)Session["UserId"];
            var plans = _db.StudyPlans
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
            return View(plans);
        }
        [HttpGet]
        [CustomAuthorize("Student")]
        public ActionResult Create()
        {
            ViewBag.Subjects = _db.Subjects
                .OrderBy(s => s.Name)
                .ToList();

            return View(new CreateStudyPlanViewModel
            {
                TestDate = DateTime.Now.AddDays(7),
                DesiredMark = 75
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateStudyPlanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
                return View(model);
            }
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");
            var studentId = (int)Session["UserId"];
            var subject = _db.Subjects.Find(model.SubjectId);

            // Get weak areas from database
            var weakAreas = _db.StudentAnswers
                .Where(sa => sa.Attempt.StudentId == studentId && !sa.IsCorrect)
                .Select(sa => sa.Question.Quiz.Topic.Name)
                .Distinct()
                .ToList();

            var weakAreasText = weakAreas.Any() ? string.Join(", ", weakAreas) : "No specific weak areas detected";

            // Get quiz scores for context
            var quizScores = _db.QuizAttempts
                .Where(a => a.StudentId == studentId && a.Quiz.SubjectId == model.SubjectId)
                .Select(a => a.Percentage)
                .ToList();

            var avgScore = quizScores.Any() ? quizScores.Average() : 0;

            var planContent = await _aiService.GenerateStudyPlanAsync(
                subject.Name,
                model.Topics,
                model.TestDate,
                model.DesiredMark,
                $"Weak areas: {weakAreasText}. Current average: {avgScore:F1}%"
            );

            var studyPlan = new StudyPlan
            {
                StudentId = studentId,
                SubjectId = model.SubjectId,
                Topics = model.Topics,
                TestDate = model.TestDate,
                DesiredMark = model.DesiredMark,
                PlanContent = planContent,
                CreatedAt = DateTime.Now
            };

            _db.StudyPlans.Add(studyPlan);
            _db.SaveChanges();

            TempData["Success"] = "Study plan generated successfully!";
            return RedirectToAction("Details", new { id = studyPlan.Id });
        }

        public ActionResult Details(int id)
        {
            var studentId = (int)Session["UserId"];
            var plan = _db.StudyPlans.Find(id);
            if (plan == null || plan.StudentId != studentId)
                return HttpNotFound();

            ViewBag.Subject = _db.Subjects.Find(plan.SubjectId);
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var studentId = (int)Session["UserId"];
            var plan = _db.StudyPlans.Find(id);
            if (plan != null && plan.StudentId == studentId)
            {
                _db.StudyPlans.Remove(plan);
                _db.SaveChanges();
                TempData["Success"] = "Study plan deleted.";
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