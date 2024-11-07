function updateFilterState(key, value) {
    filterState.filters[key] = value;
}

function filterEntries(entries, filters) {
    let filteredEntries = entries;

    if (filters.general) { //Filter by Title and Name
        filteredEntries = filter(filters.general, filteredEntries, "General");
    }
    if (filters.title) { //Filter by Title
        filteredEntries = filter(filters.title, filteredEntries, "Title");
    }
    if (filters.date) { //Filter by Date
        filteredEntries = filter(filters.date, filteredEntries, "Date");
    }
    if (filters.priority) { //Filter by Priority
        filteredEntries = filter(filters.priority, filteredEntries, "Priority");
    }
    if (filters.type) { //Filter by User Type
        filteredEntries = filter(filters.type, filteredEntries, "Type");
    }

    return filteredEntries;
}

function filter(search, reminders, category) {
    let filteredList = [];

    if (search.length == 0) return reminders; //If nothing to filter, return everything
    for (let i = 0; i < reminders.length; i++) {

        //Add the reminder to filteredList if search is a substring of Name or Title
        if (category === "General" && (compare(search.toLowerCase(), reminders[i].userName.toLowerCase()) || compare(search.toLowerCase(), reminders[i].title.toLowerCase()))) //General filter
            filteredList.push(reminders[i]);
            
        else if (category === "Title" && (compare(search.toLowerCase(), reminders[i].title.toLowerCase()))) //Title filter
            filteredList.push(reminders[i]);
        else if (category === "Date") { //Date filter
            let date = reminders[i].dueDate.split("-");
            let dateString = '';
            for (let i = 0; i < 3; i++) {
                if (date[i].length == 2 && date[i].charAt(0) === '0') date[i] = date[i].substring(1, 2);
                dateString = dateString + date[i];
            }
            if (compare(search, dateString)) filteredList.push(reminders[i]);
        }
        else if (category === "Priority" && (compare(search.toLowerCase(), reminders[i].priority.toLowerCase()))) //User Priority filter
            filteredList.push(reminders[i]);
        else if (category === "Type" && (compare(search.toLowerCase(), reminders[i].userType.toLowerCase()))) //User Type filter
            filteredList.push(reminders[i]);
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

var filterState = {
    reminderList: [],
    filters: {
        general: '',
        title: '',
        date: '',
        priority: '',
        type: ''
    }
};


var mockData = [
    {
        "title": "Alex Sponsor Payment Due Date",
        "userName": "Alex",
        "Email": "alex@email.com",
        "userType": "Sponsor",
        "priority": "High",
        "Description": "Test Description",
        "dueDate": "31-3-2024",
        "Time": "13:00"
    },
    {
        "title": "Alex Patient Payment Due Date",
        "userName": "Alex",
        "Email": "alex@email.com",
        "userType": "Patient",
        "priority": "High",
        "Description": "Test Description",
        "dueDate": "31-3-2024",
        "Time": "13:00"
    },
    {
        "title": "Title 3",
        "userName": "Aleph",
        "Email": "aleph@email.com",
        "userType": "Sponsor",
        "priority": "Medium",
        "Description": "Test Description",
        "dueDate": "31-3-2024",
        "Time": "15:00"
    },
    {
        "title": "Title@Ayin4",
        "userName": "Ayin",
        "Email": "ayin@email.com",
        "userType": "Patient",
        "priority": "Medium",
        "Description": "Test Description",
        "dueDate": "6-9-2024",
        "Time": "17:00"
    },
    {
        "title": "Title 5",
        "userName": "Alpha",
        "Email": "alpha@email.com",
        "userType": "Sponsor",
        "priority": "Low",
        "Description": "Test Description",
        "dueDate": "31-3-2024",
        "Time": "19:00"
    },
    {
        "title": "Title 6",
        "userName": "Al um i n i u m ",
        "Email": "aluminium@email.com",
        "userType": "Sponsor",
        "priority": "Low",
        "Description": "Test Description",
        "dueDate": "13-3-2024",
        "Time": "13:00"
    },
    {
        "title": "a!",
        "userName": "Test!Bug",
        "Email": "aluminium@email.com",
        "userType": "Patient",
        "priority": "Low",
        "Description": "Test Description",
        "dueDate": "13-3-2024",
        "Time": "13:00"
    },
    {
        "title": "!@$%^&",
        "userName": "&*()",
        "Email": "aluminium@email.com",
        "userType": "Patient",
        "priority": "Low",
        "Description": "Test Description",
        "dueDate": "13-3-2024",
        "Time": "13:00"
    },
    {
        "title": "😊😊😊",
        "userName": "✨✨ Sparkle",
        "Email": "aluminium@email.com",
        "userType": "Patient",
        "priority": "Low",
        "Description": "Test Description",
        "dueDate": "13-3-2024",
        "Time": "13:00"
    }
];


/* Unit Testing */
function unitTest() {
    // filterTest( General, Title , UserType, Priority, Date, ExpectedOutput);
    const testCases = [
        () => assert( filterTest('', '', '', '', '', mockData), "Test failed! Expected Output: " + mockData ), //(1) Empty input match
    
        //General Filter Tests
        () => assert( filterTest('Sponsor Payment', '', '', '', '', mockData.slice(0, 1)), "Test failed! Expected Output: " + mockData.slice(0, 1) ), //(2) Full title match
        () => assert( filterTest('Aleph', '', '', '', '', mockData.slice(2, 3)), "Test failed! Expected Output: " + mockData.slice(2, 3) ), //(3) Full name match
        () => assert( filterTest('Alex', '', '', '', '', mockData.slice(0, 2)), "Test failed! Expected Output: " + mockData.slice(0, 2) ), //(4) Title and name match
        () => assert( filterTest('yment', '', '', '', '', mockData.slice(0, 2)), "Test failed! Expected Output: " + mockData.slice(0, 2) ), //(5) Partial title match
        () => assert( filterTest('ex', '', '', '', '', mockData.slice(0, 2)), "Test failed! Expected Output: " + mockData.slice(0, 2) ), //(6) Partial title and name match
        () => assert( filterTest('Aluminium', '', '', '', '', mockData.slice(5, 6)), "Test failed! Expected Output: " + mockData.slice(5, 6) ), //(7) Full name match with space in between
        () => assert( filterTest('xyzabc', '', '', '', '', []), "Test failed! Expected output: []"), //(8) Invalid input match
        
        //Title Filter Tests
        () => assert(filterTest('', 'Alex', '', '', '', mockData.slice(0, 2)), "Test failed! Expected output: " + mockData.slice(0, 2)), //(9) Full title match
        () => assert(filterTest('', '@Ayin', '', '', '', mockData.slice(3, 4)), "Test failed! Expected output: " + mockData.slice(3, 4)), //(10) Full title match with symbols
        () => assert(filterTest('', 'tle', '', '', '', mockData.slice(2, 6)), "Test failed! Expected output: " + mockData.slice(2, 6)), //(11) Partial title match
        () => assert(filterTest('', 'e 6', '', '', '', mockData.slice(5, 6)), "Test failed! Expected output: " + mockData.slice(5, 6)), //(12) Partial title match with space in between
        
        //User Type Filter Tests
        () => assert(filterTest('', '', 'Sponsor', '', '', mockData.filter((element, index) => [0, 2, 4, 5].includes(index))), "Test failed! Expected output: " + mockData.filter((element, index) => [0, 2, 4, 5].includes(index))), //(13) Full type match - Sponsor
        () => assert(filterTest('', '', 'Patient', '', '', mockData.filter((element, index) => [1, 3, 6, 7, 8].includes(index))), "Test failed! Expected output: " + mockData.filter((element, index) => [0, 2, 4, 5].includes(index))), //(14) Full type match - Patient
        
        //User Priority Filter Tests
        () => assert(filterTest('', '', '', 'High', '', mockData.slice(0, 2)), "Test failed! Expected output: " + mockData.slice(0, 2)), //(15) Full priority match - High
        () => assert(filterTest('', '', '', 'Medium', '', mockData.slice(2, 4)), "Test failed! Expected output: " + mockData.slice(2, 4)), //(16) Full priority match - Medium
        () => assert(filterTest('', '', '', 'Low', '', mockData.slice(4, 9)), "Test failed! Expected output: " + mockData.slice(4, 9)), //(17) Full priority match - High
        
        //Date Filter Tests
        () => assert(filterTest('', '', '', '', '3132024', mockData.filter((element, index) => [0, 1, 2, 4].includes(index))), "Test failed! Expected output: " + mockData.filter((element, index) => [0, 1, 2, 4].includes(index))), //(18) Full date match
        () => assert(filterTest('', '', '', '', '112024', []), "Test failed! Expected output: []"), //(19) No date match
        
        //Pair Matching Filter Tests
        () => assert(filterTest('Sponsor', 'Alex', '', '', '', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(20) Full Match: Title + Name
        () => assert(filterTest('Alex', '', 'Patient', '', '', mockData.slice(1, 2)), "Test failed! Expected output: " + mockData.slice(1, 2)), //(21) Full Match: (Title + Name) + User Type
        () => assert(filterTest('Alex', '', '', 'High', '', mockData.slice(0, 2)), "Test failed! Expected output: " + mockData.slice(0, 2)), //(22) Full Match: (Title + Name) + User Priority
        () => assert(filterTest('Alex', '', '', '', '3132024', mockData.slice(0, 2)), "Test failed! Expected output: " + mockData.slice(0, 2)), //(23) Full Match: (Title + Name) + Date
        () => assert(filterTest('Sponsor', '', 'Patient', '', '', []), "Test failed! Expected output: " + []), //(24) Full Match: (Title + Name) + User Type; no match
        () => assert(filterTest('', 'Alex', 'Patient', '', '', mockData.slice(1, 2)), "Test failed! Expected output: " + mockData.slice(1, 2)), //(25) Full Match: Title + User Type
        () => assert(filterTest('', 'Title', '', 'Medium', '', mockData.slice(2, 4)), "Test failed! Expected output: " + mockData.slice(2, 4)), //(26) Full Match: Title + User Priority
        () => assert(filterTest('', 'Title', '', '', '1332024', mockData.slice(5, 6)), "Test failed! Expected output: " + mockData.slice(5, 6)), //(27) Full Match: Title + User Date
        () => assert(filterTest('', '', 'Sponsor', 'High', '', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(28) Full Match: User Type + User Priority
        () => assert(filterTest('', '', 'Patient', '', '692024', mockData.slice(3, 4)), "Test failed! Expected output: " + mockData.slice(3, 4)), //(29) Full Match: User Type + Date
        () => assert(filterTest('', '', '', 'High', '3132024', mockData.slice(0, 2)), "Test failed! Expected output: " + mockData.slice(0, 2)), //(30) Full Match: User Priority + Date
        
        //Triplets Matching Filter Tests
        () => assert(filterTest('Alex', 'Sponsor', 'Sponsor', '', '', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(31) Full Match: Name + Title + User Type
        () => assert(filterTest('Alex', 'Sponsor', '', 'High', '', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(31) Full Match: Name + Title + User Priority
        () => assert(filterTest('Alex', 'Sponsor', '', '', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(31) Full Match: Name + Title + Date
        () => assert(filterTest('Alex', '', 'Sponsor', 'High', '', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), // (31) Full Match: (Name + Title) + User Type + User Priority
        () => assert(filterTest('Alex', '', 'Sponsor', '', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(31) Full Match: (Name + Title) + User Type + Date
        () => assert(filterTest('Alex', '', '', 'High', '3132024', mockData.slice(0, 2)), "Test failed! Expected output: " + mockData.slice(0, 2)), //(36) Full Match: (Name + Title) + User Priority + Date
        () => assert(filterTest('', 'Sponsor', 'Sponsor', '', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(37) Full Match: Title + User Type + Date
        () => assert(filterTest('', 'Sponsor', '', 'High', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(38) Full Match: Title + User Priority + Date
        () => assert(filterTest('', '', 'Sponsor', 'High', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(39) Full Match: User Type + User Priority + Date
        
        //Quadruplets Matching Filter Tests
        () => assert(filterTest('Alex', 'Payment', 'Sponsor', 'High', '', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(39) Full Match: Title + Name + User Type + User Priority
        () => assert(filterTest('Alex', 'Payment', 'Sponsor', '', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(39) Full Match: Title + Name + User Type + Date
        () => assert(filterTest('', 'Payment', 'Sponsor', 'High', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(39) Full Match: Title + User Type + User Priority + Date
        () => assert(filterTest('Alex', '', 'Sponsor', 'High', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(39) Full Match: Full Match: (Title + Name) + User Type + User Priority + Date
        () => assert(filterTest('Alex', 'Payment', '', 'High', '3132024', mockData.slice(0, 2)), "Test failed! Expected output: " + mockData.slice(0, 2)), //(39) Full Match: Full Match: Title + Name + User Priority + Date
        
        //Full Filter Tests
        () => assert(filterTest('Alex', 'Payment', 'Sponsor', 'High', '3132024', mockData.slice(0, 1)), "Test failed! Expected output: " + mockData.slice(0, 1)), //(39) Full Match with result
        () => assert(filterTest('Alex', 'Sponsor', 'Sponsor', 'Low', '692024', []), "Test failed! Expected output: []"), //(39) Full Match without result
        
    ];

    let testsPassed = 0;
    let index = 0;
    let totalTests = testCases.length;


    testCases.forEach((testCase) => {
        filterState = {
            reminderList: [],
            filters: {
                general: '',
                title: '',
                date: '',
                priority: '',
                type: ''
            }
        };
        filterState.reminderList = [... mockData];

        const result = testCase();
        if (result === '1') {
            console.log("Test " + (index + 1) + " Passed!");
            testsPassed++;
        } else {
            console.log("Test " + (index + 1) + " failed: " + result);
        }

        index++;
    });

    console.log(testsPassed + "/" + totalTests + " test cases passed.");
}

function assert(result, errorMessage) {
    if (result) return '1';
    return errorMessage;
}

function filterTest(general, title, type, priority, date, expectedOutput) {
    if(general !== '') updateFilterState('general', general);
    if(title !== '') updateFilterState('title', title);
    if(type !== '') updateFilterState('type', type);
    if(priority !== '') updateFilterState('priority', priority);
    if(date !== '') updateFilterState('date', date);


    return equalListsCheck(filterEntries(filterState.reminderList, filterState.filters), expectedOutput);
}

function equalListsCheck(listOne, listTwo) {
    if (listOne.length === 0 && listTwo.length === 0) return true;

    let setA = new Set(listOne);
    let setB = new Set(listTwo);

    if (setA.size !== setB.size) return false;
    for (let item of setA)
        if (!setB.has(item)) return false;

    return true;
}



unitTest();