using System.ComponentModel.DataAnnotations;

namespace INFAppDatabaseDotNet.Models
{
    public class Module
    {
        public int ModuleId { get; set; }

        public int SchoolYearId { get; set; }

        [Required]
        [StringLength(200)]
        public string ModuleName { get; set; }

        [Required]
        [StringLength(50)]
        public string ModuleCode { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        // Navigation Properties
        public virtual SchoolYear SchoolYear { get; set; }
    }
}

