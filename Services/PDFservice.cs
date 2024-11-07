using PFMS_MI04.Models;
using System.Text;
using PuppeteerSharp;
using PFMS_MI04.Models.Security;
using PuppeteerSharp.Media;
using PuppeteerSharp.BrowserData;
using System.Diagnostics;
/* NOTE Currently the column 'sponsor_type' is being used temporarily as the patient's IC*/
namespace PFMS_MI04.Services
{
    public class PDFService
    {
        private static int invoiceNum = 0;
        private static string connectionString = "Server=88.222.244.88;Port=3306;Database=sponsor_schema;User ID=user;Password=mi04Curtin@2024#;";

        public PDFService()
        {
        }

        public async Task<string> CreatePDF2(SponsorListItem sponsor, string rootPath)
        {
            return await GeneratePDFInternal(sponsor, rootPath);
            
        }

        private async Task<string> GeneratePDFInternal(SponsorListItem sponsor, string rootPath)
        {
            invoiceNum++;
            string fileName = GenerateInvoiceFileName();
            // Generate the PDF content based on the fetched patient data
            string htmlContent = GetHTMLString(sponsor, fileName);

            // Load CSS from the stylesheet
            string cssPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "styles.css");
            string cssContent = await File.ReadAllTextAsync(cssPath);
            

            // Embed CSS in the HTML content
            htmlContent = $@"
                <html>
                    {headerHTML()}
                    <hr></hr>
                    <style>{cssContent}</style>
                    <body>
                        {htmlContent}
                    </body>
                </html>";

            var outputPath = Path.Combine(rootPath, "pdfs", fileName);

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = "/var/browsers/Chrome/Linux-131.0.6769.0/chrome-linux64/chrome",
                Args = new[] { "--no-sandbox" }
            });

            var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlContent);

            string footer = "Members of International Federation of Red Cross & Red Crescent Societies";
            var pdfOptions = new PdfOptions
            {
                Format = PuppeteerSharp.Media.PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "45px", Bottom = "10px", Left = "20px", Right = "20px" },
                DisplayHeaderFooter = true,
                FooterTemplate = $@"
                    <div style='font-family: Arial; font-size: 12px; text-align: center; width: 100%; padding: 5px 0;'>
                        <span>{footer}</span>
                    </div>",
            };

            await page.PdfAsync(outputPath, pdfOptions);

            Console.WriteLine("PDF_SERVICE: PDF Generated and saved");

            await browser.CloseAsync();

            return outputPath;
        }






        private string GetHTMLString(SponsorListItem sponsor, string fileName)
        {
            Console.WriteLine("PDF_SERVICE: Generating HTML");
            var tempPatients = sponsor.Items;
            var sb = new StringBuilder();
            int maxRowsPerPage = 29;  // Adjust as per content fitting on a page
            int totalRows = tempPatients.Count;
            int totalPages = (int)Math.Ceiling((double)totalRows / maxRowsPerPage);  // Calculate total pages based on the number of rows
            decimal grandTotal = 0;
            int currentPage = 1;  // Start from page 1
            int currentRowCount = 0;
            String date = DateTime.Now.ToString("dd/MM/yyyy");
            void InsertInvoiceHeader()
            {
                string address = sponsor.SponsorAddress;
                if (!string.IsNullOrEmpty(sponsor.SponsorAddress))
                {
                    address = "";
                    // Split the address into lines
                    var addressLines = sponsor.SponsorAddress.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    // Assign each line to the appropriate cell
                    if (addressLines.Length > 0) address += "<p>" + addressLines[0] + "</p>"; // Line 1
                    if (addressLines.Length > 1) address += "<p>" + addressLines[1] + "</p>"; // Line 2
                    if (addressLines.Length > 2) address += "<p>" + addressLines[2] + "</p>"; // Line 3
                    if (addressLines.Length > 3) address += addressLines[3]; // Line 4
                }

                sb.Append(@"
                    <div class='invoice-header'>
                        <div class='invoice-title'>INVOICE</div>
                        <table>
                            <tr>
                                <td class='address'>
                                    " + address + @"
                                    <p>" + sponsor.ContactPerson + @"</p>
                                    <p>TEL: " + sponsor.SponsorPhone + @"
                                       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 
                                       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                       FAX: " + sponsor.SponsorFax + @"</p>
                                </td>
                                <td class='invoice-details'>
                                    <p>No.: " + fileName.Substring(0, fileName.Length - 4) + @"</p>
                                    <p>Vendor No.: 6000235000</p>
                                    <p>Terms: DUE UPON RECEIPT</p>
                                    <p>Date: " + date + @"</p>
                                    <p class='page-number'>Page: " + currentPage + " of " + totalPages + @"</p>  <!-- Dynamic page number here -->
                                </td>
                            </tr>
                        </table>
                    </div>");

                currentPage++;  // Increment the page number for the next header
            }

            sb.Append("<html><body>");
            InsertInvoiceHeader(); // Insert first header

            sb.Append(@"
                <table align='center'>
                    <tr>
                        <th>Item</th>
                        <th class='wide-column'>Description</th>
                        <th class='centered'>Qty</th>
                        <th class='centered'>UOM</th>
                        <th class='right'>U/ Price<br>RM</th>
                        <th class='right'>Total<br>RM</th>
                    </tr>");

            int itemNumber = 1;

            // Generate table rows
            foreach (var tempPatient in tempPatients)
            {
                decimal total = tempPatient.Qty * tempPatient.Price;
                grandTotal += total;

                sb.AppendFormat(@"
                    <tr>
                        <td class='left'>{0}</td>
                        <td>{1}</td>
                        <td class='centered'>{2}</td>
                        <td class='centered'>" + tempPatient.UOM + @"</td>
                        <td class='right'>{3:N2}</td>
                        <td class='right'>{4:N2}</td>
                    </tr>",
                    itemNumber++, tempPatient.Description, tempPatient.Qty, tempPatient.Price, total);

                // Track row count to decide when to break the page
                currentRowCount++;
                if (currentRowCount >= maxRowsPerPage)
                {
                    // Close current table, insert page break, and new header for the next page
                    sb.Append(@"</table><div style='page-break-after: always;'></div>");
                    InsertInvoiceHeader();

                    // Reset row count for the new page
                    currentRowCount = 0;

                    // Start a new table for the next page
                    sb.Append(@"
                    <table align='center'>
                        <tr>
                            <th>Item</th>
                            <th class='wide-column'>Description</th>
                            <th class='centered'>Qty</th>
                            <th class='centered'>UOM</th>
                            <th class='right'>U/ Price<br>RM</th>
                            <th class='right'>Total<br>RM</th>
                        </tr>");
                }
            }

            sb.Append("</table>");
            sb.Append("<br><br>");

            String grandTotalWord = AmountToWordsConverter.ConvertAmountToWords(grandTotal);

            // Set char limit
            int charLimit = 50;

            // Less than char limit will maintain on same row
            if (grandTotalWord.Length <= charLimit)
            {
                sb.Append(@"
                    </table>
                    <br><br>
                    <!-- Adding the Total, Rounding, and Final Total section -->
                    <table class='total-table'>
                        <tr>
                            <td colspan='3' class='border-top'></td> <!-- Top line -->
                        </tr>
                        <tr>
                            <td class='left-align'>" + grandTotalWord + @"</td>
                            <td class='right-align'>Total</td>
                            <td class='right-align-value'>" + grandTotal.ToString("N2") + @"</td>
                        </tr>
                        <tr>
                            <td class='left-align'></td> <!-- Empty space for alignment -->
                            <td class='right-align'>Rounding Adj.</td>
                            <td class='right-align-value'>0.00</td>
                        </tr>
                        <tr>
                            <td class='left-align'></td> <!-- Empty space for alignment -->
                            <td class='right-align font-bold'>Final Total</td>
                            <td class='right-align-value font-bold'>" + grandTotal.ToString("N2") + @"</td>
                        </tr>
                        <tr>
                            <td colspan='3' class='border-top'></td> <!-- Top line -->
                        </tr>
                    </table>");
            }
            else
            {
                // Closes space before char limit
                int splitIndex = grandTotalWord.LastIndexOf(' ', charLimit);

                // If no space before charLimit, split at the charlimit itself
                if (splitIndex == -1)
                {
                    splitIndex = charLimit;
                }
                // Split the grandtotalword into parts
                string firstPart = grandTotalWord.Substring(0, splitIndex).Trim();
                string secondPart = grandTotalWord.Substring(splitIndex).Trim();
                sb.Append(@"
                    </table>
                    <br><br>
                    <!-- Adding the Total, Rounding, and Final Total section -->
                    <table class='total-table'>
                        <tr>
                            <td colspan='3' class='border-top'></td> <!-- Top line -->
                        </tr>
                        <tr>
                            <td class='left-align'>" + firstPart + @"</td>
                            <td class='right-align'>Total</td>
                            <td class='right-align-value'>" + grandTotal.ToString("N2") + @"</td>
                        </tr>
                        <tr>
                            <td class='left-align'>" + secondPart + @"</td> <!-- Move the rest to the next row -->
                            <td class='right-align'>Rounding Adj.</td>
                            <td class='right-align-value'>0.00</td>
                        </tr>
                        <tr>
                            <td class='left-align'></td> <!-- Empty space for alignment -->
                            <td class='right-align font-bold'>Final Total</td>
                            <td class='right-align-value font-bold'>" + grandTotal.ToString("N2") + @"</td>
                        </tr>
                        <tr>
                            <td colspan='3' class='border-top'></td> <!-- Top line -->
                        </tr>
                    </table>");
            }

            // Signature area
            sb.Append(@"
                <br><br>
                <table class='signature-table'>
                    <tr>
                        <td class='signature-cell'>
                            <hr class='signature-line'>
                            Authorised Signature
                        </td>
                        <td class='empty-cell'></td>
                        <td class='signature-cell'>
                            <hr class='signature-line'>
                            Receiver Signature
                        </td>
                    </tr>
                </table>");

            sb.Append("</body></html>");
            Console.WriteLine("PDF_SERVICE: Finished Generating HTML");
            return sb.ToString();
        }

        private string GetBase64Image(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }

        private string headerHTML()
        {
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "document_generation", "redcresent.png");

            // Get Base64 string for the image
            string base64Image = GetBase64Image(logoPath);
            string dataUrl = $"data:image/png;base64,{base64Image}";

            string header = @"
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                   <style>
                        body {
                            font-family: Arial, sans-serif;
                            text-align: center;
                            margin: 0;
                            padding: 0;
                        }

                        .header-container {
                            display: flex;
                            align-items: flex-start;
                            justify-content: center;
                            padding: 10px; 
                            max-width: 800px; 
                            margin: 0 auto;
                        }

                        .header-logo img {
                            width: 80px;
                            height: auto;
                            margin-right: 10px; 
                        }

                        .header-content {
                            display: flex;
                            flex-direction: column;
                            align-items: center; 
                            text-align: center;
                        }

                        .header-content h2 {
                            font-size: 18px;
                            margin: 0;
                        }

                        .header-content h3 {
                            font-size: 16px;
                            color: grey;
                            margin: 5px 0;
                        }

                        .header-content p {
                            font-size: 14px; 
                            margin: 5px 0;
                            color: grey;
                        }

                        .header-table {
                            margin-top: 5px;
                            width: auto;
                            font-size: 12px;
                        }

                        .header-table td {
                            padding: 0 5px 0 0;
                            border: none;
                            color: grey;
                            text-align: left;
                        }
                    </style>


                </head>
                <body>
                    <div class=""header-container"">
                        <div class=""header-logo"">
                            <img src=""" + dataUrl + @""" alt=""Red Crescent Logo"">
                        </div>
                        <div class=""header-content"">
                            <h2>BULAN SABIT MERAH MALAYSIA, DAERAH MIRI</h2>
                            <h3>MALAYSIA RED CRESCENT, MIRI BRANCH</h3>
                            <p>Lot 312, Jalan Bulan Sabit, Krokop 1, 98007, Miri, Sarawak.</p>
                            <table class=""header-table"">
                                <tr>
                                    <td>Branch Administrative Office </td>
                                    <td>T 085 411 121</td>
                                    <td>F 085 420 479</td>
                                    <td>E miri@redcrescent.org.my</td>
                                </tr>
                                <tr>
                                    <td>Training Institute </td>
                                    <td>T 085 420 236</td>
                                    <td>F 085 430 236</td>
                                    <td>E mrc.training@hotmail.com</td>
                                </tr>
                                <tr>
                                    <td>Dialysis Centre (Miri) </td>
                                    <td>T 085 436 862</td>
                                    <td>F 085 436 457</td>
                                    <td>E mrcdc1996@gmail.com</td>
                                </tr>
                                <tr>
                                    <td>Dialysis Centre (Permyjaya) </td>
                                    <td>T 085 622 588 </td>
                                    <td>F 085 418 788 </td>
                                    <td>E pmrcdc2022@gmail.com</td>
                                </tr>
                                <tr>
                                    <td>Sunflower Centre </td>
                                    <td>T 085 420 722 </td>
                                    <td>F 085 425 421</td>
                                    <td>E sunflower.miri@gmail.com</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </body>
                ";

            return header;
        }

        private async Task<string> downloadBrowser()
        {
            Console.WriteLine("Downlading Browser");
            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = "/var/browsers"
            });

            await browserFetcher.DownloadAsync(BrowserTag.Latest);

            Console.WriteLine("Downloaded Browser");

            return "Downloaded Browser";
        }

        public static string GenerateInvoiceFileName()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoiceCount.txt");

            int invoiceNum;
            string currentMonth = DateTime.Now.ToString("yyMM");

            // Read the stored data
            string[] fileData = File.Exists(filePath) ? File.ReadAllLines(filePath) : new string[] { "0", currentMonth };

            // Parse the last invoice number and month
            invoiceNum = int.Parse(fileData[0]);
            Console.WriteLine(invoiceNum);
            string lastMonth = fileData[1];

            // Check if the current month is different from the last recorded month
            if (currentMonth != lastMonth)
            {
                invoiceNum = 0;  // Reset the count if it's a new month
            }

            // Increment the invoice number
            invoiceNum++;

            // Generate the file name
            string fileName = "KDI-" + DateTime.Now.ToString("yyMM-") + invoiceNum.ToString("D4") + ".pdf";
            // Save the new invoice number and current month back to the file
            File.WriteAllLines(filePath, new string[] { invoiceNum.ToString(), currentMonth });

            return fileName;
        }
    }



}

