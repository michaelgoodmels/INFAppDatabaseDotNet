using System;
using Microsoft.AspNetCore.Mvc;
using IMS25T_Core.Models;

namespace IMS25T_Core.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserDataAccess _userDA;

        public LoginController(UserDataAccess userDA)
        {
            _userDA = userDA;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Benutzername und Passwort sind erforderlich.";
                return View();
            }

            try
            {
                User user = _userDA.AuthenticateUser(username, password);

                if (user != null)
                {
                    // Setze Session-Daten
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);
                    HttpContext.Session.SetString("FullName", user.FullName);

                    // Redirect to appropriate dashboard based on role
                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (user.Role == "Student")
                    {
                        return RedirectToAction("Dashboard", "Student");
                    }
                }
                else
                {
                    TempData["Error"] = "UngÃ¼ltige Anmeldedaten.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ein Fehler ist aufgetreten: " + ex.Message;
            }

            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
