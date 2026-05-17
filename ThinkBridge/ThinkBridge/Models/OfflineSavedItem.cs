using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThinkBridge.Models
{
    public class OfflineSavedItem
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [Required]
        [StringLength(50)]
        public string ItemType { get; set; } // Material, Video, Note

        public int ItemId { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now;
    }
}