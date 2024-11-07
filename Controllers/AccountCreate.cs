using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Controllers;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using System.Security.Cryptography;
using System.Web;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace PFMS.Controllers
{
    public class AccountCreateController : Controller
    {
        private readonly JwtService _jwtService;  //Authentication
        private static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public AccountCreateController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public IActionResult AccountCreate()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Account_Creation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Account Create Opened", "ACCESS", "Account_Creation"); // userID, Message, logType, atPage
            return View();
        }

        [HttpPost("accountdetail")]
        public IActionResult GetAccDetails(AccountModel account)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Account_Creation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            try
            {
                SecurityService.Log(GetUserID(), "New Account created: " + account.AccName + " | " + account.Role, "CHANGE", "Account_Creation"); // userID, Message, logType, atPage
                account.AccName = HttpUtility.HtmlEncode(account.AccName);
                AccountService.storeAccount(account);
                return Json(new { success = true });
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1062)
            {
                SecurityService.Log(GetUserID(), "Account Creation: SQL Exception: " + ex.Message, "ERROR", "Account_Creation"); // userID, Message, logType, atPage
                ViewBag.ErrorMessage = "An account with this name already exists.";
                return Json(new { success = false, message = "An account with this name already exists." });
            }
            catch (Exception ex)
            {
                SecurityService.Log(GetUserID(), "Account Creation: Exception: " + ex.Message, "ERROR", "Account_Creation"); // userID, Message, logType, atPage
                ViewBag.ErrorMessage = "An unexpected error occurred.";;
                return Json(new { success = false, message = "Please review your input and ensure that all fields are completed with valid characters." });
            }
        }

        [HttpGet("/AccountCreate/GenerateUserID")]
        public async Task<IActionResult> GenerateUserID()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Account_Creation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            try
            {
                string userId = await GenerateUniqueUserId(AccountService.checkStoredId);
                return Json(new { success = userId });
            }
            catch (Exception ex)
            {
                SecurityService.Log(GetUserID(), "Account Creation: Exception: " + ex.Message, "ERROR", "Account_Creation"); // userID, Message, logType, atPage
                ViewBag.ErrorMessage = "An unexpected error occurred.";
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
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

        private static string GenerateNumericUserId(int length = 8)
        {
            byte[] randomNumber = new byte[length];
            rng.GetBytes(randomNumber);
            return string.Concat(Array.ConvertAll(randomNumber, b => (b % 10).ToString()));
        }

        private static async Task<string> GenerateUniqueUserId(Func<string, Task<string>> checkStoredId, int maxAttempts = 10)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                string userId = GenerateNumericUserId();
                string exists = await checkStoredId(userId);
                if (exists == "0")
                {
                    return userId;
                }
            }
            throw new Exception("Unable to generate a unique User ID after multiple attempts");
        }
    }
}
