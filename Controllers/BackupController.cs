using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using Newtonsoft.Json;
using PFMS_MI04.Hubs;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using System.Collections.Generic;

namespace PFMS_MI04.Controllers
{
    public class BackupController : Controller
    {
        private readonly BackupService _backupService;
        private readonly IHubContext<BackupHub> _hubContext;
        private readonly ILogger<BackupController> _logger;
        private static BackupList items = new BackupList();
        private readonly JwtService _jwtService;  //Authentication

        public BackupController(ILogger<BackupController> logger, JwtService jwtService, IHubContext<BackupHub> hubContext, BackupService backupService)
        {
            _logger = logger;
            _jwtService = jwtService;
            _hubContext = hubContext;
            _backupService = backupService;
        }

        public IActionResult Backup()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Backup"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Backup Page Opened", "ACCESS", "Backup"); // userID, Message, logType, atPage
            return View();
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> GetTypes()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Backup"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            
            return Ok(items.types);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BackupDetails>>> GetItems()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || (userRole != "Admin" && userRole != "User"))
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Backup");
                return RedirectToAction("Login", "Login");
            }

            // Refreshing items
            if (items.Items.Count > 0)
            {
                items.refresh();
            }

            // Fetch data for the last 6 months
            items.Items = await _backupService.getBackupLogs();

            // Get item count
            items.getCount();

            return Ok(items.Items);
        }

        [HttpGet("GetNextItems")]
        public async Task<ActionResult<IEnumerable<BackupDetails>>> GetNextItems(DateTime lastFetchedDate)
        {
            // Authentication verification
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || (userRole != "Admin" && userRole != "User"))
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Backup");
                return RedirectToAction("Login", "Login");
            }

            // Fetch the next 6 months' data before the last fetched date
            var nextItems = await _backupService.getNextBackupLogs(lastFetchedDate);

            // If no more items, return an empty result
            if (nextItems == null || nextItems.Count == 0)
            {
                return NoContent(); // 204 No Content indicates there are no more items to fetch
            }

            items.Items.AddRange(nextItems);
            items.getCount();

            return Ok(nextItems);
        }

        [HttpGet]
        public ActionResult<IEnumerable<int>> GetPercentages()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Backup"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            return Ok(items.percentages);
        }


        [HttpPost]
        public async Task<IActionResult> CreateBackup([FromForm] string jsonData)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Backup"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var details = JsonConvert.DeserializeObject<BackupDetails>(jsonData);

            if (details == null)
            {
                SecurityService.Log(GetUserID(), "Backup: item is empty", "ERROR", "Backup"); // userID, Message, logType, atPage
                return BadRequest("item is empty");
            }

            //items.addToList(details);

            if (details.type.Contains("Full"))
            {
                _backupService.BackupDatabase_Full(details.remarks);
            }
            if (details.type.Contains("Security Logging"))
            {
                _backupService.BackupDatabase_Sec_Logging(details.remarks);
            }
            if (details.type.Contains("Backups"))
            {
                _backupService.BackupDatabase_Backups(details.remarks);
            }
            if (details.type.Contains("Documents"))
            {
                _backupService.BackupDatabase_Document(details.remarks);
            }
            if (details.type.Contains("Account"))
            {
                _backupService.BackupDatabase_Account(details.remarks);
            }
            if (details.type.Contains("Reminders"))
            {
                _backupService.BackupDatabase_Reminder(details.remarks);
            }
            if (details.type.Contains("Pricings"))
            {
                _backupService.BackupDatabase_Pricings(details.remarks);
            }

            //BackupTypes.RestoreDatabase_Reminder();

            await _hubContext.Clients.All.SendAsync("ReceiveBackupListUpdate", "A Backup has been made.");

            return Ok("Backup successful");
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