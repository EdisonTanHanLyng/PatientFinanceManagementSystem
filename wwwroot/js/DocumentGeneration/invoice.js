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

const getSponsor = `/getAllSponsor/`;

const additionalData = {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json'
    }
}

async function getSponsors() {
    try {
        const response = await fetch(getSponsor, additionalData);
        if (!response.ok) {
            throw new Error(`Get Data Failed! : Sponsor Profile (${response.status})`);
        }
        const obtainedData = await response.json();
        displaySponsors(obtainedData); // Call the function to display sponsors
    } catch (error) {
        console.error(`Error Occurred: ${error}`);
    }
}

function populateSponsorDetails(sponsorData) {
    document.getElementById('contactPerson').value = sponsorData.contactPerson || '';
    document.getElementById('telNum').value = sponsorData.sponsorPhone || '';
    document.getElementById('faxNum').value = sponsorData.sponsorFax || '';
    document.getElementById('address').value = sponsorData.sponsorAddress || '';
}

function parseSponsorInformation(jsonData) {
    const infoMap = new Map([
        ['code', jsonData.sponsorCode],
        ['name', jsonData.sponsorName],
        ['contactPerson', jsonData.contactPerson],
        ['officePhone', jsonData.sponsorPhone],
        ['sponsorId', jsonData.sponsorId],
        ['address', jsonData.sponsorAddress],
        ['fax', jsonData.sponsorFax],
    ]);
    return infoMap;
}

function displaySponsors(obtainedData) {
    const sponsorDropdown = document.getElementById('sponsor');

    obtainedData.forEach(sponsorData => {
        let option = document.createElement('option');
        const parsedData = parseSponsorInformation(sponsorData);
        option.text = parsedData.get('name');
        option.value = parsedData.get('sponsorId'); 
        option.dataset.sponsorDetails = JSON.stringify(sponsorData);
        sponsorDropdown.add(option);
    });

    // Add event listener to update details when sponsor is selected
    sponsorDropdown.addEventListener('change', function () {
        const selectedOption = sponsorDropdown.options[sponsorDropdown.selectedIndex];
        const sponsorDetails = JSON.parse(selectedOption.dataset.sponsorDetails);
        populateSponsorDetails(sponsorDetails);
    });
}

document.getElementById('generatePdfButton').addEventListener('click', function (e) {
    e.preventDefault(); // Prevent default form submission

    if (!validateForm()) {
        return; // Stop if validation fails
    }

    // Get today's date in YYYY-MM-DD format
    let today = formatDate(new Date());

    // Collect values from the form inputs
    let sponsor = document.getElementById('sponsor').value;
    let address = document.getElementById('address').value;
    let contactPerson = document.getElementById('contactPerson').value;
    let telNum = document.getElementById('telNum').value;
    let faxNum = document.getElementById('faxNum').value;

    
    let items = [];
    let rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');
    rows.forEach((row, index) => {
        let descriptionInput = row.querySelector(`input[name="description${index + 1}"]`);
        let qtyInput = row.querySelector(`input[name="qty${index + 1}"]`);
        let uomInput = row.querySelector(`input[name="uom${index + 1}"]`);
        let uPriceInput = row.querySelector(`input[name="uPrice${index + 1}"]`);

        // Check if the input element exists, then get the value, otherwise default to today
        let description = descriptionInput ? (descriptionInput.value || "") : "";
        let qty = qtyInput ? (qtyInput.value || "") : "";
        let uom = uomInput ? (uomInput.value || "") : "";
        let uPrice = uPriceInput ? (uPriceInput.value || 0) : 0;

        items.push({
            Description: description,
            Qty: qty,
            UOM: uom,
            Price: uPrice
        });

    });
    // Build the model object to send to the controller
    let model = {
        sponsorAddress: address,
        ContactPerson: contactPerson,
        SponsorPhone: telNum,
        sponsorFax: faxNum,
        Items: items
    };
    console.log(model);
    // Send data to the server
    fetch('/DocumentGen/GeneratePdf', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(model)
    })
        .then(response => response.json())
        .then(data => {
            // Handle response data (e.g., display the generated PDF)
            window.location.href = '/DocumentGen/DocuGenPreview?fileName=' + data.fileName;
        })
        .catch(error => {
            console.error('Error:', error);
        });
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
        <td><input type="text" name="description${rowCount + 1}" /></td>
        <td><input type="number" class="no-spinner" name="qty${rowCount + 1}" placeholder="0" min="0"/></td>
        <td><input type="text" name="uom${rowCount + 1}" /></td>
        <td><input type="number" step="0.01" name="uPrice${rowCount + 1}" placeholder="0.00" min="0"/></td>
        <td id="totalBayaran${rowCount + 1}">0.00</td>
    `;

    // Insert the new row before the total rows
    tableBody.insertBefore(newRow, tableBody.querySelector('.total-row'));

    // Add event listeners to the new inputs in the new row
    newRow.querySelectorAll('input[name^="uPrice"]').forEach(input => {
        input.addEventListener('input', function () {
            updateRowTotal(newRow); // Update row total when input changes
            recalculateTotal(); // Update overall total
        });

        input.addEventListener('blur', formatToTwoDecimals); // Format input on blur
    });

    newRow.querySelectorAll('input[name^="qty"]').forEach(input => {
        input.addEventListener('input', function () {
            updateRowTotal(newRow); // Update row total when input changes
            recalculateTotal(); // Update overall total
        });
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

// Validation Function
function validateForm() {
    // Validate sponsor and contact details
    const sponsor = document.getElementById('sponsor').value.trim();
    const address = document.getElementById('address').value.trim();
    const contactPerson = document.getElementById('contactPerson').value.trim();
    const telNum = document.getElementById('telNum').value.trim();
    const faxNum = document.getElementById('faxNum').value.trim();

    if (!sponsor || !address || !contactPerson || !telNum || !faxNum) {
        alert('Please fill in all sponsor and contact details.');
        return false;
    }

    // Validate table fields
    const rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');
    for (let row of rows) {
        const description = row.querySelector(`input[name^="description"]`).value.trim();
        const qty = row.querySelector(`input[name^="qty"]`).value.trim();
        const uom = row.querySelector(`input[name^="uom"]`).value.trim();
        const uPrice = row.querySelector(`input[name^="uPrice"]`).value.trim();

        if (!description || !qty || !uom || !uPrice) {
            alert('Please fill all the details for the invoice item.');
            return false;
        }
    }

    return true;
}
// Function to format date to YYYY-MM-DD
function formatDate(date) {
    let d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [year, month, day].join('-');
}

// Function to do rounding
function roundToNearestMultipleOf5(num, decimalPlaces) {
    const multiplier = Math.pow(10, decimalPlaces);
    const roundedNum = Math.round(num * multiplier);
    const nearestMultipleOf5 = Math.round(roundedNum / 5) * 5;
    return nearestMultipleOf5 / multiplier;
}

function updateRowTotal(row) {
    let qtyInput = row.querySelector(`input[name^="qty"]`);
    let uPriceInput = row.querySelector(`input[name^="uPrice"]`);

    // Parse the input values, default to 0 if they are empty
    let qty = parseFloat(qtyInput.value) || 0;
    let uPrice = parseFloat(uPriceInput.value) || 0;

    // Calculate total for the row
    let rowTotal = qty * uPrice;
    rowTotal = roundToNearestMultipleOf5(rowTotal, 2);
    console.log(rowTotal);

    // Update the total bayaran cell in this row
    let totalBayaran = row.querySelector('td[id^="totalBayaran"]');
    totalBayaran.innerText = rowTotal.toFixed(2);
}

// Function to recalculate the overall totals when any Kos Dialisis or Kos EPO value changes
function recalculateTotal() {
    let overallTotal = 0;

    // Get all rows except the total row
    let rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');

    rows.forEach((row, index) => {
        let qtyInput = row.querySelector(`input[name^="qty${index + 1}"]`);
        let uPriceInput = row.querySelector(`input[name^="uPrice${index + 1}"]`);
        let qty = qtyInput ? (qtyInput.value || 0) : 0;
        let uPrice = uPriceInput ? (uPriceInput.value || 0) : 0;

        // Accumulate totals
        overallTotal += qty * uPrice;
        overallTotal = roundToNearestMultipleOf5(overallTotal, 2);

    });
    // Update the overall total cells
    document.getElementById('totalPrice').innerText = overallTotal.toFixed(2);
   
}

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

document.querySelectorAll(`input[name^="qty"]`).forEach(input => {
    input.addEventListener('input', function () {
        // Recalculate total for the row where the input changed
        const row = this.closest('tr'); // Get the closest row
        updateRowTotal(row);
        recalculateTotal(); // Call the total function to update overall total
    });
});

document.querySelectorAll(`input[name^="uPrice"]`).forEach(input => {
    input.addEventListener('input', function () {
        // Recalculate total for the row where the input changed
        const row = this.closest('tr'); // Get the closest row
        updateRowTotal(row);
        recalculateTotal(); // Call the total function to update overall total
    });
    input.addEventListener('blur', formatToTwoDecimals); // Format input on blur
});

document.getElementById('sponsor').addEventListener('change', function () {
    const selectedOption = this.options[this.selectedIndex];

    // Check if the selected option is the blank option
    if (selectedOption.value === "") {
        // Clear the input fields if no sponsor is selected
        populateSponsorDetails({}); // Pass an empty object to clear fields
    } else {
        const sponsorDetails = JSON.parse(selectedOption.dataset.sponsorDetails);
        populateSponsorDetails(sponsorDetails);
    }
});



getSponsors();
updateDateTime();
setInterval(updateDateTime, 1000);