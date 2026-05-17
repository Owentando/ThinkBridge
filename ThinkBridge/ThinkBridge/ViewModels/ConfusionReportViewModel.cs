using System.Collections.Generic;

namespace ThinkBridge.ViewModels
{
    public class ConfusionReportViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public double Score { get; set; }
        public List<WeakAreaDetailViewModel> WeakAreas { get; set; }
        public List<SuggestionViewModel> Suggestions { get; set; }
    }

    public class WeakAreaDetailViewModel
    {
        public string TopicName { get; set; }
        public int WrongAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double WeaknessPercentage { get; set; }
        public string Status { get; set; } // Critical, Warning, Needs Review
    }

    public class SuggestionViewModel
    {
        public string Type { get; set; } // Note, Video, Quiz, AI
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
    }
}