const express = require('express');
const app = express();

app.get('/', (req, res) => res.send('Hello World!'));

/* #region Error responses */
app.get('/511', (req, res) => res.statusCode = 511);

app.get('/404', (req, res) => res.statusCode = 404);
/* #endregion Error responses */

/* #region Authentication Tests */
app.get('/Authenticate', (req, res) => res.send(JSON.stringify({ Token: '123' })));

app.post('/Authenticate', (req, res) => res.send(JSON.stringify({ Token: '123' })));
/* #endregion Authentication Tests */

/* #region Post Tests */
app.post('/Post/WithValidParams', (req, res) => {
    var data = '';
    req.on('data', (chunk) => {
        data += chunk.toString();
    })
    req.on('end', () => {
        var obj = JSON.parse(data);
        obj.StringData = 'OK';
        res.send(obj);
    });
});

app.post('/Post/WithValidParamsAndAuthorizationToken', (req, res) => res.send(JSON.stringify({ Authorization: 'Bearer Token' })));
/* #endregion Post Tests */

/* #region GetString */
app.get('/GetString/WithValidParamsAndAuthorizationToken', (req, res) => res.send(JSON.stringify(req.headers)));
/* #endregion GetString */

/* #region Put */
app.put('/Put/WithValidParams', (req, res) => {
    var data = '';
    req.on('data', (chunk) => {
        data += chunk.toString();
    })
    req.on('end', () => {
        var obj = JSON.parse(data);
        obj.StringData = 'OK';
        res.send(obj);
    });
});

app.put('/Put/WithValidParamsAndAuthorizationToken', (req, res) => res.send(JSON.stringify({ Authorization: 'Bearer Token' })));
/* #endregion Put*/

/* #region PostFileString  */
app.post('/PostFileString/WithValidParams', (req, res) => {
    var data = '';
    req.on('data', (chunk) => {
        data += chunk.toString();
    })
    req.on('end', () => {
        res.send(data);
    });
});
/* #endregion PostFileString */

/* #region PostFile */
app.post('/PostFile/WithValidParams', (req, res) => {
    var data = '';
    req.on('data', (chunk) => {
        data += chunk.toString();
    })
    req.on('end', () => {
        var obj = JSON.parse(data);
        res.send(obj);
    });
});
/* #endregion PostFile */

/* #region PostOrError */
app.post('/PostOrError/PostOrErrorTest', (req, res) => {
    var data = '';
    req.on('data', (chunk) => {
        data += chunk.toString();
    });
    req.on('end', () => {
        var obj = JSON.parse(data);
        if (obj.IntData === 1) {
            res.send(obj);
        } else {
            res.statusCode = 500;
            res.send(JSON.stringify({ Message: 'ERROR' }));
        }
    });
});
/* #endregion PostOrError */

/* #region GetBinary */
app.get('/GetBinary/WithValidParams', (req, res) => res.send(JSON.stringify(req.headers)));

app.get('/GetBinary/WithInvalidParams', (req, res) => { });
/* #endregion GetBinary */

/* #region Get */
app.get('/Get/WithValidParams', (req, res) => {
    var data = JSON.stringify({
        StringData: 'Data',
        IntData: 1,
        BoolData: true
    });
    res.send(data);
});
/* #endregion Get */

app.listen(3000, () => console.log('Webserver up!'));