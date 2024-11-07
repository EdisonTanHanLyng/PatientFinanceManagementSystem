var selectedSponsors = [];

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

function displaySponsors(sponsor) {
    let sponsorName = sponsor['SponsorName'];
    let sponsorCode = sponsor['SponsorCode'];
    let sponsorContactPerson = sponsor['SponsorContactPerson'];
    let sponsorOfficePhone = sponsor['SponsorOfficePhone'];
    let sponsorEmail = sponsor['SponsorEmail'];

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
                <div><strong>Sponsor </strong></div>
                <div><strong>Contact Person: </strong>${sponsorContactPerson}</div>
                <div><strong>Name: </strong>${sponsorName}</div>
                <div><strong>Office Phone: </strong>${sponsorOfficePhone}</div>
                <div><strong>Sponsor Code: </strong>${sponsorCode}</div>
                <div class=".hidden-email"><strong>Sponsor Email: </strong>${sponsorEmail}</div>
            </div>
        </div>
    `;

    const checkbox = itemBox.querySelector('.add-reminder-patient-checkbox');
    const checkmark = itemBox.querySelector('.checkmark');

    //Check if the patient is already selected
    let isSelected = selectedSponsors.some(selectedSponsor =>
        selectedSponsor.SponsorName === sponsorName &&
        selectedSponsor.SponsorCode === sponsorCode &&
        selectedSponsor.SponsorContactPerson === sponsorContactPerson &&
        selectedSponsor.SponsorOfficePhone === sponsorOfficePhone
    );

    if (isSelected) {
        checkbox.classList.add('active');
        checkbox.checked = true;
        checkmark.classList.add('selected');
    }

    //Toggle select/deselect on click
    checkbox.addEventListener('click', function () {
        let targetSponsor = parseSponsorInfo(sponsorName, sponsorCode, sponsorContactPerson, sponsorOfficePhone, sponsorEmail);

        if (checkbox.classList.contains('active')) {
            checkbox.classList.remove('active');
            checkmark.classList.remove('selected');
            checkbox.checked = false;

            selectedSponsors = selectedSponsors.filter(sponsor =>
                sponsor.SponsorName !== targetSponsor.SponsorName ||
                sponsor.SponsorCode !== targetSponsor.SponsorCode ||
                sponsor.SponsorContactPerson !== targetSponsor.SponsorContactPerson ||
                sponsor.SponsorOfficePhone !== targetSponsor.SponsorOfficePhone
            );

        } else {
            checkbox.classList.add('active');
            checkmark.classList.add('selected');
            checkbox.checked = true;

            selectedSponsors.push(targetSponsor);
        }
    });

    container.appendChild(itemBox);
}


function selectAllVisibleSponsors() {
    const checkboxes = document.querySelectorAll('.add-reminder-patient-checkbox');

    // Check if all visible checkboxes are already selected
    const allSelected = Array.from(checkboxes).every(checkbox => checkbox.classList.contains('active'));

    checkboxes.forEach(checkbox => {
        const checkmark = checkbox.nextElementSibling;
        const patientBox = checkbox.closest('.add-reminder-patient-reminder-box');

        const sponsorName = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(3)').textContent.split(': ')[1];
        const sponsorContactPerson = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(2)').textContent.split(': ')[1];
        const sponsorOfficePhone = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(4)').textContent.split(': ')[1];
        const sponsorCode = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(5)').textContent.split(': ')[1];
        const sponsorEmail = patientBox.querySelector('.add-reminder-patient-content-grid div:nth-child(6)').textContent.split(': ')[1];
        
        let targetSponsor = parseSponsorInfo(sponsorName, sponsorCode, sponsorContactPerson, sponsorOfficePhone, sponsorEmail);

        if (allSelected) {
            checkbox.classList.remove('active');
            checkmark.classList.remove('selected');

            // Remove patient from patientList
            let index = selectedSponsors.findIndex(sponsor =>
                sponsor.SponsorName === targetSponsor.SponsorName &&
                sponsor.SponsorCode === targetSponsor.SponsorCode &&
                sponsor.SponsorContactPerson === targetSponsor.SponsorContactPerson &&
                sponsor.SponsorOfficePhone === targetSponsor.SponsorOfficePhone
            );

            if (index !== -1) {
                selectedSponsors.splice(index, 1);
            }

        } else {
            // If not all are selected, select them
            if (!checkbox.classList.contains('active')) {
                checkbox.classList.add('active');
                checkmark.classList.add('selected');

                // Add to patientList if not already there
                let exists = selectedSponsors.some(sponsor =>
                    sponsor.SponsorName === targetSponsor.SponsorName &&
                    sponsor.SponsorCode === targetSponsor.SponsorCode &&
                    sponsor.SponsorContactPerson === targetSponsor.SponsorContactPerson &&
                    sponsor.SponsorOfficePhone === targetSponsor.SponsorOfficePhone
                );

                if (!exists) {
                    selectedSponsors.push(targetSponsor);
                }
            }
        }
    });
}




function parseSponsorInfo(name, code, contact, phone, email) {
    let info = {
        SponsorName: name,
        SponsorCode: code,
        SponsorContactPerson: contact,
        SponsorOfficePhone: phone,
        SponsorEmail: email,
        UserType: 'Sponsor'
    };

    return info;
}



/* Filter/Search functionality */
// Get filter name input from search bar
document.querySelector('#reminder-search-field-input').addEventListener('input', function (input) {
    updateFilterState('name', input.target.value);
    const container = document.querySelector('.add-reminder-patient-boxes');
    container.innerHTML = '';
    (filterEntries(filterState.sponsorList, filterState.filters)).forEach(patient => { displaySponsors(patient) });
});

// Get filter sponsor input from drop-down menu
document.getElementById("reminder-search-sponsor").addEventListener('change', function () {
    let value = this.value;
    let search = '';

    if (value !== 'none') search = value;
    updateFilterState('sponsorCode', search);

    const container = document.querySelector('.add-reminder-patient-boxes');
    container.innerHTML = '';
    (filterEntries(filterState.sponsorList, filterState.filters)).forEach(patient => { displaySponsors(patient) });
});


var filterState = {
    sponsorList: [],
    filters: {
        name: '',
        sponsorCode: ''
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
    if (filters.sponsorCode) {
        filteredEntries = filter(filters.sponsorCode, filteredEntries, "Sponsor");
    }
    return filteredEntries;
}

function filter(search, sponsors, category) {
    let filteredList = [];

    if (search.length == 0) return sponsors; //If nothing to filter, return everything
    for (let i = 0; i < sponsors.length; i++) {

        if (category === "Name" && (compare(search.toLowerCase(), sponsors[i]['SponsorName'].toLowerCase()))) //Name filter
            filteredList.push(sponsors[i]);
        else if (category === "Sponsor" && (compare(search.toLowerCase(), sponsors[i]['SponsorCode'].toLowerCase()))) //Sponsor filter
            filteredList.push(sponsors[i]);
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







async function getAllSponsors() {
    let getSponsors = `/Reminder/getAllSponsors`;
    const loadingContainer = document.querySelector('.loading-container');
    const container = document.querySelector('.add-reminder-patient-boxes');

    // Show loading spinner
    loadingContainer.style.display = 'block';
    container.style.display = 'none';

    await fetch(getSponsors)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed!: Patient List (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            Object.values(obtainedData).forEach(item => {
                const sponsor = parseSponsorInformation(item);
                filterState.sponsorList.push(sponsor);
                displaySponsors(sponsor);
            });


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
        }).finally(() => {
            // Hides spinner and shows reminder list
            loadingContainer.style.display = 'none';
            container.style.display = 'block';
        });
}


function parseSponsorInformation(item) {
    let info = {
        "SponsorName": item['sponsor_name'],
        "SponsorCode": item['sponsor_code'],
        "SponsorContactPerson": item['contact_person'],
        "SponsorOfficePhone": item['office_phone'],
        "SponsorEmail": item['email']
    };
    return info;
}


async function saveAndUpload() {
    const requestOptions = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(selectedSponsors)
    }
    const apiUrl = `/reminder/saveAndUploadSponsors`;

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
getAllSponsors();