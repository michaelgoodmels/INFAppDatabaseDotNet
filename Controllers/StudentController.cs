using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using IMS25T_Core.Models;
using System.Collections.Generic;

namespace IMS25T_Core.Controllers
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
        public IActionResult SaveGrade(int moduleId, string gradeValue)
        {
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                Student student = _studentDA.GetStudentByUserId(userId);

                if (student == null)
                {
                    return Json(new { success = false, message = "SchÃ¼ler nicht gefunden." });
                }

                if (string.IsNullOrEmpty(gradeValue))
                {
                    // LÃ¶sche die Note, wenn leer
                    Grade existingGrade = _gradeDA.GetStudentGrade(student.StudentId, moduleId);
                    if (existingGrade != null)
                    {
                        _gradeDA.DeleteGrade(existingGrade.GradeId);
                    }
                }
                else
                {
                    // Validiere und speichere die Note
                    if (decimal.TryParse(gradeValue.Replace(",", "."), out decimal grade))
                    {
                        if (grade >= 1.0m && grade <= 6.0m)
                        {
                            _gradeDA.SaveGrade(student.StudentId, moduleId, grade);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Note muss zwischen 1.0 und 6.0 liegen." });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "UngÃ¼ltige Note." });
                    }
                }

                // Aktualisiere Durchschnitte
                StudentModulesViewModel viewModel = _gradeDA.GetStudentModulesViewModel(student.StudentId);
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
