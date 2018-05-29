var dgram = require('dgram');
var server = dgram.createSocket('udp4');
var dns = require('dns');
var time_server_domain = 'pool.ntp.org';
var time_diff = 60 * 23;
var client_pool = [];
var time_server_ip = '';

server.on('message', function (msg, rinfo) {
    console.log(new Date());
    console.log(['  message from ', rinfo.address, ':', rinfo.port].join(''));

    if (rinfo.address != time_server_ip) { //time sync request from client
        client_pool.push({
            address: rinfo.address,
            port: rinfo.port
        });
        server.send(msg, 0, msg.length, 123, time_server_ip, function (err) {
            if (err) throw err;

            console.log(new Date());
            console.log(`  ask to sent to ${time_server_domain}`);
        });
    } else {
        const timeStandard = msg.readUInt32BE(32);
        msg.writeUInt32BE(timeStandard + time_diff, msg.length - 16);
        msg.writeUInt32BE(timeStandard + time_diff, msg.length - 8);
        while (client_pool.length != 0) {
            (function (toIp, toPort) {
                server.send(msg, 0, msg.length, toPort, toIp, function (err) {
                    if (err) throw err;
                    console.log(new Date());
                    console.log(`  response to ${toIp}:${toPort}`);
                });
            })(client_pool[0].address, client_pool[0].port);
            client_pool.splice(0, 1);
        }
    }
});

server.on('listening', function () {
    const address = server.address();
    console.log(`server listening ${address.address}:${address.port}`);
});

dns.lookup(time_server_domain, 4, function (err, ip) {
    if (err) {
        console.log('Error in DNS Lookup');
        console.log(err);
        return;
    }

    time_server_ip = ip;
    server.bind(1203, '127.0.0.1');
});

