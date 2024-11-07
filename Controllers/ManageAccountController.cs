using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using PFMS_MI04.Models.Authentication;
using Microsoft.AspNetCore.SignalR;
using PFMS_MI04.Models;
using PFMS_MI04.Services;
using System.Threading.Tasks;

namespace PFMS_MI04.Controllers
{
    
    public class ManageAccountController : Controller
    {
        private readonly JwtService _jwtService;
        private readonly IHubContext<ManageAccountHub> _hubContext;
        private readonly ManageAccountService _manageAccountService;

        public ManageAccountController(JwtService jwtService, IHubContext<ManageAccountHub> hubContext, ManageAccountService manageAccountService)
        {
            _jwtService = jwtService; 
            _hubContext = hubContext; // Initialize the hub context
            _manageAccountService = manageAccountService;
        }


        [HttpGet("/ManageAccount/ManageAccount")]
        public async Task<IActionResult> ManageAccount(string searchName = null, string searchRole = null, string searchUserId = null, string generalSearch = null)
        {
            if (!IsUserAuthenticated() || GetUserRole() != "Admin")
            {
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            // Handle general search across all fields if provided
            if (!string.IsNullOrEmpty(generalSearch))
            {
                searchName = generalSearch;
                searchRole = generalSearch;
                searchUserId = generalSearch;
            }

            var userData = await _manageAccountService.GetUserDataAsync(searchName, searchRole, searchUserId);
            return View(userData);
        }

        [HttpGet("/ManageAccount/CheckUserStatus")]
        public async Task<IActionResult> CheckUserStatus(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }


            var isOnline = await Task.Run(() => AuthRepoUser.IsUserLoggedIn(userId)); 

            return Json(new { isOnline });
        }

        [HttpGet("/ManageAccount/SignOffUserTesting")]
        public async Task<IActionResult> SignOffUserTesting(string userId)
        {
            if (!IsUserAuthenticated() || GetUserRole() != "Admin")
            {
                return RedirectToAction("Login", "Login");
            }

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required.");
                }

                // Sign off the user asynchronously
                await Task.Run(() => AuthRepoUser.SignOffTokenInvoke(userId));

                // Notify all clients of the update asynchronously
                await _hubContext.Clients.All.SendAsync("ReceiveManageAccUpdate", userId); // Send userId to update status

              return RedirectToAction("ManageAccount"); // Redirect back to the account management page
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while signing off the user: {ex.Message}");
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
    }
}
