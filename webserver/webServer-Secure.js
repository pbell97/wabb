var mysql = require('mysql');
const https = require('https');
var fs = require('fs');
var express = require('express');
var TLSConnection = require('./tlsTesting/nodeTLSClass.js');
const googleAudienceID = '154600092899-9tdri4c4k80lg38ak4bd9s6aro95s9nt.apps.googleusercontent.com';
var tokenCache = {};

// Caches users
var cachedUsers = {};

// Connection to SQL DB
var con = mysql.createConnection({
    host: "localhost",
    user: "testUser",
    password: "newpassword",
    database: "infoSec"
});

// Acutally connects to SQL
con.connect(function(err) {
    if (err) throw err;
    console.log("Connected to MySQL Server");
});

// Interprets all messages from socket
function interpretMessage(socket, message){
    switch (message.type){
        case "messagePost":
            messagePost(socket, message.content);
            break;
        case "messageGet":
            messageGet(socket, message.content)
            break;
        case "createUser":
            createUserPost(socket, message.content);
            break;
        case "joinChat":
            joinChatGet(socket, message.content);
            break;
        case "listChats":
            listChatsGet(socket, message.content);
            break;
        case "createChat":
            createChat(socket, message.content)
            break;
        default:
            socket.write(JSON.stringify({"response": "Invalid Request"}));
            break;
    }
}

// Posts a message
function messagePost(socket, message){
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            // messages (id char(32), chatId varchar(32), messageId int, messageContent varchar(250));
            let chatId = message.chatId;
            let id = data.user_id;
            let messageContent = message.messageContent;
            userIsInChat(chatId, id, (isInChat) => {
                if (isInChat){
                    executeSQLQuery(`SELECT MAX(messageID) FROM messages WHERE chatId = "${chatId}";`, function(error, results) {
                        // TODO add error check if doesn't exist
                        if (error) throw error;
                        var messageId = (results[0]["MAX(messageID)"] == null) ? 1 : parseInt(results[0]["MAX(messageID)"]) + 1;
                        executeSQLQuery(`INSERT INTO messages (id, chatid, messageid, messageContent) VALUES ("${id}", "${chatId}", ${messageId}, "${messageContent}");`, (error, result) => {
                            if (error) throw error;
                            // Should alert all sockets listening for this type of message
                            socket.write(JSON.stringify({"response": "Message posted successfully"}));
                        });
                    });
                } else {
                    socket.write(JSON.stringify({"response": "User is not in chat"}));
                }
            });
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });

}

// Return all messages for a user's chat
function messageGet(socket, message){
    // Add a way to check if user is in chat
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var id = data.user_id;
            var chatId = message.chatId;
            var messageId = (message.messageId) ? message.messageId : 0;
            userIsInChat(chatId, id, (isInChat) => {
                if (isInChat){
                    executeSQLQuery(`SELECT id, messageId, messageContent FROM messages WHERE chatid = "${chatId}" AND messageId > ${messageId};`, (error, results) =>{
                        if (error) throw error;
                        socket.write(JSON.stringify({"response": results}));

                    });
                } else {
                    socket.write(JSON.stringify({"response": "User is not in chat"}));
                }
            });
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

// Creates a user from the access token if they are signed in and don't have an account registered
function createUserPost(socket, message){
    var access_id = message.access_id;
    var username = (message.username) ? message.username : data.email;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            // Check if user already exists
            userExists(data.user_id, (exists) => {
                if (exists){
                    socket.write(JSON.stringify({"response": "User " + data.user_id + " already exists"}));
                    return;
                }
                // Add user
                var result = executeSQLQuery("INSERT INTO users (username, email, id) VALUES (\"" + username + "\",\"" + data.email + "\",\"" + data.user_id + "\");");
                var response = {"response": {"access_id": access_id, "username": username, "user_id": data.user_id}};
                socket.write(JSON.stringify(JSON.stringify(response)));
            });
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

// User attempts to join a chat
function joinChatGet(socket, message){
    var chatId = message.chatId;
    var pubKey = message.pubKey;
    var access_id = message.access_id;
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
                    socket.write(JSON.stringify({"response": returnData}));
                    executeSQLQuery(`SELECT users FROM chats WHERE chatId = "${chatId}";`, (error, results) => {
                        if (error) throw error;
                        var users = results[0].users + "," + joinerId;
                        executeSQLQuery(`UPDATE chats SET users = "${users}" WHERE chatId = "${chatId}";`, (error, results) => {
                            if (error) throw error;
                        })

                    })
                } else {
                    socket.write(JSON.stringify({"response": "Cannot Join"}));
                }
            });
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });

}

function listChatsGet(socket, message){
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var query = "SELECT * FROM chats;"
            executeSQLQuery(query, (error, results) => {
                if (error) throw error;
                var response = {"response": results}
                socket.write(JSON.stringify(JSON.stringify(response)));
                console.log(JSON.stringify(response));
            })
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

function createChat(socket, message){
    var chatName = message.chatName;
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var user_id = data.user_id;
            var chatId = hashCode(chatName);
            var querry = "INSERT INTO chats (chatId, chatName, founderId, users) VALUES (\"" + chatId + "\", \"" + chatName + "\", \"" + user_id + "\", \"" + user_id + "\");";
            executeSQLQuery(querry, (error, results) => {
                if (error) throw error;
                var response = {"chatCreated": chatId};
                socket.write(JSON.stringify(response));
            });
        }else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}




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

function userIsInChat(chatId, user_id, callback){
    var query = "SELECT users FROM chats WHERE chatId = \"" + chatId + "\";"; 
    executeSQLQuery(query, (error, results)=>{
        if (error) throw error;
        var users = results[0]["users"].split(',');
        callback(users.includes(user_id));
    });
}


var serverConnection = new TLSConnection();
serverConnection.messageCallbacks.push(interpretMessage);
serverConnection.listen();

function hashCode(s) {
    var h = 0, l = s.length, i = 0;
    if ( l > 0 )
      while (i < l)
        h = (h << 5) - h + s.charCodeAt(i++) | 0;
    return h;
  };

// Maybe should add some kind of 'type' on response as well???

// A 'user_id' is given in the token. Maybe use that as the ID in the database?
// Need to do some kind of checking of if message sender belongs to group/is friends with recipient
// How to store if user is in a chat or not??? Fix for get/post messages
// Need to cache users so that a non existant user can't be sending messages, need to check and cache w/ each request