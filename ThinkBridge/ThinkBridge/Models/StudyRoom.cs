using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThinkBridge.Models
{
    public class StudyRoom
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int CreatedByStudentId { get; set; }

        public int? SubjectId { get; set; }

        public int? TopicId { get; set; }

        public string MeetingUrl { get; set; }

        public bool IsLive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        [ForeignKey("CreatedByStudentId")]
        public virtual User CreatedByStudent { get; set; }

        public virtual Subject Subject { get; set; }

        public virtual Topic Topic { get; set; }
    }
}