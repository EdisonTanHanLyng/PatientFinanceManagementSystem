var searchFilter = {
    patientList: [],
    filters: {
        name: '',
        date: ''
    }
};

async function updateFilterState(key, value) {
    searchFilter.filters[key] = value;
}

async function filterEntries(entries, filters) {
    let filteredEntries = entries;

    if (filters.name) { // Filter by Name
        filteredEntries = await filter(filters.name, filteredEntries, "Name");
    }
    if (filters.date) { // Filter by Date
        filteredEntries = await filter(filters.date, filteredEntries, "Date");
    }

    return filteredEntries;
}

async function filter(search, patients, category) {
    let filteredList = [];

    if (search.length == 0) return patients; // If nothing to filter, return everything
    
    if (category === "Name") {
        for (let i = 0; i < patients.length; i++) {
            if (compare(search.toLowerCase(), patients[i].patientName.toLowerCase())) {
                filteredList.push(patients[i]); // Name filter
            }
        }
    }

    if (category === "Date") {
        const filteredEntries = patients.filter(entry => {
            const entryDate = parseInt(entry.startDate.replace(/-/g, ''));
            return entryDate >= parseInt(search);
        });

        filteredList = filteredEntries;
        filteredList.sort((a, b) => {
            const dateA = parseInt(a.startDate.replace(/-/g, ''));
            const dateB = parseInt(b.startDate.replace(/-/g, ''));

            return dateA - dateB;
        });
    }
    
    return filteredList;
}

function compare(search, target) {
    let start = false;
    let match = false;
    let index = 0;

    // Remove whitespaces
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

document.querySelector('#sponsor-info-overview-search-input').addEventListener('input', async function (input) {
    await updateFilterState('name', input.target.value);
    const filteredPatients = await filterEntries(searchFilter.patientList, searchFilter.filters);

    clearHDRecList();
    filteredPatients.forEach(patient => {
        generateEntryListItem(patient);
    });
});

document.querySelector('#sponsor-info-overview-date-input').addEventListener('input', async function (input) {
    let search = input.target.value.replace(/-/g, '');

    await updateFilterState('date', search);
    const filteredPatients = await filterEntries(searchFilter.patientList, searchFilter.filters);

    clearHDRecList();
    filteredPatients.forEach(patient => {
        generateEntryListItem(patient);
    });
    input.target.blur();
});

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



/* Page is adopted from Ariel's Patient Information Page */
// -- Page Animation Functionality --
const histList = document.getElementsByClassName('sponsor-info-overview-entry-list')[0];
const docList = document.getElementsByClassName('sponsor-info-overview-document-list')[0];

//Summary Block Element
const summBlock = document.getElementsByClassName('sponsor-info-summary')[0];
const tassleBtn = document.getElementsByClassName('sponsor-info-summary-collapse-button')[0];
let summBlockState = true;

tassleBtn.onclick = function () {
    summBlockState = summBlockM(summBlockState);
};

//My overwhelming aura is not required this time
function summBlockM(state) {
    if (state) {
        summBlock.classList.add('minimise');
        histList.classList.add('expand');
        docList.classList.add('expand');
        state = false;
    }
    else {
        summBlock.classList.remove('minimise');
        histList.classList.remove('expand');
        docList.classList.remove('expand');
        state = true;
    }
    return state;
}

//Overview Tabs Elements
const histTab = document.getElementsByClassName('sponsor-info-tab1')[0];
const docTab = document.getElementsByClassName('sponsor-info-tab2')[0];
const pageSelector = document.getElementsByClassName('sponsor-info-overview-content')[0];
const page1 = document.getElementsByClassName('sponsor-info-overview-page1')[0];
const page2 = document.getElementsByClassName('sponsor-info-overview-page2')[0];
let currTab = 0;

histTab.onclick = function () {
    currTab = tabSelector(0);

    timer = setTimeout(() => {
        entryList.classList.remove('hidden');
    }, 680);
};

docTab.onclick = function () {
    currTab = tabSelector(1);
};

// Tab Selector Functionality
function tabSelector(tabNumber) {
    if (tabNumber === 0) { //Hist Tab
        docTab.classList.remove('selected');
        histTab.classList.add('selected');
        pageSelector.classList.remove('page2');
        page1.classList.remove('unactive');
        page2.classList.add('unactive');
    }
    else if (tabNumber === 1) { //Doc Tab
        entryList.classList.add('hidden');
        histTab.classList.remove('selected');
        docTab.classList.add('selected');
        pageSelector.classList.add('page2');
        page1.classList.add('unactive');
        page2.classList.remove('unactive');
    }
    else {
        //Err
    }
    return tabNumber;
}





// Data Fetching Functionalities
const sponsorID = 500;
const patientIDEnd = 505;
const sponsorId = sessionStorage.getItem('sponsorUserId');
const getSponsor = `/getSponsor/${sponsorId}`;
const getPatient = `/getPatient/${sponsorId}/${patientIDEnd}`;
const getPatientNameAPI = `/getPatientName/${sponsorId}/${patientIDEnd}`;
var localSponsorNRIC = "";
const loadingDocBar = document.getElementsByClassName('SponsorInfo-DocBar-loading')[0];

// Get request header data
const additionalData = {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json'
    }
}

function getSponsors() {
    fetch(getSponsor, additionalData)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed! : Sponsor Profile (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            //parse and format data
            //console.log(obtainedData);
            summaryBoxGen(parseSponsorInformation(obtainedData));

            getPatientInfo(obtainedData);

            getDocs(obtainedData['sponsor_id']);
            localSponsorNRIC = obtainedData['sponsor_id'];
        })
        .catch(error => {
            console.error(`Error Occured: ${error}`);
        })
}

async function getPatientInfo() {
    try {
        const name = await getPatientName();
        const response = await fetch(getPatient, additionalData);

        if (!response.ok) {
            throw new Error(`Get Data Failed! : Patient Info (${response.status})`);
        }

        const obtainedData = await response.json();
        let list = [];

        for (let i = 0; i < obtainedData.length; i++) {
            const patientInformation = parsePatientInformation(obtainedData[i].HDRecords[0], name[i]);
            list.push(patientInformation);
        }

        // Sort by patient name
        list.sort((a, b) => a.patientName.localeCompare(b.patientName));

        list.forEach(item => {
            generateEntryListItem(item);
        });

        searchFilter.patientList = list;
    } catch (error) {
        console.error(`Error Occurred: ${error}`);
    }
}

async function getPatientName() {
    try {
        const response = await fetch(getPatientNameAPI, additionalData);

        if (!response.ok) {
            throw new Error(`Get Data Failed! : Patient Info (${response.status})`);
        }

        const obtainedData = await response.json();
        let names = obtainedData.map(name => name["FullName"]);

        return names;
    } catch (error) {
        console.error(`Error Occurred: ${error}`);
    }
}

async function uploadFile(file, docObj) {
    var formData = new FormData();
    formData.append('file', file);
    formData.append('data', JSON.stringify(docObj));

    const mainData = {
        patientDoc: docObj,
        formFile: formData
    }

    const query = "/SponsorDocumentUpload";
    const additional = {
        method: 'POST',
        body: formData
    }

    console.log(formData);
    let returnVal = false;
    await fetch(query, additional)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Post Data Failed! : Document Upload: (${response.status})`);
            }
            else {
                returnVal = true;
                console.log("Successfully Uploaded Document!");
            }
            //const data = response.json();
            return returnVal;
        })
        .catch(error => {
            console.log(`Error Occurred: ${error}`);
        });

    await getDocs(docObj.sponID);
    console.log("Refreshed!");
    return returnVal;
}

// Uses GET from controller to obtain JSON Document Data
async function getDocs(sponID) {
    query = `/getSponDocs/${sponID}`;
    loadingDocBar.classList.add('progress');
    await fetch(query, additionalData)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed! : Get Documents (${response.status})`);
            }
            const data = response.json();
            return data;
        })
        .then(obtainedData => {
            // parse and format data
            console.log("Get Documents");
            console.log(obtainedData);
            loadingDocBar.classList.remove('progress');
            clearDocList();
            obtainedData.forEach(doc => {
                // generate list view
                const processed = processParsedDoc(parseDocumentJson(doc));
                console.log(processed);
                generateDocItem(processed);
            });
        })
        .catch(error => {
            loadingDocBar.classList.remove('progress');
            console.error(`Error Occurred: ${error}`);
        });
}

// Uses GET from controller to obtain a filtered and sorted list of JSON document data
async function getDocFilt(sponID, searchStr) {
    query = `/getSponDocFilt/${sponID}/${searchStr}`;
    loadingDocBar.classList.add('progress');
    if (searchStr == "" || searchStr == " ") {
        await getDocs(sponID);
    }
    else {
        await fetch(query, additionalData)
            .then(response => {
                if (!response.ok) {
                    throw new Error(` Get Data Failed! : Get Documents (${response.status})`);
                }
                const data = response.json();
                return data;
            })
            .then(obtainedData => {
                // parse and format data
                console.log("Get Documents Filter");
                console.log(obtainedData);
                loadingDocBar.classList.remove('progress');
                clearDocList();
                obtainedData.forEach(doc => {
                    // generate list view
                    const processed = processParsedDoc(parseDocumentJson(doc));
                    generateDocItem(processed);
                });
            })
            .catch(error => {
                loadingDocBar.classList.remove('progress');
                console.error(`Error Occurred: ${error}`);
            });
    }
}

// Uses PUT to update the given doc in the database
async function updateDoc(docObj) {
    const query = "/updateSponDoc";
    const additional = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(docObj)
    }
    let returnVal = false;

    await fetch(query, additional)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Put Data Failed! : Document Update: (${response.status})`);
            }
            else {
                returnVal = true;
                console.log("Successfully Updated Document!");
            }
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });

    await getDocs(docObj.sponID);
    console.log("Refreshed!");
    return returnVal;
}

// Uses PUT to delete the document ID in teh database
async function deleteDoc(docID, sponID) {
    const query = `/deleteSponDoc/${docID}`;
    const additional = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    }
    let returnVal = false;

    await fetch(query, additional)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get? Data Failed! : Document Delete: (${response.status})`);
            }
            else {
                returnVal = true;
                console.log("Successfully Deleted Document!");
            }
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });

    await getDocs(sponID);
    console.log("Refreshed!");
    return returnVal;
}

function parseDocumentJson(jsonData) {
    const data = {
        docID: 0,
        date: "",
        docName: "Document 1234",
        docRef: "",
        isPreviewable: 0,
        remarks: "",
        sponID: "",
        base64String: ""
    };
    data.docID = jsonData["docID"];
    data.date = jsonData["date"];
    data.docName = jsonData["docName"];
    data.docRef = jsonData["docRef"];
    data.isPreviewable = jsonData["isPreviewable"];
    data.sponID = jsonData["sponID"];
    data.remarks = jsonData["remarks"];
    data.base64String = jsonData["base64String"];
    return data;
}

const sponsorInformation = {
    name: '',
    code: '',
    location: '',
    contactPerson: '',
    officePhone: '',
    officeFax: '',
    term: ''
};


function parseSponsorInformation(jsonData) {
    const info = sponsorInformation;

    info.code = jsonData['sponsor_code'];
    info.name = jsonData['sponsor_name'];
    info.location = jsonData['address'];
    info.contactPerson = jsonData['contact_person'];
    info.officePhone = jsonData['office_phone'];
    info.officeFax = jsonData['office_fax'];
    info.term = jsonData['term'];

    return info;
}

function parsePatientInformation(jsonData, name) {

    let dataObj = {
        patientName: "",
        startDate: "",
        endDate: 0,
        sponsorType: 'No of Times Per Month',
        noOfLimpSum: 0,
        amtSponsored: 0,
        epoQuantity: 0,
        epoPrice: 0
    };
    const amountSponsored = ['100', '200', '500'];
    let rand = Math.floor(Math.random() * amountSponsored.length);
    let qty = Math.floor(Math.random() * 10);
    let qty2 = Math.floor(Math.random() * 3);

    dataObj.patientName = name;
    dataObj.startDate = jsonData["date"];
    dataObj.endDate = "";
    dataObj.sponsorType = "No of Times Per Month";
    dataObj.noOfLimpSum = qty2;
    dataObj.amtSponsored = amountSponsored[rand];
    dataObj.epoQuantity = qty;
    dataObj.epoPrice = "20.00 MYR";

    return dataObj;
}


// -- Summary Block --

const sumBoxLeft = document.getElementsByClassName('sponsor-info-left')[0];
const sumBoxRight2 = document.getElementsByClassName('sponsor-info-right')[0];

// Generates the summary block presenting summary information on a specific sponsor
function summaryBoxGen(sumData) {
    sumBoxLeft.innerHTML = "";
    const desc = document.createElement('h2')
    desc.innerHTML = `Description/Information`;
    const n1 = document.createElement('p');
    n1.innerHTML = `<b>Sponsor Name:</b> ${sumData.name} &nbsp;&nbsp;&nbsp; <b> Sponsor Code:</b> ${sumData.code}`;
    const n2 = document.createElement('p');
    n2.innerHTML = `<b>Contact Person:</b> ${sumData.contactPerson} &nbsp;&nbsp;&nbsp; <b>Office Fax:</b> ${sumData.officeFax} &nbsp;&nbsp;&nbsp; <b>Office Phone:</b> ${sumData.officePhone}`;
    const n3 = document.createElement('p');
    n3.innerHTML = `<b>Location:</b> ${sumData.location}`;
    const n4 = document.createElement('p');
    n4.innerHTML = `<b>Term:</b> ${sumData.term}`;

    sumBoxLeft.appendChild(desc);
    sumBoxLeft.appendChild(n1);
    sumBoxLeft.appendChild(n2);
    sumBoxLeft.appendChild(n3);
    sumBoxLeft.appendChild(n4);
}

// -- Page 1 --

const entryList = document.getElementsByClassName('sponsor-info-overview-entry-list')[0];


let entryArray = [];
let histBtnCounter = 0;

// Clear List
function clearHDRecList() {
    entryList.innerHTML = "";
}

// Dynamic approach to creating entry list items, enables button functionality
function generateEntryListItem(data) {
    const t1Arr = [
        ["Start Date", `${data.startDate}`],
        ["End Date", `${data.endDate}`],
        ["Sponsor Type", `${data.sponsorType}`],
        ["No. of Times of Limp Sum", `${data.noOfLimpSum}`]
    ];
    const t2Arr = [
        ["Dialysis Price", `200`],
        ["Amt Sponsored", `${data.amtSponsored}`],
        ["EPO Qty", `${data.epoQuantity}`],
        ["EPO Price", `${data.epoPrice}`]
    ];

    //Level 1
    const listItem = document.createElement('li');
    //Level 2
    const nameHeader = document.createElement('div');
    const content = document.createElement('div');
    //Level 4 
    const table1 = document.createElement('table');
    const table2 = document.createElement('table');

    // Name Header
    const nameHolder = document.createElement('p');
    nameHolder.innerHTML = `<strong> ${data.patientName}</strong> `;
    nameHeader.appendChild(nameHolder);

    // Table 1
    for (let i = 0; i < t1Arr.length; i++) {
        const row = document.createElement('tr');
        const d1 = document.createElement('td');
        const d2 = document.createElement('td');
        d1.innerHTML = t1Arr[i][0];
        d2.innerHTML = t1Arr[i][1];
        row.appendChild(d1);
        row.appendChild(d2);
        table1.appendChild(row);
    }


    for (let i = 0; i < t2Arr.length; i++) {
        const row = document.createElement('tr');
        const d1 = document.createElement('td');
        const d2 = document.createElement('td');
        d1.innerHTML = t2Arr[i][0];
        d2.innerHTML = t2Arr[i][1];
        row.appendChild(d1);
        row.appendChild(d2);
        table2.appendChild(row);
    }

    content.appendChild(table1);
    content.appendChild(table2);

    listItem.appendChild(nameHeader);
    listItem.appendChild(content);

    entryList.appendChild(listItem);
}

function getCurrDate() {
    const date = new Date();
    const dateString = `${date.getDate}/${date.getMonth}/${date.getFullYear}`;
    let dateString2 = date.toLocaleDateString("zh-Hans-CN", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit"
    });
    dateString2 = dateString2.replaceAll("/", "-");
    //console.log(dateString2);
    return dateString2;
}

function convertToCommonDate(dateString) {
    const date = new Date(dateString);
    let dateString2 = date.toLocaleDateString("zh-Hans-CN", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit"
    });
    dateString2 = dateString2.replaceAll("/", "-");
    //console.log(dateString2);
    return dateString2;
}

function convertToFullDate(dateString) {
    const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    var date = new Date(dateString);
    var day = date.getDate();
    var month = months[date.getMonth()];
    var year = date.getFullYear();

    const dateString2 = `${day} ${month}, ${year}`;

    return dateString2;
}

//DP, Doc Tab Page (page 2) copied and modified from Ariel's patientinfo.js code
const uploadDocBtn = document.getElementsByClassName('sponsor-info-overview-document-bar-upload-button')[0];
const filePrompt = document.getElementById('fileInput');

const docDataStruct = {
    date: "error getting date",
    docName: "error getting name",
    docData: "",
    remarks: ""
};

const waitTime = 1000;
let docSTimer;
const docSearchBar = document.getElementById('docSearch');
docSearchBar.addEventListener('keyup', (keyEvent) => {
    clearTimeout(docSTimer);
    docSTimer = setTimeout(() => {
        console.log(`doc Typed: ${keyEvent.target.value}`);
        getDocFilt(localSponsorNRIC, keyEvent.target.value);
    }, waitTime);
});

docSearchBar.addEventListener('keypress', (keyEvent) => {
    if (keyEvent.key == "Enter") {
        clearTimeout(timer); // stop the keyup event listener
        docStimer = null;
        getDocFilt(localSponsorNRIC, keyEvent.target.value);
    }
});

// Arrays used for generateDocItem2
let docListItemArr = [];
let docContentDivArr = [];
let docControlBtnArr = [];
const docObjArr = []; // Array Containing the document objects
let docBtnCounter = 0;

function toggleDocMini(docElemIndex) {
    docContentDivArr[docElemIndex].classList.toggle('minimise');
    docListItemArr[docElemIndex].classList.toggle('minimise');
    docControlBtnArr[docElemIndex].classList.toggle('minimise');
}

function generateDocItem(dData) {
    //console.log(dData);
    const listItem = document.createElement('li');

    const heading = document.createElement('p');
    heading.innerHTML = `Date: ${dData.date} | ${dData.docName}`;
    listItem.appendChild(heading);

    const contentDiv = document.createElement('div');
    const docDiv = document.createElement('div');

    //const docPrev = document.createElement('iframe');
    //docPrev.src = dData.docRef;
    //docDiv.appendChild(docPrev);
    docDiv.appendChild(generateDocPrev(dData)); // Generate the document preview dynamically

    contentDiv.appendChild(docDiv);

    const descDiv = document.createElement('div');
    const remarks = document.createElement('p');
    remarks.innerHTML = `Remarks`;
    descDiv.appendChild(remarks);
    const remarksContent = document.createElement('div');
    remarksContent.innerHTML = `${dData.remarks}`;
    descDiv.appendChild(remarksContent);

    const buttonsDiv = document.createElement('div');

    const editBtn = document.createElement('button');
    editBtn.setAttribute("value", docBtnCounter);
    editBtn.innerHTML = `Edit`;

    const downloadBtn = document.createElement('button');
    downloadBtn.setAttribute("class", "SponsorInfo-Overview-DocList-Download");
    downloadBtn.setAttribute("value", docBtnCounter);
    downloadBtn.innerHTML = `Download`;

    buttonsDiv.appendChild(editBtn);
    buttonsDiv.appendChild(downloadBtn);

    descDiv.appendChild(buttonsDiv);
    contentDiv.appendChild(descDiv);

    docContentDivArr.push(contentDiv);

    listItem.appendChild(contentDiv);

    const controlBtn = document.createElement('button');
    const iconHolder = document.createElement('span');
    iconHolder.classList.add('material-symbols-outlined');
    iconHolder.innerHTML = `more_horiz`;
    controlBtn.appendChild(iconHolder);
    controlBtn.setAttribute("value", docBtnCounter);
    docBtnCounter++;
    listItem.appendChild(controlBtn);

    docControlBtnArr.push(controlBtn);
    docListItemArr.push(listItem);

    editBtn.onclick = function () {
        console.log(`Clicked Edit Doc: ${this.value}`);
        listItem.setAttribute("tabindex", '-1');
        listItem.focus();
        listItem.classList.toggle('editingMod');
        descDiv.classList.toggle('editingMod');
        docDiv.classList.toggle('editingMod');
        if (summBlockState) {
            summBlockState = summBlockM(summBlockState);
        }
        //editmode
        console.log(this.value);
        editModeDocListItem(docObjArr[this.value], descDiv, docDiv, listItem, this.value);
    }

    downloadBtn.onclick = function () {
        console.log(`Clicked Download Doc: ${this.value}`);
        // Trigger Download
        const down = document.createElement('a');
        down.href = docObjArr[this.value].docRef;
        down.download = docObjArr[this.value].docName;
        down.click();
    }

    controlBtn.onclick = function () {
        console.log(`Clicked Control: ${this.value}`);
        toggleDocMini(this.value);
    };
    docList.appendChild(listItem);
    toggleDocMini(controlBtn.value); // set minimised initially
}

function editModeDocListItem(dData, descDiv, docDiv, listItem, value) {
    // Doc Preview
    //console.log(dData);
    //docDiv.innerHTML = "";
    const upL = document.createElement('button');
    upL.innerHTML = "Replace";

    //const uploadFile = document.createElement('input');
    //uploadFile.innerHTML = "Change Upload";
    //uploadFile.setAttribute("type", "file");
    //uploadFile.setAttribute("accept", ".pdf, image/*");

    //docDiv.appendChild(uploadFile);

    // Remarks
    descDiv.innerHTML = "";

    const editTable = document.createElement('table');
    //Row 1
    const row1 = document.createElement('tr');
    const r1d1 = document.createElement('td');
    r1d1.innerHTML = "Date of Edit";
    const r1d2 = document.createElement('td');

    const editDate = document.createElement('input');
    editDate.setAttribute("type", "date");
    editDate.setAttribute("value", getCurrDate());
    editDate.value = getCurrDate();
    console.log(editDate.value);
    r1d2.appendChild(editDate);
    row1.appendChild(r1d1);
    row1.appendChild(r1d2);
    editTable.appendChild(row1);

    // Row 2
    const row2 = document.createElement('tr');
    const r2d1 = document.createElement('td');
    r2d1.innerHTML = "Document Name";
    const r2d2 = document.createElement('td');

    const editName = document.createElement('input');
    editName.setAttribute("type", "text");
    editName.setAttribute("value", dData.docName);
    r2d2.appendChild(editName);
    row2.appendChild(r2d1);
    row2.appendChild(r2d2);
    editTable.appendChild(row2);

    // Row 3
    const row3 = document.createElement('tr');
    const r3d1 = document.createElement('td');
    r3d1.innerHTML = "Delete?";
    const r3d2 = document.createElement('td');

    const checkbox = document.createElement('input');
    checkbox.setAttribute("type", "checkbox");
    checkbox.setAttribute("id", "checkB");
    checkbox.setAttribute("value", value);
    r3d2.appendChild(checkbox);
    row3.appendChild(r3d1);
    row3.appendChild(r3d2);
    editTable.appendChild(row3);

    //Remarks Section
    const remarksdiv = document.createElement('div');
    const rem = document.createElement('p');
    rem.innerHTML = "Remarks";
    const editRemarks = document.createElement('textarea');
    editRemarks.setAttribute("wrap", "soft");
    remarksdiv.appendChild(rem);
    remarksdiv.appendChild(editRemarks);
    editRemarks.innerHTML = dData.remarks;

    // Buttons
    const buttonsDiv = document.createElement('div');

    const saveBtn = document.createElement('button');
    saveBtn.setAttribute("value", value);
    saveBtn.innerHTML = "Save";

    const cancelBtn = document.createElement('button');
    cancelBtn.setAttribute("value", value);
    cancelBtn.innerHTML = "Cancel";

    buttonsDiv.appendChild(saveBtn);
    buttonsDiv.appendChild(cancelBtn);

    // Append Desc Elements
    descDiv.appendChild(editTable);
    descDiv.appendChild(remarksdiv);
    descDiv.appendChild(buttonsDiv);

    saveBtn.onclick = function () {
        listItem.classList.toggle('editingMod');
        descDiv.classList.toggle('editingMod');
        docDiv.classList.toggle('editingMod');
        //docObjArr[this.value].remarks = editRemarks.value;

        const deleteConf = $("#checkB").prop("checked");
        //console.log(deleteConf);

        console.log(editDate.value);
        console.log(convertToCommonDate(docObjArr[this.value].date));

        if (deleteConf) {
            console.log("DELETE DOC");
            console.log(deleteDoc(docObjArr[this.value].docID, docObjArr[this.value].sponID));
        }
        else if (editRemarks.value != docObjArr[this.value].remarks || editName.value != docObjArr[this.value].docName || editDate.value != convertToCommonDate(docObjArr[this.value].date)) {
            console.log("UPDATE DOC");
            docObjArr[this.value].remarks = editRemarks.value;
            docObjArr[this.value].docName = editName.value;
            docObjArr[this.value].date = convertToFullDate(editDate.value);
            //console.log(convertToFullDate(editDate.value));
            console.log(updateDoc(docObjArr[this.value]));
        }

        console.log(`Save Button: ${this.value}`);
        resetDocListItem(docObjArr[this.value], descDiv, docDiv, listItem, this.value);
    }

    cancelBtn.onclick = function () {
        listItem.classList.toggle('editingMod');
        descDiv.classList.toggle('editingMod');
        docDiv.classList.toggle('editingMod');
        console.log(`Cancel Button: ${this.value}`);
        resetDocListItem(docObjArr[this.value], descDiv, docDiv, listItem, this.value);
    }
}

function resetDocListItem(dData, descDiv, docDiv, listItem, value) {

    // Reset Doc Div
    //console.log(dData);
    docDiv.innerHTML = "";
    docDiv.appendChild(generateDocPrev(dData));

    descDiv.innerHTML = "";
    const remarks = document.createElement('p');
    remarks.innerHTML = `Remarks`;
    descDiv.appendChild(remarks);

    const remarksContent = document.createElement('div');
    remarksContent.innerHTML = `${dData.remarks}`;
    descDiv.appendChild(remarksContent);

    const buttonsDiv = document.createElement('div');

    const editBtn = document.createElement('button');
    editBtn.setAttribute("value", value);
    editBtn.innerHTML = `Edit`;

    const downloadBtn = document.createElement('button');
    downloadBtn.setAttribute("class", "sponsor-info-Overview-DocList-Download");
    downloadBtn.setAttribute("value", value);
    downloadBtn.innerHTML = `Download`;

    buttonsDiv.appendChild(editBtn);
    buttonsDiv.appendChild(downloadBtn);

    descDiv.appendChild(buttonsDiv);

    editBtn.onclick = function () {
        console.log(`Clicked Edit Doc: ${this.value}`);
        listItem.setAttribute("tabindex", '-1');
        listItem.focus();
        listItem.classList.toggle('editingMod');
        descDiv.classList.toggle('editingMod');
        docDiv.classList.toggle('editingMod');
        if (summBlockState) {
            summBlockState = summBlockM(summBlockState);
        }
        //editmode
        console.log(this.value);
        editModeDocListItem(docObjArr[this.value], descDiv, docDiv, listItem, this.value);
    }
    downloadBtn.onclick = function () {
        console.log(`Clicked Download Doc: ${this.value}`);
        // Trigger Download
        const down = document.createElement('a');
        down.href = docObjArr[this.value].docRef;
        down.download = docObjArr[this.value].docName;
        down.click();
    }
}

function clearDocList() {
    docList.innerHTML = "";
}

// This is used to generate the document preview on the front end
function generateDocPrev(docObj) {
    //const docObj = generateNewDocObj(file); // Generate New Document View
    if (docObj.isPreviewable == 1) {
        const docPrev = document.createElement('iframe');
        docPrev.src = docObj.docRef;
        //docDiv.appendChild(docPrev);
        return docPrev;
    }
    else {
        const docPrev = document.createElement('span');
        docPrev.setAttribute("class", "material-symbols-outlined");

        docPrev.innerHTML = "draft";

        //docDiv.appendChild(docPrev);
        return docPrev;
    }
}

// This is used to generate a new document uploaded by user
function generateNewDocObj(file) {

    //const data = docDataStruct;
    const data = {
        docID: 0,
        date: "",
        docName: "Document 1234",
        docRef: "",
        isPreviewable: 0,
        remarks: "",
        sponID: "",
        base64String: ""
    };

    const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    var date = new Date();
    var day = date.getDate();
    var month = months[date.getMonth()];
    var year = date.getFullYear();

    const fileExt = file.name.split('.').pop();

    data.docName = file.name;
    data.date = `${day} ${month}, ${year}`;
    data.sponID = localSponsorNRIC;
    switch (fileExt) { // Attempt to catch unsupported preview file types
        case "docx":
        case "doc":
            data.isPreviewable = 0;
            data.docRef = "DOC";
            break;
        case "xlsx":
        case "xls":
            data.isPreviewable = 0;
            data.docRef = "XLS";
            break;
        case "pdf":
        case "PDF":
        case "png":
        case "PNG":
        case "gif":
        case "GIF":
        case "jpeg":
        case "jpg":
        case "JPEG":
        case "JPG":
            data.isPreviewable = 1;
            data.docRef = URL.createObjectURL(file);
            data.docRef += "#toolbar=0&page=1";
            break;
        default: // Defaults to other Non previewable
            data.isPreviewable = 0;
            data.docRef = "OTHER";
            break;
    }
    docObjArr.push(data);
    const inserted = uploadFile(file, data);
    if (inserted) {
        console.log("Successful Insertion");
    }
    else {
        console.log("Upload Failure");
    }
    return data;
}

uploadDocBtn.onclick = function () {
    const data = docDataStruct;
    //fetch data
    //generate document list item
    //generateDocItem(data);
    filePrompt.click();
}

// Reference: Dick's document upload preview
filePrompt.addEventListener('change', function () {
    const file = this.files[0];
    if (file) {
        console.log(`File In: ${file.name}`);
        // Send to Backend
        // Generate the preview
        console.log("attempt to upload doc");
        const docObj = generateNewDocObj(file);
        generateDocItem(docObj);
    }
    else {
        console.log(`No file selected`);
    }

});

function processParsedDoc(parsedDoc) {
    let fileExt = parsedDoc.docName.split('.').pop();
    fileExt = fileExt.toLowerCase();
    console.log(fileExt);
    switch (fileExt) { // Attempt to catch unsupported preview file types
        case "docx":
        case "doc":
            parsedDoc.docRef = "DOC";
            if (parsedDoc.base64String != "") {
                const byteChars = atob(parsedDoc.base64String);
                const byteNums = new Array(byteChars.length);
                for (let i = 0; i < byteChars.length; i++) {
                    byteNums[i] = byteChars.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNums);
                const blobby = new Blob([byteArray], { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' });
                parsedDoc.docRef = URL.createObjectURL(blobby);
                //parsedDoc.docRef += "#toolbar=0&page=1";
            }
            break;
        case "xlsx":
        case "xls":
            parsedDoc.docRef = "XLS";
            if (parsedDoc.base64String != "") {
                const byteChars = atob(parsedDoc.base64String);
                const byteNums = new Array(byteChars.length);
                for (let i = 0; i < byteChars.length; i++) {
                    byteNums[i] = byteChars.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNums);
                const blobby = new Blob([byteArray], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                parsedDoc.docRef = URL.createObjectURL(blobby);
                //parsedDoc.docRef += "#toolbar=0&page=1";
            }
            break;
        default: // Defaults to previewable documents
            if (parsedDoc.base64String != "") {
                const byteChars = atob(parsedDoc.base64String);
                const byteNums = new Array(byteChars.length);
                for (let i = 0; i < byteChars.length; i++) {
                    byteNums[i] = byteChars.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNums);
                if (fileExt == "pdf") {
                    const blobby = new Blob([byteArray], { type: 'application/pdf' });
                    parsedDoc.docRef = URL.createObjectURL(blobby);
                    parsedDoc.docRef += "#toolbar=0&page=1";
                }
                else if (fileExt == "png") {
                    const blobby = new Blob([byteArray], { type: 'image/png' });
                    parsedDoc.docRef = URL.createObjectURL(blobby);
                    parsedDoc.docRef += "#toolbar=0";
                    //console.log("image loaded i think?");
                }
                else if (fileExt == "jpeg" || fileExt == "jpg") {
                    const blobby = new Blob([byteArray], { type: 'image/jpeg' });
                    parsedDoc.docRef = URL.createObjectURL(blobby);
                    parsedDoc.docRef += "#toolbar=0";
                }
                else if (fileExt == "gif") {
                    const blobby = new Blob([byteArray], { type: 'image/gif' });
                    parsedDoc.docRef = URL.createObjectURL(blobby);
                    parsedDoc.docRef += "#toolbar=0";
                }
                else {
                    const blobby = new Blob([byteArray], { type: 'application/octet-stream' });
                    parsedDoc.docRef = URL.createObjectURL(blobby);
                    parsedDoc.docRef += "#toolbar=0&page=1";
                }
            }
            break;
    }
    docObjArr.push(parsedDoc);
    return parsedDoc;
}

document.getElementById('sponsor-info-overview-date-input').addEventListener('focus', function () {
    this.showPicker();
});

async function initialize() {
    try {
        await getSponsors();
        await getPatientInfo();
    } catch (error) {
        console.error('Error during initialization:', error);
    }
}

// Call the async function inside window.onload
window.onload = function () {
    initialize();

    updateDateTime();
    setInterval(updateDateTime, 1000);
};