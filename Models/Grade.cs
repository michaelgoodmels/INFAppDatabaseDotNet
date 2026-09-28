using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace INFAppDatabaseDotNet.Models
{
    public class Grade
    {
        public int GradeId { get; set; }

        public int StudentId { get; set; }

        public int ModuleId { get; set; }

        [Range(1.0, 6.0, ErrorMessage = "Note muss zwischen 1.0 und 6.0 liegen")]
        public decimal? GradeValue { get; set; }

        public DateTime EnteredAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Student Student { get; set; }
        public virtual Module Module { get; set; }
    }

    public class StudentGradeViewModel
    {
        public int GradeId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleCode { get; set; }
        public decimal? GradeValue { get; set; }
    }

    public class StudentModulesViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public List<SchoolYearModulesViewModel> SchoolYearModules { get; set; }
        public decimal AverageGrade { get; set; }

        public StudentModulesViewModel()
        {
            SchoolYearModules = new List<SchoolYearModulesViewModel>();
        }
    }

    public class SchoolYearModulesViewModel
    {
        public int SchoolYearId { get; set; }
        public string SchoolYearDescription { get; set; }
        public List<StudentGradeViewModel> Modules { get; set; }
        public decimal? YearAverageGrade { get; set; }

        public SchoolYearModulesViewModel()
        {
            Modules = new List<StudentGradeViewModel>();
        }
    }
}

