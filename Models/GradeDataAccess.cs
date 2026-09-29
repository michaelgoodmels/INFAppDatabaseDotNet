using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace INFAppDatabaseDotNet.Models
{
    public class GradeDataAccess : DataAccess
    {
        public GradeDataAccess(IConfiguration configuration) : base(configuration) { }
        /// <summary>
        /// Ruft alle Noten eines SchÃ¼lers ab
        /// </summary>
        public List<Grade> GetStudentGrades(int studentId)
        {
            try
            {
                string query = @"
                    SELECT GradeId, StudentId, ModuleId, Grade, Grade2, Grade3, EnteredAt, UpdatedAt
                    FROM StudentGrades
                    WHERE StudentId = @StudentId
                    ORDER BY ModuleId";

                DataTable dt = ExecuteQuery(query, new SqlParameter("@StudentId", studentId));
                List<Grade> grades = new List<Grade>();

                foreach (DataRow row in dt.Rows)
                {
                    grades.Add(MapToGrade(row));
                }
                return grades;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der Noten: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft die Note eines SchÃ¼lers fÃ¼r ein Modul ab
        /// </summary>
        public Grade GetStudentGrade(int studentId, int moduleId)
        {
            try
            {
                string query = @"
                    SELECT GradeId, StudentId, ModuleId, Grade, Grade2, Grade3, EnteredAt, UpdatedAt
                    FROM StudentGrades
                    WHERE StudentId = @StudentId AND ModuleId = @ModuleId";

                DataTable dt = ExecuteQuery(query,
                    new SqlParameter("@StudentId", studentId),
                    new SqlParameter("@ModuleId", moduleId));

                if (dt.Rows.Count > 0)
                {
                    return MapToGrade(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der Note: " + ex.Message);
            }
        }

        /// <summary>
        /// Speichert oder aktualisiert eine Note
        /// </summary>
        public int SaveGrade(int studentId, int moduleId, decimal gradeValue)
        {
            try
            {
                // Überprüfe, ob die Note bereits existiert
                Grade existingGrade = GetStudentGrade(studentId, moduleId);

                if (existingGrade != null)
                {
                    // Update
                    string updateQuery = @"
                        UPDATE StudentGrades
                        SET Grade = @Grade, UpdatedAt = GETDATE()
                        WHERE StudentId = @StudentId AND ModuleId = @ModuleId";

                    ExecuteNonQuery(updateQuery,
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@ModuleId", moduleId),
                        new SqlParameter("@Grade", gradeValue));

                    return existingGrade.GradeId;
                }
                else
                {
                    // Insert
                    string insertQuery = @"
                        INSERT INTO StudentGrades (StudentId, ModuleId, Grade, EnteredAt, UpdatedAt)
                        VALUES (@StudentId, @ModuleId, @Grade, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    object result = ExecuteScalar(insertQuery,
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@ModuleId", moduleId),
                        new SqlParameter("@Grade", gradeValue));

                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Speichern der Note: " + ex.Message);
            }
        }

        /// <summary>
        /// Speichert oder aktualisiert 3 Noten pro Modul
        /// </summary>
        public int SaveMultipleGrades(int studentId, int moduleId, decimal? grade1, decimal? grade2, decimal? grade3)
        {
            try
            {
                Grade existingGrade = GetStudentGrade(studentId, moduleId);

                if (existingGrade != null)
                {
                    // Update existing grade
                    string updateQuery = "UPDATE StudentGrades SET Grade = @Grade1, Grade2 = @Grade2, Grade3 = @Grade3, UpdatedAt = GETDATE() WHERE StudentId = @StudentId AND ModuleId = @ModuleId";

                    ExecuteNonQuery(updateQuery,
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@ModuleId", moduleId),
                        new SqlParameter("@Grade1", grade1 ?? (object)DBNull.Value),
                        new SqlParameter("@Grade2", grade2 ?? (object)DBNull.Value),
                        new SqlParameter("@Grade3", grade3 ?? (object)DBNull.Value));

                    System.Diagnostics.Debug.WriteLine($"[GradeDA] Updated grades for Student {studentId}, Module {moduleId}: {grade1}, {grade2}, {grade3}");
                    return existingGrade.GradeId;
                }
                else
                {
                    // Insert new grade
                    string insertQuery = "INSERT INTO StudentGrades (StudentId, ModuleId, Grade, Grade2, Grade3, EnteredAt, UpdatedAt) VALUES (@StudentId, @ModuleId, @Grade1, @Grade2, @Grade3, GETDATE(), GETDATE()); SELECT SCOPE_IDENTITY();";

                    object result = ExecuteScalar(insertQuery,
                        new SqlParameter("@StudentId", studentId),
                        new SqlParameter("@ModuleId", moduleId),
                        new SqlParameter("@Grade1", grade1 ?? (object)DBNull.Value),
                        new SqlParameter("@Grade2", grade2 ?? (object)DBNull.Value),
                        new SqlParameter("@Grade3", grade3 ?? (object)DBNull.Value));

                    System.Diagnostics.Debug.WriteLine($"[GradeDA] Inserted grades for Student {studentId}, Module {moduleId}: {grade1}, {grade2}, {grade3}");
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Speichern der Noten: " + ex.Message);
            }
        }

        /// <summary>
        /// LÃ¶scht eine Note
        /// </summary>
        public bool DeleteGrade(int gradeId)
        {
            try
            {
                string query = @"
                    DELETE FROM StudentGrades
                    WHERE GradeId = @GradeId";

                ExecuteNonQuery(query, new SqlParameter("@GradeId", gradeId));
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim LÃ¶schen der Note: " + ex.Message);
            }
        }

        /// <summary>
        /// Berechnet den Durchschnitt fÃ¼r einen SchÃ¼ler (alle 3 Noten pro Modul)
        /// </summary>
        public decimal GetStudentAverageGrade(int studentId)
        {
            try
            {
                string query = @"
                    SELECT AVG(grade_value) AS AverageGrade
                    FROM (
                        SELECT Grade AS grade_value FROM StudentGrades WHERE StudentId = @StudentId AND Grade IS NOT NULL
                        UNION ALL
                        SELECT Grade2 FROM StudentGrades WHERE StudentId = @StudentId AND Grade2 IS NOT NULL
                        UNION ALL
                        SELECT Grade3 FROM StudentGrades WHERE StudentId = @StudentId AND Grade3 IS NOT NULL
                    ) AS all_grades";

                object result = ExecuteScalar(query, new SqlParameter("@StudentId", studentId));

                if (result != DBNull.Value && result != null)
                {
                    decimal avg = Math.Round(Convert.ToDecimal(result), 1);
                    System.Diagnostics.Debug.WriteLine($"[GradeDA] Average for Student {studentId}: {avg}");
                    return avg;
                }
                System.Diagnostics.Debug.WriteLine($"[GradeDA] No grades found for Student {studentId}");
                return 0m;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Berechnen des Durchschnitts: " + ex.Message);
            }
        }

        /// <summary>
        /// Berechnet den Durchschnitt für ein Lehrjahr (alle 3 Noten pro Modul)
        /// </summary>
        public decimal GetStudentYearAverageGrade(int studentId, int schoolYearId)
        {
            try
            {
                string query = @"
                    SELECT AVG(grade_value) AS AverageGrade
                    FROM (
                        SELECT sg.Grade AS grade_value FROM StudentGrades sg
                        INNER JOIN Modules m ON sg.ModuleId = m.ModuleId
                        WHERE sg.StudentId = @StudentId AND m.SchoolYearId = @SchoolYearId AND sg.Grade IS NOT NULL
                        UNION ALL
                        SELECT sg.Grade2 FROM StudentGrades sg
                        INNER JOIN Modules m ON sg.ModuleId = m.ModuleId
                        WHERE sg.StudentId = @StudentId AND m.SchoolYearId = @SchoolYearId AND sg.Grade2 IS NOT NULL
                        UNION ALL
                        SELECT sg.Grade3 FROM StudentGrades sg
                        INNER JOIN Modules m ON sg.ModuleId = m.ModuleId
                        WHERE sg.StudentId = @StudentId AND m.SchoolYearId = @SchoolYearId AND sg.Grade3 IS NOT NULL
                    ) AS all_grades";

                object result = ExecuteScalar(query,
                    new SqlParameter("@StudentId", studentId),
                    new SqlParameter("@SchoolYearId", schoolYearId));

                if (result != DBNull.Value && result != null)
                {
                    return Math.Round(Convert.ToDecimal(result), 1);
                }
                return 0m;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Berechnen des Jahres-Durchschnitts: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft ein komplettes Noten-ViewModel fÃ¼r einen SchÃ¼ler ab
        /// </summary>
        public StudentModulesViewModel GetStudentModulesViewModel(int studentId)
        {
            try
            {
                StudentDataAccess studentDA = new StudentDataAccess(Configuration);
                ModuleDataAccess moduleDA = new ModuleDataAccess(Configuration);

                Student student = studentDA.GetStudentById(studentId);
                if (student == null)
                    return null;

                StudentModulesViewModel viewModel = new StudentModulesViewModel
                {
                    StudentId = studentId,
                    StudentName = student.FullName
                };

                // Rufe alle Lehrjahre ab
                List<SchoolYear> schoolYears = moduleDA.GetAllSchoolYears();

                foreach (SchoolYear year in schoolYears)
                {
                    SchoolYearModulesViewModel yearViewModel = new SchoolYearModulesViewModel
                    {
                        SchoolYearId = year.SchoolYearId,
                        SchoolYearDescription = year.Description
                    };

                    // Rufe alle Module des Lehrjahrs ab
                    List<Module> modules = moduleDA.GetModulesBySchoolYear(year.SchoolYearId);

                    foreach (Module module in modules)
                    {
                        string gradeQuery = "SELECT GradeId, Grade, Grade2, Grade3 FROM StudentGrades WHERE StudentId = @StudentId AND ModuleId = @ModuleId";
                        DataTable gradeTable = ExecuteQuery(gradeQuery,
                            new SqlParameter("@StudentId", studentId),
                            new SqlParameter("@ModuleId", module.ModuleId));

                        decimal? grade1 = null, grade2 = null, grade3 = null;
                        int gradeId = 0;

                        if (gradeTable.Rows.Count > 0)
                        {
                            DataRow row = gradeTable.Rows[0];
                            gradeId = (int)row["GradeId"];
                            grade1 = row["Grade"] != DBNull.Value ? (decimal?)row["Grade"] : null;
                            grade2 = row["Grade2"] != DBNull.Value ? (decimal?)row["Grade2"] : null;
                            grade3 = row["Grade3"] != DBNull.Value ? (decimal?)row["Grade3"] : null;
                        }

                        yearViewModel.Modules.Add(new StudentGradeViewModel
                        {
                            GradeId = gradeId,
                            ModuleId = module.ModuleId,
                            ModuleName = module.ModuleName,
                            ModuleCode = module.ModuleCode,
                            GradeValue = grade1,
                            Grade2 = grade2,
                            Grade3 = grade3
                        });
                    }

                    // Berechne Jahres-Durchschnitt
                    yearViewModel.YearAverageGrade = GetStudentYearAverageGrade(studentId, year.SchoolYearId);

                    viewModel.SchoolYearModules.Add(yearViewModel);
                }

                // Berechne Gesamtdurchschnitt
                viewModel.AverageGrade = GetStudentAverageGrade(studentId);

                return viewModel;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen des Noten-ViewModels: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft alle Noten aller SchÃ¼ler fÃ¼r Admin-Ansicht ab
        /// </summary>
        public DataTable GetAllStudentGradesForAdmin()
        {
            try
            {
                string query = @"
                    SELECT 
                        s.StudentId,
                        s.FirstName + ' ' + s.LastName AS StudentName,
                        m.ModuleName,
                        sy.Description AS SchoolYear,
                        sg.Grade,
                        sg.UpdatedAt
                    FROM Students s
                    LEFT JOIN StudentGrades sg ON s.StudentId = sg.StudentId
                    LEFT JOIN Modules m ON sg.ModuleId = m.ModuleId
                    LEFT JOIN SchoolYears sy ON m.SchoolYearId = sy.SchoolYearId
                    ORDER BY s.LastName, s.FirstName, sy.YearNumber, m.ModuleName";

                return ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der Admin-Noten: " + ex.Message);
            }
        }

        private Grade MapToGrade(DataRow row)
        {
            return new Grade
            {
                GradeId = (int)row["GradeId"],
                StudentId = (int)row["StudentId"],
                ModuleId = (int)row["ModuleId"],
                GradeValue = row["Grade"] != DBNull.Value ? (decimal?)row["Grade"] : null,
                Grade2Value = row.Table.Columns.Contains("Grade2") && row["Grade2"] != DBNull.Value ? (decimal?)row["Grade2"] : null,
                Grade3Value = row.Table.Columns.Contains("Grade3") && row["Grade3"] != DBNull.Value ? (decimal?)row["Grade3"] : null,
                EnteredAt = (DateTime)row["EnteredAt"],
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
        }
    }
}

