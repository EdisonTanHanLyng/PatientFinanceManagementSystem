// Update date and time every second
function updateDateTime() {
    var current = new Date();

    // Date formatting options
    var dateOptions = { day: 'numeric', month: 'long', year: 'numeric' };
    var dateFormatter = new Intl.DateTimeFormat('en-US', dateOptions);
    var formattedDate = dateFormatter.format(current);

    // Time formatting options
    var timeOptions = { hour: 'numeric', minute: 'numeric', hour12: true };
    var timeFormatter = new Intl.DateTimeFormat('en-US', timeOptions);
    var formattedTime = timeFormatter.format(current);

    document.getElementById('date').innerHTML = '<strong>Date:</strong> ' + formattedDate;
    document.getElementById('time').innerHTML = '<strong>Time:</strong> ' + formattedTime;
}

// Password validation function
function validatePassword(password) {
   
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?`~₹£•])/;

    // Ensure the password is at least 8 characters long
    return password.length >= 8 && passwordPattern.test(password);
}

// Function to update user details
async function updateUserDetails(event) {
    event.preventDefault(); // Prevent the default form submission

    const form = event.target; // Get the form element
    const formData = new FormData(form); // Create FormData object from the form

    const passwordInput = document.getElementById('password');
    const password = passwordInput.value;

    // Validate password only if it's provided
    if (password && !validatePassword(password)) {
        alert('Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.');
        return; // Stop form submission if password is invalid
    }

    try {
        const response = await fetch(form.action, {
            method: 'POST',
            body: formData,
        });

        if (response.ok) {
            const result = await response.json();
            alert(result.message || 'User details updated successfully.');
            window.location.href = '/ManageAccount/ManageAccount'; // Redirect after success
        } else {
            const error = await response.json();
            alert(`Error updating user details: ${error.error}`);
        }
    } catch (error) {
        alert(`Error updating user details: ${error.message}`);
    }
}

// Function to handle user removal
async function removeUserAccount(userId) {
    if (confirm("Are you sure you want to remove this user?")) {
        try {
            const response = await fetch('/ManageUserAccount/RemoveCurrentUserAccount', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: new URLSearchParams({
                    UserID: userId,
                }),
            });

            const data = await response.json();
            if (data.message) {
                alert(data.message);
                window.location.href = '/ManageAccount/ManageAccount'; // Redirect after successful deletion
            } else {
                alert(data.error || "Failed to remove user account.");
            }
        } catch (error) {
            alert(`Error: ${response.text}`);
        }
    }
}

// Function to check password validity visually
function checkPasswordValid() {
    const password = document.getElementById('password');
    const requirements = {
        length: document.getElementById('length'),
        uppercase: document.getElementById('uppercase'),
        lowercase: document.getElementById('lowercase'),
        digit: document.getElementById('digit'),
        special: document.getElementById('special')
    };
    const passwordRequirements = document.getElementById('passwordRequirements');
    const createButton = document.querySelector('.update-button'); // Reference to the submit button

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
            checkValidPassword(value, /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?`~•]/, requirements.special),
        ];

        const allRequirementsMet = requirementsMet.every(Boolean);
       
       
    }



    password.addEventListener('input', checkPasswordReq);
    password.addEventListener('focus', function () {
        passwordRequirements.style.display = 'block'; // Show requirements when focused
    });
    password.addEventListener('blur', function () {
        if (!password.value) {
            passwordRequirements.style.display = 'none'; // Hide requirements if no password is entered
        }
    });

    checkPasswordReq(); // Initial check
}

// Event listeners for DOMContentLoaded and form submission
document.addEventListener("DOMContentLoaded", function () {
    // Update date and time
    updateDateTime();
    setInterval(updateDateTime, 1000); // Update every second

    // Set the selected option in the dropdown based on the role value
    var selectedRole = document.getElementById("role").getAttribute("data-selected-role");
    var roleSelect = document.getElementById("role");
    if (selectedRole) {
        for (var i = 0; i < roleSelect.options.length; i++) {
            if (roleSelect.options[i].value === selectedRole) {
                roleSelect.selectedIndex = i;
                break;
            }
        }
    }

    // Attach updateUserDetails function to the manage user account form
    const manageUserAccountForm = document.getElementById('manageUserAccountForm');
    if (manageUserAccountForm) {
        manageUserAccountForm.addEventListener('submit', updateUserDetails);
    }

    // Attach removeUserAccount function to remove buttons
    const removeButton = document.getElementById('removeUserButton');
    if (removeButton) {
        removeButton.addEventListener('click', function () {
            const userId = this.getAttribute('data-userid'); // Get the user ID from the button's data attribute
            removeUserAccount(userId);
        });
    }

    // Handle form submission to optionally remove the password field's required attribute
    if (manageUserAccountForm) {
        manageUserAccountForm.addEventListener('submit', function (e) {
            const passwordInput = document.getElementById('password');

            // If password field is empty, remove its required attribute to allow form submission
            if (!passwordInput.value) {
                passwordInput.removeAttribute('required');
            }
        });
    }

    checkPasswordValid(); // Initialize password validation checks
});
