var mysql = require('mysql');
const https = require('https');
var fs = require('fs');
var express = require('express');
var TLSConnection = require('./tlsTesting/nodeTLSClass.js');
const googleAudienceID = '154600092899-9tdri4c4k80lg38ak4bd9s6aro95s9nt.apps.googleusercontent.com';
// const googleAudienceID = '194187796125-1tk73jfb7ors490aj61ehh9kaos1ie5d.apps.googleusercontent.com'; // Kohler's
var tokenCache = {};

var serverConnection = new TLSConnection();


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
        case "signIn":
            signIn(socket, message.content);
            break;
        case "allowUserToJoinChat":
            allowUserToJoinChat(socket, message.content);
            break;
        case "getUsers":
            getUsers(socket, message.content);
            break;
        case "getChats":
            getChats(socket, message.content);
            break;
        case "getMyChats":
            getMyChats(socket, message.content);
            break;
        case "backupKeys":
            backupKeys(socket, message.content);
            break;
        case "restoreKeys":
            restoreKeys(socket, message.content);
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
                            sendMessageToRecipients(id, messageContent, chatId, messageId);
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
                    executeSQLQuery(`SELECT id AS user_id, messageId, messageContent, chatId FROM messages WHERE chatid = "${chatId}" AND messageId > ${messageId} ORDER BY messageId ASC;`, (error, results) =>{
                        if (error) throw error;
                        var response = {
                            "receivedMessage": results
                        };
                        socket.write(JSON.stringify(response));
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
    var pubKey = message.pubKey;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            // Check if user already exists
            userExists(data.user_id, (exists) => {
                if (exists){
                    socket.write(JSON.stringify({"response": "User " + data.user_id + " already exists"}));
                    return;
                }
                // Add user
                var result = executeSQLQuery("INSERT INTO users (username, email, id, pubKey) VALUES (\"" + username + "\",\"" + data.email + "\",\"" + data.user_id + "\", \"" + pubKey + "\");");
                var response = {"userCreated": {"access_id": access_id, "username": username, "user_id": data.user_id, "email": data.email, "pubKey": pubKey}};
                socket.write(JSON.stringify(JSON.stringify(response)));
                console.log("Created User:" + username);
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
                var response = {"requestToJoinChat": {"joinerId": joinerId, "pubKey": pubKey, "chatId": chatId}};
                var address = serverConnection.usersAndAddressPort[founderId];

                // Checks if users is connected, else put message in invites buffer
                if (address != undefined){
                    serverConnection.addressPortAndSocket[address].write(JSON.stringify(response));
                    console.log("Sending message : " + JSON.stringify(response) + "   to " + address);
                } else {
                    var query = `INSERT INTO invitesBuffer (destinationUser, messageToSend) VALUES ("${founderId}", '${JSON.stringify(response)}')`;
                    executeSQLQuery(query, (error, result)=>{
                        if (error) throw error;
                        console.log("Added message to invites buffer");
                    });
                }

            });
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });

}

function allowUserToJoinChat(socket, message){
    var chatId = message.chatId;
    var symKey = message.symKey;
    var access_id = message.access_id;
    var joinerId = message.joinerId;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var accepterId = data.user_id;
            var query = "SELECT founderId FROM chats WHERE chatId = \"" + chatId + "\";";
            executeSQLQuery(query, (error, result)=>{
                if (result[0]['founderId'] == accepterId){
                    // Add joinerId to DB
                    executeSQLQuery(`SELECT * FROM chats WHERE chatId = "${chatId}";`, (error, results) => {
                        if (error) throw error;
                        var users = results[0].users + "," + joinerId;
                        var chatName = results[0].chatName;
                        var chatUsers = results[0].users;

                        // If already added, don't re-add to list
                        if (!chatUsers.includes(joinerId)){
                            executeSQLQuery(`UPDATE chats SET users = "${users}" WHERE chatId = "${chatId}";`, (error, results) => {
                                if (error) throw error;
                            });
                        } 

                        // Message joinerId w/ symkey
                        var response = {"acceptedToChat": {"symKey": symKey, "chatId": chatId, "chatName": chatName, "users": chatUsers}};
                        var address = serverConnection.usersAndAddressPort[joinerId];

                        // Checks if users is connected, else put message in invites buffer
                        if (address != undefined){
                            serverConnection.addressPortAndSocket[address].write(JSON.stringify(response));
                            console.log("Sending message : " + JSON.stringify(response) + "   to " + address);
                        } else {
                            var query = `INSERT INTO invitesBuffer (destinationUser, messageToSend) VALUES ("${joinerId}", '${JSON.stringify(response)}')`;
                            executeSQLQuery(query, (error, result)=>{
                                if (error) throw error;
                                console.log("Added message to invites buffer: " + result);
                            });
                        } 
                    })
                }else {
                    socket.write(JSON.stringify({"error": "You are not the owner"}));
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
                var response = {"chats": results}
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

                var query = "SELECT * FROM chats WHERE chatId = \"" + chatId + "\";";
                executeSQLQuery(query, (error, results) => {
                    if (error) throw error;
                    var response = {"chatCreated": results[0]};
                    socket.write(JSON.stringify(response));
                });
            });
        }else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

function signIn(socket, message){
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var userId = data.user_id;
            serverConnection.addUser(socket, userId);

            // Returns user params
            var query = "SELECT * FROM users WHERE id = \"" + userId + "\";";
            executeSQLQuery(query, (error, results)=>{
                try {
                    if (results == undefined) return;
                    if (error) throw error;
                    var messageToSend = {
                        "signedIn" : {
                            "username": results[0].username,
                            "email": results[0].email,
                            "id": results[0].id,
                            "pubKey": results[0].pubKey
                        }
                    }
                    socket.write(JSON.stringify(messageToSend));
                    console.log("Client: " + messageToSend.signedIn.username);
                } catch (error) {
                    console.log("Had error in sign-in: " + error);
                }
            });


            // Check if any invites in buffer
            var query = "SELECT * FROM invitesBuffer WHERE destinationUser = \"" + userId + "\";";
            executeSQLQuery(query, (error, results)=>{
                if (results == undefined) return;
                for (var i = 0; i < results.length; i++){
                    var messageToSend = results[i].messageToSend;
                    socket.write(messageToSend);
                    // TODO: DELETE MESSAGE FROM BUFFER
                    var query = "DELETE FROM invitesBuffer WHERE destinationUser = \"" + userId + "\";";
                    executeSQLQuery(query, (error, results)=>{
                        if (error) throw error; 
                    });
                }
            });

        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

function getUsers(socket, message){
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var user_id = data.user_id;
            var query = "SELECT * FROM users;";
            executeSQLQuery(query, (error, results)=>{
                if (error) throw error;
                var response = {
                    "usersList": results
                };
                socket.write(JSON.stringify(response));
            })
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

function backupKeys(socket, message){
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var user_id = data.user_id;
            var backupData = message['backupData']
            var query = "SELECT username FROM users WHERE id = \"" + user_id + "\";";
            executeSQLQuery(query, (error, results)=>{
                if (error) throw error;
                var username = results[0].username;
                var query = "DELETE FROM keysBackup WHERE username = \"" + username + "\";";
                executeSQLQuery(query, (error, results)=>{
                    if (error) throw error;
                    var query = "INSERT INTO keysBackup (username, backupData) VALUES (\"" + username + "\", \"" + backupData + "\");";
                    executeSQLQuery(query, (error, results)=>{
                        if (error) throw error;
                        console.log("Backed up keys for " + username);
                    });
                });
            });
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

function restoreKeys(socket, message){
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var user_id = data.user_id;
            var query = "SELECT username FROM users WHERE id = \"" + user_id + "\";";
            executeSQLQuery(query, (error, results)=>{
                if (error) throw error;
                var username = results[0].username;
                var query = "SELECT backupData FROM keysBackup WHERE username = \"" + username + "\";";
                executeSQLQuery(query, (error, results)=>{
                    if (error) throw error;
                    socket.write(JSON.stringify({"restoreKeys": results[0].backupData}));
                    console.log("Returning keys to " + username);
                });
            });
            
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

function getChats(socket, message){
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var user_id = data.user_id;
            var query = "SELECT * FROM chats;";
            executeSQLQuery(query, (error, results)=>{
                if (error) throw error;
                var response = {
                    "chatsList": results
                };
                socket.write(JSON.stringify(response));
            })
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

function getMyChats(socket, message){
    var access_id = message.access_id;
    verifyTokenManually(access_id, (accepted, data) => {
        if (accepted){
            var user_id = data.user_id;
            var query = "SELECT * FROM chats WHERE users LIKE \"%" + user_id + "%\";";
            executeSQLQuery(query, (error, results)=>{
                if (error) throw error;
                var response = {
                    "myChatsList": results
                };
                socket.write(JSON.stringify(response));
            })
        } else {
            socket.write(JSON.stringify({"error": "Invalid Token"}));
        }
    });
}

function verifyTokenManually(access_id, callback){
    // Checks if token is cached instead of querying Google
    if (Object.keys(tokenCache).includes(access_id)){
        if (tokenCache[access_id].expireTime > new Date().getTime()){
            callback(true, tokenCache[access_id]);
            return;
        } else {
            delete tokenCache[access_id];
            callback(false, null);
            return;
        }
    }

    // Queries Google to see if token is valid
    var url = "https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=" + access_id;
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
                tokenCache[access_id] = data;
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
        if (results[0] != undefined) {
            var users = results[0]["users"].split(',');
            callback(users.includes(user_id));
            return;
        }
        console.log("Had a problem checking if user was in chat");
    });
}

function sendMessageToRecipients(senderId, message, chatId, messageId){
    var query = "SELECT users FROM chats WHERE chatid = \"" + chatId + "\";";
    executeSQLQuery(query, (error, result)=>{
        var users = result[0]['users'].split(',');
        var response = {
            "receivedMessage":[{
                "user_id": senderId,
                "chatId": chatId,
                "messageId": messageId,
                "messageContent": message
            }]
        }
        console.log(serverConnection.usersAndAddressPort);
        for (var i = 0; i < users.length; i++){
            // if (users[i] == senderId) continue;
            var addressPort = serverConnection.usersAndAddressPort[users[i]];
            if (addressPort == undefined) continue;
            console.log("Sending message to " + addressPort);
            serverConnection.addressPortAndSocket[addressPort].write(JSON.stringify(response));
        }
    });
}





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



// MUST Stop apache
//      sudo /opt/bitnami/ctlscript.sh stop
//      sudo /opt/bitnami/ctlscript.sh status       - To make sure it has stopped
//      Then make sure your server is running on port 443