using System;

namespace ThinkBridge.ViewModels
{
    public class OfflineItemViewModel
    {
        public int Id { get; set; }
        public string ItemType { get; set; }
        public string Title { get; set; }
        public DateTime SavedAt { get; set; }
    }
}