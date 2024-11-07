using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Patients;
using PFMS_MI04.Testing.PatientsUnitTest;
using RestSharp;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Services;
using System.IO;

namespace PFMS_MI04.Controllers
{
    public class PatientsController : Controller
    {
        private string dateLF = DateOnly.FromDateTime(DateTime.Now).ToString("dd MMMM, yyyy");
        private static bool tested = false;
        private readonly string apiEndpoint = "http://147.185.221.18:52757";
        private readonly int numOfListItemsPerPagePL = 20;
        private readonly int refreshDelay = 600000; // 10 Minutes
        private static List<PatientProfile> dataStash = new List<PatientProfile>(); // Stashed Data, Prevent repeating large API calls
        private StrComparator logicCore = new StrComparator();
        private SecLoggingService sLogging = new SecLoggingService();
        private PatientDocService documentService = new PatientDocService();
        private static Thread dataRefresherThread;
        private static bool threadOnFlag = true;

        private readonly JwtService _jwtService;  //Authentication

        public PatientsController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public IActionResult PatientList()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_List"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            SecurityService.Log(GetUserID(), "Patient List Opened", "ACCESS", "Patient_List"); // userID, Message, logType, atPage

            onStart();
            ViewBag.DateLongForm = dateLF;
            return View();
        }

        [HttpPost("patientquery")]
        public IActionResult patientListQuery()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_List"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            string patID = Request.Form["patID"].ToString();
            ViewBag.DateLongForm = dateLF;
            ViewBag.givenID = patID;

            SecurityService.Log(GetUserID(), "Patient Selected:" + patID, "ACCESS", "Patient_List"); // userID, Message, logType, atPage

            return View("PatientInfo");
        }

        //Will likely not be used
        public IActionResult PatientInfo()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_List"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            SecurityService.Log(GetUserID(), "Patient Info Opened: No Patient Selected", "ACCESS", "Patient_List"); // userID, Message, logType, atPage

            return View();
        }

        // ----- Data Controllers ----- //

        // -- PatientList -- //

        // Returns a list of patient list objects on the page viewed
        // Items per page: 20
        private async Task<List<PatientListItem>> getPatientListData(int lPage, bool all)
        {
            List<PatientListItem> resList = new List<PatientListItem>();
            List<PatientProfile> tempList = await getPatientProfileData(lPage, all, false);

            foreach (PatientProfile prof in tempList)
            {
                resList.Add(prof.toListItem());
            }

            tempList.Clear();

            foreach (PatientListItem item in resList)
            {
                item.PatientPhone = logicCore.dashSplitter(item.PatientPhone, "3-8");
            }

            return resList;
        }

        // Returns a list of patient profiles depending on the page viewed
        // Items per page: 20
        private async Task<List<PatientProfile>> getPatientProfileData(int lPage, bool all, bool refresh)
        {
            List<PatientProfile> resList = new List<PatientProfile>();
            int ind1 = lPage * numOfListItemsPerPagePL; // 0 = 0, 1 = 20, 2 = 40
            int ind2 = ind1 + (numOfListItemsPerPagePL - 1);
            if (all)
            {
                ind1 = 0;
                ind2 = 9999; //Funny Lag moment
            }

            if (dataStash.Count > 0 && !refresh) // If datastash is available and not on refresh mode
            {
                for (int i = ind1; i <= ind2; i++)
                {
                    if (i < dataStash.Count)
                    {
                        resList.Add(dataStash[i]);
                    }
                }
            }
            else
            {
                SecurityService.Log(GetUserID(), "API Call: PatientProfile", "API", "Patient_List"); // userID, Message, logType, atPage
                List<PatientProfile> tempList = new List<PatientProfile>();
                string requestString = "/api/patient/profile/" + 0 + "/" + 9999; // get a specified number of entries
                RestClient restClient = new RestClient(apiEndpoint);
                RestRequest req = new RestRequest(requestString, Method.Get);
                RestResponse response = await restClient.GetAsync(req);
                if (response.IsSuccessStatusCode)
                {
                    var patList = JsonConvert.DeserializeObject<List<object>>(response.Content);
                    if (patList != null)
                    {
                        foreach (var jsonItem in patList)
                        {
                            PatientProfile prof = logicCore.patientProfParser(jsonItem);
                            tempList.Add(prof);
                        }
                        tempList = logicCore.sortPatients(tempList); // Sorting alphabetically

                        for (int i = ind1; i <= ind2; i++)
                        {
                            if (i < tempList.Count)
                            {
                                resList.Add(tempList[i]);
                            }
                        }

                        if (all)
                        {
                            dataStash = new List<PatientProfile>();
                            foreach (PatientProfile prof in tempList)
                            {
                                dataStash.Add(prof);
                            }
                        }
                        patList.Clear();
                        tempList.Clear();
                    }
                }
                GC.Collect(); // Crude Memory Optimisation
            }
            return resList;
        }

        [HttpGet("getPatList/{lPage}")]
        public async Task<IActionResult> getPatientList(int lPage)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_List"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            List<PatientListItem> resList = await getPatientListData(lPage, false);
            if (resList != null)
            {
                return Ok(JsonConvert.SerializeObject(resList));
            }
            else
            {
                SecurityService.Log(GetUserID(), "Get Patient List: Get Data Failed!", "ERROR", "Patient_List"); // userID, Message, logType, atPage
                return NotFound();
            }
        }

        // Returns a sorted list of patients depending on the given search string
        [HttpGet("getPatListFilt/{searchStr}")]
        public async Task<IActionResult> patientListFilter(string searchStr)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_List"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            searchStr = logicCore.inputSanitizer(searchStr);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<PatientListItem> rPList = await getPatientListData(1, true);
            watch.Stop();
            // Sort
            if (rPList != null)
            {
                watch.Reset();
                watch.Start();
                rPList = logicCore.sortPatients(rPList, searchStr);
                watch.Stop();
                SecurityService.Log(GetUserID(), "Filter Patient: " + searchStr, "ACCESS", "Patient_List"); // userID, Message, logType, atPage
                return Ok(JsonConvert.SerializeObject(rPList));
            }
            SecurityService.Log(GetUserID(), "Get Patient List Filter: Get Data Failed!", "ERROR", "Patient_List"); // userID, Message, logType, atPage
            return NotFound();
        }

        // Returns a sorted list of patients depending on given expanded search details
        [HttpPost("getPatExpFilt")]
        public async Task<IActionResult> patientListExpFilter([FromBody] ExpPatientSearch data)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_List"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            List<PatientProfile> patProfList = await getPatientProfileData(1, true, false);
            if (patProfList.Count > 0)
            {
                List<PatientListItem> items = logicCore.sortPatients(patProfList, data);
                SecurityService.Log(GetUserID(), "Exp Filter: Search Query: " + data.ToString(), "ACCESS", "Patient_List"); // userID, Message, logType, atPage
                return Ok(JsonConvert.SerializeObject(items));
            }
            else
            {
                SecurityService.Log(GetUserID(), "Get Patient List ExpFilter: Get Data Failed!", "ERROR", "Patient_List"); // userID, Message, logType, atPage
                return NotFound();
            }
        }

        // -- PatientInfo -- //

        //Returns list of historical patient treatment data as a JSON format
        [HttpGet("getPatHist/{patId}")]
        public async Task<IActionResult> getPatientHist(string patId)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "API Call: HDRecord for :" + patId, "API", "Patient_Info"); // userID, Message, logType, atPage
            List<PatientHDRecord> records = null;
            string requestString = "/api/patient/hdrecord/" + patId;
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

        //Returns a filtered list of patient treatment data as a sorted JSON list
        [HttpGet("getPatHistFilt/{patId}/{inDate}")]
        public async Task<IActionResult> getPatientHistFilt(string patId, string inDate)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            inDate = logicCore.inputSanitizer(inDate);

            SecurityService.Log(GetUserID(), "API Call: HDRecFilter: PatientID: " + patId + " Input: " + inDate, "API", "Patient_Info"); // userID, Message, logType, atPage
            List<PatientHDRecord> records;
            PatientHDRecord record;
            string requestString = "/api/patient/hdrecord/" + patId;
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);
            if (response.IsSuccessStatusCode)
            {
                //StrComparator.println(response.Content);
                records = new List<PatientHDRecord>();
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                if (jsonData != null)
                {
                    //StrComparator.printDictionary(jsonData);
                    var hdrList = JsonConvert.DeserializeObject<List<object>>(jsonData["HDRecords"].ToString());
                    if (hdrList != null)
                    {
                        //StrComparator.printList(hdrList);
                        foreach (var hdrEntry in hdrList)
                        {
                            record = logicCore.hdrParser(hdrEntry);
                            records.Add(record);
                        }
                        // Filter and Sort :)) 
                        if (!(inDate.Equals("") || inDate.Equals(String.Empty) || inDate.Equals("q")))
                        {
                            records = logicCore.sortHDRecords(records, inDate);
                        }
                        return Ok(JsonConvert.SerializeObject(records));
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("getPatHistFilt/{patId}/{month}/{year}")]
        public async Task<IActionResult> getPatientHistFilt(string patId, int month, int year)
        {
            // Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || (userRole != "Admin" && userRole != "User"))
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info");
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), $"API Call: HDRecFilter: PatientID: {patId} Month: {month} Year: {year}", "API", "Patient_Info");
            List<PatientHDRecord> records;
            string requestString = $"/api/patient/hdrecord/{patId}";
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);

            if (response.IsSuccessStatusCode)
            {
                records = new List<PatientHDRecord>();
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                if (jsonData != null)
                {
                    var hdrList = JsonConvert.DeserializeObject<List<object>>(jsonData["HDRecords"].ToString());
                    if (hdrList != null)
                    {
                        foreach (var hdrEntry in hdrList)
                        {
                            var record = logicCore.hdrParser(hdrEntry);
                            records.Add(record);
                        }

                        // Filter by month and year
                        records = records
                            .Where(r => DateTime.TryParse(r.date, out DateTime recDate) && recDate.Month == month && recDate.Year == year)
                            .ToList();

                        return Ok(JsonConvert.SerializeObject(records));
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return NotFound();
            }
        }

        //Returns patient profile data as a JSON format
        [HttpGet("getPat/{patId}")]
        public async Task<IActionResult> getPatientData(string patId)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "API Call: PatientProfile: " + patId, "API", "Patient_Info"); // userID, Message, logType, atPage
            string requestString = "/api/patient/" + patId;
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

        // Returns a list of documents associated with the patient NRIC
        [HttpGet("getDocs/{patID}")]
        public async Task<IActionResult> getDocumentsForNRIC(string patID) // NRIC
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            List<PatientDocument> patDocs = await documentService.getDocs(patID);

            if(patDocs != null)
            {
                List<PatientDoc> documents = new List<PatientDoc>();
                foreach (PatientDocument docs in patDocs)
                {
                    docs.base64String = Convert.ToBase64String(docs.documentByteCode);
                    documents.Add(docs.toPatientDoc());
                }
                SecurityService.Log(GetUserID(), "GET Documents for: " + patID + ", returned: " + patDocs.Count, "ACCESS", "Patient_Info"); // userID, Message, logType, atPage
                return Ok(JsonConvert.SerializeObject(documents));
            }
            return NotFound();
        }

        // Returns a list of filtered and sorted documents that best suit the search String
        [HttpGet("getDocFilt/{patID}/{searchStr}")]
        public async Task<IActionResult> searchDocByName(string patID, string searchStr)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            List<PatientDocument> patDocs = await documentService.getDocs(patID);

            if (patDocs != null)
            {
                // Filter
                patDocs = logicCore.sortDocuments(patDocs, searchStr);
                List<PatientDoc> documents = new List<PatientDoc>();
                foreach (PatientDocument docs in patDocs)
                {
                    // Conversion is redundant at this point, but just in case
                    docs.base64String = Convert.ToBase64String(docs.documentByteCode);
                    documents.Add(docs.toPatientDoc());
                }
                SecurityService.Log(GetUserID(), "Patient " + patID + " documents: Searched: " + searchStr + ", returned: " + patDocs.Count() + " items", "ACCESS", "Patient_Info"); // userID, Message, logType, atPage
                return Ok(JsonConvert.SerializeObject(documents));
            }
            return NotFound();
        }

        // Returns a response depending if the uploaded document is successful
        [HttpPost("PatientDocumentUpload")]
        public async Task<IActionResult> newDocumentUpload(IFormFile file, [FromForm] string data)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            if (file != null && file.Length > 0)
            {
                StrComparator.println("Upload: Filename: " + file.FileName);

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    byte[] fileBytes = stream.ToArray();

                    PatientDoc pD = JsonConvert.DeserializeObject<PatientDoc>(data);
                    PatientDocument pDoc = new PatientDocument(pD, fileBytes);
                    if (pDoc != null)
                    {
                        //PatientDocument patientDocument = new PatientDocument(docData.patientDoc, filePath);
                        // Upload to SQL the reference
                        if (await documentService.uploadDoc(pDoc))
                        {
                            SecurityService.Log(GetUserID(), "Patient " + pDoc.patientID + " documents: Uploaded: " + pDoc.docName, "CHANGE", "Patient_Info"); // userID, Message, logType, atPage
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
        [HttpPost("updateDoc")]
        public async Task<IActionResult> updateDoc([FromBody] PatientDoc doc)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            if(doc != null)
            {
                doc.sanitizeRemarks();
                if (await documentService.updateDoc(doc))
                {
                    SecurityService.Log(GetUserID(), "Patient " + doc.patId + " documents: Updated: " + doc.docName, "CHANGE", "Patient_Info"); // userID, Message, logType, atPage
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
        [HttpGet("deleteDoc/{docID}")]
        public async Task<IActionResult> deleteDoc(string docID)
        {
            int docid = Int32.Parse(docID);
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_Info"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            if(docid > 0)
            {
                if (await documentService.deleteDoc(docid))
                {
                    SecurityService.Log(GetUserID(), "Patient Documents: Deleted: " + docID, "CHANGE", "Patient_Info"); // userID, Message, logType, atPage
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

        // -- SponsorList (Minor) : Taken from SponsorController.cs -- //
        [HttpGet("getSpon")]
        public async Task<IActionResult> getSponsorData()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Patient_List"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "API Call: Sponsors", "API", "Patient_List"); // userID, Message, logType, atPage
            string requestString = "/api/sponsor";
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

        // ----- Other Functions ----- //

        private void onStart()
        {
            if (!tested)
            {
                StrComparatorUnitTest test = new StrComparatorUnitTest();
                StrComparator.println(test.test());
                tested = true;
                SecurityService.Log(GetUserID(), "Unit Test: Patients", "SECURITY", "Patient_List"); // userID, Message, logType, atPage
            }
            if(dataStash.Count == 0)
            {
                autoRefresh(); // Starts up the data refresher thread
            }
        }

        // ----- MultiThreading ----- //

        // Initializes the auto refreshing of data
        private void autoRefresh()
        {
            dataRefresherThread = new Thread(new ThreadStart(dataRefreshT));
            dataRefresherThread.Start();
            SecurityService.Log(GetUserID(), "Patient List: Data Refresh Thread Start", "CHANGE", "Patient_List"); // userID, Message, logType, atPage
            // Let the thread do its thing
        }

        // Joins the thread
        private void stopRefresher() 
        {
            SecurityService.Log(GetUserID(), "Patient List: Data Refresh Thread Stopped", "CHANGE", "Patient_List"); // userID, Message, logType, atPage
            retreiveOnFlag(-1);
            dataRefresherThread.Join();
        }

        // Thread Function for data refresh
        private void dataRefreshT()
        {
            try
            {
                while (retreiveOnFlag(0))
                {
                    // Do data refresh if needed
                    getPatientProfileData(1, true, true);
                    Thread.Sleep(refreshDelay);
                }
            }
            catch (ThreadAbortException taE)
            {
                SecurityService.Log(GetUserID(), "Patient List: Data Refresh Thread Stopped " + taE.Message, "ERROR", "Patient_List"); // userID, Message, logType, atPage
                StrComparator.println("ThreadAbortException: " + taE.Message);
            }
            catch (ThreadInterruptedException tiE)
            {
                SecurityService.Log(GetUserID(), "Patient List: Data Refresh Thread Stopped " + tiE.Message, "ERROR", "Patient_List"); // userID, Message, logType, atPage
                StrComparator.println("ThreadInterruptedException: " + tiE.Message);
            }
        }

        // Retrieving the thread-is-on flag
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool retreiveOnFlag(int retrieval)
        {
            if(retrieval == 0)
            {
                //get the flag as is
            }
            else if(retrieval == 1) //set to true
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
