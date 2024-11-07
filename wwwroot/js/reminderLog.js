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

updateDateTime();
setInterval(updateDateTime, 1000);

let currentPage = 1;
let searchValue = '';
let dateValue = '';

//Data for table (Dashboard to view)
fetchMock_Data(currentPage, '');
async function fetchMock_Data(page, searchTerm = '') {
    try {
        const response = await fetch(`/api/ReminderLogAdmin/GetPatientsAttempt?page=${page}&searchTerm=${encodeURIComponent(searchTerm)}`);

        if (!response.ok) {
            await displayEmptyResult();
            throw new Error(`Network response was not ok: ${response.status} ${response.statusText}`);
        }
        const data = await response.json();

        await setPaginationState(data.totalUsers, page);
        await populatePatientData(data);
    } catch (error) {
        console.error('Error fetching data:', error);
    }
}


async function displayEmptyResult() {
    const main_container = document.querySelector('.content_body');
    main_container.innerHTML = '';

    const contentDivTitle = document.createElement('div');
    contentDivTitle.className = 'content_title_container';
    contentDivTitle.setAttribute('b-6vry19d86i', '');

    const contentDivName = document.createElement('span');
    contentDivName.className = 'content_Name';
    contentDivName.setAttribute('b-6vry19d86i', '');
    contentDivName.textContent = 'Recipient Name';
    contentDivTitle.appendChild(contentDivName);

    const contentDivEmail = document.createElement('span');
    contentDivEmail.className = 'content_Email';
    contentDivEmail.setAttribute('b-6vry19d86i', '');
    contentDivEmail.textContent = 'Recipient Email';
    contentDivTitle.appendChild(contentDivEmail);

    const contentDivDate = document.createElement('span');
    contentDivDate.className = 'content_Date';
    contentDivDate.setAttribute('b-6vry19d86i', '');
    contentDivDate.textContent = 'Date';
    contentDivTitle.appendChild(contentDivDate);

    const contentDivStatus = document.createElement('span');
    contentDivStatus.className = 'content_Status';
    contentDivStatus.setAttribute('b-6vry19d86i', '');
    contentDivStatus.textContent = 'Status';
    contentDivTitle.appendChild(contentDivStatus);

    main_container.appendChild(contentDivTitle);
}

async function populatePatientData(data) {
    const main_container = document.querySelector('.content_body');
    main_container.innerHTML = '';

    const contentDivTitle = document.createElement('div');
    contentDivTitle.className = 'content_title_container';
    contentDivTitle.setAttribute('b-6vry19d86i', '');

    const contentDivName = document.createElement('span');
    contentDivName.className = 'content_Name';
    contentDivName.setAttribute('b-6vry19d86i', '');
    contentDivName.textContent = 'Recipient Name'; 

    contentDivTitle.appendChild(contentDivName);

    const contentDivEmail = document.createElement('span');
    contentDivEmail.className = 'content_Email';
    contentDivEmail.setAttribute('b-6vry19d86i', '');
    contentDivEmail.textContent = 'Recipient Email';

    contentDivTitle.appendChild(contentDivEmail);

    const contentDivDate = document.createElement('span');
    contentDivDate.className = 'content_Date';
    contentDivDate.setAttribute('b-6vry19d86i', '');
    contentDivDate.textContent = 'Date';

    contentDivTitle.appendChild(contentDivDate);

    const contentDivStatus = document.createElement('span');
    contentDivStatus.className = 'content_Status';
    contentDivStatus.setAttribute('b-6vry19d86i', '');
    contentDivStatus.textContent = 'Status';

    contentDivTitle.appendChild(contentDivStatus);

    main_container.appendChild(contentDivTitle);
    const patients = await parsePatients(data.pagedUsers);

    for (const patient of patients) {
        if (patient.id) {
            const contentDiv = document.createElement('div');
            contentDiv.className = 'content_text_container';
            contentDiv.setAttribute('b-6vry19d86i', '');

            // Name span
            const nameSpan = document.createElement('span');
            nameSpan.className = 'content_ID_text';
            nameSpan.textContent = patient.id;
            nameSpan.setAttribute('b-6vry19d86i', '');
            contentDiv.appendChild(nameSpan);

            // Email span
            const emailSpan = document.createElement('span');
            emailSpan.className = 'content_Name_text';
            emailSpan.textContent = patient.name;
            emailSpan.setAttribute('b-6vry19d86i', '');
            contentDiv.appendChild(emailSpan);

            // Date span
            const dateSpan = document.createElement('span');
            dateSpan.className = 'content_CreatedBy_text';
            dateSpan.textContent = patient.date;
            dateSpan.setAttribute('b-6vry19d86i', '');
            contentDiv.appendChild(dateSpan);

            // Status span
            const statusSpan = document.createElement('span');
            statusSpan.className = 'content_Status_text';
            statusSpan.textContent = patient.status;
            statusSpan.setAttribute('b-6vry19d86i', '');
            contentDiv.appendChild(statusSpan);

            main_container.appendChild(contentDiv);
        }
    }
}

async function parsePatients(pagedUsersString) {
    return pagedUsersString.trim().split('\n').map(line => {
        const [id, name, status, date] = line.split(' , ');
        return { id, name, status, date};
    });
}

async function setPaginationState(totalPages, currentPage) {

    const paginationPages = document.querySelector('.pagination_pages');
    if (!paginationPages) {
        console.error("Pagination container '.pagination_pages' not found");
        return;
    }

    paginationPages.innerHTML = '';

    if (typeof totalPages !== 'number' || typeof currentPage !== 'number') {
        console.error("Invalid input: totalPages and currentPage must be numbers");
        return;
    }

    totalPages = Math.max(1, Math.floor(totalPages));
    currentPage = Math.max(1, Math.min(Math.floor(currentPage), totalPages));

    const pagesToShow = [];
    pagesToShow.push(1);

    if (totalPages < 7) {
        for (let i = 2; i < totalPages; i++) {
            pagesToShow.push(i);
        }
    } else {
        if (currentPage <= 3) {
            pagesToShow.push(2, 3, 4, '...', totalPages);
        } else if (currentPage >= totalPages - 2) {
            pagesToShow.push('...', totalPages - 3, totalPages - 2, totalPages - 1);
        } else {
            pagesToShow.push('...', currentPage - 1, currentPage, currentPage + 1, '...', totalPages);
        }
    }

    if (pagesToShow[pagesToShow.length - 1] !== totalPages) {
        pagesToShow.push(totalPages);
    }

    for (const page of pagesToShow) {
        const paginationItem = document.createElement('span');
        paginationItem.className = 'pagination_page';
        paginationItem.setAttribute('b-6vry19d86i', '');

        if (page === '...') {
            paginationItem.textContent = '...';
            paginationItem.classList.add('ellipsis');
        } else {
            paginationItem.textContent = page;
            if (page === currentPage) {
                paginationItem.classList.add('active');
            }
            paginationItem.addEventListener('click', async function () {
                await setActivePage(page);
            });
        }

        paginationPages.appendChild(paginationItem);
    }
}

async function setActivePage(pageNumber) {
    document.querySelectorAll('.pagination_page').forEach(item => {
        item.classList.remove('active');
    });
    pageNumber = parseInt(pageNumber, 10);
    const activePage = Array.from(document.querySelectorAll('.pagination_page')).find(item =>
        parseInt(item.textContent, 10) === pageNumber
    );

    if (activePage) {
        activePage.classList.add('active');
        await fetchMock_Data(pageNumber, searchValue || dateValue);
    } else {
        console.error(`Could not find pagination item for page ${pageNumber}`);
    }
}

document.querySelector('.material-symbols-outlined.left').addEventListener('click', async function () {
    const currentPage = document.querySelector('.pagination_page.active');
    if (currentPage && currentPage.previousElementSibling) {
        await setActivePage(parseInt(currentPage.previousElementSibling.textContent));
    }
});

document.querySelector('.material-symbols-outlined.right').addEventListener('click', async function () {
    const currentPage = document.querySelector('.pagination_page.active');
    if (currentPage && currentPage.nextElementSibling) {
        await setActivePage(parseInt(currentPage.nextElementSibling.textContent));
    }
});

document.addEventListener('DOMContentLoaded', function () {
    const mainContainer = document.querySelector('.content_body');
    mainContainer.addEventListener('click', function (e) {
        const contentOption = e.target.closest('.content_option');
        if (contentOption) {
            const popupMenu = contentOption.querySelector('.popup-menu');
            if (popupMenu) {
                contentOption.classList.toggle('active');

                const rect = popupMenu.getBoundingClientRect();

                if (rect.left < 0) {
                    popupMenu.style.left = '0';
                    popupMenu.style.right = 'auto';
                }

                if (rect.top < 0) {
                    popupMenu.style.bottom = 'auto';
                    popupMenu.style.top = '100%';
                }
            }
        }
    });

    document.addEventListener('click', function (e) {
        const activeOptions = document.querySelectorAll('.content_option.active');
        activeOptions.forEach(function (option) {
            if (!option.contains(e.target)) {
                option.classList.remove('active');
            }
        });
    });


});

let timer;
const waitTime = 1000; // 1 second
document.addEventListener('DOMContentLoaded', () => {
    const searchBar = document.getElementById('patient_attempt-search-input');
    const dateInput = document.getElementById('patient_attempt-date-input');

    if (searchBar && dateInput) {
        let previousDateValue = dateInput.value;

        const handleSearch = async () => {
            searchValue = searchBar.value;
            dateValue = dateInput.value;

            if (dateValue === '' && previousDateValue !== '') {
                searchBar.value = '';
                await clearSearch();
                await fetchMock_Data(1, '');
            } else if (dateValue.trim() !== '') {
                searchBar.value = dateValue;
                await fetchMock_Data(1, dateValue.trim());
            } else if (searchValue.trim() !== '') {
                await fetchMock_Data(1, searchValue.trim());
            } else {
                await clearSearch();
                await fetchMock_Data(1, '');
            }
            previousDateValue = dateValue;
        };

        const debounceSearch = (e) => {
            clearTimeout(timer);
            timer = setTimeout(handleSearch, waitTime);
        };

        searchBar.addEventListener('input', debounceSearch);
        dateInput.addEventListener('change', handleSearch); // Remove debounce for date changes
    } else {
        console.error('Search bar or date input element not found');
    }
});

async function clearSearch() {
    try {
        const response = await fetch('/api/ReminderLogAdmin/clearSearch', {
            method: 'POST',
        });

        if (response.redirected) {
            await fetchMock_Data(1);
        }
    } catch (error) {
        console.error('Error clearing search:', error);
    }
}