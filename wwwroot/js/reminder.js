const connection = new signalR.HubConnectionBuilder()
    .withUrl("/reminderHub")
    .build();

connection.on("ReceiveReminderUpdate", function (message) {
    console.log(message); // Handle the message received
    loadAllReminders();
});

connection.start().catch(function (err) {
    console.error(err.toString());
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


/* ------------------------------------------------------------------------------------------------------- */
//Reminder Page
//Hide search filter by default
function toggleVisibility() {
    var filterDiv = document.getElementById('reminder-filter');
    if (filterDiv.style.opacity === '0' || filterDiv.style.opacity === '') {
        filterDiv.style.display = 'flex';

        setTimeout(function () {
            filterDiv.classList.add('visible');
        }, 10);
    }
}




/* Search Filter Area */
/* ------------------------------------------------------------------------------------------------------- */
// Store list of all reminders and filters in an object
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

// Get filter input from search bar
document.querySelector('#reminder-search-input').addEventListener('input', function (input) {
    updateFilterState('general', input.target.value);
    displayReminders(filterEntries(filterState.reminderList, filterState.filters));
});

// Get filter title input from search bar
document.querySelector('#reminder-search-title').addEventListener('input', function (input) {
    updateFilterState('title', input.target.value);
    displayReminders(filterEntries(filterState.reminderList, filterState.filters));
});

// Get filter date input from calendar selection
document.querySelector('#reminder-search-date').addEventListener('input', function (input) {
    let search = input.target.value;
    if (search) {
        let date = search.split("-");
        search = '';
        for (let i = 0; i < 3; i++) {
            if (date[i].length == 2 && date[i].charAt(0) === '0') date[i] = date[i].substring(1, 2);
            search = search + date[i];
        }
    }
    updateFilterState('date', search);
    displayReminders(filterEntries(filterState.reminderList, filterState.filters));
    input.target.blur();
});

// Get priority input from drop-down menu
document.getElementById("reminder-search-priority").addEventListener('change', function () {
    let value = this.value;
    let priorityLevel = "";

    if (value === "priority-high") priorityLevel = "High";
    else if (value === "priority-medium") priorityLevel = "Medium";
    else if (value === "priority-low") priorityLevel = "Low";

    updateFilterState('priority', priorityLevel);
    displayReminders(filterEntries(filterState.reminderList, filterState.filters));
});

// Get user type input from drop-down menu
document.getElementById("reminder-search-type").addEventListener('change', function () {
    let value = this.value;
    let userType = "";

    if (value === "type-sponsor") userType = "Sponsor";
    else if (value === "type-patient") userType = "Patient";

    updateFilterState('type', userType);
    displayReminders(filterEntries(filterState.reminderList, filterState.filters));
});


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

function displayReminders(reminders) {
    const container = document.querySelector('.reminder-boxes');
    container.innerHTML = ''; //Clear display list
    reminders.forEach(item => {
        const itemBox = document.createElement('div');
        itemBox.classList.add('reminder-box');

        const maxDescriptionLength = 100; //Maximum length of Description before truncation
        let description = item.description;
        if (description.length > maxDescriptionLength)
            description = description.substring(0, maxDescriptionLength) + ' ...';

        function escapeHtml(text) {
            var escapedText = text;

            // Replace special characters with HTML entities
            escapedText = escapedText.replace(/&/g, '&amp;');
            escapedText = escapedText.replace(/</g, '&lt;');
            escapedText = escapedText.replace(/>/g, '&gt;');
            escapedText = escapedText.replace(/"/g, '&quot;');
            escapedText = escapedText.replace(/'/g, '&#039;');

            return escapedText;
        }



        itemBox.innerHTML = `
            <div class="reminder-header">
                <div class="reminder-title">${escapeHtml(item.title)}</div>
                <div class="reminder-type-priority">
                    <span class="reminder-user-type user-type-${escapeHtml(item.userType)}">${escapeHtml(item.userType)}</span>
                    <span class="reminder-user-priority priority-${escapeHtml(item.priority)}">${escapeHtml(item.priority)}</span>
                </div>
                <div class="reminder-due-date"><strong>Due Date:</strong> ${escapeHtml(item.dueDate)}</div>
            </div>
            <div class="reminder-details">
                <p><strong class="reminder-name">Name:</strong> ${escapeHtml(item.userName)}</p>
                <p><strong class="reminder-description">Description:</strong> ${escapeHtml(description)}</p>
                <p><strong class="reminder-time">Time:</strong> ${escapeHtml(item.dueTime)}</p>
            </div>
            <div class="reminder-footer">
                <div class="reminder-email">
                    <p>
                        <strong>Email: </strong>
                        <span class="reminder-email-content">${escapeHtml(item.email)}</span>
                    </p>
                </div>

                <div class="reminder-icons">
                    <button class="reminder-edit-button">
                        <img src="/Image/reminder/edit-icon.png" alt="Send" class="reminder-edit-icon">
                    </button>
                    <button class="reminder-send-button">
                        <img src="/Image/reminder/send-email-icon.png" alt="Send" class="send-email-icon">
                    </button>
                    <button class="reminder-delete-button">
                        <img src="/Image/reminder/trash-bin.png" alt="Delete" class="trash-icon">
                    </button>
                </div>
                <div class="reminder-identification">${(item.id)}</div>
            </div>
        `;


        container.appendChild(itemBox);
    });
}




//Shows calendar drop-down when input field is clicked
document.getElementById('reminder-search-date').addEventListener('focus', function () {
    this.showPicker();
});


//When trash icon of Reminder Page is clicked
document.addEventListener('click', function (event) {
    let confirm = false;
    if (event.target && event.target.classList.contains('trash-icon')) {
        const itemBox = event.target.closest('.reminder-box');

        if (itemBox) {
            //Confirm deletion before actually deleting
            if (window.confirm("Are you sure you want to delete this reminder?")) {
                // Multiple identifications to ensure the right item is deleted
                const id = itemBox.querySelector('.reminder-identification').textContent;

                const itemIndex = filterState.reminderList.findIndex(item => (item.id === id));

                // Remove the item from reminderList if it exists
                if (itemIndex !== -1) {
                    deleteReminder(filterState.reminderList[itemIndex], itemBox);
                    filterState.reminderList.splice(itemIndex, 1);
                }
            }

        }
    }
});


//When send email icon of Reminder Page is clicked
document.addEventListener('click', async function (event) {
    if (event.target && event.target.classList.contains('send-email-icon')) {
        const itemBox = event.target.closest('.reminder-box');

        if (itemBox) {
            // Confirm before actually sending the reminder
            if (window.confirm("Are you sure you want to send this reminder?")) {
                // Multiple identifications to ensure the right item is deleted
                const id = itemBox.querySelector('.reminder-identification').textContent;

                const itemIndex = filterState.reminderList.findIndex(item => (item.id === id));

                // Remove the item from reminderList if it exists
                if (itemIndex !== -1) {
                    await sendReminder(filterState.reminderList[itemIndex], itemBox);
                    filterState.reminderList.splice(itemIndex, 1);
                }
            }
        }
    }
});


//When edit icon of Reminder Page is clicked
document.addEventListener('click', async function (event) {
    if (event.target && event.target.classList.contains('reminder-edit-icon')) {
        const itemBox = event.target.closest('.reminder-box');

        if (itemBox) {
            // Confirm before actually sending the reminder
            if (window.confirm("Are you sure you want to edit this reminder?")) {
                // Multiple identifications to ensure the right item is edited
                const id = itemBox.querySelector('.reminder-identification').textContent;

                const itemIndex = filterState.reminderList.findIndex(item => (item.id === id));
                
                // Remove the item from reminderList if it exists
                if (itemIndex !== -1) {
                    sessionStorage.setItem("editReminderId", id);
                    window.location.href = '/Reminder/EditReminder/';
                }
                
            }
        }
    }
});



// Navigate to Create new Reminder page when button is clicked
document.addEventListener('click', function (event) {
    if (event.target && event.target.classList.contains('create-reminder-button')) {
        window.location.href = '/Reminder/AddReminder';
    }
});



// Get list of all reminders from Server
function loadAllReminders() {
    const loadingContainer = document.querySelector('.loading-container');
    const container = document.querySelector('.reminder-boxes');

    // Show loading spinner
    loadingContainer.style.display = 'block';
    container.style.display = 'none';

    fetch('/Reminder/GetAllReminders')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(reminders => {
            filterState.reminderList = reminders;
            reminders.sort((a, b) => {
                let dateA = new Date(a.dueDate);
                let dateB = new Date(b.dueDate);

                if (dateA - dateB !== 0) {
                    return dateA - dateB;
                }

                let timeA = a.dueTime.split(':').map(Number);
                let timeB = b.dueTime.split(':').map(Number);

                let secondsA = timeA[0] * 3600 + timeA[1] * 60 + timeA[2];
                let secondsB = timeB[0] * 3600 + timeB[1] * 60 + timeB[2];

                return secondsA - secondsB;
            });

            //reminders.forEach((item) => { console.log(item.emailContent) });

            displayReminders(filterEntries(filterState.reminderList, filterState.filters));
        })
        .catch(error => {
            console.error('Error:', error);
        })
        .finally(() => {
            // Hides spinner and shows reminder list
            loadingContainer.style.display = 'none';
            container.style.display = 'block';
        });
}

// Request to delete reminder from Database
function deleteReminder(reminder, itemBox) {
    let apiUrl = '/Reminder/deleteReminder';

    let data = {
        Title: reminder.title,
        UserName: reminder.userName,
        Email: reminder.email,
        UserType: reminder.userType,
        Priority: reminder.priority,
        DueDate: reminder.dueDate,
        DueTime: reminder.dueTime,
        Description: reminder.description,
        EmailContent: reminder.emailContent
    };

    let requestOptions = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    };

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
                reminderDeletedNotification(jsonObject.responseMessage);
                itemBox.remove();
            }
        })
        .catch(error => {
            console.error('Fetch error:', error);
        });
}


// Request to send reminder to recipient
async function sendReminder(reminder, itemBox) {
    let apiUrl = '/Reminder/sendReminder';

    let data = {
        Title: reminder.title,
        UserName: reminder.userName,
        Email: reminder.email,
        UserType: reminder.userType,
        Priority: reminder.priority,
        DueDate: reminder.dueDate,
        DueTime: reminder.dueTime,
        Description: reminder.description,
        EmailContent: reminder.emailContent
    };

    let requestOptions = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    };

    try {
        const response = await fetch(apiUrl, requestOptions);

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const jsonObject = await response.json();

        if (jsonObject.success) {
            reminderDeletedNotification(jsonObject.responseMessage);
            itemBox.remove();
        }
    } catch (error) {
        console.error('Fetch error:', error);
    }
}


//Pop up notification bubble
function reminderDeletedNotification(message) {
    const notification = document.createElement('div');
    notification.className = 'notification-bubble';
    notification.innerText = message;

    document.body.appendChild(notification);

    // Show notification and hide after 3 seconds
    setTimeout(() => {
        notification.classList.add('visible');
    }, 100); // Delay to ensure visibility transition

    setTimeout(() => {
        notification.classList.remove('visible');
        setTimeout(() => {
            notification.remove(); // Remove after fade out
        }, 500); // Time matches the fade-out duration
    }, 3000);
}






loadAllReminders();
updateDateTime();
setInterval(updateDateTime, 1000);