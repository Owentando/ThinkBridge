using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThinkBridge.Models
{
    public class StudyPlan
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }

        [Required]
        public string Topics { get; set; } // Comma-separated topic names

        [Required]
        [DataType(DataType.Date)]
        public DateTime TestDate { get; set; }

        [Required]
        [Range(0, 100)]
        public int DesiredMark { get; set; }

        [Required]
        public string PlanContent { get; set; } // AI-generated plan

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}