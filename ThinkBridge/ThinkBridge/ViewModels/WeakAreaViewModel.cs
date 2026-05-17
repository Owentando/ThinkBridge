using System.Collections.Generic;

namespace ThinkBridge.ViewModels
{
    public class WeakAreaViewModel
    {
        public string TopicName { get; set; }
        public int WrongCount { get; set; }
        public List<string> SuggestedMaterials { get; set; }
        public List<string> SuggestedVideos { get; set; }
    }
}