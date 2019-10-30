# wabb
Whats App But Better


Needed Keys:
- 1 Asymmetric key pair: for passing symmetric chat keys. We keep the private key, public key is posted with our user when we create our account.
- n number of symmetric keys: for when we create a chat and have to assign a symmetric key to it.

Storage:
- Our private key (from above).
- Symmetric keys for all chats.
- Data (messages, chat metadata, etc)

Need to be able to encrypt all our keys (sym and asym) with a password and send it up to our server for 'backup' so we can pull it down later and restore keys to a new device.
