using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThinkBridge.Models
{
    public class QuizOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OptionText { get; set; }

        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual QuizQuestion Question { get; set; }
    }
}