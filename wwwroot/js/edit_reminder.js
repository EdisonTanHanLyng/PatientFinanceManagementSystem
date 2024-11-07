var inputs = {
    dateSelected: '',
    time: '',
    sameDate: ''
};


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

function automatedFillSavedData(data) {
    document.getElementById('add-reminder-content-header').innerHTML = '<strong>Recipient Name: </strong>' + data.userName + '&emsp;<strong>Email: </strong>' + data.email;

    let priority = '';
    if (data.priority === "Low") priority = 'low-priority';
    else if (data.priority === "Medium") priority = 'medium-priority';
    else if (data.priority === "High") priority = 'high-priority';

    document.getElementById('input-title').value = data.title;
    document.getElementById('input-date').value = data.dueDate;
    document.getElementById('input-time').value = data.dueTime;
    document.getElementById('input-priority').value = priority;
    document.getElementById('input-description').value = data.description;
    document.getElementById('input-email-content').value = data.emailContent;
}

async function saveAndUpload() {
    if (checkEmptyFields()) {
        const title = document.getElementById("input-title").value;
        const date = document.getElementById("input-date").value;
        const time = document.getElementById("input-time").value;
        const prio = document.getElementById("input-priority").value;
        const description = document.getElementById("input-description").value;
        const emailContent = document.getElementById("input-email-content").value;
        let formattedContent = emailContent.replace(/\n/g, '<br>');
        formattedContent = formattedContent
        

        let priority = '';
        if (prio === "high-priority") priority = "High";
        else if (prio === "medium-priority") priority = "Medium";
        else if (prio === "low-priority") priority = "Low";

        let data = {
            Id: currentId,
            Title: title,
            Priority: priority,
            DueDate: date,
            DueTime: time,
            Description: description,
            EmailContent: formattedContent
        };

        const requestOptions = {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        }

        const apiUrl = `/reminder/updateReminder`;

        if (checkTime(document.getElementById("input-time"))) {
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
                        reminderCreatedNotification(jsonObject.responseMessage);
                    }
                })
                .catch(error => {
                    // Handle any errors that occurred during the fetch
                    console.error('Fetch error:', error);
                });
        }

    }
}

async function getCurrentReminder() {
    const apiUrl = `/reminder/getReminderById?reminderId=` + currentId;
    const requestOptions = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    }
    
    await fetch(apiUrl, requestOptions)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            const jsonObject = data;
            updateInputs('dateSelected', jsonObject.dueDate);
            updateInputs('time', jsonObject.dueTime);

            const selectedDate = new Date(jsonObject.dueDate);
            const currentDate = new Date();

            if (selectedDate.getFullYear() === currentDate.getFullYear() && selectedDate.getMonth() === currentDate.getMonth() && selectedDate.getDate() === currentDate.getDate()) {
                updateInputs('sameDate', true);
                updateInputs('dateSelected', selectedDate);
            }
            

            automatedFillSavedData(jsonObject);
        })
        .catch(error => {
            // Handle any errors that occurred during the fetch
            console.error('Fetch error:', error);
        });
}


//Pop up notification bubble
function reminderCreatedNotification(message) {
    const notification = document.createElement('div');
    notification.className = 'notification-bubble';
    notification.innerText = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.classList.add('visible');
    }, 100);
    setTimeout(() => {
        notification.classList.remove('visible');
        setTimeout(() => {
            notification.remove();
            window.location.href = '/Reminder/Reminder';
        }, 500);
    }, 3000);
}


//Shows calendar drop-down when input field is clicked
document.getElementById('input-date').addEventListener('focus', function () {
    this.showPicker();
});


document.querySelector('#input-date').addEventListener('input', function (input) {
    if (input.target.value !== null)
        input.target.blur();

    if (input.target.value) {
        const selectedDate = new Date(input.target.value);
        const currentDate = new Date();

        updateInputs('sameDate', false);
        updateInputs('dateSelected', input.target.value);


        currentDate.setHours(0, 0, 0, 0);

        if (selectedDate < currentDate) {
            input.target.value = "";
            updateInputs('dateSelected', '');

            const notification = document.createElement('div');
            notification.className = 'notification-bubble';
            notification.innerText = 'Please select a date that is not in the past!';
            document.body.appendChild(notification);

            setTimeout(() => {
                notification.classList.add('visible');
            }, 100);
            setTimeout(() => {
                notification.classList.remove('visible');
                setTimeout(() => {
                    notification.remove();
                }, 500);
            }, 3000);
        } else if (selectedDate.getFullYear() === currentDate.getFullYear() && selectedDate.getMonth() === currentDate.getMonth() && selectedDate.getDate() === currentDate.getDate()) {
            updateInputs('sameDate', true);
            updateInputs('dateSelected', selectedDate);
        }
    }
});

document.querySelector('#input-time').addEventListener('input', function (input) {
    if (input.target.value) {
        input.target.blur();
    }
});


function checkEmptyFields() {
    const title = document.getElementById("input-title").value;
    const date = document.getElementById("input-date").value;
    const time = document.getElementById("input-time").value;
    const prio = document.getElementById("input-priority").value;
    const email = document.getElementById("input-email-content").value;

    if (title !== '' && date !== '' && time !== '' && prio !== '' && email !== '') {
        return true;
    }

    const notification = document.createElement('div');
    notification.className = 'notification-bubble';
    notification.innerText = 'Please Fill Out all The Blanks Fields!';
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.classList.add('visible');
    }, 100);
    setTimeout(() => {
        notification.classList.remove('visible');
        setTimeout(() => {
            notification.remove();
        }, 500);
    }, 3000);


    return false;
}


function checkTime(input) {
    if (inputs['dateSelected'] === '') {
        const notification = document.createElement('div');
        notification.className = 'notification-bubble';
        notification.innerText = 'Please select a Date first!';
        document.body.appendChild(notification);

        setTimeout(() => {
            notification.classList.add('visible');
        }, 100);
        setTimeout(() => {
            notification.classList.remove('visible');
            setTimeout(() => {
                notification.remove();
            }, 500);
        }, 3000);

        input.value = '';
        updateInputs('time', '');
        return false;

    } else if (input.value !== null) {
        timeSelected = true;
        updateInputs('time', input.value);

        if (inputs.sameDate) {
            const selectedTime = input.value;
            const currentTime = new Date();

            const [selectedHours, selectedMinutes] = selectedTime.split(':').map(Number);

            const selectedDateTime = new Date(currentTime.getFullYear(), currentTime.getMonth(), currentTime.getDate(), selectedHours, selectedMinutes);

            const oneHourLater = new Date(currentTime.getTime() + 60 * 60 * 1000);

            if (selectedDateTime < oneHourLater) {
                const notification = document.createElement('div');
                notification.className = 'notification-bubble';
                notification.innerText = 'Please select a time that is at least one hour from now!';
                document.body.appendChild(notification);

                setTimeout(() => {
                    notification.classList.add('visible');
                }, 100);
                setTimeout(() => {
                    notification.classList.remove('visible');
                    setTimeout(() => {
                        notification.remove();
                    }, 500);
                }, 3000);

                input.value = "";
                updateInputs('time', '');

                return false;
            }
        }

        return true;
    }
}

function updateInputs(key, value) {
    inputs[key] = value;
}


const currentId = sessionStorage.getItem("editReminderId");
updateDateTime();
setInterval(updateDateTime, 1000);
getCurrentReminder();