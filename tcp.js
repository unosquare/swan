var net = require('net');

var server = net.createServer(function (socket) {
    setTimeout(() => {
            socket.on('error', (err) => console.log('IDK'));
            socket.write('Hello World!\r\n');
            socket.end();
        },
        900);
});

server.on('error', (err) => console.log('OOPS'));
server.listen(1337, '127.0.0.1');