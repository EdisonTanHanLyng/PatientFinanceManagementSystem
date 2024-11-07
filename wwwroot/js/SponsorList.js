//This function displays and updates the time
function updateDateTime() {
    var current = new Date();

    // Date
    var dateOptions = { day: 'numeric', month: 'long', year: 'numeric' };
    var dateFormatter = new Intl.DateTimeFormat('en-US', dateOptions);
    var formattedDate = dateFormatter.format(current);

    // Time
    var timeOptions = { hour: 'numeric', minute: 'numeric', hour12: true };
    var timeFormatter = new Intl.DateTimeFormat('en-US', timeOptions);
    var formattedTime = timeFormatter.format(current);

    document.getElementById('date').innerHTML = '<strong>Date:</strong> ' + formattedDate;
    document.getElementById('time').innerHTML = '<strong>Time:</strong> ' + formattedTime;
}

updateDateTime();
setInterval(updateDateTime, 1000);

// This function toggles the search filter visible and invisible
function toggleFilterVisibility() {
    var filterDiv = document.getElementById('sponsor-filter');
    if (filterDiv.style.opacity === '0' || filterDiv.style.opacity === '') {
        filterDiv.style.display = 'flex';
        setTimeout(function () {
            filterDiv.classList.add('visible');
        }, 10);
    }
}

//Elmer
const getSponsor = `/getAllSponsor/`;

const additionalData = {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json'
    }
}

getSponsors();

async function getSponsors() {
    try {
        const response = await fetch(getSponsor, additionalData);
        if (!response.ok) {
            throw new Error(`Get Data Failed! : Sponsor Profile (${response.status})`);
        }
        const obtainedData = await response.json();
        displaySponsorList(obtainedData);
    } catch (error) {
        console.error(`Error Occurred: ${error}`);
    }
}


function parseSponsorInformation(jsonData) {
    const infoMap = new Map([
        ['code', jsonData.sponsorCode],
        ['name', jsonData.sponsorName],
        ['contactPerson', jsonData.contactPerson],
        ['officePhone', jsonData.sponsorPhone],
        ['sponsorId', jsonData.sponsorId]
    ]);
    return infoMap;
}
// This function is to display all sponsor information retrieved from the database
async function displaySponsorList(obtainedData) {
    const sponsorsMap = new Map();

    for (const [key, sponsorData] of Object.entries(obtainedData)) {
        const parsedData = parseSponsorInformation(sponsorData);
        sponsorsMap.set(sponsorData.sponsorId.toString(), parsedData);
    }

    const container = document.querySelector('.sponsorL-listContainer'); 
    container.setAttribute('b-fb5hzfbk4d', '');
    const ol = container.querySelector('.sponsorL-listContainer_ol');
    ol.setAttribute('b-fb5hzfbk4d', '');
    ol.innerHTML = ''; // Clear existing content
    
    for (const [sponsorKey, sponsorMap] of sponsorsMap) {
        const li = document.createElement('li');
        li.setAttribute('b-fb5hzfbk4d', '');
        li.className = "sponsorL-listContainer_list";

        const imageContainer = document.createElement('div');
        imageContainer.setAttribute('b-fb5hzfbk4d', '');
        imageContainer.className = "sponsorL-listContainer_list_image";
        
        const name_text = document.createElement('p');
        name_text.setAttribute('b-fb5hzfbk4d', '');
        name_text.className = "sponsorL-listContainer_list_text";
        name_text.innerHTML = "<strong>Sponsor Name:</strong> " + sponsorMap.get('name');
        imageContainer.appendChild(name_text);

        const code_text = document.createElement('p');
        code_text.setAttribute('b-fb5hzfbk4d', '');
        code_text.className = "sponsorL-listContainer_list_text";
        code_text.innerHTML = "<strong>Sponsor Code:</strong> " + sponsorMap.get('code');
        imageContainer.appendChild(code_text);

        const contactPerson_text = document.createElement('p');
        contactPerson_text.setAttribute('b-fb5hzfbk4d', '');
        contactPerson_text.className = "sponsorL-listContainer_list_text";
        contactPerson_text.innerHTML = "<strong>Contact Person:</strong> " + sponsorMap.get('contactPerson');
        imageContainer.appendChild(contactPerson_text);

        const tel_text = document.createElement('p');
        tel_text.setAttribute('b-fb5hzfbk4d', '');
        tel_text.className = "sponsorL-listContainer_list_text";
        tel_text.innerHTML = "<strong>Tel:</strong> " + sponsorMap.get('officePhone');
        imageContainer.appendChild(tel_text);

        li.appendChild(imageContainer);

        const submitButton = document.createElement('button');
        submitButton.setAttribute('b-fb5hzfbk4d', '');
        submitButton.className = "sponsorL-listContainer_button";
        submitButton.textContent = "Enter";
        li.appendChild(submitButton);

        ol.appendChild(li);

        submitButton.dataset.sponsorId = sponsorMap.get('sponsorId');
        submitButton.addEventListener('click', function () {
            const sponsorId = this.dataset.sponsorId;
            const encodeId = encodeURIComponent(sponsorId);
            fetch(`/getSponsorId/${encodeId}`, additionalData)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                if (response.redirected) {
                    sessionStorage.setItem('sponsorUserId', sponsorId);
                    window.location.href = '/Sponsor/SponsorInfo';
                    return;
                }
            })
            .catch((error) => {
                console.error('Error:', error);
            });
        });
    }
    container.appendChild(ol);
}

let timer;
const waitTime = 1000; // 1 second
document.addEventListener('DOMContentLoaded', () => {
    const searchName = document.getElementById('sponsor-search-name');
    if (searchName) {
        searchName.addEventListener('input', (e) => {
            clearTimeout(timer);
            timer = setTimeout(async () => {
                const searchValue = e.target.value;
                searchSponsorDetail();
            }, waitTime);
        });

    } else {
        console.error('Search bar element not found');
    }

    const searchCode = document.getElementById('sponsor-search-code');
    if (searchCode) {
        searchCode.addEventListener('input', (e) => {
            clearTimeout(timer);
            timer = setTimeout(async () => {
                const searchValue = e.target.value;
                searchSponsorDetail();
            }, waitTime);
        });
        
    } else {
        console.error('Search bar element not found');
    }

    const searchTel = document.getElementById('sponsor-search-tel');
    if (searchTel) {
        searchTel.addEventListener('input', (e) => {
            clearTimeout(timer);
            timer = setTimeout(async () => {
                const searchValue = e.target.value;
                searchSponsorDetail();
            }, waitTime);
        });
  
    } else {
        console.error('Search bar element not found');
    }

    const generalSearch = document.getElementById('sponsor-search-input');
    if (generalSearch) {
        generalSearch.addEventListener('input', (e) => {
            clearTimeout(timer);
            timer = setTimeout(async () => {
                const searchValue = e.target.value;
                searchGeneralSponsor();
            }, waitTime);
        });

    } else {
        console.error('Search bar element not found');
    }
});

async function searchGeneralSponsor() {
    try {
        const searchGeneral = document.getElementById('sponsor-search-input').value.trim() || "NULL";
        const encodedGeneral = encodeURIComponent(searchGeneral);
        const response = await fetch(`/getSearchSponsorGeneral/${encodedGeneral}`, additionalData);
        if (!response.ok) {
            throw new Error(`Get Data Failed! : Sponsor Profile (${response.status})`);
        }
        const data = await response.json();
        displaySponsorList(data);
    } catch (error) {
        clearScreen();
    }
}

async function searchSponsorDetail() {
    try {
        const searchName = document.getElementById('sponsor-search-name').value.trim() || "NULL";
        const searchCode = document.getElementById('sponsor-search-code').value.trim() || "NULL";
        const searchTel = document.getElementById('sponsor-search-tel').value.trim() || "NULL";

        const encodedName = encodeURIComponent(searchName);
        const encodedCode = encodeURIComponent(searchCode);
        const encodedTel = encodeURIComponent(searchTel);

        const response = await fetch(`/getSearchSponsor/${encodedName}/${encodedCode}/${encodedTel}`, additionalData);
        if (!response.ok) {
            throw new Error(`Get Data Failed! : Sponsor Profile (${response.status})`);
        }
        const data = await response.json();
        displaySponsorList(data);
    } catch (error) {
        console.error('Error:', error);
        clearScreen();
    }
}

async function clearScreen() {
    const container = document.querySelector('.sponsorL-listContainer');
    container.setAttribute('b-fb5hzfbk4d', '');
    const ol = container.querySelector('.sponsorL-listContainer_ol');
    ol.setAttribute('b-fb5hzfbk4d', '');
    ol.innerHTML = ''; // Clear existing content
}