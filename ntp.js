var dgram = require("dgram");
var server = dgram.createSocket("udp4");
var dns = require("dns");
var time_server_domain = "pool.ntp.org";
var time_diff = 60 * 23;
var client_pool = [];
var time_server_ip = '';
var prev_checktime = 0;
var ttl = 10000;

server.on("message", function(msg, rinfo) {
	var serverMessageHandler = function() {
		console.log(new Date());
		console.log(["  message from ", rinfo.address, ":", rinfo.port].join(''));
		if (rinfo.address != time_server_ip) { //time sync request from client
			console.log(rinfo.address + ' is different with ' + time_server_ip);
			client_pool.push({
				address: rinfo.address,
				port: rinfo.port
			});
			server.send(msg, 0, msg.length, 123, time_server_ip, function(err, bytes) {
				if (err) throw err;
				console.log(new Date());
				console.log('  ask to sent to ' + time_server_domain);
			});
		} else {
			var time_standard = msg.readUInt32BE(32);
			msg.writeUInt32BE(time_standard + time_diff, msg.length - 16);
			msg.writeUInt32BE(time_standard + time_diff, msg.length - 8);
			while (client_pool.length != 0) { (function(to_ip, to_port) {
					server.send(msg, 0, msg.length, to_port, to_ip, function(err, bytes) {
						if (err) throw err;
						console.log(new Date());
						console.log('  response to ' + to_ip + ':' + to_port);
					});
				})(client_pool[0].address, client_pool[0].port);
				client_pool.splice(0, 1);
			}
		}
	};
	if (prev_checktime + ttl < (new Date()).getTime()) { //TTL 3 hours
		console.log('\n\nTTL Expired '+prev_checktime+' '+(new Date()).getTime()+'. Relookup ' + time_server_domain);
		dns.lookup(time_server_domain, 4, function(err, ip, ipv) {
			if (err) {
				console.log('Error in DNS Lookup');
				console.log(err);
				return
			}
			time_server_ip = ip;
			prev_checktime = (new Date()).getTime();
			console.log('Prev Checktime is '+prev_checktime);
			console.log('Got ip address: '+ ip);
			serverMessageHandler();
		});
	} else {
		serverMessageHandler();
	}
});
server.on("listening", function() {
	var address = server.address();
	console.log("server listening " + address.address + ":" + address.port);
});

server.bind(1203);
