using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_Project_1.Controllers;
using System.Text.Json;
using System.Collections.Generic;
using System.Globalization;
using PFMS_MI04.Services;
using PFMS.Controllers;
using PFMS_MI04.Models.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace PFMS_MI04.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainMenuStaffController : Controller
    {
        private readonly ILogger<MainMenuStaffController> _logger;
        private readonly JwtService _jwtService;

        public MainMenuStaffController(ILogger<MainMenuStaffController> logger, JwtService jwtService)
        {
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpGet("MockGetPatientsLogin")]
        public IActionResult mock_getPatients_Login()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "MainMenu_Staff"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            string jsonString = @"
            [
                { ""ID"": ""123456-12-0001"", ""Name"": ""Alice Johnson Kho"", ""Status"": ""Login"" },
                { ""ID"": ""123456-12-0002"", ""Name"": ""Bob Smith"", ""Status"": ""Offline"" },
                { ""ID"": ""123456-12-0003"", ""Name"": ""Charlie Brown"", ""Status"": ""Offline"" },
                { ""ID"": ""123456-12-0001"", ""Name"": ""Alice Johnson Kho"", ""Status"": ""Login"" },
                { ""ID"": ""123456-12-0002"", ""Name"": ""Bob Smith"", ""Status"": ""Offline"" },
                { ""ID"": ""123456-12-0003"", ""Name"": ""Charlie Brown"", ""Status"": ""Offline"" }

            ]";

            // Parse JSON into a list of User objects
            List<PatientsAttempsModel> items = PatientAttemptService.GetAllPatientAttemptListMain();
            //var users = JsonSerializer.Deserialize<List<MainMenuModel_Patients_Attemps>>(jsonString);
            var users = items;
            string userList = null;

            foreach (var user in users)
            {
                _logger.LogInformation($"ID: {user.DataReturn()}"); // Logging
                userList += user.DataReturn() + "\n";
            }

            SecurityService.Log(GetUserID(), "Get Patient Attempts", "ACCESS", "MainMenu_Staff"); // userID, Message, logType, atPage
            return Ok(userList);
        }

        [HttpGet("Reminders")]
        public IActionResult mock_getPatients_Reminder()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "MainMenu_Staff"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            string jsonString = @"
            [
                { ""DueDate"": ""31/8/2024"", ""Name"": ""Alice Johnson Kho"", ""UserType"": ""Patients"", ""Description"": ""Have to make the payment before the due date"" },
                { ""DueDate"": ""30/8/2024"", ""Name"": ""Alice Johnson Kho"", ""UserType"": ""Sponsor"", ""Description"": ""Have to make the payment before the due date"" },
                { ""DueDate"": ""31/8/2024"", ""Name"": ""Alice Johnson Kho"", ""UserType"": ""Patients"", ""Description"": ""Have to make the payment before the due date"" },
                { ""DueDate"": ""30/8/2024"", ""Name"": ""Alice Johnson Kho"", ""UserType"": ""Sponsor"", ""Description"": ""Have to make the payment before the due date"" }

            ]";

            //List<MainMenuModel_Reminder> items = JsonSerializer.Deserialize<List<MainMenuModel_Reminder>>(jsonString);
            List<MainMenuModel_Reminder> items = MainmenuService.GetSomeReminders();

            _logger.LogInformation("Here: " + string.Join(", ", items.Select(item => item.DueDate)));

            var dateParser = new Func<string, DateTime>(dateStr =>
                DateTime.ParseExact(dateStr.Split(' ')[0], "M/d/yyyy", CultureInfo.InvariantCulture));

            var users = items.ToList();

            if (users == null || !users.Any())
            {
                SecurityService.Log(GetUserID(), "MainMenu: No Reminders Returned", "ACCESS", "MainMenu_Staff"); // userID, Message, logType, atPage
                return NotFound("No reminders found.");
            }

            string userList = string.Empty;

            foreach (var user in users)
            {
                _logger.LogInformation($"Name: {user.Name}, DueDate: {user.DueDate}, UserType: {user.UserType}, Description: {user.Description}"); // Logging
                userList += user.reminderMessageReturn() + "\n"; // Assuming `user.Name` is the property for the name
            }

            SecurityService.Log(GetUserID(), "Get Reminders", "ACCESS", "MainMenu_Staff"); // userID, Message, logType, atPage
            return Ok(userList);
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
