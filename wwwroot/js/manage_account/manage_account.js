const connection = new signalR.HubConnectionBuilder()
    .withUrl("/ManageAccountHub") // Connection to hub route
    .build();




// Handle user login event from the server via SignalR
connection.on("UserLoggedIn", async function (userId) {
    
    await updateUserStatus(userId);
    
});

connection.on("UserLoggedOut", async function (userId) {
    
    await updateUserStatus(userId);
    
});

async function start() {
    try {
        await connection.start();
        
    } catch (err) {
        console.error(err);
        setTimeout(start, 5000); // Retry connection after 5 seconds
    }
}

start();


function manageUser(userId) {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/ManageUserAccount/ManageUserAccount';

    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = 'userId';
    input.value = userId;

    form.appendChild(input);
    document.body.appendChild(form);
    form.submit();
}

async function updateUserStatus(userId) {
    // querySelector to find the list item containing the userId
    const userListItem = Array.from(document.querySelectorAll('.userL-listContainer_list'))
        .find(item => item.querySelector('.user-id-value').textContent.trim() === userId);

    // To Ensure userListItem is found and then get the status circle
    if (userListItem) {
        const userCircle = userListItem.querySelector('.user-status-circle');

        try {
            const response = await fetch(`/api/userstatus/CheckUserStatus?userId=${encodeURIComponent(userId)}`);
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            const data = await response.json();
            userCircle.style.backgroundColor = data.isOnline ? '#00FF00' : 'grey'; // Update status circle color
        } catch (error) {
            console.error('Error updating user status:', error);
        }
    }
}


// Initialize user status colors based on their data-status attributes
document.addEventListener('DOMContentLoaded', function () {
    const statusCircles = document.querySelectorAll('.user-status-circle');

    statusCircles.forEach(circle => {
        const status = circle.getAttribute('data-status');
        circle.style.backgroundColor = status === 'Online' ? '#00FF00' : 'grey'; // Set color based on status
    });

    // Setup sign-off buttons
    const signOffButtons = document.querySelectorAll('.sign-off-button');
    signOffButtons.forEach(button => {
        button.addEventListener('click', async function () {
            const userId = button.closest('.userL-listContainer_list').querySelector('.user-id-text').textContent; // Find userId from the context
            await signOffUserTesting(userId); // Call sign off user function
        });
    });

    // Added input event listener for dynamic searching
    const accountSearchInput = document.getElementById('account-search-input');
    accountSearchInput.addEventListener('input', performSearch); // Trigger search on input

});


document.addEventListener('DOMContentLoaded', function () {
    const createAccountButton = document.querySelector('.create-account-button');

    createAccountButton.addEventListener('click', function () {
        // Redirect to the account creation page
        window.location.href = '/AccountCreate/AccountCreate'; // Change this to the actual URL for the account creation page
    });
});

// Function to sign off the user
async function signOffUserTesting(userId) {
    try {
        const response = await fetch(`/ManageAccount/SignOffUserTesting?userId=${encodeURIComponent(userId)}`, {
            method: 'GET'
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Error: ${response.status} - ${errorText}`);
        }

        alert(`User ${userId} has been successfully signed off.`); 

        await updateUserStatus(userId);

    } catch (error) {
        alert('An error occurred while signing off the user. Please try again.');
    }
}

// Update the current date and time in the specified HTML elements
function updateDateTime() {
    var current = new Date();
    var dateOptions = { day: 'numeric', month: 'long', year: 'numeric' };
    var dateFormatter = new Intl.DateTimeFormat('en-US', dateOptions);
    var formattedDate = dateFormatter.format(current);
    var timeOptions = { hour: 'numeric', minute: 'numeric', hour12: true };
    var timeFormatter = new Intl.DateTimeFormat('en-US', timeOptions);
    var formattedTime = timeFormatter.format(current);

    document.getElementById('date').innerHTML = '<strong>Date:</strong> ' + formattedDate;
    document.getElementById('time').innerHTML = '<strong>Time:</strong> ' + formattedTime;
}

updateDateTime();
setInterval(updateDateTime, 1000);


function performSearch() {
    const searchTerm = document.getElementById('account-search-input').value.toLowerCase();

    const users = document.querySelectorAll('#user-list .userL-listContainer_list');
    users.forEach(user => {
        const name = user.querySelector('.name-text').textContent.toLowerCase();
        const role = user.querySelector('.role-text').textContent.toLowerCase();
        const userId = user.querySelector('.user-id-value').textContent.toLowerCase();
        // Perform a general search across all fields
        user.style.display = (name.includes(searchTerm) || role.includes(searchTerm) || userId.includes(searchTerm))
            ? ''
            : 'none'; // Show or hide user based on search term
    });
}

function toggleVisibility() {
    const filter = document.getElementById("account-filter");
    filter.classList.toggle("show");

    if (filter.classList.contains("show")) {
        filter.style.display = 'flex'; // Set to flex when visible
    } else {
        filter.style.display = 'none'; // Hide when not visible
    }
}


document.addEventListener('DOMContentLoaded', function () {
    // Added input event listener for dynamic searching for account filter fields
    document.querySelectorAll('.account-filter-name, .account-filter-role, .account-filter-username').forEach(inputField => {
        inputField.addEventListener('input', function () {
            filterUsers(); // Call filterUsers on input
        });
    });

    const accountSearchInput = document.getElementById('account-search-input');
    accountSearchInput.addEventListener('input', performSearch); // Trigger search on input
});

// Filters the displayed user accounts based on the search criteria entered by the user.
function filterUsers() {
    const searchName = document.getElementById('account-search-name').value.toLowerCase();
    const searchRole = document.getElementById('account-search-role').value.toLowerCase();
    const searchUserId = document.getElementById('account-search-username').value.toLowerCase();

    const users = document.querySelectorAll('#user-list .userL-listContainer_list');

    users.forEach(user => {
        const name = user.querySelector('.name-text').textContent.toLowerCase();
        const role = user.querySelector('.role-text').textContent.trim().toLowerCase();
        const userId = user.querySelector('.user-id-value').textContent.toLowerCase();

        // Adjusted the normalization to check against the dropdown values directly
        let normalizedRole = (role === 'staff') ? 'user' : role;
        


        user.style.display = (name.includes(searchName) || !searchName) &&
            (normalizedRole === searchRole || searchRole === '') && // Compare with normalized role
            (userId.includes(searchUserId) || !searchUserId) ? '' : 'none'; // Show or hide user
    });
}

document.getElementById('account-search-role').addEventListener('change', filterUsers);


// Function to toggle menu visibility
function toggleMenu(menuIcon) {
    const menuContainer = menuIcon.parentElement;
    menuContainer.classList.toggle('show');
}

// Close the menu if clicked outside
window.onclick = function (event) {
    if (!event.target.matches('.three-dots')) {
        const dropdowns = document.querySelectorAll('.menu-container');
        dropdowns.forEach(dropdown => {
            if (dropdown.classList.contains('show')) {
                dropdown.classList.remove('show');
            }
        });
    }
};
