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
        public IActionResult EditGrade(int studentId, int moduleId, string gradeValue)
        {
            try
            {
                if (string.IsNullOrEmpty(gradeValue))
                {
                    Grade existingGrade = _gradeDA.GetStudentGrade(studentId, moduleId);
                    if (existingGrade != null)
                    {
                        _gradeDA.DeleteGrade(existingGrade.GradeId);
                    }
                }
                else
                {
                    if (decimal.TryParse(gradeValue.Replace(",", "."), out decimal grade))
                    {
                        if (grade >= 1.0m && grade <= 6.0m)
                        {
                            _gradeDA.SaveGrade(studentId, moduleId, grade);
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

