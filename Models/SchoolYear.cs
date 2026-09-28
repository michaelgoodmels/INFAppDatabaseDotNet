using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace INFAppDatabaseDotNet.Models
{
    public class SchoolYear
    {
        public int SchoolYearId { get; set; }

        [Required]
        public int YearNumber { get; set; } // 1, 2, 3, 4

        [Required]
        [StringLength(100)]
        public string Description { get; set; } // z.B. "Lehrjahr 1"

        // Navigation Properties
        public virtual ICollection<Module> Modules { get; set; }

        public SchoolYear()
        {
            Modules = new List<Module>();
        }
    }
}

