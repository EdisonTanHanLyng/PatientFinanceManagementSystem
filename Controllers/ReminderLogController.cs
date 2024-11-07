using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using System.Text.Json;

namespace PFMS_MI04.Controllers
{
    public class ReminderLogController : Controller
    {
        private readonly ILogger<ReminderLogController> _logger;
        private readonly JwtService _jwtService;  //Authentication

        public ReminderLogController(ILogger<ReminderLogController> logger, JwtService jwtService)
        {
            _logger = logger;
            _jwtService = jwtService;
        }
        public IActionResult ReminderLog()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Attempt"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Patient Attempt", "ACCESS", "Patient_Attempt"); // userID, Message, logType, atPage
            return View();
        }

        //Authentication
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
