net = require('net')
tls = require('tls')
fs = require('fs')

const options = {
    key: fs.readFileSync("/Users/patrickbell/Desktop/classes/infoAndCompSecurity/wabb/webserver/localhost-key.pem"),
    cert: fs.readFileSync("/Users/patrickbell/Desktop/classes/infoAndCompSecurity/wabb/webserver/localhost.pem"),
    rejectUnauthorized: false
};


// Keep a pool of sockets ready for everyone
var sockets = [];


const server = tls.createServer(options, (socket) => {
    console.log('server connected',
                socket.authorized ? 'authorized' : 'unauthorized');
    socket.write('welcome!');
    socket.setEncoding('utf8');
    // socket.pipe(socket);
    sockets.push(socket);

    socket.on('data', function(msg_sent){
        console.log("Got: " + msg_sent);
        try{
            socket.write("Thanks for the message: " + msg_sent);
        } catch (error) {
            console.error(error);
        }
    });

    socket.on('end', function () {
        console.log("Client disconnected");
    });

  });

server.listen(8000, () => {
console.log('server bound');
});




// // Create a TCP socket listener
// var s = net.Server(function (socket) {

//     // Add the new client socket connection to the array of sockets
//     sockets.push(socket);

//     // 'data' is an event that means that a message was just sent by the client application
//     socket.on('data', function (msg_sent) {
//         console.log("Get message: " + msg_sent);
//         socket.write("Thanks for the message: " + msg_sent);
//     });

//     // The 'end' event means tcp client has disconnected.
//     socket.on('end', function () {
//         var i = sockets.indexOf(socket);
//         sockets.splice(i, 1);
//     });


// });

// s.listen(8000);
// console.log('System waiting at http://localhost:8000');