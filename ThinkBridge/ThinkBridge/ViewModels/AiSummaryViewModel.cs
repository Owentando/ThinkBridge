using System;

namespace ThinkBridge.ViewModels
{
    public class AiSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string OriginalText { get; set; }
        public string SummaryText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}