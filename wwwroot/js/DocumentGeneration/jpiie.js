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

const monthSelect = document.getElementById('month');
const months = [
    'Januari', 'Februari', 'Mac', 'April', 'Mei', 'Jun', 'Julai',
    'Ogos', 'September', 'Oktober', 'November', 'Disember'
];
function formatDate(dateString) {
    const [year, month, day] = dateString.split('-');
    const monthIndex = parseInt(month, 10) - 1;

    return `${day} ${months[monthIndex]} ${year}`; // Format date
}

months.forEach((month, index) => {
    let option = document.createElement('option');
    option.value = index + 1; // value is 1-12 for months
    option.text = month;
    monthSelect.appendChild(option);
});
const yearSelect = document.getElementById('year');
const currentYear = new Date().getFullYear();
const startYear = 2020; // Customize this start year
for (let year = currentYear; year >= startYear; year--) {
    let option = document.createElement('option');
    option.value = year;
    option.text = year;
    yearSelect.appendChild(option);
}

document.getElementById('generateExcelButton').addEventListener('click', function (e) {
    e.preventDefault(); // Prevent default form submission

    // Collect values from the form inputs
    let name = document.getElementById('namaPesakit').value;
    let ic = document.getElementById('noKadPengenalan').value;
    let theirReference = document.getElementById('noRujukanTuan').value;
    let month = document.getElementById('month').value;
    let year = document.getElementById('year').value;
    let ourReference = document.getElementById('noRujukanKami').value;
    let totalTreatment = parseFloat(document.getElementById('totalRawatan').innerText);

    // Collect treatments (Tarikh Rawatan and Kos Rawatan)
    let treatments = [];
    let rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');
    rows.forEach((row, index) => {
        let tarikhRawatanInput = row.querySelector(`input[name="tarikhRawatan${index + 1}"]`);
        let kosRawatanInput = row.querySelector(`input[name="kosRawatan${index + 1}"]`);

        // Check if the input element exists, then get the value, otherwise default
        let tarikhRawatan = tarikhRawatanInput ? (tarikhRawatanInput.value || "") : "";
        let kosRawatan = kosRawatanInput ? (kosRawatanInput.value || 0) : 0;

        treatments.push({
            TreatmentDate: tarikhRawatan,
            TreatmentCost: kosRawatan,
        });
        
    });
    // Build the model object to send to the controller
    let model = {
        Name: name,
        Ic: ic,
        TheirReference: theirReference,
        OurReference: ourReference,
        Month: month,
        Year: year,
        Treatments: treatments,
        totalTreatment: totalTreatment,
    };
    console.log(model);
    // Send data to the server
    fetch('/DocumentGen/GenerateJPIIE', {
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
    let overallTotalRawatan = 0;

    // Get all rows except the total row
    let rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');

    rows.forEach((row) => {
        let kosRawatanInput = row.querySelector(`input[name^="kosRawatan"]`);

        // Parse the input values, default to 0 if they are empty
        let kosRawatan = parseFloat(kosRawatanInput.value) || 0;

        // Accumulate totals
        overallTotalRawatan += kosRawatan;
    });

    // Update the overall total cells
    document.getElementById('totalRawatan').innerText = overallTotalRawatan.toFixed(2);
}

// Add event listeners to all Kos Rawatan and Kos Suntikan cells
document.querySelectorAll(`input[name^="kosRawatan"]`).forEach(input => {
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
document.querySelectorAll('input[name^="kosRawatan"]').forEach(input => {
    input.addEventListener('blur', formatToTwoDecimals); // Format on focus out
});

const hdRecStruct = {
    date: "",
    time: "",
    tdMin: 0,
    weightPre: 0.0,
    weightPost: 0.0,
    dw: 0.0,
    idw: 0.0,
    dwPercent: 0.0,
    ufGoal: 0.0,
    preBP_SBP: 0,
    preBP_DBP: 0,
    postBP_SBP: 0,
    postBP_DBP: 0,
    prePulse: 0,
    postPulse: 0,
    bfr: 0,
    epoType: "",
    epoDosage: 0.0,
    epoQty: 0,
    ocm_KtV: 0.0,
    dial_Cal: "",
    dialyzer: "",
    remarks: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis ac dui vestibulum urna commodo molestie at sed urna. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. In porta malesuada lacus ac luctus."
};
// Function to parse a single HD record
function parseHDRecData2(jsonData) {
    const dataObj = { ...hdRecStruct };
    dataObj.date = jsonData["date"];
    dataObj.time = jsonData["time"];
    dataObj.tdMin = jsonData["tdMin"];
    dataObj.weightPre = jsonData["weightPre"];
    dataObj.weightPost = jsonData["weightPost"];
    dataObj.dw = jsonData["dw"];
    dataObj.idw = jsonData["idw"];
    dataObj.dwPercent = jsonData["dwPercent"];
    dataObj.ufGoal = jsonData["ufGoal"];
    dataObj.preBP_SBP = jsonData["preBP_SBP"];
    dataObj.preBP_DBP = jsonData["preBP_DBP"];
    dataObj.postBP_SBP = jsonData["postBP_SBP"];
    dataObj.postBP_DBP = jsonData["postBP_DBP"];
    dataObj.prePulse = jsonData["prePulse"];
    dataObj.postPulse = jsonData["postPulse"];
    dataObj.bfr = jsonData["bfr"];
    dataObj.epoType = jsonData["epoType"];
    dataObj.epoDosage = jsonData["epoDosage"];
    dataObj.epoQty = jsonData["epoQty"];
    dataObj.ocm_KtV = jsonData["ocm_KtV"];
    dataObj.dial_Cal = jsonData["dial_Cal"];
    dataObj.dialyzer = jsonData["dialyzer"];
    return dataObj;
}
// Function to search for HD records
async function searchDate() {
    let patientID = document.getElementById('patientID').value;
    const additionalData = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    };
    const monthSelect = document.getElementById('month');
    const yearSelect = document.getElementById('year');
    const selectedMonthIndex = monthSelect.value;
    const month = selectedMonthIndex.padStart(2, '0');
    const year = yearSelect.value;
    if (!patientID) {
        alert("Patient must be selected from the Patient List");
        return;
    } else if (!month || !year) {
        alert("Please select both Month and Year");
        return;
    }
    // Form the API query
    const getHDRecFiltAPI = `/getPatHistFilt/${patientID}/${month}/${year}`;
    await fetch(getHDRecFiltAPI, additionalData)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Get Data Failed! : HD Rec Filter (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            console.log("Obtained Data:", obtainedData);
            if (Array.isArray(obtainedData)) {
                const parsedRecords = obtainedData.map(record => parseHDRecData2(record));
                if (parsedRecords.length === 0) {
                    alert("No data found for selected Month and Year.");
                } else {
                    populateTreatmentDates(parsedRecords);
                }
            } else {
                console.error("The obtained data is not an array.");
            }
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });
}
function populateTreatmentDates(parsedRecords) {
    const rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');
    const firstRowInput = rows[0]?.querySelector(`input[name="tarikhRawatan1"]`);
    if (firstRowInput && firstRowInput.value.trim() !== "") {
        rows.forEach(row => {
            const tarikhRawatanInput = row.querySelector(`input[name^="tarikhRawatan"]`);
            if (tarikhRawatanInput) {
                tarikhRawatanInput.value = "";
            }
        });
    }
    // Populate the treatment dates
    parsedRecords.forEach((record, index) => {
        if (index < rows.length) {
            const tarikhRawatanInput = rows[index].querySelector(`input[name="tarikhRawatan${index + 1}"]`);
            if (tarikhRawatanInput) {
                tarikhRawatanInput.value = formatDate(record.date);
            }
        }
    });
}

function openPopup() {
    document.getElementById("select-patient-sponsor-popup").style.display = "block";
}

function closePopup() {
    document.getElementById("select-patient-sponsor-popup").style.display = "none";
}

function confirmSelection() {
    sessionStorage.setItem("currentSponsor", "JKM");
    sessionStorage.setItem("currentPage", "JPIIE");
    window.location.href = '/DocumentGen/SelectPatient';
}

function autoFillDetails() {
    const selectedPatient = sessionStorage.getItem("selectedPatient");
    console.log(sessionStorage);
    if (selectedPatient) {
        const patientData = JSON.parse(selectedPatient);

        document.getElementById("namaPesakit").value = patientData.PatientName || '';
        document.getElementById("noKadPengenalan").value = patientData.PatientIC || '';
        document.getElementById("patientID").value = patientData.PatientID;

        sessionStorage.removeItem("selectedPatient");
    }
}

document.getElementById('fillBtn').addEventListener('click', function () {
    const confirmFill = confirm("Fill in prices for all rows that have dates?");

    if (!confirmFill) {
        return;
    }

    // Get the values from the pRawatan
    const pRawatanValue = parseFloat(document.getElementById('pRawatan').value);

    // Check if the values are 0
    if (pRawatanValue === 0) {
        alert("Prices are 0.");
        return;
    }

    let rows = document.querySelectorAll('.table-wrapper tbody tr:not(.total-row)');

    rows.forEach((row, index) => {

        let tarikhRawatanInput = row.querySelector(`input[name="tarikhRawatan${index + 1}"]`);

        if (tarikhRawatanInput && tarikhRawatanInput.value.trim() !== "") {
            let kosRawatanInput = row.querySelector(`input[name="kosRawatan${index + 1}"]`);

            if (kosRawatanInput) kosRawatanInput.value = pRawatanValue.toFixed(2);
        }
    });

    // Recalculate the total after filling the costs
    recalculateTotal();
});

document.querySelectorAll('input[name^="pRawatan"]').forEach(input => {
    input.addEventListener('blur', formatToTwoDecimals);
});

function openPrice() {
    document.getElementById("pricePopup").style.display = "block";
}

function closePrice() {
    document.getElementById("pricePopup").style.display = "none";
}

async function updatePrice() {
    const sponsorCode = "JPIIE";
    const dialysisCost = document.getElementById("pRawatan").value;
    const epoCost = 0;



    let model = {
        SponsorCode: sponsorCode,
        DialysisCost: dialysisCost,
        EPOCost: epoCost
    };

    const requestOptions = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(model)
    }

    const apiUrl = `/DocumentGen/UpdatePricing`;

    await fetch(apiUrl, requestOptions)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            const jsonObject = data;
            if (jsonObject.success) {
                alert(jsonObject.responseMessage);
                closePrice();
            }
        })
        .catch(error => {
            // Handle any errors that occurred during the fetch
            console.error('Fetch error:', error);
        });
}

async function getPrice() {
    const sponsorCode = "JPIIE";

    const apiUrl = `/DocumentGen/GetPricing?sponsorCode=${encodeURIComponent(sponsorCode)}`;

    const requestOptions = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    };

    try {
        const response = await fetch(apiUrl, requestOptions);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.json();
        if (data.success) {
            document.getElementById('pRawatan').value = data.data.dialysisCost;
        } else {
            alert(data.responseMessage);
        }
    } catch (error) {
        // Handle any errors that occurred during the fetch
        console.error('Fetch error:', error);
    }
}

getPrice();
autoFillDetails();
updateDateTime();
setInterval(updateDateTime, 1000);