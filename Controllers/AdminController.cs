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

        [HttpPost]
        public IActionResult EditGrades(int studentId, int moduleId, string grade1, string grade2, string grade3)
        {
            try
            {
                // Konvertiere und validiere Noten
                decimal? parsedGrade1 = null;
                decimal? parsedGrade2 = null;
                decimal? parsedGrade3 = null;

                if (!string.IsNullOrEmpty(grade1) && decimal.TryParse(grade1.Replace(",", "."), out decimal g1))
                {
                    if (g1 >= 1.0m && g1 <= 6.0m) parsedGrade1 = g1;
                    else return Json(new { success = false, message = "Note 1 ungültig (1.0-6.0)" });
                }

                if (!string.IsNullOrEmpty(grade2) && decimal.TryParse(grade2.Replace(",", "."), out decimal g2))
                {
                    if (g2 >= 1.0m && g2 <= 6.0m) parsedGrade2 = g2;
                    else return Json(new { success = false, message = "Note 2 ungültig (1.0-6.0)" });
                }

                if (!string.IsNullOrEmpty(grade3) && decimal.TryParse(grade3.Replace(",", "."), out decimal g3))
                {
                    if (g3 >= 1.0m && g3 <= 6.0m) parsedGrade3 = g3;
                    else return Json(new { success = false, message = "Note 3 ungültig (1.0-6.0)" });
                }

                // Mindestens eine Note muss vorhanden sein
                if (parsedGrade1 == null && parsedGrade2 == null && parsedGrade3 == null)
                {
                    return Json(new { success = false, message = "Mindestens eine Note muss eingegeben werden." });
                }

                _gradeDA.SaveMultipleGrades(studentId, moduleId, parsedGrade1, parsedGrade2, parsedGrade3);

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
    }
}

