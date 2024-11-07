using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using Org.BouncyCastle.Tls;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace PFMS_MI04.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly JwtService _jwtService;
        private readonly IHubContext<ManageAccountHub> _hubContext;
        public static ConcurrentDictionary<string, DateTime> LastHeartbeats { get; } = new ConcurrentDictionary<string, DateTime>();

        public HomeController(ILogger<HomeController> logger, IHubContext<ManageAccountHub> hubContext)
        {
            _logger = logger;
            _jwtService = new JwtService();
            _hubContext = hubContext; 
        }

        public IActionResult Index()
        {
            AuthRepoUser.PrintUserMap();
            return RedirectToAction("Login", "Login"); //Re-direct to login page
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                _logger.LogInformation("Logout initiated");
                var jwt = Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(jwt))
                {
                    var userId = AuthRepoUser.GetUserId(jwt);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        AuthRepoUser.RemoveUser(jwt);
                        SecurityService.Log("" + userId, "Log Out: User: " + userId , "AUTH", "Log_Out"); // userID, Message, logType, atPage
                        _logger.LogInformation($"User {userId} logged out successfully");
                         
                        
                        _hubContext.Clients.All.SendAsync("UserLoggedOut", userId); //Added this to Signal all clients
                    }
                    else
                    {
                        SecurityService.Log("UnAuth", "Log Out: Invalid JWT Logout", "AUTH", "Log_Out"); // userID, Message, logType, atPage
                        _logger.LogWarning($"Attempted to logout with invalid JWT");
                    }
                    // Delete the JWT cookie
                    Response.Cookies.Delete("jwt");
                }
                else
                {
                    SecurityService.Log("UnAuth", "Log Out: Null JWT Logout", "AUTH", "Log_Out"); // userID, Message, logType, atPage
                    _logger.LogWarning("Logout attempted without a JWT cookie");
                }
                AuthRepoUser.PrintUserMap();
                return Ok(new { message = "Logout successful" });
            }
            catch (Exception ex)
            {
                SecurityService.Log("UnAuth", "Log Out: Error: " + ex.Message, "ERROR", "Log_Out"); // userID, Message, logType, atPage
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        [HttpPost("verifyRefresh")]
        public async Task<IActionResult> verifyRefresh()
        {
            var jwt = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(jwt))
            {
                return Unauthorized(new { message = "No JWT token found" });
            }

            try
            {
                var userId = _jwtService.getToken(jwt);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid JWT token" });
                }
                LastHeartbeats[userId] = DateTime.UtcNow; //Update to current time
                return Ok(new { message = "valid" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying JWT token");
                return Unauthorized(new { message = "Invalid JWT token" });
            }
        }

    }

}