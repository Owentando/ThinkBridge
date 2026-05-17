using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThinkBridge.Models
{
    public class ProgressRecord
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        public int? SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }

        public int? TopicId { get; set; }

        [ForeignKey("TopicId")]
        public virtual Topic Topic { get; set; }

        [StringLength(50)]
        public string ActivityType { get; set; } // Lesson, Quiz, Note, etc.

        [StringLength(200)]
        public string ActivityName { get; set; }

        public int? Score { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;
    }
}