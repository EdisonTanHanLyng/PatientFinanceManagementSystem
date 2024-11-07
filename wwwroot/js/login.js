document.getElementById("accountForm").addEventListener("submit", function (event) {
    event.preventDefault(); // Prevent the default form submission

    const formData = new FormData(this);
    const loginButton = document.querySelector(".login-button");

    // Disable the login button
    loginButton.disabled = true;

    fetch("/login", {
        method: "POST",
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("Logged in", data.path);
                window.location.href = data.path;
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

        })
        .finally(() => {
            // Re-enable the login button after the request completes
            loginButton.disabled = false;
        });
});