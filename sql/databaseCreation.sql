CREATE DATABASE infoSec;
USE infoSec;

CREATE TABLE users (username varchar(25), email varchar(35), id char(32), pubKey varchar(128));
CREATE TABLE messages (id char(32), chatId varchar(32), messageId int, messageContent varchar(250));
CREATE TABLE chats (chatId varchar(32), chatName varchar(100), founderId varChar(32), users TEXT);
CREATE TABLE invitesBuffer (destinationUser varchar(32), messageToSend varchar(500));

CREATE USER 'testUser'@'%' IDENTIFIED WITH mysql_native_password BY 'newpassword';
GRANT ALL PRIVILEGES ON infoSec.* to 'testUser'@'%';

INSERT INTO users (username, email, id, pubKey) VALUES ("PatricksMSU", "pjb183@msstate.edu", "114493981662126316478", "testPubForMSU");
INSERT INTO users (username, email, id, pubKey) VALUES ("PatricksGMail", "mewingfugur@gmail.com", "113467843674295288430", "testPubForGMail");

INSERT INTO chats (chatId, chatName, founderId, users) VALUES ("2136482312", "FirstChat", "114493981662126316478", "114493981662126316478,113467843674295288430");
INSERT INTO chats (chatId, chatName, founderId, users) VALUES ("-1060544732", "myChat", "114493981662126316478", "114493981662126316478");

-- INSERT INTO messages(id, chatId, messageId, messageContent) VALUES ("114493981662126316478", "2136482312", 1, "FirstMessage");
-- INSERT INTO messages(id, chatId, messageId, messageContent) VALUES ("113467843674295288430", "2136482312", 2, "SecoondMessage");