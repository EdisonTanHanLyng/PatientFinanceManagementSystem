using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using Newtonsoft.Json.Linq;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using System.IO;

namespace PFMS.Controllers
{
    //[RequireHttps]
    public class LoginController : Controller
    {
        private readonly JwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<ManageAccountHub> _hubContext;
        public LoginController(JwtService jwtService, IHttpContextAccessor httpContextAccessor, IHubContext<ManageAccountHub> hubContext)
        {
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;


        }

        public IActionResult Login()
        {
            if (IsUserAuthenticated())
            {
                SecurityService.Log(GetUserID(), "User Login", "AUTH", "Login_Page"); // userID, Message, logType, atPage
                if (GetUserRole().Equals("Admin"))
                {
                    return RedirectToAction("Mainmenu", "Mainmenu");
                }
                else
                {
                    return RedirectToAction("Mainmenu_staff", "Mainmenu");
                }
            }
            return View();
        }

        [HttpPost("login")]
        //[ValidateAntiForgeryToken]
        public IActionResult VerifyAccount(AccountModel account)
        {
            try
            {
                /*
                if (!HttpContext.Request.IsHttps)
                {
                    return Json(new { success = false, message = "Invalid hhtp access." });
                }
                */

                (bool isAuthenticated, string userId, string accRole) = AccountService.verifyAccount(account.AccName, account.Password);

                if (!isAuthenticated)
                {
                    Console.WriteLine("Authentication failed. Please check your credentials.");
                    SecurityService.Log("UnAuth", "Authentication Failed: AccName:[" + account.AccName + "] | Pass:[" + account.Password + "]", "AUTH", "Login_Page"); // userID, Message, logType, atPage
                    return Json(new { success = false, message = "Authentication failed. Please check your credentials." });
                }

                if (AuthRepoUser.IsUserLoggedIn(userId))
                {
                    SecurityService.Log(userId, "Already Logged In: AccName:[" + account.AccName + "] | Pass:[" + account.Password + "]", "AUTH", "Login_Page"); // userID, Message, logType, atPage
                    return Json(new { success = false, message = "User is currently logged in" });
                    
                }

                Console.WriteLine("Logged in" + userId + " and " + accRole);
                string jwt = _jwtService.Generate(userId, accRole);

                AuthRepoUser.SetJwt(jwt, userId, accRole);
                
                _hubContext.Clients.All.SendAsync("UserLoggedIn", userId); // Notify about the new user login
                

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append("jwt", jwt, cookieOptions);

                Console.WriteLine("Currently jwt: " + jwt);
                AuthRepoUser.PrintUserMap();
                string role = _jwtService.GetRole(jwt);
                Console.WriteLine($"Retrieved role: {role}");
                String path = null;

                if (accRole.Equals("Admin"))
                {
                    path = "/MainMenu/Mainmenu";
                }
                else
                {
                    path = "/MainMenu/Mainmenu_staff";
                }

                SecurityService.Log(userId, "Log In: AccName:[" + account.AccName + "] | Pass:[" + account.Password + "] | Role:[" + accRole + "] | Path:[" + path + "]", "AUTH", "Login_Page"); // userID, Message, logType, atPage
                return Json(new { success = true, path = path });
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = "Username or Password Incorrect.";
                SecurityService.Log("UnAuth", "Login Error: " + ex.Message, "ERROR", "Login_Page"); // userID, Message, logType, atPage
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Username or Password Incorrect." });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An unexpected error occurred.";
                SecurityService.Log("UnAuth", "Login Error: " + ex.Message, "ERROR", "Login_Page"); // userID, Message, logType, atPage
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "An unexpected error occurred." });
            }

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
