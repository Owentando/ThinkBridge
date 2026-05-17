using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ThinkBridge.Models;
using ThinkBridge.Helpers;
using ThinkBridge.Services;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Student")]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly OpenTurnerAiService _aiService = new OpenTurnerAiService();

        public ActionResult Index(int? subjectId)
        {
            var studentId = (int)Session["UserId"];

            var notes = _db.PersonalNotes
                .Where(n => n.StudentId == studentId);

            if (subjectId.HasValue)
                notes = notes.Where(n => n.SubjectId == subjectId.Value);

            ViewBag.SubjectId = subjectId;
            ViewBag.Subject = subjectId.HasValue ? _db.Subjects.Find(subjectId.Value) : null;

            return View(notes.OrderByDescending(n => n.CreatedAt).ToList());
        }

        public ActionResult Create(int? subjectId)
        {
            ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
            ViewBag.Topics = subjectId.HasValue
                ? _db.Topics.Where(t => t.SubjectId == subjectId.Value).OrderBy(t => t.Name).ToList()
                : _db.Topics.OrderBy(t => t.Name).ToList();

            var note = new PersonalNote
            {
                SubjectId = subjectId
            };

            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PersonalNote note, HttpPostedFileBase uploadedFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
                ViewBag.Topics = note.SubjectId.HasValue
                    ? _db.Topics.Where(t => t.SubjectId == note.SubjectId.Value).OrderBy(t => t.Name).ToList()
                    : _db.Topics.OrderBy(t => t.Name).ToList();

                return View(note);
            }

            var studentId = (int)Session["UserId"];

            note.StudentId = studentId;
            note.CreatedAt = DateTime.Now;

            if (uploadedFile != null && uploadedFile.ContentLength > 0)
            {
                var extension = Path.GetExtension(uploadedFile.FileName).ToLower();

                if (extension != ".txt")
                {
                    ModelState.AddModelError("", "Only TXT files are allowed. Please convert your notes to TXT first.");

                    ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
                    ViewBag.Topics = note.SubjectId.HasValue
                        ? _db.Topics.Where(t => t.SubjectId == note.SubjectId.Value).OrderBy(t => t.Name).ToList()
                        : _db.Topics.OrderBy(t => t.Name).ToList();

                    return View(note);
                }

                using (var reader = new StreamReader(uploadedFile.InputStream))
                {
                    note.Content = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(note.Title))
                    note.Title = Path.GetFileNameWithoutExtension(uploadedFile.FileName);
            }

            if (string.IsNullOrWhiteSpace(note.Content))
            {
                ModelState.AddModelError("", "Please paste notes or upload a TXT file.");

                ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
                ViewBag.Topics = note.SubjectId.HasValue
                    ? _db.Topics.Where(t => t.SubjectId == note.SubjectId.Value).OrderBy(t => t.Name).ToList()
                    : _db.Topics.OrderBy(t => t.Name).ToList();

                return View(note);
            }

            _db.PersonalNotes.Add(note);
            _db.SaveChanges();

            try
            {
                if (note.SubjectId.HasValue)
                {
                    var topicsText = await _aiService.GenerateTopicsFromNotesAsync(note.Content);

                    var topicNames = topicsText
                        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Select(t => t.TrimStart('-', '*', '•').Trim())
                        .Where(t => t.Length > 2)
                        .Where(t => !t.ToLower().StartsWith("ai service error"))
                        .Distinct()
                        .Take(6)
                        .ToList();

                    foreach (var topicName in topicNames)
                    {
                        var exists = _db.Topics.Any(t =>
                            t.SubjectId == note.SubjectId.Value &&
                            t.Name.ToLower() == topicName.ToLower());

                        if (!exists)
                        {
                            _db.Topics.Add(new Topic
                            {
                                Name = topicName,
                                Description = "Auto-generated from uploaded notes.",
                                SubjectId = note.SubjectId.Value
                            });
                        }
                    }

                    _db.SaveChanges();

                    if (!note.TopicId.HasValue && topicNames.Any())
                    {
                        var firstTopicName = topicNames.First();

                        var firstTopic = _db.Topics.FirstOrDefault(t =>
                            t.SubjectId == note.SubjectId.Value &&
                            t.Name.ToLower() == firstTopicName.ToLower());

                        if (firstTopic != null)
                        {
                            note.TopicId = firstTopic.Id;
                            _db.SaveChanges();
                        }
                    }
                }
            }
            catch
            {
                TempData["Error"] = "Note saved, but AI topic detection failed.";
            }

            var keywords = ExtractKeywords(note.Content);
            var suggestedTopics = FindRelatedTopics(keywords);
            var suggestedMaterials = FindRelatedMaterials(keywords);
            var suggestedVideos = FindRelatedVideos(keywords);

            AiSummary createdSummary = null;
            Quiz createdQuiz = null;

            try
            {
                var subject = note.SubjectId.HasValue ? _db.Subjects.Find(note.SubjectId.Value) : null;
                var topic = note.TopicId.HasValue ? _db.Topics.Find(note.TopicId.Value) : null;

                var contextText =
                    "Subject: " + (subject != null ? subject.Name : "General") + "\n" +
                    "Subject Description: " + (subject != null ? subject.Description : "") + "\n" +
                    "Topic: " + (topic != null ? topic.Name : "General") + "\n\n" +
                    note.Content;

                var summaryText = await _aiService.SummarizeTextAsync(contextText);

                createdSummary = new AiSummary
                {
                    StudentId = studentId,
                    Title = "AI Summary: " + note.Title,
                    OriginalText = note.Content,
                    SummaryText = summaryText,
                    CreatedAt = DateTime.Now
                };

                _db.AiSummaries.Add(createdSummary);
                _db.SaveChanges();
            }
            catch
            {
                TempData["Error"] = "Note saved, but AI summary failed. Check your API key/model.";
            }

            try
            {
                createdQuiz = await CreateAiQuizFromNoteAsync(note, studentId);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Note saved, but AI quiz generation failed.";
            }

            TempData["Success"] = "Note saved. Topics, AI summary, and practice quiz were created.";
            TempData["Keywords"] = string.Join(", ", keywords);

            if (suggestedTopics.Any())
                TempData["SuggestedTopicsText"] = string.Join(", ", suggestedTopics.Select(t => t.Name));

            if (suggestedMaterials.Any())
                TempData["SuggestedMaterialsText"] = string.Join(", ", suggestedMaterials.Select(m => m.Title));

            if (suggestedVideos.Any())
                TempData["SuggestedVideosText"] = string.Join(", ", suggestedVideos.Select(v => v.Title));

            if (createdSummary != null)
                TempData["CreatedSummaryId"] = createdSummary.Id;

            if (createdQuiz != null)
                TempData["CreatedQuizId"] = createdQuiz.Id;

            return RedirectToAction("Suggestions", new { id = note.Id });
        }

        public ActionResult Edit(int id)
        {
            var studentId = (int)Session["UserId"];
            var note = _db.PersonalNotes.Find(id);

            if (note == null || note.StudentId != studentId)
                return HttpNotFound();

            ViewBag.Topics = _db.Topics.OrderBy(t => t.Name).ToList();
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PersonalNote note)
        {
            var studentId = (int)Session["UserId"];
            var existingNote = _db.PersonalNotes.Find(note.Id);

            if (existingNote == null || existingNote.StudentId != studentId)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                existingNote.Title = note.Title;
                existingNote.Content = note.Content;
                existingNote.TopicId = note.TopicId;
                existingNote.UpdatedAt = DateTime.Now;

                _db.SaveChanges();

                TempData["Success"] = "Note updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.Topics = _db.Topics.OrderBy(t => t.Name).ToList();
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var studentId = (int)Session["UserId"];
            var note = _db.PersonalNotes.Find(id);

            if (note != null && note.StudentId == studentId)
            {
                _db.PersonalNotes.Remove(note);
                _db.SaveChanges();

                TempData["Success"] = "Note deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        public ActionResult Suggestions(int id)
        {
            var studentId = (int)Session["UserId"];
            var note = _db.PersonalNotes.Find(id);

            if (note == null || note.StudentId != studentId)
                return HttpNotFound();

            var keywords = ExtractKeywords(note.Content);
            var suggestedTopics = FindRelatedTopics(keywords);
            var suggestedMaterials = FindRelatedMaterials(keywords);
            var suggestedVideos = FindRelatedVideos(keywords);

            ViewBag.Keywords = keywords;
            ViewBag.Note = note;
            ViewBag.SuggestedMaterials = suggestedMaterials;
            ViewBag.SuggestedVideos = suggestedVideos;

            return View(suggestedTopics);
        }
        private async Task<Quiz> CreateAiQuizFromNoteAsync(PersonalNote note, int studentId)
        {
            var topic = note.TopicId.HasValue
                ? _db.Topics.Find(note.TopicId.Value)
                : null;

            int subjectId;

            if (topic != null)
            {
                subjectId = topic.SubjectId;
            }
            else if (note.SubjectId.HasValue)
            {
                subjectId = note.SubjectId.Value;
            }
            else
            {
                var fallbackSubject = _db.Subjects.FirstOrDefault();

                if (fallbackSubject == null)
                    throw new Exception("No subject found.");

                subjectId = fallbackSubject.Id;
            }

            var quiz = new Quiz
            {
                Title = "Practice Quiz: " + note.Title,
                Description = "AI-generated quiz from note: " + note.Title,
                SubjectId = subjectId,
                TopicId = note.TopicId,
                CreatedAt = DateTime.Now
            };

            _db.Quizzes.Add(quiz);
            _db.SaveChanges();

            int questionCount = 5;

            if (note.Content.Length > 1500)
                questionCount = 10;

            if (note.Content.Length > 3500)
                questionCount = 15;

            if (note.Content.Length > 6000)
                questionCount = 20;

            var prompt = $@"
Generate {questionCount} multiple choice quiz questions from these study notes.

RULES:
- Return ONLY valid JSON
- No markdown
- No explanation
- 4 options per question
- One correct answer
- Questions must test understanding
- Questions must be educational and accurate

JSON FORMAT:

[
  {{
    ""question"": ""Question text"",
    ""optionA"": ""Option A"",
    ""optionB"": ""Option B"",
    ""optionC"": ""Option C"",
    ""optionD"": ""Option D"",
    ""correctAnswer"": ""A""
  }}
]

NOTES:
{note.Content}
";

            var rawJson = await _aiService.GenerateQuizQuestionsAsync(note.Content, questionCount);

            if (string.IsNullOrWhiteSpace(rawJson) ||
                rawJson.StartsWith("AI Service Error") ||
                rawJson.StartsWith("Error:"))
            {
                throw new Exception("AI generation failed.");
            }

            rawJson = rawJson.Trim();

            if (rawJson.StartsWith("```json"))
                rawJson = rawJson.Replace("```json", "");

            if (rawJson.StartsWith("```"))
                rawJson = rawJson.Replace("```", "");

            if (rawJson.EndsWith("```"))
                rawJson = rawJson.Replace("```", "");

            rawJson = rawJson
                .Replace("\n", "")
                .Replace("\r", "")
                .Trim();

            var questions =
                Newtonsoft.Json.JsonConvert.DeserializeObject<List<AiQuizQuestion>>(rawJson);

            if (questions == null || !questions.Any())
                throw new Exception("AI returned invalid questions.");

            foreach (var aiQuestion in questions)
            {
                var question = new QuizQuestion
                {
                    QuizId = quiz.Id,
                    QuestionText = aiQuestion.question
                };

                _db.QuizQuestions.Add(question);
                _db.SaveChanges();

                var options = new List<QuizOption>
        {
            new QuizOption
            {
                QuestionId = question.Id,
                OptionText = aiQuestion.optionA
            },
            new QuizOption
            {
                QuestionId = question.Id,
                OptionText = aiQuestion.optionB
            },
            new QuizOption
            {
                QuestionId = question.Id,
                OptionText = aiQuestion.optionC
            },
            new QuizOption
            {
                QuestionId = question.Id,
                OptionText = aiQuestion.optionD
            }
        };

                _db.QuizOptions.AddRange(options);
                _db.SaveChanges();

                switch (aiQuestion.correctAnswer.ToUpper())
                {
                    case "A":
                        question.CorrectOptionId = options[0].Id;
                        break;

                    case "B":
                        question.CorrectOptionId = options[1].Id;
                        break;

                    case "C":
                        question.CorrectOptionId = options[2].Id;
                        break;

                    case "D":
                        question.CorrectOptionId = options[3].Id;
                        break;

                    default:
                        question.CorrectOptionId = options[0].Id;
                        break;
                }

                _db.SaveChanges();
            }

            return quiz;
        }

        //private Quiz CreateSimpleQuizFromNote(PersonalNote note, int studentId)
        //{
        //    var topic = note.TopicId.HasValue ? _db.Topics.Find(note.TopicId.Value) : null;

        //    int subjectId;

        //    if (topic != null)
        //    {
        //        subjectId = topic.SubjectId;
        //    }
        //    else
        //    {
        //        var fallbackSubject = _db.Subjects.FirstOrDefault();

        //        if (fallbackSubject == null)
        //            throw new Exception("No subject exists. Please seed or create at least one subject first.");

        //        subjectId = fallbackSubject.Id;
        //    }

        //    var quiz = new Quiz
        //    {
        //        Title = "Practice Quiz: " + note.Title,
        //        Description = "Auto-created from student note: " + note.Title,
        //        SubjectId = subjectId,
        //        TopicId = note.TopicId,
        //        CreatedAt = DateTime.Now
        //    };

        //    _db.Quizzes.Add(quiz);
        //    _db.SaveChanges();

        //    var keywords = ExtractKeywords(note.Content);

        //    if (!keywords.Any())
        //    {
        //        keywords.Add("main idea");
        //        keywords.Add("important concept");
        //        keywords.Add("revision");
        //    }

        //    var quizKeywords = keywords.Take(5).ToList();

        //    foreach (var keyword in quizKeywords)
        //    {
        //        var question = new QuizQuestion
        //        {
        //            QuizId = quiz.Id,
        //            QuestionText = "Which concept from your note is most related to \"" + keyword + "\"?"
        //        };

        //        _db.QuizQuestions.Add(question);
        //        _db.SaveChanges();

        //        var options = new List<QuizOption>
        //        {
        //            new QuizOption { QuestionId = question.Id, OptionText = keyword },
        //            new QuizOption { QuestionId = question.Id, OptionText = "Unrelated concept" },
        //            new QuizOption { QuestionId = question.Id, OptionText = "Random example" },
        //            new QuizOption { QuestionId = question.Id, OptionText = "None of the above" }
        //        };

        //        _db.QuizOptions.AddRange(options);
        //        _db.SaveChanges();

        //        question.CorrectOptionId = options[0].Id;
        //        _db.SaveChanges();
        //    }

        //    return quiz;
        //}

        private List<string> ExtractKeywords(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<string>();

            var commonWords = new[]
            {
                "the","and","for","are","but","not","you","all","can","was","one","our","out",
                "has","his","how","its","may","new","now","old","see","two","who","did","she",
                "use","way","many","any","say","try","ask","end","why","let","put","own","tell",
                "very","when","much","would","there","their","what","said","each","which","will",
                "about","could","other","after","first","never","these","think","where","being",
                "every","great","might","still","those","while","this","that","with","have","from",
                "they","been","were","time","than","them","into","just","like","over","also","back",
                "only","know","take","good","some","come","make","well","work","even","more","want",
                "here","look","down","most","long","find","give","does","made","part","such","need"
            };

            return content.ToLower()
                .Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\n', '\r', '\t', '-', '_', '/', '\\', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3 && !commonWords.Contains(w))
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => g.Key)
                .ToList();
        }

        private List<Topic> FindRelatedTopics(List<string> keywords)
        {
            var allTopics = _db.Topics.ToList();
            var related = new List<Topic>();

            foreach (var topic in allTopics)
            {
                var topicText = ((topic.Name ?? "") + " " + (topic.Description ?? "")).ToLower();

                if (keywords.Any(k => topicText.Contains(k)))
                    related.Add(topic);
            }

            return related.Take(5).ToList();
        }

        private List<CourseMaterial> FindRelatedMaterials(List<string> keywords)
        {
            var allMaterials = _db.CourseMaterials.ToList();
            var related = new List<CourseMaterial>();

            foreach (var material in allMaterials)
            {
                var materialText = ((material.Title ?? "") + " " + (material.Description ?? "")).ToLower();

                if (keywords.Any(k => materialText.Contains(k)))
                    related.Add(material);
            }

            return related.Take(5).ToList();
        }

        private List<VideoLesson> FindRelatedVideos(List<string> keywords)
        {
            var allVideos = _db.VideoLessons.ToList();
            var related = new List<VideoLesson>();

            foreach (var video in allVideos)
            {
                var videoText = ((video.Title ?? "") + " " + (video.Description ?? "")).ToLower();

                if (keywords.Any(k => videoText.Contains(k)))
                    related.Add(video);
            }

            return related.Take(5).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();

            base.Dispose(disposing);
        }
    }
}