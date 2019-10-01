var mysql = require('mysql');
const https = require('https');
var fs = require('fs');
var express = require('express');

var con = mysql.createConnection({
    host: "localhost",
    user: "testUser",
    password: "newpassword",
    database: "infoSec"
});

con.connect(function(err) {
    if (err) throw err;
    console.log("Connected!");
    // con.query("SELECT * FROM users;", function (err, result) {
    //   if (err) throw err;
    //   console.log("Result: " + result[0].email);
    // });
});


const options = {
    key: fs.readFileSync("/Users/patrickbell/Desktop/classes/infoAndCompSecurity/mainProject/webserver/localhost-key.pem"),
    cert: fs.readFileSync("/Users/patrickbell/Desktop/classes/infoAndCompSecurity/mainProject/webserver/localhost.pem")
};

const app = express();

app.use((req, res) => {

    if (req.method == "GET"){
        res.writeHead(200);
        con.query("SELECT * FROM users;", (error, result) => {
            res.end("Database replied: " + result[0].email);
            console.log("Got stuff");
        });
        console.log("after");
        // res.end("This was a get request");
    } else if (req.method == "POST"){
        res.writeHead(200);
        res.end("This was a post request");
    } else {
        res.end();
    }

});

app.listen(8000);       
https.createServer(options, app).listen(8080, function(){
    console.log("Server is listening on port 8080...");
});
