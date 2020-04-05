var https = require('https');
var fs = require('fs');
var ssl_server_key = '../server/server_key.pem';
var ssl_server_crt = '../server/server_crt.pem';
var port = 8443;
 
var options = {
        key: fs.readFileSync(ssl_server_key),
        cert: fs.readFileSync(ssl_server_crt)
};
 
https.createServer(options, function (req,res) {
        res.writeHead(200, {
                'Content-Type': 'text/plain'
        });
        res.end("Hello, world\n");
}).listen(port);

console.log('started');
