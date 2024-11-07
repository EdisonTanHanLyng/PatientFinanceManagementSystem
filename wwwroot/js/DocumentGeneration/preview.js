
function updateDateTime() {
    var current = new Date();
    
    //Date
    var dateOptions = { day: 'numeric', month: 'long', year: 'numeric' };
    var dateFormatter = new Intl.DateTimeFormat('en-US', dateOptions);
    var formattedDate = dateFormatter.format(current);
    
    //Time
    var timeOptions = { hour: 'numeric', minute: 'numeric', hour12: true };
    var timeFormatter = new Intl.DateTimeFormat('en-US', timeOptions);
    var formattedTime = timeFormatter.format(current);

    document.getElementById('date').innerHTML = '<strong>Date:</strong> ' + formattedDate;
    document.getElementById('time').innerHTML = '<strong>Time:</strong> ' + formattedTime;
}

document.addEventListener("DOMContentLoaded", function () {
    var generatePDFBtn = document.getElementById('download-button');
    var pdfViewer = document.getElementById('pdfViewer');
    var pdfFilePath; // Variable to store the file path of the generated PDF

    if (generatePDFBtn) {
        generatePDFBtn.addEventListener('click', function () {
            // Check if the iframe is already showing a PDF
            if (pdfViewer.src) {
                // Extract the PDF file path from the iframe's `src` attribute
                pdfFilePath = pdfViewer.src;

                // Create a temporary link to trigger the download
                var downloadLink = document.createElement('a');
                downloadLink.href = pdfFilePath;
                downloadLink.download = pdfFilePath.split('/').pop(); // Use the same file name as in the iframe
                downloadLink.click(); // Trigger the download
            } else {
                alert('No PDF is currently being previewed.');
            }
        });
    }
});



updateDateTime();
setInterval(updateDateTime, 1000);
