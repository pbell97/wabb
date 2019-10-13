net = require('net')
tls = require('tls')
fs = require('fs')


class TLSConnection {
    constructor(){
        // EVENTUALLY MAKE THESE FILE PATHS READ FROM A CONFIG FILE
        this.options = {
            key: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/privkey.pem"),
            cert: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/fullchain.pem"),
            ca: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/chain.pem"),
            rejectUnauthorized: false
        };

        this.sockets = [];
        this.messageCallbacks = [];
        this.disconnectCallbacks = [];
        this.server;
    }

    listen() {
        var thisClass = this;
        this.server = tls.createServer(this.options, (socket) => {
            console.log('Server connected to client: ',
                        socket.authorized ? 'authorized' : 'unauthorized');
            socket.setEncoding('utf8');
            thisClass.sockets.push(socket);
        
            socket.on('data', function(msgReceived){
                try{
                    var receivedJSON = JSON.parse(msgReceived);
                    for (var i = 0; i < thisClass.messageCallbacks.length; i++){
                        thisClass.messageCallbacks[i](socket, receivedJSON);
                    }
                    
                } catch (error) {
                    console.error(error);
                }
            });
        
            socket.on('end', function () {
                for (var i = 0; i < thisClass.disconnectCallbacks.length; i++){
                    thisClass.disconnectCallbacks[i]();
                }
                thisClass.sockets.splice(thisClass.sockets.indexOf(socket), 1);
            });
        
            socket.on('error', function(err) {
                console.log("Got error: " + err);
            });
        
        
        });

        this.server.listen(443, () => {
            console.log('Server Started');
        });
    }

    

}

module.exports = TLSConnection;

// ----------------- EXAMPLE USUAGE -----------------
// function printReceivedMessage(socket, messageJSON){
//     console.log("GOT: " + messageJSON.type);
//     socket.write("Thanks for the message - from funciton");
// }

// function printClientDisconnected(){
//     console.log("Client Disconnected -- from function");
// }

// const TLSCon = new TLSConnection();
// TLSCon.listen();
// TLSCon.messageCallbacks.push(printReceivedMessage)
// TLSCon.disconnectCallbacks.push(printClientDisconnected);
