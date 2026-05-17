using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThinkBridge.Models
{
    public class StudentAnswer
    {
        [Key]
        public int Id { get; set; }

        public int AttemptId { get; set; }

        [ForeignKey("AttemptId")]
        public virtual QuizAttempt Attempt { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual QuizQuestion Question { get; set; }

        public int SelectedOptionId { get; set; }
        public bool IsCorrect { get; set; }
    }
}