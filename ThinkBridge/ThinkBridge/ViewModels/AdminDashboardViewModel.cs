using System.Collections.Generic;
using ThinkBridge.Models;

namespace ThinkBridge.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public List<User> RecentUsers { get; set; }
        public List<Subject> AllSubjects { get; set; }
        public List<ProgressRecord> RecentActivity { get; set; }
    }
}