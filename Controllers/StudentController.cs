using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using INFAppDatabaseDotNet.Models;
using System.Collections.Generic;

namespace INFAppDatabaseDotNet.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly StudentDataAccess _studentDA;
        private readonly GradeDataAccess _gradeDA;
        private readonly ModuleDataAccess _moduleDA;

        public StudentController(StudentDataAccess studentDA, GradeDataAccess gradeDA, ModuleDataAccess moduleDA)
        {
            _studentDA = studentDA;
            _gradeDA = gradeDA;
            _moduleDA = moduleDA;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                filterContext.Result = RedirectToAction("Index", "Login");
            }
            base.OnActionExecuting(filterContext);
        }

        public IActionResult Dashboard()
        {
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                Student student = _studentDA.GetStudentByUserId(userId);

                if (student == null)
                {
                    return NotFound();
                }

                StudentModulesViewModel viewModel = _gradeDA.GetStudentModulesViewModel(student.StudentId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Fehler beim Laden des Dashboards: " + ex.Message;
                return View(new StudentModulesViewModel());
            }
        }

        [HttpPost]
        public IActionResult SaveGrades(int moduleId, string grade1, string grade2, string grade3)
        {
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                Student student = _studentDA.GetStudentByUserId(userId);

                if (student == null)
                {
                    return Json(new { success = false, message = "Schüler nicht gefunden." });
                }

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

                _gradeDA.SaveMultipleGrades(student.StudentId, moduleId, parsedGrade1, parsedGrade2, parsedGrade3);

                StudentModulesViewModel viewModel = _gradeDA.GetStudentModulesViewModel(student.StudentId);
                System.Diagnostics.Debug.WriteLine($"[SaveGrades] Returning average: {viewModel.AverageGrade}");
                return Json(new { success = true, averageGrade = viewModel.AverageGrade });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Fehler beim Speichern: " + ex.Message });
            }
        }

        public IActionResult Profile()
        {
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                Student student = _studentDA.GetStudentByUserId(userId);

                if (student == null)
                {
                    return NotFound();
                }

                return View(student);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Fehler beim Laden des Profils: " + ex.Message;
                return View();
            }
        }
    }
}

