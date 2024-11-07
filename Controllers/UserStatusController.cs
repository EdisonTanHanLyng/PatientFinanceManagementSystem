using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models.Authentication; 
using System.Threading.Tasks;

using MySql.Data.MySqlClient;
using System.Collections.Generic;

using Microsoft.AspNetCore.SignalR;
using PFMS_MI04.Models;
using PFMS_MI04.Services;
using System.Threading.Tasks;


namespace PFMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserStatusController : ControllerBase
    {
        

        public UserStatusController(AuthRepoUser authRepoUser)
        {
            
        }

        [HttpGet("checkUserStatus")]
        public async Task<IActionResult> CheckUserStatus(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var isOnline = AuthRepoUser.IsUserLoggedIn(userId);
            return Ok(new { isOnline });
        }
    }
}