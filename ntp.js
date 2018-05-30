var dgram = require('dgram');
var server = dgram.createSocket('udp4');

server.on('message', function (msg, rinfo) {
    server.send(msg, 0, msg.length, rinfo.port, rinfo.address, function (err) {
        if (err) throw err;
    });
});

server.on('listening', function () {
    const address = server.address();
    console.log(`server listening ${address.address}:${address.port}`);
});

server.bind(1203, '127.0.0.1');

