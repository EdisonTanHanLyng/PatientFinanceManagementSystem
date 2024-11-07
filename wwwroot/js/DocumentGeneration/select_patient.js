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
    let patientID = patient['PatientID'];

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
        selectedPatient.PatientPhone === patientPhone &&
        selectedPatient.PatientID === patientID
    );

    if (isSelected) {
        checkbox.classList.add('active');
        checkbox.checked = true;
        checkmark.classList.add('selected');
    }

    //Toggle select/deselect on click
    checkbox.addEventListener('click', function () {
        let targetPatient = parsePatientInfo(patientName, patientIC, patientSponsor, patientPhone, patientID);

        // Uncheck any other active checkboxes
        const allCheckboxes = document.querySelectorAll('.add-reminder-patient-checkbox');
        allCheckboxes.forEach(cb => {
            cb.classList.remove('active');
            cb.checked = false;
            cb.closest('.add-reminder-patient-reminder-box').querySelector('.checkmark').classList.remove('selected');
        });

        // Clear the selectedPatients array
        selectedPatients = [];

        // Mark the current checkbox as selected
        checkbox.classList.add('active');
        checkmark.classList.add('selected');
        checkbox.checked = true;

        // Add the current patient to the selectedPatients array
        selectedPatients.push(targetPatient);
    });

    container.appendChild(itemBox);
}



function parsePatientInfo(name, nric, sponsor, phone, id) {
    let info = {
        PatientName: name,
        PatientNRIC: nric,
        PatientSponsor: sponsor,
        PatientPhone: phone,
        PatientID: id,
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
    let getPatList = `/DocumentGen/getAllPatients`;
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
                if (patient.SponsorList === filterState.filters.sponsor) {
                    filterState.patientList.push(patient);
                    displayPatients(patient);
                }
                
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
        "PatientID": item["PatientID"]
    };
    return info;
}


function saveAndUpload() {
    sessionStorage.setItem("selectedPatient", JSON.stringify({
        PatientName: selectedPatients[0].PatientName,
        PatientIC: selectedPatients[0].PatientNRIC,
        PatientID: selectedPatients[0].PatientID
    }));
    
    window.location.href = "/DocumentGen/" + sessionStorage.getItem("currentPage");
    sessionStorage.removeItem("currentSponsor");
    sessionStorage.removeItem("currentPage");
}


updateDateTime();
setInterval(updateDateTime, 1000);
//load();
updateFilterState('sponsor', sessionStorage.getItem("currentSponsor"));
getAllPatients();
//getAllSponsors();