
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

document.querySelector('.document-a').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/DocumentGen/STEC';
});
document.querySelector('.document-b').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/DocumentGen/IPPKL';
});
document.querySelector('.document-c').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/DocumentGen/JPIIE';
});
document.querySelector('.document-d').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/DocumentGen/LHDN';
});
document.querySelector('.document-e').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/DocumentGen/SOCSO';
});
document.querySelector('.document-f').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = '/DocumentGen/DocuGenInvoice';
});
updateDateTime();
setInterval(updateDateTime, 1000);
