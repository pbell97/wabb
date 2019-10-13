var mysql = require('mysql');
const https = require('https');
var fs = require('fs');
var express = require('express');
var TLSConnection = require('./tlsTesting/nodeTLSClass.js');
const {OAuth2Client} = require('google-auth-library');
const googleAudienceID = '154600092899-9tdri4c4k80lg38ak4bd9s6aro95s9nt.apps.googleusercontent.com';
var tokenCache = {};

// Caches users
var cachedUsers = {};

var con = mysql.createConnection({
    host: "localhost",
    user: "testUser",
    password: "newpassword",
    database: "infoSec"
});

con.connect(function(err) {
    if (err) throw err;
    console.log("Connected to MySQL Server");
});


// IMPLEMENT CONNECTION CLASS HERE

// CREATE FUNCTIONS FOR MESSAGES, ADD USERS, ETC HERE --- THEN ADD THEM TO CALLBACKS OF CONNECTION CLASS






const options = {
    key: fs.readFileSync("/Users/patrickbell/Desktop/classes/infoAndCompSecurity/mainProject/webserver/localhost-key.pem"),
    cert: fs.readFileSync("/Users/patrickbell/Desktop/classes/infoAndCompSecurity/mainProject/webserver/localhost.pem")
};

const app = express();

app.use((req, res) => {

    if (req.method == "GET"){

        if (req.path == "/auth"){
            res.status(200);
            res.end("Auth endpoint hit");
        } 
        
        // Returns all messages for a user's chat
        if (req.path == "/messages"){
            // Add a way to check if user is in chat
            var access_id = req.headers.authorization.substring(7);
            verifyTokenManually(access_id, (accepted, data) => {
                if (accepted){
                    var id = data.user_id;
                    var chatId = req.query.chatId;
                    var messageId = (req.query.messageId) ? req.query.messageId : 0;
                    executeSQLQuery(`SELECT id, messageId, messageContent FROM messages WHERE chatid = "${chatId}" AND messageId > ${messageId};`, (error, results) =>{
                        if (error) throw error;
                        res.status(200);
                        res.json(results);
                    });
                } else {
                    res.status(401);
                    res.end();
                }
            });
        }

        // User trying to join a chat
        if (req.path == "/joinChat"){
            var chatId = req.query.chatId;
            var pubKey = req.query.pubKey;
            var access_id = req.headers.authorization.substring(7);
            verifyTokenManually(access_id, (accepted, data) => {
                if (accepted){
                    // Check if chat exists
                    // TODO, combine with next SQL query for users, just get founderId and users
                    executeSQLQuery(`SELECT founderId FROM chats WHERE chatId ="${chatId}";`, (error, results) => {
                        if (error) throw error;
                        // TODO add error checking if chat doesn't exist
                        var joinerId = data.user_id;
                        var founderId = results[0].founderId;
                        // TODO ---Send request to user to join, including the pubKye and user name---
                        var tempSymKey = 'abcdefgSYMETRICKEYabcdefg';
                        var canJoin = true;
                        if (canJoin){
                            var returnData = {
                                symKey: tempSymKey,
                                chatId: chatId              //Maybe add a list of active users?
                            }
                            res.status(200);
                            res.json(returnData);
                            executeSQLQuery(`SELECT users FROM chats WHERE chatId = "${chatId}";`, (error, results) => {
                                if (error) throw error;
                                var users = results[0].users + "," + joinerId;
                                executeSQLQuery(`UPDATE chats SET users = "${users}" WHERE chatId = "${chatId}";`, (error, results) => {
                                    if (error) throw error;
                                })

                            })
                        } else {
                            res.status(200);
                            res.end("Nope, cant join");
                        }
                    });
                } else {
                    res.status(401);
                    res.end();
                }
            });

        }
        
    } else if (req.method == "POST"){

        if (req.path == "/createUser"){
            var access_id = req.headers.authorization.substring(7);
            verifyTokenManually(access_id, (accepted, data) => {
                if (accepted){
                    // Check if user already exists
                    userExists(data.user_id, (exists) => {
                        if (exists){
                            res.status(400);
                            res.end("User " + data.user_id + " already exists");
                            return;
                        }
                        // Add user
                        var username = data.email;
                        if (req.query.username){
                            username = req.query.username;
                        }
                        var result = executeSQLQuery("INSERT INTO users (username, email, id) VALUES (\"" + username + "\",\"" + data.email + "\",\"" + data.user_id + "\");");
                        res.status(201);
                        res.end("Created user: " + data.user_id);
                    });
                } else {
                    res.status(400);
                    res.end("User not created");
                }
            })
        }

        // Posts a user's messages
        if (req.path == "/messages"){
            var access_id = req.headers.authorization.substring(7);
            verifyTokenManually(access_id, (accepted, data) => {
                if (accepted){
                    // messages (id char(32), chatId varchar(32), messageId int, messageContent varchar(250));
                    let chatId = req.query.chatId;
                    let id = data.user_id;
                    let messageContent = req.query.messageContent;
                    executeSQLQuery(`SELECT MAX(messageID) FROM messages WHERE chatId = "${chatId}";`, function(error, results) {
                        // TODO add error check if doesn't exist
                        if (error) throw error;
                        let messageId = (results == null) ? 1 : parseInt(results[0]["MAX(messageID)"]) + 1;
                        executeSQLQuery(`INSERT INTO messages (id, chatid, messageid, messageContent) VALUES ("${id}", "${chatId}", ${messageId}, "${messageContent}");`, (error, result) => {
                            // TODO check if error adding
                            // TODO Alert to send message via TCP stream
                            console.log("Should be sending message now...");
                            res.status(201);
                            res.end("Message sent placeholder");
                        });
                    });
                } else {
                    res.status(401);
                    res.end('Invalid Token');
                }
            });
        }
        
    }

});

app.listen(8000);       
https.createServer(options, app).listen(8080, function(){
    console.log("Server is listening on port 8080...");
});


function verifyTokenManually(id, callback){
    // Checks if token is cached instead of querying Google
    if (Object.keys(tokenCache).includes(id)){
        if (tokenCache[id].expireTime > new Date().getTime()){
            callback(true, tokenCache[id]);
            return;
        } else {
            delete tokenCache[id];
            callback(false, null);
            return;
        }
    }

    // Queries Google to see if token is valid
    var url = "https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=" + id;
    https.get(url, (response, otherPossibleParam) => {
        var data = '';
        response.on('data', (chunk) => {
            data += chunk;
        });
        response.on('end', () => {
            data = JSON.parse(data);

            // Add check for if in cache or table (function that checks memory, if not in mem then in SQL) ***
            if (data.audience == googleAudienceID && parseInt(data.expires_in) > 0){
                data.expireTime = new Date().getTime() + data.expires_in*1000;
                tokenCache[id] = data;
                callback(true, data);
            } else {
                callback(false, data);
            }

        });
    });
}

function executeSQLQuery(query, callback){
    con.query(query, callback);
}

// Returns bool -> Checks if a user is cached, else query the DB and cache if found
function userExists(id, callback){
    if (cachedUsers[id]){
        return callback(true);
    } else {
        executeSQLQuery("SELECT id, username FROM users WHERE id = \"" + id + "\";", (error, result) => {
            if (error) throw error;
            if (result.length != 0){
                cachedUsers[id] = result[0].username;
                return callback(true);
            } else {
                return callback(false);
            }
        });
        
    }
}


// A 'user_id' is given in the token. Maybe use that as the ID in the database?

// Need to fix async nature of userExits...
// Need to do some kind of checking of if message sender belongs to group/is friends with recipient
// How to store if user is in a chat or not??? Fix for get/post messages
// Need to cache users so that a non existant user can't be sending messages, need to check and cache w/ each request