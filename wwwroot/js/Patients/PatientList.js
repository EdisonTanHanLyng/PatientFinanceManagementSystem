
/* Supporting Javascript file for PatientList.cshtml*/

console.log("PatientList says hello");

//Live clock on top right
const timeContainer = document.getElementById('time');
const dateContainer = document.getElementById('date');

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

// ----- Expanded Searching Feature ----- //

const cursedTechnique = document.getElementsByClassName("patientL-expandedSearch")[0];
const searchBar = document.getElementById('search');
const dialSch = document.getElementById("dialSche");
const stat = document.getElementById("searchStatus");
const race = document.getElementById("searchRace");
const tel = document.getElementById("searchTel");
const pid = document.getElementById("searchPID"); // patient id
const spons = document.getElementById("searchSpons");

// Enabling Expanded Searching 
let isAdvSearch = false;
function domainExpansion() {
    cursedTechnique.classList.toggle('enabled');
    itemList.classList.toggle('enabled');
    isAdvSearch = !isAdvSearch;
    console.log(isAdvSearch);
}

let sL = []; // sponsor List
// Populate options for Dialysis Schedules
function nahIdWin() {
    const dS = [ // dialysis Search
        "Tuesday-Thursday-Saturday",
        "Monday-Wednesday-Friday",
        "Empty"
    ];
    const sS = [ // status Search
        "Active",
        "Inactive",
        "Empty"
    ];
    sL.push("Empty");

    dialSch.innerHTML = "";
    stat.innerHTML = "";
    for (let i = 0; i < dS.length; i++) {
        const option = document.createElement("option");
        option.setAttribute("value", dS[i]);
        option.innerHTML = dS[i];
        dialSch.appendChild(option);
    }
    sS.forEach(item => {
        const option = document.createElement("option");
        option.setAttribute("value", item);
        option.innerHTML = item;
        stat.appendChild(option);
    });
    if (sL.length > 0) {
        sL.forEach(item => {
            const option = document.createElement("option");
            option.setAttribute("value", item);
            option.innerHTML = item;
            spons.appendChild(option);
        });
    }
}

let advSearchEmpty = false;

// Generates the json for advanced searching
function convAdvSearchToJson() {
    let dialSchTemp = dialSch.value;
    let statTemp = stat.value;
    let sponsTemp = spons.value;
    if (dialSchTemp == "Empty") {
        dialSchTemp = "";
    }
    if (statTemp == "Empty") {
        statTemp = "";
    }
    if (spons.value == "Empty") {
        sponsTemp = "";
    }

    if (searchBar.value == "" && dialSch.value == "Empty" && stat.value == "Empty" && race.value == "" && tel.value == "" && pid.value == "" && spons.value == "Empty") {
        advSearchEmpty = true;
    }
    else {
        advSearchEmpty = false;
    }

    const data = {
        searchName: searchBar.value,
        dialScheduling: dialSchTemp,
        searchStatus: statTemp,
        searchRace: race.value,
        searchTel: tel.value,
        searchId: pid.value,
        searchSpons: sponsTemp
    };

    return JSON.stringify(data);
}

// ----- DATA FETCHING ----- //

// Get request header data
const additionalData = {
    method: 'GET',
    headers: {
        'Content-Type': 'application/json'
    }
}

let additionalData2 = {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: 0
}

let viewPage = 0;
let getPatList = `/getPatList/${viewPage}`;
const getPatListFilt = `/getPatListFilt`;

let patListJsonData = null; 
// If stored data is unavailable or expired
// Mainly used to initiate in obtaining patient list data
async function getPatientList() {
    if (patListJsonData == null) { //Fetch Data from controller if null
        await getPatientListBGT();
        if (patListJsonData == null) {
            throw new Error(" Data Failed to be obtained! ");
        }
        else {
            clearPList();
            patListJsonData.forEach(profile => {
                generatePatientListItem(parsePatientListItem(profile));
            });
        }
    }
    else {
        clearPList();
        patListJsonData.forEach(profile => {
            generatePatientListItem(parsePatientListItem(profile));
        });
    }
}

const loadingBar = document.getElementsByClassName('patientL-SearchBar-loading')[0];
// Uses GET from controller to obtain Patient List data from controller
async function getPatientListBGT() { // Britains Got Talent / BackGround Task
    getPatList = `/getPatList/${viewPage}`;
    loadingBar.classList.add('progress');
    await fetch(getPatList, additionalData)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed!: Patient List (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            if (patListJsonData == null) {
                patListJsonData = obtainedData;
            }
            else {
                obtainedData.forEach(item => {
                    patListJsonData.push(item);
                    generatePatientListItem(parsePatientListItem(item));
                });
            }
            loadingBar.classList.remove('progress');
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
            loadingBar.classList.remove('progress');
        });
}

// Send JSON data back to filter if stored data
// Uses GET from controller to obtain a filtered and sorted Patient JSON List from controller
async function getPatientFiltList(searchStr) {
    if (searchStr.includes("/")) {
        searchStr = searchStr.replaceAll("/", "");
    }
    let query = `${getPatListFilt}/${searchStr}`;
    let request = additionalData;
    advSearchEmpty = false;
    const queryData = convAdvSearchToJson();

    if (isAdvSearch) {
        query = '/getPatExpFilt';
        request = {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: queryData //Convert data from expanded search to JSON to send to controller
        };
    }

    if (advSearchEmpty) {
        getPatientList();
    }
    else {
        loadingBar.classList.add('progress');
        await fetch(query, request)
            .then(response => {
                if (!response.ok) {
                    throw new Error(` Get Data Failed!: Patient List Filter (${response.status})`);
                }
                return response.json();
            })
            .then(obtainedData => {
                clearPList();
                // Get Filtered Patient List
                if (obtainedData.length != 0) {
                    generatePatientList(obtainedData);
                    var child = patientList.children;
                    child[0].setAttribute("tabindex", '-1');
                    child[0].focus();
                }
                else {
                    noSearchResult();
                }
                loadingBar.classList.remove('progress');
            })
            .catch(error => {
                console.error(`Error Occurred: ${error}`);
                loadingBar.classList.remove('progress');
            });
    }
}

// Getting a full list of sponsors 
function getSponsorList() {
    const query = "/getSpon";
    fetch(query, additionalData)
        .then(response => {
            if (!response.ok) {
                throw new Error(` Get Data Failed!: Patient List Filter (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            sL = Object.keys(obtainedData);
            // But would you lose?
            nahIdWin();
            /*
            ..........................::...:......+.-........-==........:..::.....:...:-..................-.    
            ...                       :::..-.    .-...--=....:.--:... ..-=.::....:-..=:...               .-     
                        ..            .:...=-.   ..:...---..-..:::-=...:=.-.:....:=.=.......             .-.    
                        ...           .-.:..---....:::...--=+.....::..:=-.=.=...=::--:-:...:=..     ..:. .-     
                            ............--..::::..:=+=:::..:=:..:...--.-::-:-=.---:...      ..:=..  ...  .-     
                            ...:=-:::::..:+-::.:::..:.....:..: ....:.-=*-::=:=:.....          ..:-..     .-     
                        ........:............::.:..:-:..::...::.     ..:.-*=..-...               ...::..   .-     
                        .:.::-=-:::-==-:::.:...:::..:::...:.....     ......::.:.                    .:=..  .-.    
                    ..:..:........:=-:...-=-:.:-:....:.. .:.-.     .:.  .-..:.                     .-:.  .-.    
                    ..:.:::::...-:::.....:::::::...::. ..:......-..   .:.  .-..:..                    .::.  .-.    
                .... ..::..:--.:::::::..::..-:  ...   ..-........  .....-:.::        Nah I'd       .:..  .-     
                    .....::.:+=:......:.:....:-. ..-.   .-:...::-.  :-...:.:-:                      .:.   .-     
                    ..   ...:=-::--:. ..::....::...::.. ..:::.-::...-=..:-.-.:.         Win         .:.   .-     
                        ..:-:..::--:-:...::......:..:.:.   .-::-.....-:.:-:=:..                      .:.   .-.    
                    ..:-:::::-:....=-...:.=.:......::..  ..::-     ....+:-..:..                    .:..  .-.    
                .:-....:=...::::.::-.....-..:.:..:::...:::.. ..::.. ..:.-.:-:::=..                    .=..  .-.    
                .:-:..=+:.....--.......:::::-:..:..:-:...:..  ..:.. .:.... .::.+:.                   ..=.   .-.    
                ..---........::::........:-::-=:....:-:....   .......      .:.=...                 ..:..   .-.    
                ..-=::---::..::.:.......-......:-.........     .:-.:..    ..=..=:..             ...=.     .-.    
                    .:-:::::-::.....:::...    ....-:::-:.. ......--:..      ..-..--.=-..... .......=+....   .-.    
                    ..=:::.:...:.::..... ...:-::::...  ...:-:.-::..  ..:...::.::-....:-*-#++=:.=:-:.-==-. .-     
                    .:-.:.:..:::--::..-.  .:-::......    ..:...::.....:::.........:-.-..==-...-:.-.=-...... .-.    
                ..:....--:..:::....:..:.:.... .....:--......::......-:..:..... .:.:..=.--::.:-:.::=..     .-     
                ..:.-:==.::--.:---.:..-::::.:-:::.....-....--::.....:.:-....:...-.:...:.:--:-.:::.:-=.     .-.    
                .-:..:.-:.--::=::-:..--:..-.:.::....-:..:.::-..:..--:::.:-::.::.+... ..:.=.--:..--.:=-..   .-.    
                .:..:.=.:--:=:.:-.:.-::.:.:.:--..:.-..::-::..-:--::--..-::=:::-:-..-..:-:=.-:.=..-=:.=..   .-.    
                ...:-:=:.::.---::-=-:::::=::+:..::.::-:.::--:::::-:..-:::-::-:=::.:.:::.=.-..-=.::.:=:..  .-     
                    ..--.::--+:=:-.:-:-:---:::-.:-..:::.-==:::-..-::.::.-.:-=:-.=::.:..:.:..--:---::.....   .-.    
                ....---:.-.:-----=-:-::-----::-:..=:.::...-:.:::..:.-.:--.:.--.:.-.-.:.-*.-=:.=.:..      .-     
                    ..:-...:::---=--=-::--...-+---=-:...:..:-::..::...-..--:::.--.:.::..=-=.--.+.-:..       .-     
                    ..:-----:-.--:-+=::::::-=---=:........-::..::.:.:...-:-=..::::.::.:--.--:-:--=.        .-     
                    .:-:.-::::---:+--::=..=--:-=---:.....:-....::-:-...-+---:..:.:.:::-=.:.:-::-=..        .-     
                    ....-::--:--.::=--:::.:-:-.....::--:.::. .::.:-:-...::....:-:.:::=..--::+:--..        .-.    
                    ..--::..::---:::+::..:==+====---:....::   ...:.....:---=====-:.::--. :=:=-:-:..        .-.    
                    ....  .:=::-.=:-=..:..::..-.::.=*-...-:     .-.. .=*=-:-:=..-:---=-..:=:%:-.-=..       .-     
                        ..--..=:.:+.=......::..:-:...+...:.   ..:..  :+:..-:......-----.::.-.-::....       .-.    
                        .. .=:--=-:= ......:.....:+.-::..     ...-:::-:... ....:-::..#::*...:=.  ...     .-.    
                            ...-:.--*.=.           .....:...       ..::......    .-::...+.+=.  :-.  ...     .-     
                            .....:-:.*#-.              ..                       .-..:..-.+==.           ... .-     
                            .::.  ..+..                                           ..##---:.           .. .-     
                        ..         .*..                                           .*.-=.:-..             .-.    
                    .......        .:=.                                          .=-.=:...               .-     
                    .... ....       .=..                                        .:#.=-..                 .-.    
                            ...       .-:.               ...   ....              ..#:....             ..   .-.    
                                        .%-.              ..-...:...              .*+.               .....  .-     
                    ....               .=#=.              ......              ..:-#-.               ...    .-     
                                        :*-=..                         .... ...=.=#.          ..:...       .-.    
                                        :+-.--.  .-::::....:::......::::-....-:.-*=.        ......         .-.    
                                        .+::..=.. .....:+*+----==+=-....  .:-..--*:.        .....          .-.    
                        ...           .+.:...-:..    ..-=====-::...  ...=...:--+..                       .-     
                        ...           .+... ..==..       ....       ..+:.  .:-#=.                        .-     
                                        .:+.:.....=+:.                .==.   .::.*:                         .-.    
                                    ..=#@%.:::.. :-==...          ..-+-..   ...:*.                         .-     
                                ...:+@@@@@#...--....=--*-..........=*=-..      .=@=...                      .-.    
                            ..-#@@@@@@@@@:..---. .:=...:==+*****+---.......  .+@@@%=..                    .-.    
                        ...:+@@@@@@@@@@@@@@=..-::...:-......... ::::...-:... ..#@@@@@@%=...                .-     
                    ...-%@@@@@@@@@@@@@@@@@@@-:--:. ..-:--......--....=-:-.. .=@@@@@@@@@@%=:..             .-     
            .....:=#@@@@@@@@@@@@@@@@@@@@@@@@@=:-.....--:.:-..:=...-+-:::..:*@@@@@@@@@@@@@@@@*:...        .-     
            :*@@@@@@@@@@@@%%#%%@@@@@@@@@@@@@@@@%=.. .+-:.=...=...*==-:--#@@@@@@@@@@@@@@@@@@@@@@@=...     .-     
            @@@@@@@@@@@%#%%#@@@@@@@@@@@@@@@@@@@@@@@#-..::.......====%@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@#:.....-.    
            @@@@@@@@%%@@#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%#@@@@@@@@@@@@#=.    
            @@@@@@@@%@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@#@@@@@@@@@+.    
            @@@@@%@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@#%@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%@@@@@@@@@+     
            */
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });
}

// ----- JSON Parsing ----- //

// Parses patient list items from JSON
function parsePatientListItem(jsonData) {
    const pListI = pListItem;
    pListI.patientID = jsonData["PatientId"];
    pListI.patientName = jsonData["PatientName"];
    pListI.patientNRIC = jsonData["PatientNRIC"];
    pListI.patientPhone = jsonData["PatientPhone"];
    pListI.patientStatus = jsonData["Status"];
    pListI.sponsor = jsonData["Sponsor"];
    return pListI;
}

// ----- Data Constructs ----- //

// patient list item
const pListItem = {
    patientID: "0000000000000",
    patientNRIC: "000000000000",
    patientName: "No Name",
    patientPhone: "016-123456789",
    patientStatus: "Inactive",
    sponsor: "None"
};

// ----- Generation ----- //

const patientList = document.getElementsByClassName('patientL-list')[0];

// Clears the list
function clearPList() {
    patientList.innerHTML = "";
}

// Clears the list and inserts a "No Search Results"
function noSearchResult() {
    const listItem = document.createElement('li');
    const h3 = document.createElement('h3');
    h3.innerHTML = " No Search Results...";
    listItem.appendChild(h3);
    clearPList();
    patientList.appendChild(listItem);
}

// Async function to generate the patient list
async function generatePatientList(obtainedData) {
    obtainedData.forEach(profile => {
        generatePatientListItem(parsePatientListItem(profile));
    });
}

// generates and inserts the patient list item into the list
function generatePatientListItem(data) {

    //Level 0
    const listItem = document.createElement('li');

    //Level 1
    const pHolder = document.createElement('div');
    const pName = document.createElement('p');
    const pID = document.createElement('p');
    const tel = document.createElement('p');
    const stat = document.createElement('p');
    const spons = document.createElement('p');

    pHolder.innerHTML = "\n";
    pName.innerHTML = `<b>Patient Name:</b> ${data.patientName}`;
    pID.innerHTML = `<b>Patient NRIC:</b> ${dashSplitter(data.patientNRIC, "6-2-4")}`;
    tel.innerHTML = `<b>Tel:</b> ${data.patientPhone}`;
    stat.innerHTML = `<b>Status:</b> ${data.patientStatus}`;
    spons.innerHTML = `<b>Sponsor:</b> ${data.sponsor}`;

    //Level 2 - Form
    const fm = document.createElement('form');
    fm.setAttribute("action", "/patientquery");
    fm.setAttribute("method", "post");
    const input = document.createElement('input');
    input.setAttribute("type", "hidden");
    input.setAttribute("id", "patID");
    input.setAttribute("name", "patID");
    input.setAttribute("value", data.patientID);
    const btn = document.createElement('button');
    btn.setAttribute("type", "submit");

    btn.innerHTML = "Enter";
    fm.appendChild(input);
    fm.appendChild(btn);

    pHolder.appendChild(pName);
    pHolder.appendChild(pID);
    pHolder.appendChild(tel);
    pHolder.appendChild(stat);
    pHolder.appendChild(spons);
    listItem.appendChild(pHolder);
    listItem.appendChild(fm);

    patientList.appendChild(listItem);
}

// ----- Event Listeners ----- //
let timer;
const waitTime = 1000; //1 second
const advButton = document.getElementsByClassName("patientL-advSearchBtn")[0];
const searchBtn = document.getElementsByClassName('patientL-SearchBtn')[0];
const itemList = document.getElementsByClassName('patientL-listContainer')[0];

// When the advanced search button is clicked
advButton.onclick = function () {
    domainExpansion();
}

// When the search button is clicked
searchBtn.onclick = function () {
    searchByString(searchBar.value);
}

// When the "Enter" key is pressed
searchBar.addEventListener('keypress', (keyEvent) => {
    if (keyEvent.key == "Enter") {
        searchByString(keyEvent.target.value);
    }
});

//EventListener for delayed input from the user
searchBar.addEventListener('keyup', (keyEvent) => {
    if (keyEvent.key != "Enter") {
        clearTimeout(timer);

        timer = setTimeout(() => {
            searchByString(keyEvent.target.value);
        }, waitTime);
    }
});

race.addEventListener('keypress', (keyEvent) => {
    if (keyEvent.key == "Enter") {
        searchByString(keyEvent.target.value);
    }
});

tel.addEventListener('keypress', (keyEvent) => {
    if (keyEvent.key == "Enter") {
        searchByString(keyEvent.target.value);
    }
});

pid.addEventListener('keypress', (keyEvent) => {
    if (keyEvent.key == "Enter") {
        searchByString(keyEvent.target.value);
    }
});

// When the user scrolled to the bottom of the list
itemList.addEventListener('scroll', () => {
    if (itemList.scrollTop + itemList.clientHeight >= itemList.scrollHeight) {
        viewPage++;
        getPatientListBGT();
    }
});

// ----- Other Functions ----- //

// Initiates the searching process
const patientListQuery = document.querySelectorAll('.patientL-list li');
function searchByString(searchStr) {
    if (searchStr != "" || isAdvSearch) {
        getPatientFiltList(searchStr);
    }
    else {
        getPatientList();
    }
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

// Initialise the page
window.onload = function () {
    liveTime();
    setInterval(liveTime, 30000); //every 30 seconds
    getSponsorList();
    getPatientList();
}