using PFMS_MI04.Models;
using OfficeOpenXml;

namespace PFMS_MI04.Services
{
    public class ExcelService
    {
        private static string[] months = {"Januari", "Februari", "Mac", "April", "Mei", "Jun", "Julai",
                                    "Ogos", "September", "Oktober", "November", "Disember"
                                   };


        public static string STECTemplate(STEC model, string rootPath)
        {
            string patientName = "No_Name";
            if(model.Name != "") { patientName = model.Name; }
            string fileName = patientName + ".xlsx";

            // Load the template
            var templatePath = Path.Combine(rootPath, "template", "STEC_Template.xlsx");
            var newFilePath = Path.Combine(rootPath, "template", fileName);

            FileInfo templateFile = new FileInfo(templatePath);
            FileInfo newFile = new FileInfo(newFilePath);

            using (ExcelPackage package = new ExcelPackage(templateFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                // Set static data in the worksheet
                string today = DateTime.Now.ToString("MMMM dd, yyyy");
                worksheet.Cells["C3"].Value = today;
                worksheet.Cells["C12"].Value = model.Name;
                worksheet.Cells["G13"].Value = model.Day + "/" + model.Month + "/" + model.Year;

                if (!string.IsNullOrEmpty(model.Address))
                {
                    // Split the address into lines
                    var addressLines = model.Address.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    // Assign each line to the appropriate cell
                    if (addressLines.Length > 0) worksheet.Cells["B6"].Value = addressLines[0]; // Line 1
                    if (addressLines.Length > 1) worksheet.Cells["B7"].Value = addressLines[1]; // Line 2
                    if (addressLines.Length > 2) worksheet.Cells["B8"].Value = addressLines[2]; // Line 3
                    if (addressLines.Length > 3) worksheet.Cells["B9"].Value = addressLines[3]; // Line 4
                }

                // Starting row for treatments
                int curRow = 16; // Starting row for the first item
                int baseRow = curRow; // Keep track of the base row for style copying

                // Insert additional rows if there are more items
                if (model.Items.Count > 1)
                {
                    // Insert rows for additional items (model.Items.Count - 1) as we already have one row
                    worksheet.InsertRow(curRow + 1, model.Items.Count - 1);

                    // Copy style from the base row to the newly inserted rows
                    for (int i = 1; i < model.Items.Count; i++)
                    {
                        worksheet.Cells[curRow + i, 3].StyleID = worksheet.Cells[baseRow, 3].StyleID;
                        worksheet.Cells[curRow + i, 7].StyleID = worksheet.Cells[baseRow, 7].StyleID;

                        var sourceRange = worksheet.Cells[baseRow, 4, baseRow, 6];
                        var destinationRange = worksheet.Cells[curRow + i, 4, curRow + i, 6];

                        destinationRange.StyleID = sourceRange.StyleID;
                        worksheet.Cells[curRow + i, 4, curRow + i, 6].Merge = true;
                    }
                }
                
                // Populate the worksheet with treatment data
                int row = 1;
                foreach (var item in model.Items)
                {
                    worksheet.Cells[$"C{curRow}"].Value = row;  // Row number
                    worksheet.Cells[$"D{curRow}"].Value = item.Particular;  // Item particular
                    worksheet.Cells[$"G{curRow}"].Value = item.Cost;  // Item cost

                    curRow++; // Move to the next row
                    row++;
                }
                int rowsIncreased = model.Items.Count - 1;
                // Set additional fields (after items)
                worksheet.Cells[$"C{rowsIncreased + 20}"].Value = AmountToWordsConverter.ConvertAmountToWords((decimal)model.TotalCost);
                worksheet.Cells[$"G{rowsIncreased + 30}"].Value = model.Name;
                worksheet.Cells[$"G{rowsIncreased + 31}"].Value = "( " + model.Ic + " )";
                worksheet.Name = patientName;
                // Save the new file
                package.SaveAs(newFile);
            }

            // Schedule file deletion in the background
            DeleteFileAfterDelay(newFilePath, TimeSpan.FromMinutes(1)); // Delete after 5 minutes

            return fileName;
        }

        public static string IPPKTemplate(IPPKKL model, string rootPath)
        {
            string patientName = "No_Name";
            if (model.Name != "") { patientName = model.Name; }
            string fileName = patientName + ".xlsx";
            // Load the template
            var templatePath = Path.Combine(rootPath, "template", "IPPKKL_Template.xlsx");
            var newFilePath = Path.Combine(rootPath, "template", fileName);

            FileInfo templateFile = new FileInfo(templatePath);
            FileInfo newFile = new FileInfo(newFilePath);

            using (ExcelPackage package = new ExcelPackage(templateFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                worksheet.Cells["C7"].Value = model.Name;
                worksheet.Cells["C8"].Value = model.Ic;
                worksheet.Cells["C9"].Value = model.TheirReference;
                worksheet.Cells["C10"].Value = model.OurReference;
                string monthInWords = "";
                if (model.Month != "") { monthInWords = months[int.Parse(model.Month) - 1]; }
                worksheet.Cells["C11"].Value = monthInWords.ToUpper() + " " + model.Year;

                // Starting row for treatments
                int curRow = 14; // Starting row is 14

                // Loop through the treatments and add them to the worksheet
                foreach (var treatment in model.Treatments)
                {
                    // Assign treatment details to specific cells
                    worksheet.Cells[$"B{curRow}"].Value = treatment.TreatmentDate;   // Tarikh Rawatan (Column B)
                    worksheet.Cells[$"C{curRow}"].Value = treatment.TreatmentCost;      // Kos Rawatan (Column C)
                    worksheet.Cells[$"D{curRow}"].Value = treatment.MedicineCost;     // Kos Suntikan (Column D)

                    // Move to the next row for the next treatment
                    curRow++;
                }
                //worksheet.Cells["C28"].Value = model.TotalTreatment;
                //worksheet.Cells["D28"].Value = model.TotalMedicine;
                worksheet.Name = patientName;
                // Save the new file
                package.SaveAs(newFile);
            }

            // Schedule file deletion in background
            DeleteFileAfterDelay(newFilePath, TimeSpan.FromMinutes(1)); // Delete after 5 minutes

            return fileName;
        }

        public static string JPIIETemplate(JPIIE model, string rootPath)
        {
            string patientName = "No_Name";
            if (model.Name != "") { patientName = model.Name; }
            string fileName = patientName + ".xlsx";
            // Load the template
            var templatePath = Path.Combine(rootPath, "template", "JPIIE_Template.xlsx");
            var newFilePath = Path.Combine(rootPath, "template", fileName);

            FileInfo templateFile = new FileInfo(templatePath);
            FileInfo newFile = new FileInfo(newFilePath);

            using (ExcelPackage package = new ExcelPackage(templateFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                worksheet.Cells["B8"].Value = model.Name;
                worksheet.Cells["B9"].Value = model.Ic;
                worksheet.Cells["B10"].Value = model.TheirReference;
                worksheet.Cells["B11"].Value = model.OurReference;

                string monthInWords = "";
                if (model.Month != "") { monthInWords = months[int.Parse(model.Month) - 1]; }
                worksheet.Cells["B12"].Value = monthInWords.ToUpper() + " " + model.Year;

                // Starting row for treatments
                int curRow = 15; // Starting row is 15

                // Loop through the treatments and add them to the worksheet
                foreach (var treatment in model.Treatments)
                {
                    // Assign treatment details to specific cells
                    worksheet.Cells[$"B{curRow}"].Value = treatment.TreatmentDate;   // Tarikh Rawatan (Column B)
                    worksheet.Cells[$"C{curRow}"].Value = treatment.TreatmentCost;      // Kos Rawatan (Column C)

                    // Move to the next row for the next treatment
                    curRow++;
                }
                worksheet.Name = patientName;
                // Save the new file
                package.SaveAs(newFile);
            }

            // Schedule file deletion in background
            DeleteFileAfterDelay(newFilePath, TimeSpan.FromMinutes(1)); // Delete after 5 minutes

            return fileName;
        }


        public static string LHDNTemplate(LHDN model, string rootPath)
        {
            string patientName = "No_Name";
            if (model.Name != "") { patientName = model.Name; }
            string fileName = patientName + ".xlsx";
            // Load the template
            var templatePath = Path.Combine(rootPath, "template", "LHDN_Template.xlsx");
            var newFilePath = Path.Combine(rootPath, "template", fileName);

            FileInfo templateFile = new FileInfo(templatePath);
            FileInfo newFile = new FileInfo(newFilePath);

            using (ExcelPackage package = new ExcelPackage(templateFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                worksheet.Cells["C6"].Value = model.Name;
                worksheet.Cells["C7"].Value = model.Ic;
                worksheet.Cells["C8"].Value = model.TheirReference;
                worksheet.Cells["C9"].Value = model.OurReference;
                string monthInWords = "";
                if (model.Month != "") { monthInWords = months[int.Parse(model.Month) - 1]; }
                worksheet.Cells["C10"].Value = monthInWords.ToUpper() + " " + model.Year;

                // Starting row for treatments
                int curRow = 13; // Starting row is 13

                // Loop through the treatments and add them to the worksheet
                foreach (var treatment in model.Treatments)
                {
                    // Assign treatment details to specific cells
                    worksheet.Cells[$"B{curRow}"].Value = treatment.TreatmentDate;   // Tarikh Rawatan (Column B)
                    worksheet.Cells[$"C{curRow}"].Value = treatment.TreatmentCost;      // Kos Rawatan (Column C)
                    worksheet.Cells[$"D{curRow}"].Value = treatment.MedicineCost;     // Kos Suntikan (Column D)

                    // Move to the next row for the next treatment
                    curRow++;
                }
                worksheet.Name = patientName;
                // Save the new file
                package.SaveAs(newFile);
            }

            // Schedule file deletion in background
            DeleteFileAfterDelay(newFilePath, TimeSpan.FromMinutes(1)); // Delete after 5 minutes

            return fileName;
        }

        public static string SOCSOTemplate(SOCSO model, string rootPath)
        {
            string patientName = "No_Name";
            if (model.Name != "") { patientName = model.Name; }
            string fileName = patientName + ".xlsx";
            // Load the template
            var templatePath = Path.Combine(rootPath, "template", "SOCSO_Template.xlsx");
            var newFilePath = Path.Combine(rootPath, "template", fileName);

            FileInfo templateFile = new FileInfo(templatePath);
            FileInfo newFile = new FileInfo(newFilePath);

            using (ExcelPackage package = new ExcelPackage(templateFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                worksheet.Cells["D6"].Value = model.TheirReference;
                worksheet.Cells["D7"].Value = model.Name;
                worksheet.Cells["D8"].Value = model.Ic;
                string monthInWords = "";
                if (model.Month != "") { monthInWords = months[int.Parse(model.Month) - 1]; }
                worksheet.Cells["D9"].Value = monthInWords.ToUpper();
                worksheet.Cells["E9"].Value = "Tahun: " + model.Year;
                worksheet.Cells["D10"].Value = model.PaymentTo;

                // Starting row for treatments
                int curRow = 13; // Starting row is 13

                // Loop through the treatments and add them to the worksheet
                foreach (var treatment in model.Treatments)
                {
                    //Assign treatment details to specific cells
                    worksheet.Cells[$"B{curRow}"].Value = treatment.TreatmentDate;
                    worksheet.Cells[$"D{curRow}"].Value = treatment.DialysisCost;
                    worksheet.Cells[$"E{curRow}"].Value = treatment.MedicineCost;
                    worksheet.Cells[$"F{curRow}"].Value = treatment.Status;

                    // Move to the next row for the next treatment
                    curRow++;
                }
                worksheet.Name = patientName;
                // Save the new file
                package.SaveAs(newFile);
            }

            // Schedule file deletion in background
            DeleteFileAfterDelay(newFilePath, TimeSpan.FromMinutes(1)); // Delete after 5 minutes

            return fileName;
        }

        private static void DeleteFileAfterDelay(string filePath, TimeSpan delay)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay); // Wait for the specified delay
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath); // Delete the file
                    }
                }
                catch (Exception ex)
                {
                    // Log error if needed
                    Console.WriteLine($"Failed to delete file {filePath}: {ex.Message}");
                }
            });
        }

    }
}

