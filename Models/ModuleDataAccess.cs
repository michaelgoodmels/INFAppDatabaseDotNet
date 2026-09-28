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

        /// <summary>
        /// Ruft alle Module nach Lehrjahr ab
        /// </summary>
        public List<Module> GetModulesBySchoolYear(int schoolYearId)
        {
            try
            {
                string query = @"
                    SELECT ModuleId, SchoolYearId, ModuleName, ModuleCode, Description
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
                        Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null
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
                    SELECT ModuleId, SchoolYearId, ModuleName, ModuleCode, Description
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
                        Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null
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
                    SELECT ModuleId, SchoolYearId, ModuleName, ModuleCode, Description
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
                        Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null
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

