var patientList = [];
var selectedPatients = [];

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

function displayPatients(patient) {
    let patientName = patient['PatientName'];
    let patientIC = patient['PatientIC'];
    let patientSponsor = patient['SponsorList'];
    let patientPhone = patient['PatientPhone'];
    let patientEmail = patient['PatientEmail'];

    const container = document.querySelector('.add-reminder-patient-boxes');
    const itemBox = document.createElement('div');
    itemBox.classList.add('add-reminder-patient-reminder-box');

    itemBox.innerHTML = `
        <div class="add-reminder-patient-grid">
            <div class="add-reminder-patient-checkbox-container">
                <label class="custom-radio">
                    <input type="checkbox" class="add-reminder-patient-checkbox">
                    <span class="checkmark"></span>
                </label>
            </div>
            <div class="add-reminder-patient-content-grid">
                <div><strong>Patient </strong></div>
                <div><strong>Patient IC: </strong>${patientIC}</div>
                <div><strong>Name: </strong>${patientName}</div>
                <div><strong>Tel: </strong>${patientPhone}</div>
                <div><strong>Sponsor: </strong>${patientSponsor}</div>
                <div class=".hidden-email"><strong>Patient Email: </strong>${patientEmail}</div>
            </div>
        </div>
    `;

    const checkbox = itemBox.querySelector('.add-reminder-patient-checkbox');
    const checkmark = itemBox.querySelector('.checkmark');

    //Check if the patient is already selected
    let isSelected = selectedPatients.some(selectedPatient =>
        selectedPatient.PatientName === patientName &&
        selectedPatient.PatientNRIC === patientIC &&
        selectedPatient.PatientSponsor === patientSponsor &&
        selectedPatient.PatientPhone === patientPhone
    );

    if (isSelected) {
        checkbox.classList.add('active');
        checkbox.checked = true;
        checkmark.classList.add('selected');
    }

    //Toggle select/deselect on click
    checkbox.addEventListener('click', function () {
        let targetPatient = parsePatientInfo(patientName, patientIC, patientSponsor, patientPhone, patientEmail);

        if (checkbox.classList.contains('active')) {
            checkbox.classList.remove('active');
            checkmark.classList.remove('selected');
            checkbox.checked = false;

            selectedPatients = selectedPatients.filter(patient =>
                patient.PatientName !== targetPatient.PatientName ||
                patient.PatientNRIC !== targetPatient.PatientNRIC ||
                patient.PatientSponsor !== targetPatient.PatientSponsor ||
                patient.PatientPhone !== targetPatient.PatientPhone
            );

        } else {
            checkbox.classList.add('active');
            checkmark.classList.add('selected');
            checkbox.checked = true;

            selectedPatients.push(targetPatient);
        }
    });

    container.appendChild(itemBox);
}


function selectAllVisiblePatients() {
    const checkboxes = document.querySelectorAll('.add-reminder-patient-checkbox');

    // Check if all visible checkboxes are already selected
    const allSelected = Array.from(checkboxes).every(checkbox => checkbox.classList.contains('active'));

    checkboxes.forEach(checkbox => {
        const checkmark = checkbox.nextElementSibling;
        const patientBox = checkbox.closest('.add-reminder-patient-reminder-box');

        const patientName = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(3)').textContent.split(': ')[1];
        const patientIC = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(2)').textContent.split(': ')[1];
        const patientPhone = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(4)').textContent.split(': ')[1];
        const patientSponsor = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(5)').textContent.split(': ')[1];
        const patientEmail = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(6)').textContent.split(': ')[1];

        let targetPatient = parsePatientInfo(patientName, patientIC, patientSponsor, patientPhone, patientEmail);

        if (allSelected) {
            checkbox.classList.remove('active');
            checkmark.classList.remove('selected');

            // Remove patient from patientList
            let index = selectedPatients.findIndex(patient =>
                patient.PatientName === targetPatient.PatientName &&
                patient.PatientNRIC === targetPatient.PatientNRIC &&
                patient.PatientSponsor === targetPatient.PatientSponsor &&
                patient.PatientPhone === targetPatient.PatientPhone
            );

            if (index !== -1) {
                selectedPatients.splice(index, 1);
            }

        } else {
            // If not all are selected, select them
            if (!checkbox.classList.contains('active')) {
                checkbox.classList.add('active');
                checkmark.classList.add('selected');

                // Add to patientList if not already there
                let exists = selectedPatients.some(patient =>
                    patient.PatientName === targetPatient.PatientName &&
                    patient.PatientNRIC === targetPatient.PatientNRIC &&
                    patient.PatientSponsor === targetPatient.PatientSponsor &&
                    patient.PatientPhone === targetPatient.PatientPhone
                );

                if (!exists) {
                    selectedPatients.push(targetPatient);
                }
            }
        }
    });
}




function parsePatientInfo(name, nric, sponsor, phone, email) {
    let info = {
        PatientName: name,
        PatientNRIC: nric,
        PatientSponsor: sponsor,
        PatientPhone: phone,
        PatientEmail: email,
        UserType: 'Patient'
    };

    return info;
}



/* Filter/Search functionality */
// Get filter name input from search bar
document.querySelector('#reminder-search-field-input').addEventListener('input', function (input) {
    updateFilterState('name', input.target.value);
    const container = document.querySelector('.add-reminder-patient-boxes');
    container.innerHTML = '';
    (filterEntries(filterState.patientList, filterState.filters)).forEach(patient => { displayPatients(patient) });
});

// Get filter sponsor input from drop-down menu
document.getElementById("reminder-search-sponsor").addEventListener('change', function () {
    let value = this.value;
    let search = '';

    if (value !== 'none') search = value;
    updateFilterState('sponsor', search);
    const container = document.querySelector('.add-reminder-patient-boxes');
    container.innerHTML = '';
    (filterEntries(filterState.patientList, filterState.filters)).forEach(patient => { displayPatients(patient) });
});


var filterState = {
    patientList: [],
    filters: {
        name: '',
        sponsor: ''
    }
};

function updateFilterState(key, value) {
    filterState.filters[key] = value;
}

function filterEntries(entries, filters) {
    let filteredEntries = entries;

    if (filters.name) {
        filteredEntries = filter(filters.name, filteredEntries, "Name");
    }
    if (filters.sponsor) {
        filteredEntries = filter(filters.sponsor, filteredEntries, "Sponsor");
    }
    return filteredEntries;
}

function filter(search, patients, category) {
    let filteredList = [];

    if (search.length == 0) return patients; //If nothing to filter, return everything
    for (let i = 0; i < patients.length; i++) {

        if (category === "Name" && (compare(search.toLowerCase(), patients[i]['PatientName'].toLowerCase()))) //Name filter
            filteredList.push(patients[i]);
        else if (category === "Sponsor" && (compare(search.toLowerCase(), patients[i]['SponsorList'].toLowerCase()))) //Sponsor filter
            filteredList.push(patients[i]);
    }

    return filteredList;
}

//Compare search string and target
function compare(search, target) {
    let start = false;
    let match = false;
    let index = 0;

    //Remove whitespaces
    search = search.replace(/\s/g, '');
    target = target.replace(/\s/g, '');

    if (target.length < search.length) return false;

    for (let i = 0; i < target.length; i++) {
        let char = target[i];

        if (char === search[0]) start = true;

        if (start && char !== search[index]) {
            start = false;
            index = 0;
        }
        if (start) index++;

        if (start && index === search.length) {
            match = true;
            break;
        }
    }

    return match;
}


async function getAllPatients() {
    let getPatList = `/Reminder/getAllPatients`;
    const loadingContainer = document.querySelector('.loading-container');
    const container = document.querySelector('.add-reminder-patient-boxes');

    // Show loading spinner
    loadingContainer.style.display = 'block';
    container.style.display = 'none';

    await fetch(getPatList)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed!: Patient List (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            obtainedData.forEach(item => {
                patient = parsePatientProfileInformation(item);
                filterState.patientList.push(patient);
                displayPatients(patient);
            });
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        }).finally(() => {
            // Hides spinner and shows reminder list
            loadingContainer.style.display = 'none';
            container.style.display = 'block';
        });
}

async function getAllSponsors() {
    let getSponsors = `/Reminder/getAllSponsors`;
    await fetch(getSponsors)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed!: Patient List (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            const selectElement = document.getElementById('reminder-search-sponsor');
            selectElement.innerHTML = '';

            // Add default option
            const defaultOption = document.createElement('option');
            defaultOption.value = 'none';
            defaultOption.text = 'None';
            defaultOption.selected = true;
            selectElement.appendChild(defaultOption);

            Object.keys(obtainedData).forEach(sponsor => {
                const option = document.createElement('option');
                option.value = sponsor;
                option.text = sponsor;
                selectElement.appendChild(option);
            });
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });
}


function parsePatientProfileInformation(item) {
    let info = {
        "PatientName": item["FullName"],
        "PatientIC": item["NRIC"],
        "SponsorList": item["SponsorList"],
        "PatientPhone": item["PhoneNumber"],
        "PatientEmail": item["Email"]
    };
    return info;
}


async function saveAndUpload() {
    const requestOptions = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(selectedPatients)
    }

    const apiUrl = `/reminder/saveAndUploadPatients`;

    fetch(apiUrl, requestOptions)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            const jsonObject = data;
            if (jsonObject.success) {
                //reminderCreatedNotification(jsonObject.responseMessage);
                alert("Upload successful");
                window.location.href = "/reminder/AddReminder";
            }
        })
        .catch(error => {
            // Handle any errors that occurred during the fetch
            console.error('Fetch error:', error);
        });
}


updateDateTime();
setInterval(updateDateTime, 1000);
//load();
getAllPatients();
getAllSponsors();