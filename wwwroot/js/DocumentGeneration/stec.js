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

function formatDate(dateString) {
    const [year, month, day] = dateString.split('-');
    const monthIndex = parseInt(month, 10) - 1;

    return `${day} ${months[monthIndex]} ${year}`; // Format date
}

const monthSelect = document.getElementById('month');
const yearSelect = document.getElementById('year');
const daySelect = document.getElementById('day');

const months = [
    'Januari', 'Februari', 'Mac', 'April', 'Mei', 'Jun', 'Julai',
    'Ogos', 'September', 'Oktober', 'November', 'Disember'
];

months.forEach((month, index) => {
    let option = document.createElement('option');
    option.value = index + 1;
    option.text = month;
    monthSelect.appendChild(option);
});

const currentYear = new Date().getFullYear();
const startYear = 2020;
for (let year = currentYear; year >= startYear; year--) {
    let option = document.createElement('option');
    option.value = year;
    option.text = year;
    yearSelect.appendChild(option);
}

function populateDays(month, year) {
    daySelect.innerHTML = '<option value="" disabled selected>Select day</option>';
    const daysInMonth = new Date(year, month, 0).getDate();
    for (let day = 1; day <= daysInMonth; day++) {
        let option = document.createElement('option');
        option.value = day;
        option.text = day;
        daySelect.appendChild(option);
    }
}

monthSelect.addEventListener('change', () => {
    const selectedMonth = parseInt(monthSelect.value);
    const selectedYear = parseInt(yearSelect.value);
    if (selectedMonth && selectedYear) {
        populateDays(selectedMonth, selectedYear);
    }
});

yearSelect.addEventListener('change', () => {
    const selectedMonth = parseInt(monthSelect.value);
    const selectedYear = parseInt(yearSelect.value);
    if (selectedMonth && selectedYear) {
        populateDays(selectedMonth, selectedYear);
    }
});

document.getElementById('generateExcelButton').addEventListener('click', function (e) {
    e.preventDefault(); // Prevent default form submission

    // Collect values from the form inputs
    let name = document.getElementById('namaPesakit').value;
    let ic = document.getElementById('noKadPengenalan').value;
    let address = document.getElementById('address').value;
    let day = document.getElementById('day').value;
    let month = document.getElementById('month').value;
    let year = document.getElementById('year').value;
    let totalCost = document.getElementById('totalAmount').value;

    let items = [];
    let rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');
    rows.forEach((row, index) => {
        let particularInput = row.querySelector(`input[name="particular${index + 1}"]`);
        let amountInput = row.querySelector(`input[name="amount${index + 1}"]`);

        // Check if the input element exists, then get the value, otherwise default
        let particular = particularInput ? (particularInput.value || "") : "";
        let amount = amountInput ? (amountInput.value || 0) : 0;

        items.push({
            Particular: particular,
            Cost: amount,
        });
        
    });
    // Build the model object to send to the controller
    let model = {
        Name: name,
        Ic: ic,
        Address: address,
        Day: day,
        Month: month,
        Year: year,
        Items: items,
        TotalCost: totalCost
    };
    console.log(model);
    // Send data to the server
    fetch('/DocumentGen/GenerateSTEC', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(model)
    })
        .then(response => response.json())  // Parse the JSON response
        .then(data => {
            if (data.fileName) {
                
                // Create a temporary download link and click it
                let downloadUrl = '/template/' + data.fileName; // Adjust this path if necessary
                let a = document.createElement('a');
                a.href = downloadUrl;
                a.download = data.fileName; // Specify the download file name
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            } else {
                console.log(data.fileName);
                alert('Failed to download Excel file Please try again.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error generating Excel.');
        });
});

// Function to recalculate the overall totals when any Kos Rawatan value changes
function recalculateTotal() {
    let overallTotal = 0;

    // Get all rows except the total row
    let rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');

    rows.forEach((row) => {
        let amountInput = row.querySelector(`input[name^="amount"]`);

        // Parse the input values, default to 0 if they are empty
        let amount = parseFloat(amountInput.value) || 0;

        // Accumulate totals
        overallTotal += amount;
    });

    // Update the overall total cells
    document.getElementById('totalAmount').innerText = overallTotal.toFixed(2);
}

// Add event listeners to all Kos Rawatan and Kos Suntikan cells
document.querySelectorAll(`input[name^="amount"]`).forEach(input => {
    input.addEventListener('input', function () {
        // Recalculate total for the row where the input changed
        const row = this.closest('tr'); // Get the closest row
        recalculateTotal(); // Call the total function to update overall total
    });
});

// Function to format input value to 2 decimal places
function formatToTwoDecimals(event) {
    const input = event.target;
    let value = parseFloat(input.value);

    // If the input is a valid number, format it
    if (!isNaN(value)) {
        input.value = value.toFixed(2);
    } else {
        input.value = ''; // Reset to empty if input is not valid
    }
}

// Add event listeners to all Kos Rawatan
document.querySelectorAll('input[name^="amount"]').forEach(input => {
    input.addEventListener('blur', formatToTwoDecimals); // Format on focus out
});

document.getElementById('addBtn').addEventListener('click', function () {
    // Get the table body
    var tableBody = document.querySelector('.table-wrapper tbody');

    // Get the current number of rows (excluding the total rows)
    var rowCount = tableBody.querySelectorAll('tr').length - 1;

    // Create a new row
    var newRow = document.createElement('tr');

    // Set the inner HTML of the new row
    newRow.innerHTML = `
        <td>${rowCount + 1}</td>
        <td><input type="text" name="particular${rowCount + 1}" /></td>
        <td><input type="number" class="no-spinner" name="amount${rowCount + 1}" placeholder="0.00" min="0"/></td>
    `;

    // Insert the new row before the total rows
    tableBody.insertBefore(newRow, tableBody.querySelector('.total-row'));

    // Add event listeners to the new inputs in the new row
    newRow.querySelectorAll('input[name^="amount"]').forEach(input => {
        input.addEventListener('input', function () {
            recalculateTotal(); // Update overall total
        });

        input.addEventListener('blur', formatToTwoDecimals); // Format input on blur
    });
});

document.getElementById('rmvBtn').addEventListener('click', function () {
    var tableBody = document.querySelector('.table-wrapper tbody');
    var rows = tableBody.querySelectorAll('tr:not(.total-row)');
    if (rows.length > 1) {
        var lastRow = rows[rows.length - 1];

        lastRow.remove();

        recalculateTotal();
    } else {
        alert("You cannot remove the first row.")
    }
});


function openPopup() {
    document.getElementById("select-patient-sponsor-popup").style.display = "block";
}

function closePopup() {
    document.getElementById("select-patient-sponsor-popup").style.display = "none";
}

function confirmSelection() {
    sessionStorage.setItem("currentSponsor", "JKM");
    sessionStorage.setItem("currentPage", "STEC");
    window.location.href = '/DocumentGen/SelectPatient';
}

function autoFillDetails() {
    const selectedPatient = sessionStorage.getItem("selectedPatient");
    console.log(sessionStorage);
    if (selectedPatient) {
        const patientData = JSON.parse(selectedPatient);

        document.getElementById("namaPesakit").value = patientData.PatientName || '';
        document.getElementById("noKadPengenalan").value = patientData.PatientIC || '';

        sessionStorage.removeItem("selectedPatient");
    }
}

function fillAddress() {
    var address = `STEC KIDNEY FOUNDATION
3rd floor, No 2
Jalan Song Thian Cheok
93100 Kuching Sarawak`;
    document.getElementById('address').value = address;
}

fillAddress();
autoFillDetails();
updateDateTime();
setInterval(updateDateTime, 1000);