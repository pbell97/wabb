const https = require('https');
var fs = require('fs');
var express = require('express');
const googleAudienceID = '154600092899-9tdri4c4k80lg38ak4bd9s6aro95s9nt.apps.googleusercontent.com';
var tokenCache = {};

const options = {
    key: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/privkey.pem"),
    cert: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/fullchain.pem"),
    ca: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/chain.pem")
};

const app = express();

app.use((req, res) => {
    if (req.method == "GET"){
        if (req.path == "/auth"){
            res.status(200);
            res.end("Auth endpoint hit");
        } 
    } 
});


https.createServer(options, app).listen(443, function(){
    console.log("Starting...");
});


// MUST Stop apache
//      sudo /opt/bitnami/ctlscript.sh stop
//      sudo /opt/bitnami/ctlscript.sh status       - To make sure it has stopped
//      Then make sure your server is running on port 443