using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ThinkBridge.Helpers;
using ThinkBridge.Models;
using ThinkBridge.Services;
using ThinkBridge.ViewModels;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Admin", "Lecturer", "Student")]
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly OpenTurnerAiService _aiService = new OpenTurnerAiService(); [CustomAuthorize("Student")]
        public ActionResult Take(int id)
        {
            var quiz = _db.Quizzes.Find(id);
            if (quiz == null)
                return HttpNotFound();

            var questions = _db.QuizQuestions.Where(q => q.QuizId == id).ToList();
            foreach (var q in questions)
            {
                q.Options = _db.QuizOptions.Where(o => o.QuestionId == q.Id).ToList();
            }

            var viewModel = new TakeQuizViewModel
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                Questions = questions.Select(q => new QuizQuestionViewModel
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options.Select(o => new QuizOptionViewModel { Id = o.Id, OptionText = o.OptionText }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [CustomAuthorize("Student")]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(FormCollection form)
        {
            int quizId = int.Parse(form["QuizId"]);
            var quiz = _db.Quizzes.Find(quizId);
            if (quiz == null)
                return HttpNotFound();

            var studentId = (int)Session["UserId"];
            var questions = _db.QuizQuestions.Where(q => q.QuizId == quizId).ToList();
            int score = 0;
            var questionResults = new List<QuestionResultViewModel>();
            var weakAreaDict = new Dictionary<string, int>();

            var attempt = new QuizAttempt
            {
                StudentId = studentId,
                QuizId = quizId,
                TotalQuestions = questions.Count,
                AttemptDate = DateTime.Now
            };
            _db.QuizAttempts.Add(attempt);
            _db.SaveChanges();

            foreach (var question in questions)
            {
                var selectedOptionId = int.Parse(form[$"question_{question.Id}"]);
                var isCorrect = selectedOptionId == question.CorrectOptionId;

                if (isCorrect)
                    score++;
                else
                {
                    var topicName = quiz.Topic?.Name ?? quiz.Subject?.Name ?? "General";
                    if (!weakAreaDict.ContainsKey(topicName))
                        weakAreaDict[topicName] = 0;
                    weakAreaDict[topicName]++;
                }

                var answer = new StudentAnswer
                {
                    AttemptId = attempt.Id,
                    QuestionId = question.Id,
                    SelectedOptionId = selectedOptionId,
                    IsCorrect = isCorrect
                };
                _db.StudentAnswers.Add(answer);

                var options = _db.QuizOptions.Where(o => o.QuestionId == question.Id).ToList();
                questionResults.Add(new QuestionResultViewModel
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    SelectedOptionId = selectedOptionId,
                    CorrectOptionId = question.CorrectOptionId,
                    IsCorrect = isCorrect,
                    SelectedOptionText = options.FirstOrDefault(o => o.Id == selectedOptionId)?.OptionText,
                    CorrectOptionText = options.FirstOrDefault(o => o.Id == question.CorrectOptionId)?.OptionText
                });
            }

            attempt.Score = score;
            attempt.Percentage = questions.Count > 0 ? (double)score / questions.Count * 100 : 0;
            _db.SaveChanges();

            // Save progress record
            _db.ProgressRecords.Add(new ProgressRecord
            {
                StudentId = studentId,
                SubjectId = quiz.SubjectId,
                TopicId = quiz.TopicId,
                ActivityType = "Quiz",
                ActivityName = quiz.Title,
                Score = (int)attempt.Percentage,
                CompletedAt = DateTime.Now
            });
            _db.SaveChanges();

            // Build weak areas for result view
            var weakAreas = new List<WeakAreaViewModel>();
            foreach (var area in weakAreaDict)
            {
                var suggestedMaterials = _db.CourseMaterials
                    .Where(m => m.Topic.Name == area.Key)
                    .Select(m => m.Title)
                    .Take(3)
                    .ToList();

                var suggestedVideos = _db.VideoLessons
                    .Where(v => v.Topic.Name == area.Key)
                    .Select(v => v.Title)
                    .Take(3)
                    .ToList();

                weakAreas.Add(new WeakAreaViewModel
                {
                    TopicName = area.Key,
                    WrongCount = area.Value,
                    SuggestedMaterials = suggestedMaterials,
                    SuggestedVideos = suggestedVideos
                });
            }

            var resultViewModel = new QuizResultViewModel
            {
                QuizId = quizId,
                QuizTitle = quiz.Title,
                Score = score,
                TotalQuestions = questions.Count,
                Percentage = attempt.Percentage,
                QuestionResults = questionResults,
                WeakAreas = weakAreas
            };
            ViewBag.AttemptId = attempt.Id;

            TempData["AttemptId"] = attempt.Id;
            return View("Result", resultViewModel);
        }

        [CustomAuthorize("Student")]
        public ActionResult ConfusionReport(int attemptId)
        {
            var attempt = _db.QuizAttempts.Find(attemptId);
            if (attempt == null || attempt.StudentId != (int)Session["UserId"])
                return HttpNotFound();

            var quiz = _db.Quizzes.Find(attempt.QuizId);
            var wrongAnswers = _db.StudentAnswers.Where(sa => sa.AttemptId == attemptId && !sa.IsCorrect).ToList();

            var weakAreas = new List<WeakAreaDetailViewModel>();
            var suggestions = new List<SuggestionViewModel>();

            if (wrongAnswers.Any())
            {
                var topicName = quiz.Topic?.Name ?? quiz.Subject?.Name ?? "General";
                weakAreas.Add(new WeakAreaDetailViewModel
                {
                    TopicName = topicName,
                    WrongAnswers = wrongAnswers.Count,
                    TotalQuestions = attempt.TotalQuestions,
                    WeaknessPercentage = attempt.TotalQuestions > 0 ? (double)wrongAnswers.Count / attempt.TotalQuestions * 100 : 0,
                    Status = wrongAnswers.Count > attempt.TotalQuestions / 2 ? "Critical" : "Warning"
                });

                // Add suggestions
                var materials = _db.CourseMaterials.Where(m => m.TopicId == quiz.TopicId).ToList();
                foreach (var mat in materials)
                {
                    suggestions.Add(new SuggestionViewModel
                    {
                        Type = "Note",
                        Title = mat.Title,
                        Description = "Review this material",
                        Link = Url.Action("Details", "CourseMaterials", new { id = mat.Id })
                    });
                }

                var videos = _db.VideoLessons.Where(v => v.TopicId == quiz.TopicId).ToList();
                foreach (var vid in videos)
                {
                    suggestions.Add(new SuggestionViewModel
                    {
                        Type = "Video",
                        Title = vid.Title,
                        Description = "Watch this lesson",
                        Link = Url.Action("Watch", "VideoLessons", new { id = vid.Id })
                    });
                }

                suggestions.Add(new SuggestionViewModel
                {
                    Type = "Quiz",
                    Title = "Practice Quiz",
                    Description = "Take another quiz on this topic",
                    Link = Url.Action("Index", "Quizzes", new { subjectId = quiz.SubjectId })
                });

                suggestions.Add(new SuggestionViewModel
                {
                    Type = "AI",
                    Title = "AI Explanation",
                    Description = "Get AI help with this topic",
                    Link = Url.Action("Index", "AiTutor")
                });
            }

            var report = new ConfusionReportViewModel
            {
                QuizId = attempt.QuizId,
                QuizTitle = quiz.Title,
                Score = attempt.Percentage,
                WeakAreas = weakAreas,
                Suggestions = suggestions
            };

            return View(report);
        }
        public ActionResult Index(int? subjectId, int? topicId)
        {
            var quizzes = _db.Quizzes.AsQueryable();

            if (subjectId.HasValue)
                quizzes = quizzes.Where(q => q.SubjectId == subjectId.Value);

            if (topicId.HasValue)
                quizzes = quizzes.Where(q => q.TopicId == topicId.Value);

            ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();

            ViewBag.Topics = subjectId.HasValue
                ? _db.Topics.Where(t => t.SubjectId == subjectId.Value).OrderBy(t => t.Name).ToList()
                : _db.Topics.OrderBy(t => t.Name).ToList();

            ViewBag.SelectedSubjectId = subjectId;
            ViewBag.SelectedTopicId = topicId;

            var quizList = quizzes.OrderByDescending(q => q.CreatedAt).ToList();

            foreach (var quiz in quizList)
            {
                quiz.Subject = _db.Subjects.Find(quiz.SubjectId);
                quiz.Topic = quiz.TopicId.HasValue ? _db.Topics.Find(quiz.TopicId.Value) : null;
                quiz.Questions = _db.QuizQuestions.Where(q => q.QuizId == quiz.Id).ToList();
            }

            return View(quizList);
        }

        public ActionResult Details(int id)
        {
            var quiz = _db.Quizzes.Find(id);
            if (quiz == null)
                return HttpNotFound();

            var questions = _db.QuizQuestions.Where(q => q.QuizId == id).ToList();
            foreach (var q in questions)
            {
                q.Options = _db.QuizOptions.Where(o => o.QuestionId == q.Id).ToList();
            }
            ViewBag.Questions = questions;
            ViewBag.QuestionCount = questions.Count;

            return View(quiz);
        }

        [CustomAuthorize("Student")]
        public ActionResult Create()
        {
            ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            ViewBag.Topics = _db.Topics.OrderBy(t => t.Name).ToList();

            return View(new Quiz());
        }

        [HttpPost]
        [CustomAuthorize("Student")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                quiz.CreatedAt = DateTime.Now;
                _db.Quizzes.Add(quiz);
                _db.SaveChanges();
                TempData["Success"] = "Quiz created! Now add questions.";
                return RedirectToAction("AddQuestions", new { id = quiz.Id });
            }
            ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            ViewBag.Topics = _db.Topics.OrderBy(t => t.Name).ToList();
            return View(quiz);
        }

        [CustomAuthorize("Student")]
        public ActionResult AddQuestions(int id)
        {
            var quiz = _db.Quizzes.Find(id);
            if (quiz == null)
                return HttpNotFound();

            var questions = _db.QuizQuestions.Where(q => q.QuizId == id).ToList();
            foreach (var q in questions)
            {
                q.Options = _db.QuizOptions.Where(o => o.QuestionId == q.Id).ToList();
            }

            ViewBag.Quiz = quiz;
            ViewBag.ExistingQuestions = questions;
            return View();
        }

        [HttpPost]
        [CustomAuthorize("Student")]
        [ValidateAntiForgeryToken]
        public ActionResult AddQuestions(int id, string questionText, string optionA, string optionB, string optionC, string optionD, string correctOption)
        {
            var quiz = _db.Quizzes.Find(id);
            if (quiz == null)
                return HttpNotFound();

            var question = new QuizQuestion
            {
                QuizId = id,
                QuestionText = questionText
            };
            _db.QuizQuestions.Add(question);
            _db.SaveChanges();

            var options = new List<QuizOption>
            {
                new QuizOption { QuestionId = question.Id, OptionText = optionA },
                new QuizOption { QuestionId = question.Id, OptionText = optionB },
                new QuizOption { QuestionId = question.Id, OptionText = optionC },
                new QuizOption { QuestionId = question.Id, OptionText = optionD }
            };
            _db.QuizOptions.AddRange(options);
            _db.SaveChanges();

            var correctIndex = char.ToUpper(correctOption[0]) - 'A';
            if (correctIndex >= 0 && correctIndex < options.Count)
            {
                question.CorrectOptionId = options[correctIndex].Id;
                _db.SaveChanges();
            }

            TempData["Success"] = "Question added!";
            return RedirectToAction("AddQuestions", new { id = id });
        }
        [HttpPost]
        [CustomAuthorize("Student")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateFromNotes(int quizId, string notes)
        {
            var quiz = _db.Quizzes.Find(quizId);
            if (quiz == null)
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(notes) || notes.Trim().Length < 100)
            {
                TempData["Error"] = "Please paste proper notes before generating questions. The notes are too short.";
                return RedirectToAction("AddQuestions", new { id = quizId });
            }

            var rawJson = await _aiService.GenerateQuizQuestionsAsync(notes, 5);

            if (string.IsNullOrWhiteSpace(rawJson) ||
                rawJson.StartsWith("AI Service Error") ||
                rawJson.StartsWith("Error:"))
            {
                TempData["Error"] = "AI could not generate questions. Check your API key or try again later.";
                return RedirectToAction("AddQuestions", new { id = quizId });
            }

            try
            {
                rawJson = CleanJson(rawJson);

                List<GeneratedQuizQuestionVm> generatedQuestions = null;

                try
                {
                    generatedQuestions =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<List<GeneratedQuizQuestionVm>>(rawJson);
                }
                catch
                {
                    rawJson = rawJson
                        .Replace("\n", "")
                        .Replace("\r", "")
                        .Trim();

                    generatedQuestions =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<List<GeneratedQuizQuestionVm>>(rawJson);
                }

                if (generatedQuestions == null || !generatedQuestions.Any())
                {
                    TempData["Error"] = "AI returned no valid questions.";
                    return RedirectToAction("AddQuestions", new { id = quizId });
                }

                int savedCount = 0;

                foreach (var item in generatedQuestions.Take(5))
                {
                    if (string.IsNullOrWhiteSpace(item.question) ||
                        string.IsNullOrWhiteSpace(item.optionA) ||
                        string.IsNullOrWhiteSpace(item.optionB) ||
                        string.IsNullOrWhiteSpace(item.optionC) ||
                        string.IsNullOrWhiteSpace(item.optionD) ||
                        string.IsNullOrWhiteSpace(item.correctAnswer))
                    {
                        continue;
                    }

                    var correct = item.correctAnswer.Trim().ToUpper();

                    if (!new[] { "A", "B", "C", "D" }.Contains(correct))
                        continue;

                    var question = new QuizQuestion
                    {
                        QuizId = quizId,
                        QuestionText = item.question.Trim()
                    };

                    _db.QuizQuestions.Add(question);
                    _db.SaveChanges();

                    var options = new List<QuizOption>
            {
                new QuizOption { QuestionId = question.Id, OptionText = item.optionA.Trim() },
                new QuizOption { QuestionId = question.Id, OptionText = item.optionB.Trim() },
                new QuizOption { QuestionId = question.Id, OptionText = item.optionC.Trim() },
                new QuizOption { QuestionId = question.Id, OptionText = item.optionD.Trim() }
            };

                    _db.QuizOptions.AddRange(options);
                    _db.SaveChanges();

                    int correctIndex = correct[0] - 'A';
                    question.CorrectOptionId = options[correctIndex].Id;
                    _db.SaveChanges();

                    savedCount++;
                }

                if (savedCount == 0)
                {
                    TempData["Error"] = "AI returned data, but it was not in a usable quiz format.";
                    return RedirectToAction("AddQuestions", new { id = quizId });
                }

                TempData["Success"] = savedCount + " AI-generated questions were added to the quiz.";
                return RedirectToAction("AddQuestions", new { id = quizId });
            }
            catch
            {
                TempData["Error"] = "AI response could not be understood. Try again with clearer notes.";
                TempData["GeneratedQuestions"] = rawJson;
                return RedirectToAction("AddQuestions", new { id = quizId });
            }
        }
        private class GeneratedQuizQuestionVm
        {
            public string question { get; set; }
            public string optionA { get; set; }
            public string optionB { get; set; }
            public string optionC { get; set; }
            public string optionD { get; set; }
            public string correctAnswer { get; set; }
        }
        private string CleanJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = input.Trim();

            if (input.StartsWith("```"))
            {
                input = input.Replace("```json", "")
                             .Replace("```", "")
                             .Trim();
            }

            var start = input.IndexOf("[");
            var end = input.LastIndexOf("]");

            if (start >= 0 && end > start)
                input = input.Substring(start, end - start + 1);

            return input;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}