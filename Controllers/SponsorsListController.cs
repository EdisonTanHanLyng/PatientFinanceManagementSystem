using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using RestSharp;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace PFMS_MI04.Controllers
{
    public class SponsorsListController : Controller
    {
        private List<SponsorListItem> sList;
        private string dateLF = DateOnly.FromDateTime(DateTime.Now).ToString();
        private static bool tested = false;
        private readonly string apiEndpoint = "http://147.185.221.18:52757";
        private static List<SponsorListItem> sponsorAllLists = new List<SponsorListItem>();
        private List<SponsorListItem> searchResults;
        private static Boolean checkInit = false;

        private readonly JwtService _jwtService; //Authentication

        public SponsorsListController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public IActionResult SponsorList()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_List_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Sponsor List", "ACCESS", "Sponsor_List_Page"); // userID, Message, logType, atPage
            return View();
        }

        //Elmer
        //Retrieve sponsor information from database
        public async Task initializeData()
        {
            SecurityService.Log(GetUserID(), "Sponsor List: Get Sponsor List", "API", "Sponsor_List_Page"); // userID, Message, logType, atPage
            string requestString = "/api/sponsor/";
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.ExecuteAsync(req);
            if (response.IsSuccessful)
            {
                sponsorAllLists = dataCovertIntoList(response.Content);
            }
            else
            {
                Console.WriteLine($"Error: {response.ErrorMessage}");
            }
        }

        private List<SponsorListItem> dataCovertIntoList(string content)
        {
            List<SponsorListItem> result = new List<SponsorListItem>();
            try
            {
                var jsonDocument = JsonDocument.Parse(content);
                var root = jsonDocument.RootElement;
                foreach (var property in root.EnumerateObject())
                {
                    var sponsor = property.Value;
                    SponsorListItem sponsorItem = new SponsorListItem
                    {
                        SponsorId = sponsor.GetProperty("sponsor_id").GetInt64(),
                        SponsorName = sponsor.GetProperty("sponsor_name").GetString(),
                        SponsorPhone = sponsor.GetProperty("office_phone").GetString(),
                        ContactPerson = sponsor.GetProperty("contact_person").GetString(),
                        SponsorCode = sponsor.GetProperty("sponsor_code").GetString(),
                        SponsorAddress = sponsor.GetProperty("address").GetString(),
                        SponsorFax = sponsor.GetProperty("office_fax").GetString()
                    };
                    result.Add(sponsorItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
            return result;
        }


        [HttpGet("getAllSponsor/")]
        public async Task<IActionResult> getAllSponsor()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_List_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            if (!checkInit)
            {
                await initializeData();
                checkInit = false;
            }

            if (sponsorAllLists.Any())
            {
                var searchResults = sortByalphabel(sponsorAllLists);
                SecurityService.Log(GetUserID(), "Sponsor List Data", "ACCESS", "Sponsor_List_Page"); // userID, Message, logType, atPage
                return Ok(searchResults);
            }
            else
            {
                SecurityService.Log(GetUserID(), "Sponsor List Data: Failed", "ACCESS", "Sponsor_List_Page"); // userID, Message, logType, atPage
                Console.WriteLine("No sponsor data available.");
                return NotFound("No sponsor data available");
            }
        }

        [HttpGet("getSponsorId/{sponsorID}")]
        public async Task<IActionResult> getSponsorId(string sponsorID)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_List_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            return RedirectToAction("getSponsorDataTest", "Sponsor", new { sponsorID = sponsorID });
        }

        [HttpGet("getSearchSponsor/{searchName}/{searchCode}/{searchTel}")]
        public async Task<IActionResult> getSearchSponsor(string searchName, string searchCode, string searchTel)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_List_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            searchName = searchName.Trim();
            searchCode = searchCode.Trim();
            searchTel = searchTel.Trim();

            searchResults = new List<SponsorListItem>();
            searchResults = sponsorAllLists;

            searchResults = searchResults.AsParallel().WithDegreeOfParallelism(3).Where(sponsor =>
                (searchCode.Equals("NULL") || sponsor.SponsorCode.IndexOf(searchCode, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (searchName.Equals("NULL") || sponsor.SponsorName.IndexOf(searchName, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (searchTel.Equals("NULL") || sponsor.SponsorPhone.Contains(searchTel))
            ).ToList(); //Create 3 thread running parallel for the search O((N * (code + name + phone)) / 3)

            searchResults = sortByalphabel(searchResults);

            SecurityService.Log(GetUserID(), "Sponsor List: Search Name:[" + searchName + "] | Code:[" + searchCode + "] | Tel:[" + searchTel + "] | Result:[" + searchResults.Count + "]", "ACCESS", "Sponsor_List_Page"); // userID, Message, logType, atPage
            if (searchResults.Count == 0)
            {
                return Ok("Not Found");
            }
            return Ok(searchResults);
        }

        [HttpGet("getSearchSponsorGeneral/{searchGeneral}")]
        public async Task<IActionResult> getSearchSponsorGeneral(string searchGeneral)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_List_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            searchGeneral = searchGeneral.Trim();

            searchResults = new List<SponsorListItem>();
            searchResults = sponsorAllLists;
            bool isNumber = int.TryParse(searchGeneral, out _);

            if (!searchGeneral.Equals("NULL") && isNumber)
            {
                searchResults = searchResults.Where(sponsor =>
                   sponsor.SponsorPhone.Contains(searchGeneral)
               ).ToList();
            }
            else if (!searchGeneral.Equals("NULL"))
            {
                searchResults = searchResults.AsParallel().WithDegreeOfParallelism(3).Where(sponsor =>
                   sponsor.SponsorCode.IndexOf(searchGeneral, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   sponsor.SponsorName.IndexOf(searchGeneral, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   sponsor.ContactPerson.IndexOf(searchGeneral, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   sponsor.SponsorPhone.IndexOf(searchGeneral, StringComparison.OrdinalIgnoreCase) >= 0
               ).ToList();
            }
            searchResults = sortByalphabel(searchResults);

            SecurityService.Log(GetUserID(), "Sponsor List: Search General:[" + searchGeneral + "] | Results:[" + searchResults.Count + "]", "ACCESS", "Sponsor_List_Page"); // userID, Message, logType, atPage
            if (searchResults.Count == 0)
            {
                return Ok("Not Found");
            }
            return Ok(searchResults);
        }

        private List<SponsorListItem> sortByalphabel(List<SponsorListItem> searchResults)
        {
            if (searchResults.Count > 1000)
            {
                var sortedResults = new ConcurrentBag<SponsorListItem>();
                Parallel.ForEach(searchResults.OrderBy(s => s.SponsorCode), sponsor =>
                {
                    sortedResults.Add(sponsor);
                });
                searchResults = sortedResults.ToList();  //(O(n)
            }
            else
            {
                searchResults = searchResults.OrderBy(sponsor => sponsor.SponsorCode).ToList(); // Sort the results alphabetically by sponsor code - O(n log n)
            }

            return searchResults; 
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
