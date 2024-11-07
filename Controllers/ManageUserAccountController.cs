using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using System;
using System.Threading.Tasks;

namespace PFMS_MI04.Controllers
{
    public class ManageUserAccountController : Controller
    {
        private readonly ManageUserAccountService _manageUserAccountService;
        private readonly JwtService _jwtService;

        public ManageUserAccountController(ManageUserAccountService manageUserAccountService, JwtService jwtService)
        {
            _manageUserAccountService = manageUserAccountService;
            _jwtService = jwtService;
        }

        // Display the Manage User Account page with user data
        public async Task<IActionResult> ManageUserAccountAsync(string userId)
        {

            if (!IsUserAuthenticated() || GetUserRole() != "Admin")
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            var user = await _manageUserAccountService.GetUserDetailsAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found.";
                return View("Error");
            }
            return View(user);
        }

        [HttpPost]

        public async Task<IActionResult> UpdateUserDetails([FromForm] ManageUserAccountItemList.ManageUser user) // Updated model reference
        {
            if (!IsUserAuthenticated() || GetUserRole() != "Admin")
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            bool updateSuccessful;

            // If a password is provided, hash and salt it using AccountService before updating
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                byte[] salt = AccountService.generateSalt(); // Generate salt using AccountService
                byte[] hashedPassword = AccountService.hashPassword(user.Password, salt); // Hash the password

                // Convert hashed password and salt to Base64 strings for database storage
                string hashedPasswordBase64 = Convert.ToBase64String(hashedPassword);
                string saltBase64 = Convert.ToBase64String(salt);

                updateSuccessful = await _manageUserAccountService.UpdateUserInDatabaseAsync(user, hashedPasswordBase64, saltBase64);
            }
            else
            {
                updateSuccessful = await _manageUserAccountService.UpdateUserInDatabaseAsync(user, null, null);
            }

            if (updateSuccessful)
            {
                // Return a JSON response with success message and userId
                return Json(new { userId = user.UserId, message = "User details updated successfully." });
            }
            else
            {
                // Return an error message if the update failed
                return Json(new { error = "Failed to update user details." });
            }
        }

        [HttpPost("/ManageUserAccount/RemoveCurrentUserAccount")]
        public async Task<IActionResult> RemoveCurrentUserAccount([FromForm] AccountModel request)
        {
            if (!IsUserAuthenticated() || GetUserRole() != "Admin")
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            if (request == null || string.IsNullOrEmpty(request.UserID))
            {
                return BadRequest("Invalid request data: UserID is required");
            }

            if (GetUserID() == request.UserID)
            {
                return Json(new { message = "Invalid request data: You cannot delete your own account" });
            }

            try
            {
                bool deleteSuccess = await _manageUserAccountService.RemoveUserInDb(request);
                if (deleteSuccess)
                {
                    // Return a JSON response indicating success
                    return Json(new { message = "User account removed successfully." });
                }
                else
                {
                    return StatusCode(500, "Failed to remove user account");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the user account: {ex.Message}");
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

