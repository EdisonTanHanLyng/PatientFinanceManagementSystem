using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using PFMS_MI04.Models;
using PFMS_MI04.Models.Authentication;
using PFMS_MI04.Models.Patients;
using PFMS_MI04.Services;
using RestSharp;
using System;
using System.Text;

namespace PFMS.Controllers
{
    public class DocumentGenController : Controller
    {
        private readonly string apiEndpoint = "http://147.185.221.18:52757";
        private readonly PDFService _pdfService;
        private readonly JwtService _jwtService;  //Authentication
        private readonly IWebHostEnvironment _env; //File Path
        public DocumentGenController(PDFService pdfService, JwtService jwtService, IWebHostEnvironment env)
        {
            _pdfService = pdfService;
            _jwtService = jwtService;
            _env = env;
        }

        /* Selection page for document type */
        public IActionResult DocumentGen()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Document_Generation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            SecurityService.Log(GetUserID(), "Document Page Opened", "ACCESS", "Document_Generation"); // userID, Message, logType, atPage
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";
            return View();
        }

        public ActionResult DocuGenInvoice()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            return View();
        }

        public ActionResult STEC()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            return View();
        }

        public ActionResult IPPKL()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            return View();
        }
        public ActionResult JPIIE()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            return View();
        }

        public ActionResult LHDN()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            return View();
        }
        public ActionResult SOCSO()
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            return View();
        }

        public IActionResult SelectPatient() 
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

        /* =====================================Document Generation==================================*/

        /* Generates Document */
        [HttpPost]
        public async Task<IActionResult> GeneratePdf([FromBody] SponsorListItem model)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Document_Generation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            if (model == null)
            {
                model = new SponsorListItem();
            }
            // Generate the PDF based on the selected patients
            string pdfFilePath = await _pdfService.CreatePDF2(model , _env.WebRootPath);
            SecurityService.Log(GetUserID(), "Document Generated from: " + model.SponsorCode, "ACCESS", "Document_Generation"); // userID, Message, logType, atPage
            // Return the path or filename of the generated PDF
            return Json(new { fileName = Path.GetFileName(pdfFilePath) });
        }

        public IActionResult DocuGenPreview(string fileName)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Layout = GetUserRole() == "Admin" ? "_Layout" : "_Layout_staff";

            string pdfPath = Path.Combine(_env.WebRootPath, "pdfs", fileName);
            // Pass the PDF file path to the view as a ViewBag or ViewData
            
            if (System.IO.File.Exists(pdfPath))
            {
                var fileStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read);
                ViewBag.PdfFilePath = "/pdfs/" + Path.GetFileName(pdfPath);
                SecurityService.Log(GetUserID(), "Document Preview: " + fileName, "ACCESS", "Document_Generation"); // userID, Message, logType, atPage
                return View();
            }
            else
            {
                return NotFound(); // Handle missing PDF scenario
            }
            // Return the view with the file path
            
        }

        /* =====================================Excel Generation=======================================*/
        [HttpPost]
        public async Task<IActionResult> UpdatePricing([FromBody] PricingModel model)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Create_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var creationStatus = new { success = false, responseMessage = "Failed to update price!" };
            if (model != null)
            {
                var response = await PricingService.updatePrice(model);
                if (response)
                {
                    creationStatus = new { success = true, responseMessage = "Successfully updated price" };
                }
                SecurityService.Log(GetUserID(), "Updated Price: [Sponsor = " + model.SponsorCode + "," + "Dialysis Cost = " + model.DialysisCost + "," + "EPO Cost = " + model.EPOCost + "]", "CHANGE", "Document_Generation_Page"); 
            }

            return Json(creationStatus);
        }

        [HttpGet]
        public async Task<IActionResult> GetPricing(string sponsorCode)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Create_Reminder_Page"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            Console.WriteLine("HELLO");
            var creationStatus = new { success = false, responseMessage = "Failed to get price!", data = new PricingModel() };
            if (!string.IsNullOrEmpty(sponsorCode))
            {
                var response = await PricingService.getPrice(sponsorCode);
                if (response != null)
                {
                    creationStatus = new { success = true, responseMessage = "Successfully got price", data = response };
                }
            }

            return Json(creationStatus);
        }

        [HttpPost]
        public IActionResult GenerateSTEC([FromBody] STEC model)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Document_Generation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var fileName = ExcelService.STECTemplate(model, _env.WebRootPath);
            // Return the path or filename of the generated PDF
            return Ok(new { fileName = Path.GetFileName(fileName) });
        }

        [HttpPost]
        public IActionResult GenerateIPPKL([FromBody] IPPKKL model)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Document_Generation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var fileName = ExcelService.IPPKTemplate(model, _env.WebRootPath);
            // Return the path or filename of the generated PDF
            return Ok(new { fileName = Path.GetFileName(fileName) } );
        }

        [HttpPost]
        public IActionResult GenerateJPIIE([FromBody] JPIIE model)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Document_Generation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var fileName = ExcelService.JPIIETemplate(model, _env.WebRootPath);
            // Return the path or filename of the generated PDF
            return Ok(new { fileName = Path.GetFileName(fileName) });
        }

        [HttpPost]
        public IActionResult GenerateLHDN([FromBody] LHDN model)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Document_Generation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }

            var fileName = ExcelService.LHDNTemplate(model, _env.WebRootPath);
            // Return the path or filename of the generated PDF
            return Ok(new { fileName = Path.GetFileName(fileName) });
        }

        [HttpGet("/getPatientDoc/{patientIDStart}/{patientIDEnd}")]
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

            Console.WriteLine(response.Content);
            List<PatientHDRecord> list = JsonConvert.DeserializeObject<List<PatientHDRecord>>(response.Content);

            foreach (var i in list) {
                Console.WriteLine(i.patientID);
            }
            
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost]
        public IActionResult GenerateSOCSO([FromBody] SOCSO model)
        {
            //Authentication verify
            string userRole = GetUserRole();
            if (!IsUserAuthenticated() || userRole != "Admin" && userRole != "User") //If user enter without cookie back to login page
            {
                SecurityService.Log("UnAuth", "User Redirected to Login", "AUTH", "Document_Generation"); // userID, Message, logType, atPage
                return RedirectToAction("Login", "Login");
            }
            Console.WriteLine("======================================================="+model);
            var fileName = ExcelService.SOCSOTemplate(model, _env.WebRootPath);
            // Return the path or filename of the generated PDF
            return Ok(new { fileName = Path.GetFileName(fileName) });
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
            string requestString = "/api/patient/profile/0/9999";
            RestClient restClient = new RestClient(apiEndpoint);
            RestRequest req = new RestRequest(requestString, Method.Get);
            RestResponse response = await restClient.GetAsync(req);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content);
            }

            return NotFound();
        }

        /* =============================================================================================*/
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
