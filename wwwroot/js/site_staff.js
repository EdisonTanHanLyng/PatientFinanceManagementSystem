// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Show sidebar on hover
document.querySelector('.sidebar_body_default').addEventListener('mouseenter', function () {
    const sidebar_body = document.querySelector('.sidebar_body');
    const sidebar_body_default = document.querySelector('.sidebar_body_default');

    sidebar_body.style.display = 'block';
    sidebar_body_default.style.display = 'none';
});

// Hide sidebar when mouse leaves the entire sidebar area
document.querySelector('.sidebar_body').addEventListener('mouseleave', function () {
    const sidebar_body = document.querySelector('.sidebar_body');
    const sidebar_body_default = document.querySelector('.sidebar_body_default');

    sidebar_body.style.display = 'none';
    sidebar_body_default.style.display = 'block';
});


disableScrolling();
function disableScrolling() {
    document.body.style.overflow = 'hidden';
}

document.querySelector('.sidebar_body_home').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/MainMenu/Mainmenu_staff';
});

document.querySelector('.sidebar_body_document').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/DocumentGen/documentGen';
});

document.querySelector('.sidebar_body_sponsor').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/SponsorsList/SponsorList';
});


document.querySelector('.sidebar_body_patient').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/Patients/patientList';
});


document.querySelector('.sidebar_body_reminder').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/Reminder/reminder';
});

document.querySelector('.sidebar_body_backup').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/Backup/backup';
});

//Logout
document.querySelector('.sidebar_body_logout').addEventListener('click', function (e) {
    e.preventDefault();
    logout()
        .then(() => {
            window.location.href = '/Login/login';
        })
        .catch(() => {
        });
});

function logout() {
    return fetch('/logout', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        keepalive: true
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Logout failed');
            }
        })
        .then(data => {
            console.log(data.message);
            return data;
        })
        .catch(error => {
            console.error('Error during logout:', error);
            throw error;
        });
}
async function startUserActiveSession() {
    heartbeatInterval = setInterval(() => {

        fetch('/verifyRefresh', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            keepalive: true
        })
            .then(response => {
                return response.json();
            })
            .then(data => {
                if (data.message === 'invalid') {
                    console.log('Session expired. Logging out.');
                    logout();
                }
            })
            .catch(error => {
                console.error('Error sending :', error);
                logout();
            });

    }, 1000); // Send message every 1 second
}

startUserActiveSession();