using System.Collections.Generic;

namespace ThinkBridge.ViewModels
{
    public class TakeQuizViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public List<QuizQuestionViewModel> Questions { get; set; }
    }

    public class QuizQuestionViewModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<QuizOptionViewModel> Options { get; set; }
    }

    public class QuizOptionViewModel
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
    }
}