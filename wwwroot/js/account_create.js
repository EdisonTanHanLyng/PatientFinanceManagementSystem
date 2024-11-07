
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

document.addEventListener("DOMContentLoaded", function () {

    const deleteButton = document.querySelector('.trash-btn');
    if (deleteButton) {
        deleteButton.addEventListener('click', handleDelete);
    }

    createUserID();
    createAccountname();
    checkPasswordValid();
});

document.getElementById("accountForm").addEventListener("submit", function (event) {
    event.preventDefault(); // Prevent the default form submission

    const formData = new FormData(this);

    fetch("/accountdetail", {
        method: "POST",
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("Account succesfully created.")
                window.location.href = "/ManageAccount/ManageAccount";
            } else {
                // If there's an error, display the error message
                const errorMessageDiv = document.querySelector(".error-package .error-message");
                if (!errorMessageDiv) {
                    // Create error message div if it doesn't exist
                    const errorDiv = document.createElement("div");
                    errorDiv.classList.add("error-message");
                    errorDiv.textContent = data.message;
                    document.querySelector(".error-package").appendChild(errorDiv);
                } else {
                    errorMessageDiv.textContent = data.message;
                }
            }
        })
        .catch(error => {
            console.error("Error:", error);
        });
});

async function handleDelete(event) {
    event.preventDefault();
    const confirmDelete = confirm("Are you confirm with the cancelation of the account creation");
    if (!confirmDelete) {
        window.location.reload();
        return;
    }

    try {

        window.location.href = "/MainMenu/Mainmenu"
    } catch (error) {
        alert(`Failed to cancel user creation: ${error.message}`);
    }
}

updateDateTime();
setInterval(updateDateTime, 1000);

function checkPasswordValid() {
    const password = document.getElementById('password');
    const createButton = document.querySelector('.create-button');
    const requirements = {
        length: document.getElementById('length'),
        uppercase: document.getElementById('uppercase'),
        lowercase: document.getElementById('lowercase'),
        digit: document.getElementById('digit'),
        special: document.getElementById('special')
    };

    function checkValidPassword(value, regex, element) {
        const isValid = regex.test(value);
        element.classList.toggle('valid', isValid);
        element.classList.toggle('invalid', !isValid);
        return isValid;
    }

    function checkPasswordReq() {
        const value = password.value;

        const requirementsMet = [
            checkValidPassword(value, /.{8,}/, requirements.length),
            checkValidPassword(value, /[A-Z]/, requirements.uppercase),
            checkValidPassword(value, /[a-z]/, requirements.lowercase),
            checkValidPassword(value, /\d/, requirements.digit),
            checkValidPassword(value, /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?`~]/, requirements.special)
        ];

        const allRequirementsMet = requirementsMet.every(Boolean);

        createButton.disabled = !allRequirementsMet;
        createButton.style.backgroundColor = allRequirementsMet ? '' : 'gray';
    }

    password.addEventListener('input', checkPasswordReq);

    checkPasswordReq();
}

async function createUserID() {
    try {
        const response = await fetch('/AccountCreate/GenerateUserID'); // Update the URL
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.json();
        const userID = document.getElementById("userid");
        if (userID) {
            userID.value = data.success;
            userID.setAttribute('readonly', true);  // Make the field read-only
        } 

    } catch (error) {
        console.error('Error fetching data:', error);
    }
}

async function createAccountname() {
    const accname = document.getElementById('accname');
    const username = document.getElementById('username');
    username.setAttribute('readonly', true);
    accname.addEventListener('input', function () {
        username.value = this.value;
    });

    username.addEventListener('input', function (event) {
        event.preventDefault();
        this.value = accname.value;
    });

}
