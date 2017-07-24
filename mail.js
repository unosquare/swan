const MailDev = require('maildev')
const fs = require('fs');
const os = require('os');
const path = require('path');

const maildev = new MailDev({
  smtp: 1025 // incoming SMTP port - default is 1025
})

maildev.listen(function(err) {
  console.log('We can now sent emails to port 1025!')
})

// Print new emails to the console as they come in
maildev.on('new', function(email){
    const fileDir = path.join(os.tmpdir(), 'tempFile.msg');

    console.log('New email saving at ' + fileDir);
    fs.writeFile(fileDir, JSON.stringify(email), () => {});
})