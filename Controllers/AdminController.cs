using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using INFAppDatabaseDotNet.Models;

namespace INFAppDatabaseDotNet.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly StudentDataAccess _studentDA;
        private readonly GradeDataAccess _gradeDA;
        private readonly ModuleDataAccess _moduleDA;

        public AdminController(StudentDataAccess studentDA, GradeDataAccess gradeDA, ModuleDataAccess moduleDA)
        {
            _studentDA = studentDA;
            _gradeDA = gradeDA;
            _moduleDA = moduleDA;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Session.GetString("UserId") == null || HttpContext.Session.GetString("Role") != "Admin")
            {
                filterContext.Result = RedirectToAction("Index", "Login");
            }
            base.OnActionExecuting(filterContext);
        }

        public IActionResult Dashboard()
        {
            try
            {
                List<Student> students = _studentDA.GetAllStudents();
                return View(students);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Fehler beim Laden des Admin-Dashboards: " + ex.Message;
                return View(new List<Student>());
            }
        }

        public IActionResult StudentDetails(int studentId)
        {
            try
            {
                Student student = _studentDA.GetStudentById(studentId);
                if (student == null)
                {
                    return NotFound();
                }

                StudentModulesViewModel viewModel = _gradeDA.GetStudentModulesViewModel(studentId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Fehler beim Laden der SchÃ¼lerdetails: " + ex.Message;
                return View();
            }
        }

        public IActionResult AllGrades()
        {
            try
            {
                DataTable gradesTable = _gradeDA.GetAllStudentGradesForAdmin();
                return View(gradesTable);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Fehler beim Laden der Noten: " + ex.Message;
                return View(new DataTable());
            }
        }

        /// <summary>
        /// Admin-Endpunkt zum Bearbeiten von Schülernoten (3 Noten pro Modul)
        /// </summary>
        [HttpPost]
        public IActionResult EditGrades(int studentId, int moduleId, string grade1, string grade2, string grade3, string grade4 = "", string grade5 = "")
        {
            try
            {
                decimal? p1 = null, p2 = null, p3 = null, p4 = null, p5 = null;

                if (!string.IsNullOrEmpty(grade1) && decimal.TryParse(grade1.Replace(",", "."), out decimal g1) && g1 >= 1.0m && g1 <= 6.0m) p1 = g1;
                else if (!string.IsNullOrEmpty(grade1)) return Json(new { success = false, message = "Note ungültig" });

                if (!string.IsNullOrEmpty(grade2) && decimal.TryParse(grade2.Replace(",", "."), out decimal g2) && g2 >= 1.0m && g2 <= 6.0m) p2 = g2;
                else if (!string.IsNullOrEmpty(grade2)) return Json(new { success = false, message = "Note ungültig" });

                if (!string.IsNullOrEmpty(grade3) && decimal.TryParse(grade3.Replace(",", "."), out decimal g3) && g3 >= 1.0m && g3 <= 6.0m) p3 = g3;
                else if (!string.IsNullOrEmpty(grade3)) return Json(new { success = false, message = "Note ungültig" });

                if (!string.IsNullOrEmpty(grade4) && decimal.TryParse(grade4.Replace(",", "."), out decimal g4) && g4 >= 1.0m && g4 <= 6.0m) p4 = g4;
                else if (!string.IsNullOrEmpty(grade4)) return Json(new { success = false, message = "Note ungültig" });

                if (!string.IsNullOrEmpty(grade5) && decimal.TryParse(grade5.Replace(",", "."), out decimal g5) && g5 >= 1.0m && g5 <= 6.0m) p5 = g5;
                else if (!string.IsNullOrEmpty(grade5)) return Json(new { success = false, message = "Note ungültig" });

                if (p1 == null && p2 == null && p3 == null && p4 == null && p5 == null)
                    return Json(new { success = false, message = "Mindestens eine Note erforderlich" });

                _gradeDA.SaveMultipleGrades(studentId, moduleId, p1, p2, p3, p4, p5);

                StudentModulesViewModel viewModel = _gradeDA.GetStudentModulesViewModel(studentId);
                return Json(new { success = true, averageGrade = viewModel.AverageGrade });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Fehler beim Speichern: " + ex.Message });
            }
        }

        public IActionResult Statistics()
        {
            try
            {
                List<Student> students = _studentDA.GetAllStudents();
                Dictionary<int, decimal> studentAverages = new Dictionary<int, decimal>();

                foreach (Student student in students)
                {
                    decimal average = _gradeDA.GetStudentAverageGrade(student.StudentId);
                    studentAverages[student.StudentId] = average;
                }

                TempData["StudentAverages"] = studentAverages;
                return View(students);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Fehler beim Laden der Statistiken: " + ex.Message;
                return View(new List<Student>());
            }
        }

        public IActionResult ManageModules()
        {
            try
            {
                List<Module> modules = _moduleDA.GetAllModules();
                return View(modules);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Fehler beim Laden der Module: " + ex.Message;
                return View(new List<Module>());
            }
        }

        [HttpPost]
        public IActionResult UpdateGradeCount(int moduleId, int gradeCount)
        {
            try
            {
                _moduleDA.UpdateModuleGradeCount(moduleId, gradeCount);
                return Json(new { success = true, message = "Notenanzahl aktualisiert" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

