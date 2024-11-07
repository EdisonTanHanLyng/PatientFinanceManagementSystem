var inc = 1000;

function clock() {
    const date = new Date();

    const hours = ((date.getHours() + 11) % 12 + 1);
    const minutes = date.getMinutes();
    const seconds = date.getSeconds();

    const hour = hours * 30;
    const minute = minutes * 6;
    const second = seconds * 6;

    document.querySelector('.clock_hour').style.transform = `rotate(${hour}deg)`
    document.querySelector('.clock_minute').style.transform = `rotate(${minute}deg)`
    document.querySelector('.clock_second').style.transform = `rotate(${second}deg)`

}
clock();
setInterval(clock, inc);

window.onload = function () {

    const date_week = document.querySelector('.date_week');
    const date_day = document.querySelector('.date_day');
    const date_month = document.querySelector('.date_month');
    const date_year = document.querySelector('.date_year');

    const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    const month = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    const currentDate = new Date();
    date_week.textContent = days[(currentDate.getDay() + 6) % 7];
    date_day.textContent = currentDate.getDate();
    date_month.textContent = month[currentDate.getMonth()];
    date_year.textContent = currentDate.getFullYear();

    recent_patient_textDisplay(month[currentDate.getMonth()], currentDate.getFullYear());
    fetchMock_Data();
    fetchMock_Data_Reminder(month[currentDate.getMonth()], currentDate.getFullYear());
};

function recent_patient_textDisplay(month, year) {
    document.querySelector('.recent_patients_attempts_text').textContent = "Reminder Log - " + month + " " + year;
}

async function fetchMock_Data() {
    try {
        const response = await fetch('/api/MainMenuStaff/MockGetPatientsLogin'); // Update the URL
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.text(); // Use text() since the API returns a string

        const records = data.split('\n'); // Split the data into lines
        const container = document.querySelector('.recent_patients_attemps_zone_section'); // Ensure this container exists
        const containerElement = document.querySelector('.quickAccess_zone_inner_section');
        if (containerElement && containerElement.parentNode) {
            containerElement.parentNode.removeChild(containerElement);
        }

        records.forEach(record => {
            const [id, name, status, date] = record.split(',').map(item => item.trim());
            if (id && name && status && date) {
                // Create a new wrapper for each record
                const recordWrapper = document.createElement('div');
                recordWrapper.setAttribute('b-o24x7k4ubc', '');
                recordWrapper.className = 'quickAccess_zone_inner_section'; // Correct class name

                // Create and append ID element
                const idElement = document.createElement('div');
                idElement.setAttribute('b-o24x7k4ubc', '');
                idElement.className = 'quickAccess_zone_inner_section_Id'; // Correct class name

                const idElementText = document.createElement('span');
                idElementText.setAttribute('b-o24x7k4ubc', '');
                idElementText.className = 'quickAccess_zone_inner_section_Id_span'; // Correct class name
                idElementText.textContent = id; // Use textContent for plain text

                idElement.appendChild(idElementText);
                recordWrapper.appendChild(idElement);

                // Create and append Name element
                const nameElement = document.createElement('div');
                nameElement.className = 'quickAccess_zone_inner_section_name'; // Correct class name
                nameElement.setAttribute('b-o24x7k4ubc', '');

                const nameElementText = document.createElement('span');
                nameElementText.setAttribute('b-o24x7k4ubc', '');
                nameElementText.className = 'quickAccess_zone_inner_section_name_span'; // Correct class name
                nameElementText.textContent = name; // Use textContent for plain text

                nameElement.appendChild(nameElementText);
                recordWrapper.appendChild(nameElement);

                // Create and append Status element
                const statusElement = document.createElement('div');
                statusElement.setAttribute('b-o24x7k4ubc', '');
                statusElement.className = 'quickAccess_zone_inner_section_status'; // Correct class name

                const statusElementText = document.createElement('span');
                statusElementText.setAttribute('b-o24x7k4ubc', '');
                statusElementText.className = 'quickAccess_zone_inner_section_status_span'; // Correct class name
                statusElementText.textContent = date; // Use textContent for plain text
                statusElement.appendChild(statusElementText);
                recordWrapper.appendChild(statusElement);

                // Append the new record wrapper to the container
                container.appendChild(recordWrapper);
            }
        });

    } catch (error) {
        console.error('Error fetching data:', error);
    }
}


async function fetchMock_Data_Reminder(month, year) {
    try {
        const response = await fetch('/api/MainMenuStaff/Reminders');
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.text(); // Use text() since the API returns a string

        const records = data.split('\n'); // Split the data into lines
        const container = document.querySelector('.reminder_zone_wrap'); // Ensure this container exists
        // Create a wrapper for each reminder
        const reminderDateWrapper = document.createElement('div');
        reminderDateWrapper.setAttribute('b-o24x7k4ubc', '');
        reminderDateWrapper.className = 'reminder_zone_inner_date';

        // Create and add the inner HTML content
        reminderDateWrapper.innerHTML = `<span b-o24x7k4ubc>${month}, ${year}</span>`;
        container.appendChild(reminderDateWrapper);

        records.forEach(record => {
            const [date, name, tag, remark] = record.split(',').map(item => item.trim());

            if (date) {
                const reminderInformWrapper = document.createElement('div');
                reminderInformWrapper.setAttribute('b-o24x7k4ubc', '');
                reminderInformWrapper.className = 'reminder_zone_inner_inform';

                const reminderInformDate = document.createElement('div');
                reminderInformDate.setAttribute('b-o24x7k4ubc', '');
                reminderInformDate.className = 'reminder_zone_inner_inform_date';
                reminderInformDate.innerHTML = `<span b-o24x7k4ubc> ${getDayOfWeek(date)} </span>`;
                reminderInformWrapper.appendChild(reminderInformDate);

                const reminderInformGap = document.createElement('div');
                reminderInformGap.setAttribute('b-o24x7k4ubc', '');
                reminderInformGap.className = 'reminder_zone_inner_inform_gap';
                reminderInformWrapper.appendChild(reminderInformGap);


                const reminderInformRemark = document.createElement('div');
                reminderInformRemark.setAttribute('b-o24x7k4ubc', '');
                reminderInformRemark.className = 'reminder_zone_inner_inform_remarks';

                const reminderInformRemarkUpper = document.createElement('div');
                reminderInformRemarkUpper.setAttribute('b-o24x7k4ubc', '');
                reminderInformRemarkUpper.className = 'reminder_zone_inner_inform_remarks_upper';

                const reminderInformRemarkUpperUserTag = document.createElement('div');
                reminderInformRemarkUpperUserTag.setAttribute('b-o24x7k4ubc', '');
                reminderInformRemarkUpperUserTag.className = 'reminder_zone_inner_inform_remarks_upper_userTag';

                if (tag === "Sponsor") {
                    reminderInformRemarkUpperUserTag.style.background = '#FFEDBD'; // Change the background color to yellow
                } else {
                    reminderInformRemarkUpperUserTag.style.background = '#B2E3F4'; // Revert to the original background color
                }
                reminderInformRemarkUpperUserTag.innerHTML = `<span b-o24x7k4ubc> ${tag} </span>`;
                reminderInformRemarkUpper.appendChild(reminderInformRemarkUpperUserTag);

                const nameElement = document.createElement('span');
                nameElement.setAttribute('b-o24x7k4ubc', '');
                nameElement.textContent = name;
                reminderInformRemarkUpper.appendChild(nameElement);

                reminderInformRemark.appendChild(reminderInformRemarkUpper);


                const reminderInformRemarkLower = document.createElement('div');
                reminderInformRemarkLower.setAttribute('b-o24x7k4ubc', '');
                reminderInformRemarkLower.className = 'reminder_zone_inner_inform_remarks_lower';

                reminderInformRemarkLower.innerHTML = `<strong b-o24x7k4ubc style="font-size: 1vw;">Remarks: </strong>`;
                const remarkElement = document.createElement('span');
                remarkElement.setAttribute('b-o24x7k4ubc', '');
                remarkElement.textContent = remark;

                reminderInformRemarkLower.appendChild(remarkElement);

                reminderInformRemark.appendChild(reminderInformRemarkLower);

                reminderInformWrapper.appendChild(reminderInformRemark);

                // Append the reminder wrapper to the container
                container.appendChild(reminderInformWrapper);
            }
        });

    } catch (error) {
        console.error('Error fetching data:', error);
    }
}

function getDayOfWeek(dateString) {
    const data = dateString.split(' ')[0];
    const [month, day, year] = data.split('/').map(Number);

    const date = new Date(year, month - 1, day);

    const dayOfWeek = date.getDay();

    const daysOfWeek = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

    return daysOfWeek[dayOfWeek] + "<br>" + day;
}

document.querySelector('.recent_patients_attemps_zone_section').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/ReminderLog/ReminderLog';
});

document.querySelector('.quick_document_generation').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/DocumentGen/documentGen';
});

document.querySelector('.quick_sponsor_information').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/SponsorsList/SponsorList';
});

document.querySelector('.quick_patients_information').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/Patients/patientList';
});

document.querySelector('.reminder_zone_section').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/Reminder/reminder';
});


