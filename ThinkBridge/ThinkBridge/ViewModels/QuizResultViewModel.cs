using System.Collections.Generic;

namespace ThinkBridge.ViewModels
{
    public class QuizResultViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public List<QuestionResultViewModel> QuestionResults { get; set; }
        public List<WeakAreaViewModel> WeakAreas { get; set; }
    }

    public class QuestionResultViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int SelectedOptionId { get; set; }
        public int CorrectOptionId { get; set; }
        public bool IsCorrect { get; set; }
        public string SelectedOptionText { get; set; }
        public string CorrectOptionText { get; set; }
    }
}