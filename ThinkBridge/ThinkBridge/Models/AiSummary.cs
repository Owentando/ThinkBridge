using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThinkBridge.Models
{
    public class AiSummary
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string OriginalText { get; set; }

        [Required]
        public string SummaryText { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}