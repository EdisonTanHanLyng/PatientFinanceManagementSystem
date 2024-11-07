
// -- Live clock on top right --
const timeContainer = document.getElementById('time');
const dateContainer = document.getElementById('date');

// Timer update
function liveTime() {
    const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    var date = new Date();
    var day = date.getDate();
    var month = months[date.getMonth()];
    var year = date.getFullYear();
    var min = date.getMinutes();
    var hour = date.getHours();

    if (min < 10) {
        min = "0" + min;
    }

    dateContainer.innerText = (day + " " + month + ", " + year);
    timeContainer.innerText = (hour + ":" + min);
}

// sleep function
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// -- Page Animation Functionality --
const histList = document.getElementsByClassName('patientI-Overview-EntryList')[0];
const docList = document.getElementsByClassName('patientI-Overview-DocList')[0];

//Summary Block Elements
const summBlock = document.getElementsByClassName('patientI-Summary')[0];
const tassleBtn = document.getElementsByClassName('patientI-tassleBtn')[0];
let summBlockState = true;

tassleBtn.onclick = function () {
    summBlockState = summBlockM(summBlockState);
};

//Fixed: by Edison's Aura
// Expand content functionality
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
const histTab = document.getElementsByClassName('tab1')[0];
const docTab = document.getElementsByClassName('tab2')[0];
const pageSelector = document.getElementsByClassName('patientI-Overview-Content')[0];
const page1 = document.getElementsByClassName('patientI-Overview-Page1')[0];
const page2 = document.getElementsByClassName('patientI-Overview-Page2')[0];
let currTab = 0;

histTab.onclick = function () {
    currTab = tabSelector(0);
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
//const httpsURL = "https://localhost:7073";
const loadingDocBar = document.getElementsByClassName('patientI-DocBar-loading')[0];
const patientID = document.getElementById('givenID').value;
var localPatientNRIC = "";
const getPatAPI = `/getPat/${patientID}`;
const getHDRecAPI = `/getPatHist/${patientID}`;
const getHDRecFiltAPI = `/getPatHistFilt/${patientID}`;
const getDocAPI = `GetDocs/${localPatientNRIC}`;
const postDocAPI = "PatientDocumentUpload";

// Get request header data
const additionalData = {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json'
    }
}

// Uses GET from controller to obtain HD Record JSON data from controller
function getHDRec() {
    loaderBar.classList.add('loaded');
    fetch(getHDRecAPI, additionalData)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed! : HD Record (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            //parse and format data
            loaderBar.classList.remove('loaded');
            obtainedData.HDRecords.forEach(record => {
                generateEntryListItem(parseHDRecData(record));
            });
        })
        .catch(error => {
            loaderBar.classList.remove('loaded');
            console.error(`Error Occurred: ${error}`);
        });
}

// Uses GET from controller to obtain a filtered and sorted HD Record JSON List from controller
async function getHDRecFilt(searchStr) {
    if (searchStr === "" || searchStr === " ") {
        searchStr = "q";
    }
    if (searchStr.includes("/")) {
        searchStr = searchStr.replaceAll("/","-");
    }
    loaderBar.classList.add('loaded');
    const query = `${getHDRecFiltAPI}/${searchStr}`; 
    await fetch(query, additionalData)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed! : HD Rec Filter (${response.status})`);
            }
            const data = response.json();
            return data;
        })
        .then(obtainedData => {
            //parse and format data
            console.log("HD Rec Filter");
            clearHDRecList();
            loaderBar.classList.remove('loaded');
            obtainedData.forEach(record => {
                generateEntryListItem(parseHDRecData2(record));
            });
        })
        .catch(error => {
            loaderBar.classList.remove('loaded');
            console.error(`Error Occurred: ${error}`);
        });
}

async function uploadFile(file, docObj) {
    var formData = new FormData();
    formData.append('file', file);
    formData.append('data', JSON.stringify(docObj));

    const mainData = {
        patientDoc: docObj,
        formFile: formData
    }
    const query = postDocAPI;
    const additional = {
        method: 'POST',
        body: formData
    }
    let returnVal = false;
    await fetch(query, additional)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Post Data Failed! : Document Upload: (${response.status})`);
            }
            else {
                returnVal = true;
            }
            return returnVal;
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });

    await getDocs(docObj.patId);
    return returnVal;
}

// Uses GET from controller to obtain JSON Document Data
async function getDocs(patID) {
    query = `getDocs/${patID}`;
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

// Uses GET from controller to obtain a filtered and sorted list of JSON document data
async function getDocFilt(patID, searchStr) {
    query = `getDocFilt/${patID}/${searchStr}`;
    loadingDocBar.classList.add('progress');
    if (searchStr == "" || searchStr == " ") {
        await getDocs(patID);
    }
    else {
        await fetch(query, additionalData)
            .then(response => {
                if (!response.ok) {
                    throw new Error(` Get Data Failed! : Get Search Documents (${response.status})`);
                }
                const data = response.json();
                return data;
            })
            .then(obtainedData => {
                // parse and format data
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

// Uses POST to update the given doc in the database
async function updateDoc(docObj) {
    const query = "updateDoc";
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
            }
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });

    await getDocs(docObj.patId);
    return returnVal;
}

// Uses PUT to delete the document ID in teh database
async function deleteDoc(docID, patID) {
    const query = `deleteDoc/${docID}`;
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
            }
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });

    await getDocs(patID);
    return returnVal;
}

// Uses GET from controller to obtain JSON patient profile data from controller
async function getPatient() {
    await fetch(getPatAPI, additionalData)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed! : Patient Profile (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            //parse and format data
            const data = parsePatientProfile(obtainedData);
            summaryBoxGen(data);
            getDocs(data.patientNRIC);
        })
        .catch(error => {
            console.error(`Error Occured: ${error}`);
        })
}

// Parses Patient Profile JSON data directly from the API Endpoint
function parsePatientProfile(jsonData) {
    const dataObj = sumInfoStruct;
    dataObj.patientName = jsonData["FullName"];
    dataObj.patientNRIC = jsonData["NRIC"];
    localPatientNRIC = dataObj.patientNRIC;
    dataObj.patientAge = jsonData["Age"];
    dataObj.patientGender = jsonData["Gender"];
    dataObj.patientRace = jsonData["Ethnic"];
    dataObj.patientOccStat = jsonData["CurrentOccupation"];
    dataObj.patientTel = jsonData["PhoneNumber"];
    dataObj.dialSchedule = jsonData["DialysisSchedule"];
    dataObj.location = jsonData["DialysisCenterBranchLocation"];
    dataObj.patientStatus = jsonData["ActivityStatus"];
    dataObj.patientSponsor = jsonData["SponsorList"];
    dataObj.patientABO = jsonData["ABOGrouping"];
    dataObj.patientRhesus = jsonData["RhesusFactor"];
    return dataObj;
}

// Parses HD Record JSON that is derived from the model class
function parseHDRecData2(jsonData) {
    const dataObj = hdRecStruct;
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

// Parses HD Record JSON that comes directly from the API Endpoint
function parseHDRecData(jsonData) {
    const dataObj = hdRecStruct;
    dataObj.date = jsonData["date"];
    dataObj.time = jsonData["time"];
    dataObj.tdMin = jsonData["td_min"];
    dataObj.weightPre = jsonData["weight"]["pre"];
    dataObj.weightPost = jsonData["weight"]["post"];
    dataObj.dw = jsonData["dw"];
    dataObj.idw = jsonData["idw"];
    dataObj.dwPercent = jsonData["%dw"];
    dataObj.ufGoal = jsonData["uf_goal"];
    dataObj.preBP_SBP = jsonData["pre_bp"]["sbp"];
    dataObj.preBP_DBP = jsonData["pre_bp"]["dbp"];
    dataObj.postBP_SBP = jsonData["post_bp"]["sbp"];
    dataObj.postBP_DBP = jsonData["post_bp"]["dbp"];
    dataObj.prePulse = jsonData["pulse"]["pre"];
    dataObj.postPulse = jsonData["pulse"]["post"];
    dataObj.bfr = jsonData["bpr"];
    dataObj.epoType = jsonData["epo"]["type"];
    dataObj.epoDosage = jsonData["epo"]["dosage"];
    dataObj.epoQty = jsonData["epo"]["quantity"];
    dataObj.ocm_KtV = jsonData["ocm_dialysate"]["kt_v"];
    dataObj.dial_Cal = jsonData["ocm_dialysate"]["calcium"];
    dataObj.dialyzer = jsonData["haemofilter_dialyzer_used"];
    return dataObj;
}

function parseDocumentJson(jsonData) {
    const data = {
        docID: 0,
        date: "",
        docName: "Document 1234",
        docRef: "",
        isPreviewable: 0,
        remarks: "",
        patId: "",
        base64String: ""
    };
    data.docID = jsonData["docID"];
    data.date = jsonData["date"];
    data.docName = jsonData["docName"];
    data.docRef = jsonData["docRef"];
    data.isPreviewable = jsonData["isPreviewable"];
    data.patId = jsonData["patId"];
    data.remarks = jsonData["remarks"];
    data.base64String = jsonData["base64String"];
    return data;
}

// -- Summary Block --

const sumBoxLeft = document.getElementsByClassName('patientI-left')[0];
const sumBoxRight = document.getElementsByClassName('patientI-right1')[0];
const sumBoxRight2 = document.getElementsByClassName('patientI-right2')[0];
const sumInfoStruct = {
    descBox: "Description/Information",
    patientName: "Name",
    patientNRIC: "PatientNRIC",
    patientAge: 0,
    patientGender: "Gender",
    patientRace: "Race",
    patientOccStat: "OccupationStatus",
    patientTel: "Tel",
    dialSchedule: "Dialysis Schedule",
    location: "Location",
    patientStatus: "Status",
    patientSponsor: "Sponsor",
    patientABO: "ABO",
    patientRhesus: "Rhesus"
};

// Generates the summary block presenting summary information on a specific patient
function summaryBoxGen(sumData) {
    sumBoxLeft.innerHTML = "";
    const desc = document.createElement('h2')
    desc.innerHTML = `${sumData.descBox}`;
    const n1 = document.createElement('p');
    n1.innerHTML = `<b>Full Name:</b> ${sumData.patientName} <b> NRIC:</b> ${dashSplitter(sumData.patientNRIC, "6-2-4")}`;
    const n2 = document.createElement('p');
    n2.innerHTML = `<b>Age:</b> ${sumData.patientAge} <b> Sex:</b> ${sumData.patientGender} <b> Ethnic:</b> ${sumData.patientRace} <b> Occupation:</b> ${sumData.patientOccStat} <b> Tel:</b> ${dashSplitter(sumData.patientTel, "3-8")}`;
    const n3 = document.createElement('p');
    n3.innerHTML = `<b>Dialysis Schedule:</b> ${sumData.dialSchedule}`;
    const n4 = document.createElement('p');
    n4.innerHTML = `<b>Location:</b> ${sumData.location}`;

    sumBoxLeft.appendChild(desc);
    sumBoxLeft.appendChild(n1);
    sumBoxLeft.appendChild(n2);
    sumBoxLeft.appendChild(n3);
    sumBoxLeft.appendChild(n4);

    const status = document.createElement('p');
    status.innerHTML = ` <b>Status:</b> ${sumData.patientStatus}`;
    const spons = document.createElement('p');
    spons.innerHTML = `<b>Sponsor:</b> ${sumData.patientSponsor}`;
    const abo = document.createElement('p');
    abo.innerHTML = ` <b>ABO Grouping:</b> ${sumData.patientABO}`;
    const rhe = document.createElement('p');
    rhe.innerHTML = ` <b>Rhesus Factor:</b> ${sumData.patientRhesus}`;

    sumBoxRight.innerHTML = "";
    sumBoxRight.appendChild(status);
    sumBoxRight.appendChild(spons);
    sumBoxRight.appendChild(abo);
    sumBoxRight.appendChild(rhe);
}

// -- Page 1 --
// HD Record Data Structure
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
    remarks: ""
};

const loaderBar = document.getElementsByClassName('patientI-HistBar-Loading')[0];
let loaderbarStatus = false;

// History Overview Search Bar 
let timer;
const waitTime = 1000; //1 second
const searchBar = document.getElementById('histSearch');
const searchDate = document.getElementById('dateHSearch');
const searchBarDoc = document.getElementById('docSearch');

//EventListener for delayed input from the user
searchBar.addEventListener('keyup', (keyEvent) => {
    if (keyEvent.key != "Enter") {
        clearTimeout(timer);

        timer = setTimeout(() => {
            getHDRecFilt(keyEvent.target.value);
        }, waitTime);
    }
    else if (searchDate.value) {
        searchDate.value = "";
    }
});

//EventListener when user presses 'Enter'
searchBar.addEventListener('keypress', (keyEvent) => {
    if (keyEvent.key == "Enter") {
        clearTimeout(timer); // stop the keyup event listener
        timer = null;

        getHDRecFilt(keyEvent.target.value);
    }
    else if(searchDate.value){
        searchDate.value = "";
    }
});

//EventListener for delayed input from the user
searchBarDoc.addEventListener('keyup', (keyEvent) => {
    if (keyEvent.key != "Enter") {
        clearTimeout(timer);

        timer = setTimeout(() => {
            getDocFilt(localPatientNRIC, keyEvent.target.value);
        }, waitTime);
    }
});

//EventListener when user presses 'Enter'
searchBarDoc.addEventListener('keypress', (keyEvent) => {
    if (keyEvent.key == "Enter") {
        clearTimeout(timer); // stop the keyup event listener
        timer = null;
        getDocFilt(localPatientNRIC, keyEvent.target.value);
    }
});

// Date Search Assistant Box
searchDate.addEventListener('input', function () {
    if (searchDate.value) {
        const date = new Date(searchDate.value);
        const dateStr = date.toLocaleDateString("en-GB", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit"
        });
        searchBar.value = dateStr;
    }
});

const entryList = document.getElementsByClassName('patientI-Overview-EntryList')[0];

//Add Entry functionality
const addEntryButton = document.getElementsByClassName('patientI-Overview-AddEntry')[0];

let entryArray = [];
let histBtnCounter = 0;

// Clear List
function clearHDRecList() {
    entryList.innerHTML = "";
}

// Dynamic approach to creating entry list items, enables button functionality | 7 7 6
function generateEntryListItem(data) {
    //console.log(data);
    const t1Arr = [
        ["Td (min)", `${data.tdMin}`],
        ["Weight (pre)", `${data.weightPre}`],
        ["Weight (post)", `${data.weightPost}`],
        ["DW", `${data.dw}`],
        ["IDW", `${data.idw}`],
        ["%DW", `${data.dwPercent}`],
        ["UF Goal", `${data.ufGoal}`]
    ];
    const t2Arr = [
        ["Pre-BP (SBP)", `${data.preBP_SBP}`],
        ["Pre-BP (DBP)", `${data.preBP_DBP}`],
        ["Post-BP (SBP)", `${data.postBP_SBP}`],
        ["Post-BP (DBP)", `${data.postBP_DBP}`],
        ["Pulse (pre)", `${data.prePulse}`],
        ["Pulse (post)", `${data.postPulse}`],
        ["BFR", `${data.bfr}`]
    ];
    const t3Arr = [
        ["EPO: Type", `${data.epoType}`],
        ["EPO: Dosage", `${data.epoDosage}`],
        ["EPO: Qty", `${data.epoQty}`],
        ["OCM Kt/V", `${data.ocm_KtV}`],
        ["Dialysate Calcium", `${data.dial_Cal}`],
        ["Haemofilter/Dialyzer Used", `${data.dialyzer}`]
    ];

    //Level 1
    const listItem = document.createElement('li');
    //Level 2
    const dateHeader = document.createElement('div');
    const content = document.createElement('div');
    //Level 3
    const table1 = document.createElement('table');
    const table2 = document.createElement('table');
    //Level 4 
    const table3 = document.createElement('table');

    // Date Header
    const timeHolder = document.createElement('p');
    timeHolder.innerHTML = ` <b>Date:</b> ${data.date} | <b>Time:</b> ${data.time} `;
    dateHeader.appendChild(timeHolder);

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

    // Table 2
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

    // Nested Container
    // Table 3
    for (let i = 0; i < t3Arr.length; i++) {
        const row = document.createElement('tr');
        const d1 = document.createElement('td');
        const d2 = document.createElement('td');
        d1.innerHTML = t3Arr[i][0];
        d2.innerHTML = t3Arr[i][1];
        row.appendChild(d1);
        row.appendChild(d2);
        table3.appendChild(row);
    }

    content.appendChild(table1);
    content.appendChild(table2);
    content.appendChild(table3);

    listItem.appendChild(dateHeader);
    listItem.appendChild(content);

    entryList.appendChild(listItem);
}

// ----- Page 2 : Document Uploads ------
const uploadDocBtn = document.getElementsByClassName('patientI-DocBar-uploadBtn')[0];
const filePrompt = document.getElementById('patientI-FileInput');

const docDataStruct = {
    date: "",
    docName: "Document 1234",
    docData: "",
    docRef: "",
    isPreviewable: 0,
    remarks: ""
};

function clearDocList() {
    docList.innerHTML = "";
}

function getCurrDate() {
    const date = new Date();
    const dateString = `${date.getDate}/${date.getMonth}/${date.getFullYear}`;
    let dateString2 = date.toLocaleDateString("zh-Hans-CN", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit"
    });
    dateString2 = dateString2.replaceAll("/","-");
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

// Arrays used for generateDocItem2
let docListItemArr = [];
let docContentDivArr = [];
let docControlBtnArr = [];
const docObjArr = []; // Array Containing the document objects
let docBtnCounter = 0;

// helper function used to toggle css classes of different styles
function toggleDocMini(docElemIndex) {
    docContentDivArr[docElemIndex].classList.toggle('minimise');
    docListItemArr[docElemIndex].classList.toggle('minimise');
    docControlBtnArr[docElemIndex].classList.toggle('minimise');
}

// Dynamic approach to creating docListItems enables buttons within each listitem to function
function generateDocItem(dData) {
    const listItem = document.createElement('li');

    const heading = document.createElement('p');
    heading.innerHTML = `Date: ${dData.date} | ${dData.docName}`;
    listItem.appendChild(heading);

    const contentDiv = document.createElement('div');
    const docDiv = document.createElement('div');

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
    downloadBtn.setAttribute("class", "patientI-Overview-DocList-Download");
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
        listItem.setAttribute("tabindex", '-1');
        listItem.focus();
        listItem.classList.toggle('editingMod');
        descDiv.classList.toggle('editingMod');
        docDiv.classList.toggle('editingMod');
        if (summBlockState) {
            summBlockState = summBlockM(summBlockState);
        }
        //editmode
        editModeDocListItem(docObjArr[this.value], descDiv, docDiv, listItem, this.value);
    }

    downloadBtn.onclick = function () {
        // Trigger Download
        const down = document.createElement('a');
        down.href = docObjArr[this.value].docRef;
        down.download = docObjArr[this.value].docName;
        down.click();
    }

    controlBtn.onclick = function () {
        toggleDocMini(this.value);
    };
    docList.appendChild(listItem);
    toggleDocMini(controlBtn.value); // set minimised initially
}

function editModeDocListItem(dData, descDiv, docDiv, listItem, value) {

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

        const deleteConf = $("#checkB").prop("checked");

        if (deleteConf) {
            console.log(deleteDoc(docObjArr[this.value].docID, docObjArr[this.value].patId));
        }
        else if (editRemarks.value != docObjArr[this.value].remarks || editName.value != docObjArr[this.value].docName || editDate.value != convertToCommonDate(docObjArr[this.value].date)) {
            const fileExt = docObjArr[this.value].docName.split('.').pop();
            if (editName.value != "" && editName.value != " ") {
                const fileExtIn = editName.value.split('.').pop();
                const fileNameIn = editName.value.split('.')[0];
                if (fileExtIn != fileNameIn) {
                    if (fileNameIn == "") {
                        // Dont Change
                    }
                    else if (editName.value.split('.').length > 2) {
                        // Dont Change
                    }
                    else if (fileExt != fileExtIn) {
                        docObjArr[this.value].docName = `${fileNameIn}.${fileExt}`;
                    }
                    else {
                        docObjArr[this.value].docName = editName.value;
                    }
                }
            }
            docObjArr[this.value].remarks = editRemarks.value;
            docObjArr[this.value].date = convertToFullDate(editDate.value);
            console.log(updateDoc(docObjArr[this.value]));
        }

        resetDocListItem(docObjArr[this.value], descDiv, docDiv, listItem, this.value);
    }

    cancelBtn.onclick = function () {
        listItem.classList.toggle('editingMod');
        descDiv.classList.toggle('editingMod');
        docDiv.classList.toggle('editingMod');
        resetDocListItem(docObjArr[this.value], descDiv, docDiv, listItem, this.value);
    }
}

function resetDocListItem(dData, descDiv, docDiv, listItem, value) {

    // Reset Doc Div
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
    downloadBtn.setAttribute("class", "patientI-Overview-DocList-Download");
    downloadBtn.setAttribute("value", value);
    downloadBtn.innerHTML = `Download`;

    buttonsDiv.appendChild(editBtn);
    buttonsDiv.appendChild(downloadBtn);

    descDiv.appendChild(buttonsDiv);

    editBtn.onclick = function () {
        listItem.setAttribute("tabindex", '-1');
        listItem.focus();
        listItem.classList.toggle('editingMod');
        descDiv.classList.toggle('editingMod');
        docDiv.classList.toggle('editingMod');
        if (summBlockState) {
            summBlockState = summBlockM(summBlockState);
        }
        //editmode
        editModeDocListItem(docObjArr[this.value], descDiv, docDiv, listItem, this.value);
    }
    downloadBtn.onclick = function () {
        // Trigger Download
        const down = document.createElement('a');
        down.href = docObjArr[this.value].docRef;
        down.download = docObjArr[this.value].docName;
        down.click();
    }
}

// This is used to generate the document preview on the front end
function generateDocPrev(docObj) {
    if (docObj.isPreviewable == 1) {
        const docPrev = document.createElement('iframe');
        docPrev.src = docObj.docRef;
        return docPrev;
    }
    else {
        const docPrev = document.createElement('span');
        docPrev.setAttribute("class", "material-symbols-outlined");
        docPrev.innerHTML = "draft";
        return docPrev;
    }
}

// This is used to generate a new document uploaded by user
function generateNewDocObj(file) {
    const data = {
        docID: 0,
        date: "",
        docName: "Document 1234",
        docRef: "",
        isPreviewable: 0,
        remarks: "",
        patId: "",
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
    data.patId = localPatientNRIC;
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
        console.log("Upload: Successfully Uploaded");
    }
    else {
        console.log("Upload: Failure");
    }
    return data;
}

// Processes the parsed document for displaying on the front end
function processParsedDoc(parsedDoc) {
    let fileExt = parsedDoc.docName.split('.').pop();
    fileExt = fileExt.toLowerCase();
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

// Takes in a string and masks the last n characters with '*'
function stringMasker(str, maskLen) {
    if (maskLen > 0 && maskLen < str.length) {
        const ind = str.length - maskLen;
        str = str.substring(0, ind) + '*'.repeat(maskLen);
    }
    return str;
}

// Given an input string (preferably a long hard-to-read string of numbers),
// will insert dashes depending on a format string such as: "3-3-3" to the 'format' parameter
// Example: input: "123456789", "3-3-3" => output: "123-456-789"
function dashSplitter(inputStr, format) {
    const numstr = format.split("-");
    let splitAt = 0;
    for (let i = 0; i < numstr.length - 1; i++) {
        try {
            splitAt += parseInt(numstr[i]);
            inputStr = inputStr.substring(0, splitAt) + "-" + inputStr.substring(splitAt);
            splitAt++;
        } catch (error) {
            console.error("Error:", error);
        }
    }
    return inputStr;
}

uploadDocBtn.onclick = function () {
    const data = docDataStruct;
    //fetch data
    filePrompt.click();
};

// Reference: Dick's document upload preview
filePrompt.addEventListener('change', function () {
    const file = this.files[0];
    if (file) {
        // Send to Backend
        // Generate the preview
        const docObj = generateNewDocObj(file)
        generateDocItem(docObj);
    }
    else {
        console.log(`Upload: No file selected...`);
    }

});

// -- Initialise Page functions --
window.onload = function () {
    liveTime();
    setInterval(liveTime, 30000); //every 30 seconds
    tabSelector(currTab);
    getHDRec();
    getPatient();
};
