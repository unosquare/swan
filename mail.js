const SMTPServer = require('smtp-server').SMTPServer;
const fs = require('fs');
const os = require('os');
const path = require('path');

const server = new SMTPServer({
    allowInsecureAuth : true,
    onAuth(auth, session, callback){
        if(auth.username !== 'mail' || auth.password !== 'pass'){
            return callback(new Error('Invalid username or password'));
        }
        callback(null, {user: 123}); // where 123 is the user id or similar property
    },
    onData(stream, session, callback){
        const fileDir = path.join(os.tmpdir(), 'tempFile.msg');

        console.log('New email saving at ' + fileDir);
        fs.writeFile(fileDir, JSON.stringify(session), () => { });

        stream.on('end', callback);
    }
});
server.on('error', err => {
    console.log('Error %s', err.message);
});
server.listen(1030);
console.log('We can now sent emails to port 1030!');