window.onload =  async function () {
    await fetch('/Sponsor/getList')
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to fetch mock data');
            }
            return response.json();
        }).then(mockData => {
            fillIntoList(mockData);
        })
        .catch(error => console.error('Error fetching mockData:', error));
}

function fillIntoList(mockData) {
    const frame = document.getElementById('list-display');

    console.log(mockData);

    mockData.forEach(data => {
        const container = document.createElement('div');
        container.style.padding = '0.5%';
        container.style.borderRadius = '5%';
        container.style.display = 'flex';
        container.style.flexDirection = 'column';
        container.style.justifyContent = 'space-between';
        container.style.alignItems = 'center';
        container.style.width = '20%';
        container.style.height = '65%';
        container.style.backgroundColor = '#E3E3E3';

        const imageFrame = document.createElement('img');
        imageFrame.id = 'patient-img';
        imageFrame.src = 'https://img.freepik.com/premium-vector/test-icon-illustration_430232-32.jpg';
        imageFrame.alt = 'Image';
        imageFrame.style.borderRadius = '10%';
        imageFrame.style.width = '80%';
        imageFrame.style.height = '30%';
        imageFrame.style.objectFit = 'cover';

        const details = document.createElement('div');
        details.style.width = '80%';

        const name = document.createElement('p');
        name.innerHTML = `<strong>Patient Name :</strong> ${data.patientName}`;

        const id = document.createElement('p');
        id.innerHTML = `<strong>Patient ID :</strong> ${data.patientId}`;

        const phone = document.createElement('p');
        phone.innerHTML = `<strong>Phone NO. :</strong> ${data.patientPhone}`;

        const status = document.createElement('p');
        status.innerHTML = `<strong>Status :</strong> ${data.status}`;

        const sponsor = document.createElement('p');
        sponsor.innerHTML = `<strong>Sponsor :</strong> ${data.sponsor}`;

        const choose = document.createElement('button');
        choose.textContent = 'Enter';
        choose.style.width = '80%';
        choose.style.height = '10%';
        choose.style.borderRadius = '10%';

        details.appendChild(name);
        details.appendChild(id);
        details.appendChild(phone);
        details.appendChild(status);
        details.appendChild(sponsor);

        container.appendChild(imageFrame);
        container.appendChild(details);
        container.appendChild(choose);
        frame.appendChild(container);
    });
}