using System;
using System.ComponentModel.DataAnnotations;

namespace ThinkBridge.ViewModels
{
    public class CreateStudyPlanViewModel
    {
        [Required]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        [Required]
        [Display(Name = "Topics to Cover")]
        public string Topics { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Test Date")]
        public DateTime TestDate { get; set; }

        [Required]
        [Range(0, 100)]
        [Display(Name = "Desired Mark (%)")]
        public int DesiredMark { get; set; }
    }

    public class StudyPlanViewModel
    {
        public int Id { get; set; }
        public string SubjectName { get; set; }
        public string Topics { get; set; }
        public DateTime TestDate { get; set; }
        public int DesiredMark { get; set; }
        public string PlanContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}