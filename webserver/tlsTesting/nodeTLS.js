net = require('net')
tls = require('tls')
fs = require('fs')

const options = {
    key: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/privkey.pem"),
    cert: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/fullchain.pem"),
    ca: fs.readFileSync("/etc/letsencrypt/live/patricksproj.codes/chain.pem"),
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

    socket.on('error', function(err) {
        console.log("Got error: " + err);
    });


  });

server.listen(443, () => {
console.log('server bound');
});


// How to get keys on AWS: https://www.youtube.com/watch?v=uwYH83OFGTo