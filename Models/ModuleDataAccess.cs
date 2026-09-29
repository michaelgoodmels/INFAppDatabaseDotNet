using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace INFAppDatabaseDotNet.Models
{
    public class ModuleDataAccess : DataAccess
    {
        public ModuleDataAccess(IConfiguration configuration) : base(configuration) { }
        /// <summary>
        /// Ruft alle Lehrjahre ab
        /// </summary>
        public List<SchoolYear> GetAllSchoolYears()
        {
            try
            {
                string query = @"
                    SELECT SchoolYearId, YearNumber, Description
                    FROM SchoolYears
                    ORDER BY YearNumber";

                DataTable dt = ExecuteQuery(query);
                List<SchoolYear> schoolYears = new List<SchoolYear>();

                foreach (DataRow row in dt.Rows)
                {
                    schoolYears.Add(new SchoolYear
                    {
                        SchoolYearId = (int)row["SchoolYearId"],
                        YearNumber = (int)row["YearNumber"],
                        Description = row["Description"].ToString()
                    });
                }
                return schoolYears;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der Lehrjahre: " + ex.Message);
            }
        }

        public bool UpdateModuleGradeCount(int moduleId, int gradeCount)
        {
            try
            {
                if (gradeCount < 1 || gradeCount > 5)
                    throw new Exception("Notenanzahl muss zwischen 1 und 5 liegen");

                // Get current GradeCount to check if we need to clean up grades
                string selectQuery = "SELECT GradeCount FROM Modules WHERE ModuleId = @ModuleId";
                object result = ExecuteScalar(selectQuery, new SqlParameter("@ModuleId", moduleId));
                int currentGradeCount = result != null ? Convert.ToInt32(result) : 3;

                // Update the module
                string updateModuleQuery = @"
                    UPDATE Modules
                    SET GradeCount = @GradeCount
                    WHERE ModuleId = @ModuleId";

                ExecuteNonQuery(updateModuleQuery,
                    new SqlParameter("@ModuleId", moduleId),
                    new SqlParameter("@GradeCount", gradeCount));

                // If GradeCount is reduced, clear the extra grades for all students
                if (gradeCount < currentGradeCount)
                {
                    string cleanupQuery = "UPDATE StudentGrades SET ";
                    List<string> updateStatements = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    // Add NULL assignments for grades that exceed the new count
                    if (gradeCount < 5 && currentGradeCount >= 5)
                        updateStatements.Add("Grade5 = NULL");
                    if (gradeCount < 4 && currentGradeCount >= 4)
                        updateStatements.Add("Grade4 = NULL");
                    if (gradeCount < 3 && currentGradeCount >= 3)
                        updateStatements.Add("Grade3 = NULL");
                    if (gradeCount < 2 && currentGradeCount >= 2)
                        updateStatements.Add("Grade2 = NULL");

                    if (updateStatements.Count > 0)
                    {
                        cleanupQuery += string.Join(", ", updateStatements);
                        cleanupQuery += " WHERE ModuleId = @ModuleId";

                        parameters.Add(new SqlParameter("@ModuleId", moduleId));
                        ExecuteNonQuery(cleanupQuery, parameters.ToArray());
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Aktualisieren der Notenanzahl: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft alle Module nach Lehrjahr ab
        /// </summary>
        public List<Module> GetModulesBySchoolYear(int schoolYearId)
        {
            try
            {
                string query = @"
                    SELECT ModuleId, SchoolYearId, ModuleName, ModuleCode, Description, ISNULL(GradeCount, 3) AS GradeCount
                    FROM Modules
                    WHERE SchoolYearId = @SchoolYearId
                    ORDER BY ModuleName";

                DataTable dt = ExecuteQuery(query, new SqlParameter("@SchoolYearId", schoolYearId));
                List<Module> modules = new List<Module>();

                foreach (DataRow row in dt.Rows)
                {
                    modules.Add(new Module
                    {
                        ModuleId = (int)row["ModuleId"],
                        SchoolYearId = (int)row["SchoolYearId"],
                        ModuleName = row["ModuleName"].ToString(),
                        ModuleCode = row["ModuleCode"].ToString(),
                        Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null,
                        GradeCount = (int)row["GradeCount"]
                    });
                }
                return modules;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der Module: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft ein Modul nach ID ab
        /// </summary>
        public Module GetModuleById(int moduleId)
        {
            try
            {
                string query = @"
                    SELECT ModuleId, SchoolYearId, ModuleName, ModuleCode, Description, ISNULL(GradeCount, 3) AS GradeCount
                    FROM Modules
                    WHERE ModuleId = @ModuleId";

                DataTable dt = ExecuteQuery(query, new SqlParameter("@ModuleId", moduleId));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new Module
                    {
                        ModuleId = (int)row["ModuleId"],
                        SchoolYearId = (int)row["SchoolYearId"],
                        ModuleName = row["ModuleName"].ToString(),
                        ModuleCode = row["ModuleCode"].ToString(),
                        Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null,
                        GradeCount = (int)row["GradeCount"]
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen des Moduls: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft alle Module ab
        /// </summary>
        public List<Module> GetAllModules()
        {
            try
            {
                string query = @"
                    SELECT ModuleId, SchoolYearId, ModuleName, ModuleCode, Description, ISNULL(GradeCount, 3) AS GradeCount
                    FROM Modules
                    ORDER BY SchoolYearId, ModuleName";

                DataTable dt = ExecuteQuery(query);
                List<Module> modules = new List<Module>();

                foreach (DataRow row in dt.Rows)
                {
                    modules.Add(new Module
                    {
                        ModuleId = (int)row["ModuleId"],
                        SchoolYearId = (int)row["SchoolYearId"],
                        ModuleName = row["ModuleName"].ToString(),
                        ModuleCode = row["ModuleCode"].ToString(),
                        Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null,
                        GradeCount = (int)row["GradeCount"]
                    });
                }
                return modules;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der Module: " + ex.Message);
            }
        }
    }
}

