using System;
using System.ComponentModel.DataAnnotations;

namespace ThinkBridge.ViewModels
{
    public class AiTutorAskViewModel
    {
        [Required]
        public string Question { get; set; }
        public int? SubjectId { get; set; }
        public int? TopicId { get; set; }
    }

    public class AiTutorMessageViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public bool IsFromAi { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SubjectName { get; set; }
    }
}