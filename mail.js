const MailDev = require('maildev');
const fs = require('fs');
const os = require('os');
const path = require('path');

const maildev = new MailDev({
    smtp: 1030,
    ip: '127.0.0.1'
});

maildev.listen(function() {
    console.log('We can now sent emails to port 1030!');
});

// Print new emails to the console as they come in
maildev.on('new', function(email){
    const fileDir = path.join(os.tmpdir(), 'tempFile.msg');

    console.log('New email saving at ' + fileDir);
    fs.writeFile(fileDir, JSON.stringify(email), () => {});
})