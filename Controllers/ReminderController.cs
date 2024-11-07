using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Models.Patients;
using PFMS_MI04.Services;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PFMS.Controllers
{
    public class ReminderController : Controller
    {
        private readonly string apiEndpoint = "http://147.185.221.18:52757";
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        private static Dictionary<string, string> tempData = new Dictionary<string, string>();

        public ReminderController(JwtService jwtService, EmailService emailService)
        {
            _jwtService = jwtService;
            _emailService = emailService;
        }

        public IActionResult Reminder()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Reminders_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            var jwt = Request.Cookies["jwt"];
            string currentUserId = _jwtService.getToken(jwt);
            string patientTempData = currentUserId + convertToBase64("Patient");
            string sponsorTempData = currentUserId + convertToBase64("Sponsor");

            tempData.Remove(patientTempData);
            tempData.Remove(sponsorTempData);

            SecurityService.Log(GetUserID(), "Reminders", "ACCESS", "Reminders_Page"); // userID, Message, logType, atPage
            return View();
        }

        public IActionResult AddReminder()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Create_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Create Reminder", "ACCESS", "Create_Reminder_Page"); // userID, Message, logType, atPage
            return View();
        }

        public IActionResult AddReminderPatient()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Add_Patients_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Add Patients", "ACCESS", "Add_Patients_Reminder_Page"); // userID, Message, logType, atPage
            return View();
        }

        public IActionResult AddReminderSponsor()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Add_Sponsors_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Add Sponsors", "ACCESS", "Add_Sponsors_Reminder_Page"); // userID, Message, logType, atPage
            return View();
        }

        [HttpGet]
        public IActionResult GetAllReminders()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Reminders_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            List<ReminderItem> reminderList = ReminderService.GetAllReminders();
            if (reminderList.Count == 0)
            {
                return NotFound();
            }
            return Ok(reminderList);
        }

        public IActionResult EditReminder()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Create_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Create Reminder", "ACCESS", "Create_Reminder_Page"); // userID, Message, logType, atPage
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> createNewReminder([FromBody] List<ReminderItemJson> reminder)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Create_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var creationStatus = new { success = false, responseMessage = "Failed to create a new reminder!" };
            var jwt = Request.Cookies["jwt"];
            string currentUserId = _jwtService.getToken(jwt);
            string patientTempData = currentUserId + convertToBase64("Patient");
            string sponsorTempData = currentUserId + convertToBase64("Sponsor");

            if (reminder != null)
            {
                foreach (var i in reminder)
                {
                    ReminderItem reminderItem = new ReminderItem(
                        i.Title,
                        i.UserName,
                        i.Email,
                        i.UserType,
                        i.Priority,
                        DateOnly.Parse(i.DueDate),
                        TimeOnly.Parse(i.DueTime),
                        i.Description,
                        i.EmailContent
                    );

                    var response = await ReminderService.createNewReminder(reminderItem);
                    if (response)
                    {
                        creationStatus = new { success = true, responseMessage = "Successfully added new reminder" };
                    }
                }
                SecurityService.Log(GetUserID(), "New Reminder Created: Reminder Title: [" + reminder[0].Title + "]", "CHANGE", "Create_Reminder_Page"); // userID, Message, logType, atPage
            }

            tempData.Remove(patientTempData);
            tempData.Remove(sponsorTempData);

            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ReminderHub>>();
            await hubContext.Clients.All.SendAsync("ReceiveReminderUpdate", "New reminders have been deleted.");

            return Json(creationStatus);
        }

        [HttpPost]
        public async Task<IActionResult> updateReminder([FromBody] ReminderItemJson reminder)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Create_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var creationStatus = new { success = false, responseMessage = "Failed to update reminder!" };
            var jwt = Request.Cookies["jwt"];
            string currentUserId = _jwtService.getToken(jwt);
            string patientTempData = currentUserId + convertToBase64("Patient");
            string sponsorTempData = currentUserId + convertToBase64("Sponsor");

            if (reminder != null)
            {
                ReminderItem reminderItem = new ReminderItem(
                    reminder.Id,
                    reminder.Title,
                    reminder.UserName,
                    reminder.Email,
                    reminder.UserType,
                    reminder.Priority,
                    DateOnly.Parse(reminder.DueDate),
                    TimeOnly.Parse(reminder.DueTime),
                    reminder.Description,
                    reminder.EmailContent
                );

                var response = await ReminderService.updateReminder(reminderItem);
                if (response)
                {
                    creationStatus = new { success = true, responseMessage = "Successfully updated reminder" };
                }
                SecurityService.Log(GetUserID(), "Updated Reminder: Reminder Title: [Reminder ID: " + reminder.Id + reminder.Title + "]", "CHANGE", "Create_Reminder_Page"); // userID, Message, logType, atPage
            }

            tempData.Remove(patientTempData);
            tempData.Remove(sponsorTempData);

            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ReminderHub>>();
            await hubContext.Clients.All.SendAsync("ReceiveReminderUpdate", "New reminders have been deleted.");

            return Json(creationStatus);
        }

        [HttpPost]
        public async Task<IActionResult> deleteReminder([FromBody] ReminderItemJson reminder)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Reminders_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var deletionStatus = new { success = false, responseMessage = "Failed to delete reminder!" };

            if (reminder != null)
            {
                ReminderItem reminderItem = new ReminderItem(reminder.Title, reminder.UserName, reminder.Email, reminder.UserType, reminder.Priority, DateOnly.Parse(reminder.DueDate), TimeOnly.Parse(reminder.DueTime), reminder.Description, reminder.EmailContent);

                var response = await ReminderService.DeleteReminder(reminderItem);
                if (response)
                {
                    // Notify all clients about the deletion
                    var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ReminderHub>>();
                    await hubContext.Clients.All.SendAsync("ReceiveReminderUpdate", "A reminder has been deleted.");

                    deletionStatus = new { success = true, responseMessage = "Successfully deleted reminder" };
                    SecurityService.Log(GetUserID(), "Reminder Deleted: Reminder Title:[" + reminderItem.Title + "]", "CHANGE", "Reminders_Page"); // userID, Message, logType, atPage
                }
            }

            return Json(deletionStatus);
        }

        [HttpGet]
        public IActionResult GetReminderById([FromQuery] int reminderId)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Reminders_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            ReminderItem reminderItem = ReminderService.GetReminderById(reminderId);
            if (reminderItem == null)
            {
                return NotFound();
            }
            return Ok(reminderItem);
        }

        [HttpGet]
        public async Task<IActionResult> getAllPatients()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Add_Patients_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "Reminders: Get All Patients", "API", "Add_Patients_Reminder_Page"); // userID, Message, logType, atPage
            string requestString = "/api/patient/profile/0/99";
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content);
            }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> getAllSponsors()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Add_Sponsors_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "Reminders: Get All Sponsors", "API", "Add_Sponsors_Reminder_Page"); // userID, Message, logType, atPage
            string requestString = "/api/sponsor";
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content);
            }

            return NotFound();
        }


        [HttpPost]
        public IActionResult saveAndUploadSponsors([FromBody] List<AddReminderSponsorItemJson> reminders)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Add_Sponsors_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var uploadStatus = new { success = false, responseMessage = "Failed to upload reminder!" };

            List<AddReminderItem> list = new List<AddReminderItem>();
            var jwt = Request.Cookies["jwt"];
            string currentUserId = _jwtService.getToken(jwt);
            string sponsorTempData = currentUserId + convertToBase64("Sponsor");

            if (tempData.ContainsKey(sponsorTempData))
                list = JsonConvert.DeserializeObject<List<AddReminderItem>>(tempData[sponsorTempData].ToString());

            if (reminders != null && reminders.Count > 0)
            {
                foreach (var reminder in reminders)
                {
                    if (reminder.UserType == "Sponsor")
                    {
                        var reminderItem = new AddReminderItem(
                            reminder.SponsorName,
                            reminder.SponsorCode,
                            reminder.SponsorContactPerson,
                            reminder.SponsorOfficePhone,
                            reminder.SponsorEmail,
                            reminder.UserType
                        );

                        list.Add(reminderItem);
                    }
                }


                tempData[sponsorTempData] = JsonConvert.SerializeObject(list);



                uploadStatus = new { success = true, responseMessage = "Upload successful!" };
                SecurityService.Log(GetUserID(), "Reminders: UploadSponsors", "CHANGE", "Add_Sponsors_Reminder_Page"); // userID, Message, logType, atPage
            }
            return Json(uploadStatus);
        }

        [HttpPost]
        public IActionResult saveAndUploadPatients([FromBody] List<AddReminderPatientItemJson> reminders)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Add_Patients_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var uploadStatus = new { success = false, responseMessage = "Failed to upload reminder!" };

            List<AddReminderItem> list = new List<AddReminderItem>();
            var jwt = Request.Cookies["jwt"];
            string currentUserId = _jwtService.getToken(jwt);
            string patientTempData = currentUserId + convertToBase64("Patient");

            if (tempData.ContainsKey(patientTempData))
                list = JsonConvert.DeserializeObject<List<AddReminderItem>>(tempData[patientTempData].ToString());

            if (reminders != null && reminders.Count > 0)
            {
                foreach (var reminder in reminders)
                {
                    if (reminder.UserType == "Patient")
                    {
                        var reminderItem = new AddReminderItem(
                            reminder.PatientName,
                            reminder.PatientNRIC,
                            reminder.PatientSponsor,
                            reminder.PatientPhone,
                            reminder.PatientEmail,
                            reminder.UserType
                        );

                        list.Add(reminderItem);
                    }
                }

                tempData[patientTempData] = JsonConvert.SerializeObject(list);

                uploadStatus = new { success = true, responseMessage = "Upload successful!" };
                SecurityService.Log(GetUserID(), "Reminders: UploadPatients", "CHANGE", "Add_Patients_Reminder_Page"); // userID, Message, logType, atPage
            }
            return Json(uploadStatus);
        }

        [HttpGet]
        public IActionResult getAllSelectedSponsors()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Create_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var jwt = Request.Cookies["jwt"];
            string currentUserId = _jwtService.getToken(jwt);
            string sponsorTempData = currentUserId + convertToBase64("Sponsor");

            List<AddReminderItem> sponsors = new List<AddReminderItem>();
            //TempData.Keep(sponsorTempData);

            

            if (tempData.ContainsKey(sponsorTempData))
            {
                sponsors = JsonConvert.DeserializeObject<List<AddReminderItem>>(tempData[sponsorTempData].ToString());
                SecurityService.Log(GetUserID(), "Reminders: GetSelectSponsors", "ACCESS", "Create_Reminder_Page"); // userID, Message, logType, atPage
            }

            return Ok(sponsors);
        }

        [HttpGet]
        public IActionResult getAllSelectedPatients()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Reminders_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var jwt = Request.Cookies["jwt"];
            string currentUserId = _jwtService.getToken(jwt);
            string patientTempData = currentUserId + convertToBase64("Patient");

            List<AddReminderItem> patients = new List<AddReminderItem>();
            //TempData.Keep(patientTempData);

            if (tempData.ContainsKey(patientTempData))
            {
                patients = JsonConvert.DeserializeObject<List<AddReminderItem>>(tempData[patientTempData].ToString());
                SecurityService.Log(GetUserID(), "Reminders: GetSelectPatients", "ACCESS", "Create_Reminder_Page"); // userID, Message, logType, atPage
            }

            return Ok(patients);
        }


        [HttpPost]
        public async Task<IActionResult> sendReminder([FromBody] ReminderItemJson reminder)
        {
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || (userRole != "Admin" && userRole != "User"))
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Reminders_Page");
                return RedirectToAction("Login", "Login");
            }

            var reminderStatus = new { success = false, responseMessage = "Failed to send reminder!" };

            if (reminder != null)
            {
                ReminderItem reminderItem = new ReminderItem(reminder.Title, reminder.UserName, reminder.Email, reminder.UserType, reminder.Priority, DateOnly.Parse(reminder.DueDate), TimeOnly.Parse(reminder.DueTime), reminder.Description, reminder.EmailContent);

                try
                {
                    var emailResponse = await _emailService.SendEmailAsync(reminderItem.Email, reminderItem.Title, reminderItem.EmailContent, reminderItem.UserName);
                    //bool emailResponse = true;

                    if (emailResponse)
                    {
                        var response = await ReminderService.DeleteReminder(reminderItem, emailResponse);
                        if (response)
                        {
                            // Notify all clients about the reminder sent
                            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ReminderHub>>();
                            await hubContext.Clients.All.SendAsync("ReceiveReminderUpdate", "A reminder has been sent.");

                            reminderStatus = new { success = true, responseMessage = "Successfully sent reminder." };

                            SecurityService.Log(GetUserID(), "Reminder Sent: Reminder Recipient:[" + reminderItem.Email + "]", "CHANGE", "Reminders_Page");
                        }
                        else
                        {
                            reminderStatus = new { success = true, responseMessage = "Email sent, but failed to delete the reminder entry." };

                            SecurityService.Log(GetUserID(), "Email sent but failed to delete entry: [" + reminderItem.Email + "]", "WARNING", "Reminders_Page");
                        }
                    }
                }
                catch (Exception ex)
                {
                    reminderStatus = new { success = false, responseMessage = $"Error sending email or deleting entry: {ex.Message}" };

                    SecurityService.Log(GetUserID(), $"Exception: {ex.Message}", "ERROR", "Reminders_Page");
                }
            }

            return Json(reminderStatus);
        }



        private string convertToBase64(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
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
