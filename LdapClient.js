var ldap = require('ldapjs');
var client = ldap.createClient({
    url: 'ldap://127.0.0.1:1089'
});

client.bind('cn=root', 'secret', function (err) {
    console.log(err);
})

var entries = [
    {
        cn: 'Nestor',
        sn: 'Soto',
        email: 'nsoto@unosquare.com',
        objectClass: 'Person'
    },
    {
        cn: 'Israel',
        sn: 'Ramos',
        email: 'iramos@unosquare.com',
        objectClass: 'Person'
    },
    {
        cn: 'Simio',
        sn: 'Perez',
        email: 'gperez@unosquare.com',
        objectClass: 'Person'
    },
    {
        cn: 'Mario',
        sn: 'DiVecce',
        email: 'gperez@unosquare.com',
        objectClass: 'Person'
    }
];

entries.forEach(function (entry) {
    var dn = 'cn=' + entry.cn + ', o=joyent';
    client.add(dn, entry, function (err) {
        console.log(err);
    });
});