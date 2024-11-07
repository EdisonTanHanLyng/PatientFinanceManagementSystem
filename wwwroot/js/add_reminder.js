var inputs = {
    dateSelected : '',
    time : '',
    sameDate : ''
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

function displaySelected(selected) {
    const container = document.querySelector('.reminder-boxes');
    const itemBox = document.createElement('div');
    itemBox.classList.add('reminder-box');

    if (selected.userType === 'Patient') {
        itemBox.innerHTML = `
            <div class="reminder-content-grid">
                <div><strong>User Type: </strong>${selected.userType}</div>
                <div><strong>Name: </strong>${selected.name}</div>
                <div><strong>Patient IC: </strong>${selected.identification}</div>
                <div><strong>Tel: </strong>${selected.phone}</div>
                <div><strong>Sponsor: </strong>${selected.dependency}</div>
                <div><strong>Email: </strong>${selected.email}</div>
            </div>
        `;
    } else if (selected.userType === 'Sponsor') {
        itemBox.innerHTML = `
            <div class="reminder-content-grid">
             <div><strong>User Type: </strong>${selected.userType}</div>
                <div><strong>Sponsor Name: </strong>${selected.name}</div>
                <div><strong>Sponsor ID: </strong>${selected.identification}</div>
                <div><strong>Person in Charge: </strong>${selected.dependency}</div>
                <div><strong>Tel: </strong>${selected.phone}</div>
                <div><strong>Email: </strong>${selected.email}</div>
            </div>
        `;
    }

    container.appendChild(itemBox);
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

        const activeButton = document.querySelector('.add-reminder-sponsor-button.active, .add-reminder-patient-button.active');
        let type = '';
        if (activeButton) type = activeButton.innerText;

        let priority = '';
        if (prio === "high-priority") priority = "High";
        else if (prio === "medium-priority") priority = "Medium";
        else if (prio === "low-priority") priority = "Low";


        data = getAllReminderData(title, date, time, description, priority, formattedContent);

        const requestOptions = {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        }

        const apiUrl = `/reminder/createNewReminder`;

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

function getAllReminderData(title, dueDate, dueTime, description, priority, emailContent) {
    const reminderBoxes = document.querySelectorAll('.reminder-box');
    const data = [];

    reminderBoxes.forEach(box => {
        const userType = box.querySelector('div:nth-child(1)').textContent.split(': ')[1].substr(0, 7)
        const name = box.querySelector('div:nth-child(2)').textContent.split(': ')[1];
        const email = box.querySelector('div:nth-child(6)').textContent.split(': ')[1];

        let reminderData = {
            Title: title,
            UserType: userType,
            UserName: name,
            Email: email,
            Priority: priority,
            DueDate: dueDate,
            DueTime: dueTime,
            Description: description,
            EmailContent: emailContent
        };

        data.push(reminderData);
    });

    return data;
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

async function getAllSelectedReminders() {
    let getSponsors = `/Reminder/getAllSelectedSponsors`;
    let getPatients = `/Reminder/getAllSelectedPatients`;

    await fetch(getSponsors)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Get Data Failed!: Selected User List (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            obtainedData.forEach(data => {
                displaySelected(data);
            });
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });

    await fetch(getPatients)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Get Data Failed!: Selected Patient List (${response.status})`);
            }
            return response.json();
        })
        .then(obtainedData => {
            obtainedData.forEach(data => {
                displaySelected(data);
            });
        })
        .catch(error => {
            console.error(`Error Occurred: ${error}`);
        });
}


function checkEmptyFields() {
    const title = document.getElementById("input-title").value;
    const date = document.getElementById("input-date").value;
    const time = document.getElementById("input-time").value;
    const prio = document.getElementById("input-priority").value;
    const description = document.getElementById("input-description").value;
    const email = document.getElementById("input-email-content").value;

    if (title !== '' && date !== '' && time !== '' && prio !== '' && description !== '' && email !== '') {
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


let selectedOption = '';
function openPopup() {
    document.getElementById("select-patient-sponsor-popup").style.display = "block";
}

function closePopup() {
    document.getElementById("select-patient-sponsor-popup").style.display = "none";
}

function selectOption(option, button) {
    document.getElementById('patient-select').classList.remove('active');
    document.getElementById('sponsor-select').classList.remove('active');

    selectedOption = option;
    button.classList.add('active');
}

function confirmSelection() {
    if (selectedOption === 'patient') {
        window.location.href = '/Reminder/AddReminderPatient';
        
    } else if (selectedOption === 'sponsor') {
        window.location.href = '/Reminder/AddReminderSponsor';
    }
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



updateDateTime();
setInterval(updateDateTime, 1000);
getAllSelectedReminders();