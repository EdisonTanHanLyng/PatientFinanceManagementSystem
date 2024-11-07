using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Models.Patients;
using PFMS_MI04.Services;
using RestSharp;
using System.IO;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PFMS_MI04.Controllers
{
    public class SponsorController : Controller
    {
        private readonly ILogger<SponsorController> _logger;
        private static int limit = 10;
        private static List<PatientListItem> patientList = new List<PatientListItem>();
        private static bool filledList = false;
        private static string sponsorIDFix = "1";
        
        private StrComparator logicCore = new StrComparator();
        private SecLoggingService sLogging = new SecLoggingService();
        private SponsorDocService documentService = new SponsorDocService();
        private static bool threadOnFlag = true;

        private readonly string apiEndpoint = "http://147.185.221.18:52757";
        private readonly JwtService _jwtService;  //Authentication

        public SponsorController(ILogger<SponsorController> logger, JwtService jwtService)
        {
            _logger = logger;
            _jwtService = jwtService; //Authentication

            if (filledList == false)
            {
                Console.WriteLine(filledList);
                generateList();
                filledList = true;
                Console.WriteLine(filledList);
            }
        }

        public IActionResult Add_Patient_List()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Sponsor Info: Add Patient List", "ACCESS", "Sponsor_Info_Page"); // userID, Message, logType, atPage
            return View();
        }

        public IActionResult SponsorInfo() 
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            SecurityService.Log(GetUserID(), "Sponsor Info", "ACCESS", "Sponsor_Info_Page"); // userID, Message, logType, atPage
            return View(); 
        }

        /* Ariel here: This was causing a build error
        public IActionResult SponsorInfo()
        {
            return View();
        }
        */

        [HttpGet]
        public ActionResult<IEnumerable<PatientListItem>> getList()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            return Ok(patientList);
        }

        private void generateList()
        {
            for (int i = 0; i < limit; i++)
            {
                patientList.Add(new PatientListItem());
            }
        }

        public IActionResult tem()
        {
            return View();
        }

        //Elmer Testing
        [HttpGet("/getSponsorIdTest/{sponsorID}")]
        public async Task<IActionResult> getSponsorDataTest(string sponsorID)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "Sponsor Info: Get Sponsor Data", "API", "Sponsor_Info_Page"); // userID, Message, logType, atPage
            sponsorIDFix = sponsorID.Trim();
            string requestString = "/api/sponsor/" + sponsorID;
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content);
            }
            else
            {
                return NotFound();
            }
        }


        // ----- Data Controllers ----- //
        [HttpGet("/getSponsor/{sponsorID}")]
        public async Task<IActionResult> getSponsorData(string sponsorID)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "Sponsor Info: Get Sponsor Data", "API", "Sponsor_Info_Page"); // userID, Message, logType, atPage
            Console.WriteLine("Sponsor ID Recieve: " + sponsorID);
            //string requestString = "/api/sponsor/" + sponsorID;
            string requestString = "/api/sponsor/" + sponsorID;    //Elmer: Just to get the specific user
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("/getPatient/{patientIDStart}/{patientIDEnd}")]
        public async Task<IActionResult> getPatientData(string patientIDStart, string patientIDEnd)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "Sponsor Info: Get Patient List", "API", "Sponsor_Info_Page"); // userID, Message, logType, atPage
            string requestString = "/api/patient/hdrecord/" + patientIDStart + "/" + patientIDEnd;
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("/getPatientName/{patientIDStart}/{patientIDEnd}")]
        public async Task<IActionResult> getPatientName(string patientIDStart, string patientIDEnd)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "Sponsor Info: Get Patient Name List", "API", "Sponsor_Info_Page"); // userID, Message, logType, atPage
            string requestString = "/api/patient/profile/" + patientIDStart + "/" + patientIDEnd;
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content);
            }
            else
            {
                return NotFound();
            }
        }

        //copied and modified from Ariel's code as he came up with a better way
        // Returns a list of documents associated with the patient
        [HttpGet("getSponDocs/{sponID}")]
        public async Task<IActionResult> getDocumentsForNRIC(string sponID) // NRIC!
        {
            //StrComparator.println("Get Docs Called");
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            List<SponsorDocument> SponDocs = await documentService.getDocs(sponID);

            if (SponDocs != null)
            {
                List<SponsorDoc> documents = new List<SponsorDoc>();
                foreach (SponsorDocument docs in SponDocs)
                {
                    docs.base64String = Convert.ToBase64String(docs.documentByteCode);
                    documents.Add(docs.toPatientDoc());
                }
                return Ok(JsonConvert.SerializeObject(documents));
            }
            return NotFound();
        }

        // Returns a list of filtered and sorted documents that best suit the search String
        [HttpGet("getSponDocFilt/{spoID}/{searchStr}")]
        public async Task<IActionResult> searchDocByName(string spoID, string searchStr)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            List<SponsorDocument> SponDocs = await documentService.getDocs(spoID);

            if (SponDocs != null)
            {
                // Filter
                SponDocs = logicCore.sortDocuments(SponDocs, searchStr);
                List<SponsorDoc> documents = new List<SponsorDoc>();
                foreach (SponsorDocument docs in SponDocs)
                {
                    // Conversion is redundant at this point, but just in case
                    docs.base64String = Convert.ToBase64String(docs.documentByteCode);
                    documents.Add(docs.toPatientDoc());
                }
                SecurityService.Log(GetUserID(), "Sponsor " + spoID + " documents: Searched: " + searchStr + ", returned: " + SponDocs.Count() + " items", "ACCESS", "Sponsor_Info"); // userID, Message, logType, atPage
                return Ok(JsonConvert.SerializeObject(documents));
            }
            return NotFound();
        }

        // Returns a response depending if the uploaded document is successful
        [HttpPost("/SponsorDocumentUpload")]
        public async Task<IActionResult> newDocumentUpload(IFormFile file, [FromForm] string data)
        {
            Console.WriteLine(file.FileName);
            Console.WriteLine(data);
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            StrComparator.println("Enter Sponsor Upload Document");
            if (file != null && file.Length > 0)
            {
                StrComparator.println("Upload: Filename: " + file.FileName);

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    byte[] fileBytes = stream.ToArray();

                    SponsorDoc sD = JsonConvert.DeserializeObject<SponsorDoc>(data);
                    SponsorDocument sDoc = new SponsorDocument(sD, fileBytes);
                    if (sDoc != null)
                    {
                        //PatientDocument patientDocument = new PatientDocument(docData.patientDoc, filePath);
                        // Upload to SQL the reference
                        //StrComparator.println("sDoc Upload");
                        if (await documentService.uploadDoc(sDoc))
                        {
                            SecurityService.Log(GetUserID(), "Sponsor " + sDoc.sponsorId + " documents: Uploaded: " + sDoc.docName, "CHANGE", "Sponsor_Info"); // userID, Message, logType, atPage
                            StrComparator.println("POST:[SUCCESS => Uploaded New Document!]");
                            return Ok();
                        }
                        else
                        {
                            StrComparator.println("POST:[FAILED => Failed to Upload...]");
                            return BadRequest();
                        }
                    }
                    else
                    {
                        NotFound();
                    }
                }
            }
            else
            {
                StrComparator.println("formFile was NULL!");
            }
            return BadRequest();
        }

        // Returns a response depending if the document has been successfully updated
        [HttpPost("updateSponDoc")]
        public async Task<IActionResult> updateDoc([FromBody] SponsorDoc doc)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            if (doc != null)
            {
                if (await documentService.updateDoc(doc))
                {
                    SecurityService.Log(GetUserID(), "Sponsor " + doc.sponID + " documents: Updated: " + doc.docName, "CHANGE", "Sponsor_Info"); // userID, Message, logType, atPage
                    StrComparator.println("POST:[SUCCESS => Updated Document!]");
                    return Ok();
                }
                else
                {
                    StrComparator.println("POST:[FAILED => Failed to Update Document...]");
                    return BadRequest();
                }
            }
            else
            {
                StrComparator.println("doc was NULL!");
                return NotFound();
            }
        }

        // Returns a response depending is the document has been successfully deleted
        [HttpGet("deleteSponDoc/{docID}")]
        public async Task<IActionResult> deleteDoc(string docID)
        {
            int docid = Int32.Parse(docID);
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Sponsor_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            if (docid > 0)
            {
                if (await documentService.deleteDoc(docid))
                {
                    SecurityService.Log(GetUserID(), "Sponsor Documents: Deleted: " + docID, "CHANGE", "Sponsor_Info"); // userID, Message, logType, atPage
                    StrComparator.println("POST:[SUCCESS => Deleted Document!]");
                    return Ok();
                }
                else
                {
                    StrComparator.println("POST:[FAILED => Failed to Delete Document...]");
                    return BadRequest();
                }
            }
            else
            {
                StrComparator.println("docID was set to " + docid + "!");
                return NotFound();
            }
        }


        // Retrieving the thread-is-on flag
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool retreiveOnFlag(int retrieval)
        {
            if (retrieval == 0)
            {
                //get the flag as is
            }
            else if (retrieval == 1) //set to true
            {
                threadOnFlag = true;
            }
            else // set to false
            {
                threadOnFlag = false;
            }
            return threadOnFlag;
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

   

