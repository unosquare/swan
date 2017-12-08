const express = require('express');
const app = express();

app.get('/', (req, res) => res.send('Hello World!'));

/* #region Authentication Tests */
    app.get('/Authenticate', (req, res) => res.send(JSON.stringify({ Token : '123'})));

    app.post('/Authenticate', (req, res) => res.send(JSON.stringify({ Token: '123'})));
/* #endregion Authentication Tests */

/* #region Post Tests */
    app.post('/Post/WithValidParams', (req, res) => {
        var data = { StringData: 'OK' };
        res.send(JSON.stringify(data));
    });

    app.post('/Post/WithValidParamsAndAuthorizationToken', (req, res) => res.send(JSON.stringify({ Authorization : 'Bearer Token'})));
/* #endregion Post Tests */

/* #region GetString */
    app.get('/GetString/WithValidParamsAndAuthorizationToken', (req, res) => {
        res.send(JSON.stringify(req.headers));
    });
/* #endregion GetString */


app.listen(3000, () => console.log('Webserver up!'));