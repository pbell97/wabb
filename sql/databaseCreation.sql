CREATE DATABASE infoSec;
USE infoSec;

CREATE TABLE users (username varchar(25), email varchar(35), id char(32));
CREATE TABLE messages (id char(32), chatId varchar(32), messageId int, messageContent varchar(250));
CREATE TABLE chats (chatId varchar(32), chatName varchar(100), founderId varChar(32), users TEXT);

CREATE USER 'testUser'@'%' IDENTIFIED WITH mysql_native_password BY 'newpassword';
GRANT ALL PRIVILEGES ON infoSec.* to 'testUser'@'%';

INSERT INTO users (username, email, id) VALUES ("pbell", "pjb183@msstate.edu", "CC946988938A2944C4985A8027CF7B52");