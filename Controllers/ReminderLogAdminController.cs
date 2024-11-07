using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using System.Text.RegularExpressions;
using System.Globalization;

namespace PFMS_MI04.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderLogAdminController : Controller
    {
        private readonly ILogger<ReminderLogAdminController> _logger;
        private readonly JwtService _jwtService;
        private const int _usersPerPage = 13;

        public ReminderLogAdminController(ILogger<ReminderLogAdminController> logger, JwtService jwtService)
        {
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpGet("GetPatientsAttempt")]
        public async Task<IActionResult> GetPatientsAttempt([FromQuery] int page, [FromQuery] string searchTerm = "")
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Attempt_Admin"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var allUsers = PatientAttemptService.GetAllPatientAttemptList();

            SecurityService.Log(GetUserID(), "Get Patients Attempt: SearchTerm: [" + searchTerm + "]", "ACCESS", "Patient_Attempt_Admin"); // userID, Message, logType, atPage
            Console.WriteLine("HERE search: " , searchTerm);
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await PaginateUsers(allUsers, page);
            }

            var searchResults = SearchUsers(allUsers, searchTerm);
            return await PaginateUsers(searchResults, page);
        }

        private async Task<IActionResult> PaginateUsers(List<PatientsAttempsModel> users, int page)
        {
            if (users == null || !users.Any())
            {
                return BadRequest("No user data available");
            }

            var pagedUsers = users.Skip((page - 1) * _usersPerPage).Take(_usersPerPage).ToList();
            int totalPages = (int)Math.Ceiling((double)users.Count / _usersPerPage);

            var response = new
            {
                TotalUsers = totalPages,
                PagedUsers = string.Join("\n", pagedUsers.Select(u => $"{u.ID} , {u.Name} , {u.Status} , {u.Date}"))
            };

            return Ok(response);
        }

        private List<PatientsAttempsModel> SearchUsers(List<PatientsAttempsModel> allUsers, string searchTerm)
        {
            string noSpaceValue = searchTerm.Replace(" ", "");
            bool isIdFormat = Regex.IsMatch(noSpaceValue, @"^-?[\d-]+$");
            DateTime parsedDate;
            bool isDateFormat = TryParseFlexibleDate(searchTerm, out parsedDate);
            if (isDateFormat)
            {
                return allUsers.FindAll(patient =>
                    DateTime.TryParse(patient.Date, out DateTime patientDate) &&
                    patientDate.Date == parsedDate.Date);
            }
            else
            {
                string[] searchTerms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return allUsers.AsParallel().WithDegreeOfParallelism(3).Where(patient =>
                    (searchTerm.Equals("NULL") || patient.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (searchTerm.Equals("NULL") || patient.ID.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }
        }
        private static bool TryParseFlexibleDate(string input, out DateTime result) //ensure it support differnet date setting from window 
        {
            string[] formats = {
            "yyyy-MM-dd", "yyyy-M-d", "M/d/yyyy", "MM/dd/yyyy",
            "d/M/yyyy", "dd/MM/yyyy", "yyyy/MM/dd", "yyyy/M/d"
        };

            return DateTime.TryParseExact(input, formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out result);
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