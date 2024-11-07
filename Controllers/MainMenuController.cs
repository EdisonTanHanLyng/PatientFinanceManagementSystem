using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Data;
using PFMS_MI04.Services;

namespace PFMS_Project_1.Controllers
{
    public class MainMenuController : Controller
    {
        private readonly ILogger<MainMenuController> _logger;
        private readonly JwtService _jwtService;

        public MainMenuController(ILogger<MainMenuController> logger, JwtService jwtService)
        {
            _logger = logger;
            _jwtService = jwtService;
        }

        public IActionResult Mainmenu()
        {

            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "MainMenu"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Redirecting to " + userRole, "AUTH", "MainMenu"); // userID, Message, logType, atPage
            return View();
        }

        public IActionResult Mainmenu_staff()
        {

            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "MainMenu"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Redirecting to " + userRole, "AUTH", "MainMenu"); // userID, Message, logType, atPage
            return View();
        }

        private bool IsUserAuthenticated()
        {
            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {  
                return false;
            }
            try
            {
                _jwtService.Verify(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetUserRole()
        {
            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            return _jwtService.GetRole(token);
        }

        private string GetUserID()
        {
            var jwt = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(jwt))
            {
                return null;
            }
            return _jwtService.getToken(jwt);
        }
    }
}