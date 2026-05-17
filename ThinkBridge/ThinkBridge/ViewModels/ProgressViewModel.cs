using System;
using System.Collections.Generic;

namespace ThinkBridge.ViewModels
{
    public class ProgressViewModel
    {
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalQuizzes { get; set; }
        public double AverageScore { get; set; }
        public double BestScore { get; set; }
        public double LowestScore { get; set; }
        public double OverallProgress { get; set; }
        public List<WeakAreaViewModel> WeakAreas { get; set; }
        public List<ProgressRecordViewModel> RecentActivity { get; set; }
        public List<TopicRevisionViewModel> TopicsNeedingRevision { get; set; }
    }

    public class ProgressRecordViewModel
    {
        public string ActivityType { get; set; }
        public string ActivityName { get; set; }
        public int? Score { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class TopicRevisionViewModel
    {
        public string TopicName { get; set; }
        public string SubjectName { get; set; }
        public int WrongCount { get; set; }
    }
}