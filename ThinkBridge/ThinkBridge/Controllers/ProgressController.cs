using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ThinkBridge.Models;
using ThinkBridge.ViewModels;
using ThinkBridge.Helpers;

namespace ThinkBridge.Controllers
{
    [CustomAuthorize("Student")]
    public class ProgressController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var studentId = (int)Session["UserId"];

            var totalLessons = _db.VideoLessons.Count();
            var completedLessons = _db.ProgressRecords.Count(p => p.StudentId == studentId && p.ActivityType == "Lesson");
            var quizAttempts = _db.QuizAttempts.Where(a => a.StudentId == studentId).ToList();
            var averageScore = quizAttempts.Any() ? quizAttempts.Average(a => a.Percentage) : 0;
            var bestScore = quizAttempts.Any() ? quizAttempts.Max(a => a.Percentage) : 0;
            var lowestScore = quizAttempts.Any() ? quizAttempts.Min(a => a.Percentage) : 0;
            var overallProgress = totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0;

            // Calculate weak areas
            var weakAreas = CalculateWeakAreas(studentId);

            var recentActivity = _db.ProgressRecords
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.CompletedAt)
                .Take(20)
                .Select(p => new ProgressRecordViewModel
                {
                    ActivityType = p.ActivityType,
                    ActivityName = p.ActivityName,
                    Score = p.Score,
                    CompletedAt = p.CompletedAt
                })
                .ToList();

            var topicsNeedingRevision = CalculateTopicsNeedingRevision(studentId);

            var viewModel = new ProgressViewModel
            {
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                TotalQuizzes = quizAttempts.Count,
                AverageScore = averageScore,
                BestScore = bestScore,
                LowestScore = lowestScore,
                OverallProgress = overallProgress,
                WeakAreas = weakAreas,
                RecentActivity = recentActivity,
                TopicsNeedingRevision = topicsNeedingRevision
            };

            return View(viewModel);
        }

        private List<WeakAreaViewModel> CalculateWeakAreas(int studentId)
        {
            var weakAreas = new List<WeakAreaViewModel>();

            var wrongAnswers = _db.StudentAnswers
                .Where(sa => sa.Attempt.StudentId == studentId && !sa.IsCorrect)
                .ToList();

            if (!wrongAnswers.Any())
                return weakAreas;

            var topicGroups = wrongAnswers
                .GroupBy(sa => sa.Question.Quiz.Topic != null ? sa.Question.Quiz.Topic.Name : (sa.Question.Quiz.Subject != null ? sa.Question.Quiz.Subject.Name : "General"))
                .Select(g => new { Topic = g.Key, Count = g.Count() })
                .Where(g => g.Count > 0)
                .OrderByDescending(g => g.Count)
                .ToList();

            foreach (var group in topicGroups)
            {
                var suggestedMaterials = _db.CourseMaterials
                    .Where(m => m.Topic != null && m.Topic.Name == group.Topic)
                    .Select(m => m.Title)
                    .Take(3)
                    .ToList();

                var suggestedVideos = _db.VideoLessons
                    .Where(v => v.Topic != null && v.Topic.Name == group.Topic)
                    .Select(v => v.Title)
                    .Take(3)
                    .ToList();

                weakAreas.Add(new WeakAreaViewModel
                {
                    TopicName = group.Topic,
                    WrongCount = group.Count,
                    SuggestedMaterials = suggestedMaterials,
                    SuggestedVideos = suggestedVideos
                });
            }

            return weakAreas;
        }

        private List<TopicRevisionViewModel> CalculateTopicsNeedingRevision(int studentId)
        {
            var revisionTopics = new List<TopicRevisionViewModel>();

            var wrongAnswers = _db.StudentAnswers
                .Where(sa => sa.Attempt.StudentId == studentId && !sa.IsCorrect)
                .ToList();

            if (!wrongAnswers.Any())
                return revisionTopics;

            var topicGroups = wrongAnswers
                .GroupBy(sa => sa.Question.Quiz.Topic != null ? sa.Question.Quiz.Topic.Name : (sa.Question.Quiz.Subject != null ? sa.Question.Quiz.Subject.Name : "General"))
                .Select(g => new { Topic = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            foreach (var group in topicGroups)
            {
                var subjectName = _db.Topics
                    .Where(t => t.Name == group.Topic)
                    .Select(t => t.Subject.Name)
                    .FirstOrDefault() ?? "General";

                revisionTopics.Add(new TopicRevisionViewModel
                {
                    TopicName = group.Topic,
                    SubjectName = subjectName,
                    WrongCount = group.Count
                });
            }

            return revisionTopics;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}