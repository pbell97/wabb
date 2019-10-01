# wabb Notes
Users log in using Google (and Microsoft) api
Each device has a public/private key pair
Each chat is encrypted by its own symmetric key
Each user's bag of chat keys is encrypted with user-made password

### Logging in
Sign in using Google (or Microsoft) api
Receive token
Confirm user by token email and id 
Request server for messages related to user since XX time

### Joining chat
Request to join chat
Group owner confirms request
Owner sends chat symm key, encrypted in recipient device public key

### Storing user data
User's settings and chat keys are stored in JSON bag
Bag is encrypted with user-made password
Bag's contents is updated when explicitly updated by user
User must type password
New bag will fully overwrite old bag

### Storing messages
Message content is encrypted with chat key
Unencrypted header with chat ID and time
Server database stores chat ID, time, contents, and message ID 

