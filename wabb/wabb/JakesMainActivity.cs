using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using wabb;
using wabb.Utilities;
using System;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Android.Content;

namespace Chat_UI
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    //[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class JakesMainActivity : AppCompatActivity
    {
        TLSConnector serverConnection;
        string access_id = "NoToken";
        string myAsymKeyPairAlias = "myKeyPair";   // ALWAYS PUT "+ mainUser.username" to the key
        string[] convoList = { "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat" };
        User mainUser;
        Dictionary<string, User> otherUsers = new Dictionary<string, User> { }; // username:user
        Dictionary<string, string> usernameIdMatches = new Dictionary<string, string> { };  //user_id:username
        Dictionary<string, Chat> myChats = new Dictionary<string, Chat> { };    // chatname:chat
        Dictionary<string, string> chatNameMatches = new Dictionary<string, string> { };    // chat_id:chatname

        string activeChatName = "";
        string activeChatId = "";
        string currentView = "";
        string suppliedRestorePassword = "";
        bool restoreKeysNow = false;

        // MESSAGE SCREEN
        void messageScreen(string chatName)
        {
            // If "Empty Chat", dont do anything
            if (!myChats.Keys.Contains<string>(chatName))
            {
                return;
            }

            if (!myChats[chatName].hasKey())
            {
                // TODO: Provide on screen error message
                Console.WriteLine("\nTried to go to chat without a sym key. Please restore...\n");
                restoreAccountScreen();
                return;
            }

            getNewMessages(myChats[chatName]);


            SetContentView(Resource.Layout.messages);
            currentView = "messageScreen";

            FindViewById<TextView>(Resource.Id.chatNameTitle).Text = chatName;
            activeChatName = chatName;
            activeChatId = myChats[chatName].chatId;

            // INSERTION OF MESSAGE CONTENTS
            string messages = "";

            int numOfMessages = myChats[chatName].messages.Count;
            for (int i = 0; i < numOfMessages; i++)
            {
                string username = usernameIdMatches[myChats[chatName].messages[i].user_id];
                string decryptedContents = myChats[chatNameMatches[myChats[chatName].messages[i].chatId]].decryptMessage(myChats[chatName].messages[i].messageContent);
                string messageToAdd = username + "\n\t" + decryptedContents + "\n";
                messages += messageToAdd;
            }

            // Display built string
            FindViewById<TextView>(Resource.Id.messageDisplay).Text = messages;

            // INPUT HANDLING
            // Getting message from field
            string newMessage = "";
            var inputField = FindViewById<EditText>(Resource.Id.newMessage);
            inputField.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                newMessage = e.Text.ToString();
            };

            // Send message from field
            var SendButton = FindViewById<Button>(Resource.Id.sendButton);
            var that = this;
            SendButton.Click += (sender, e) =>
            {
                // SEND MESSAGE TO RECVR
                that.sendChatMessage(activeChatId, newMessage);

                inputField.Text = "";
            };
            // Send back to conversation page
            var BackButton = FindViewById<Button>(Resource.Id.backButton);
            BackButton.Click += (sender, e) =>
            {
                convoScreen();
            };
            // Invite more users
            var inviteUsersButton = FindViewById<Button>(Resource.Id.inviteUsers);
            inviteUsersButton.Click += (sender, e) =>
            {
                inviteUsersScreen(chatName);
            };
        }

        // Invite users to a chat
        void inviteUsersScreen(string chatName)
        {
            SetContentView(Resource.Layout.inviteUsers);
            currentView = "inviteUsersScreen";

            var inviteUsersButton = FindViewById<Button>(Resource.Id.inviteUsers);
            inviteUsersButton.Click += (sender, e) =>
            {
                // Invite Users
                string[] usersToInvite = FindViewById<EditText>(Resource.Id.chatInvites).Text.Split(',');
                for (int i = 0; i < usersToInvite.Length; i++)
                {
                    inviteUsersToChat(chatName, usersToInvite[i]);
                }
                FindViewById<EditText>(Resource.Id.chatInvites).Text = "";
            };
            var backButton = FindViewById<Button>(Resource.Id.backButtonOnInviteUsers);
            backButton.Click += (sender, e) =>
            {
                messageScreen(chatName);
            };

        }

        // Conversation Screen
        void convoScreen(string username = null)
        {
            if (username == null) username = this.mainUser.username;
            if (restoreKeysNow)
            {
                restoreKeysRequest();
            }

            SetContentView(Resource.Layout.conversations);
            currentView = "convoScreen";

            // Saves new 
            saveChatsAndUsersToStorage();


            var createChatButton = FindViewById<Button>(Resource.Id.createNewChatButton);
            createChatButton.Click += (sender, e) =>
            {
                createChatScreen();
            };

            var backupDataButton = FindViewById<Button>(Resource.Id.backupDataButton);
            backupDataButton.Click += (sender, e) =>
            {
                backupAccountScreen();
            }; 

             // POPULATE CONVERSATION BUTTONS
             var button0 = FindViewById<Button>(Resource.Id.button0);
            var button1 = FindViewById<Button>(Resource.Id.button1);
            var button2 = FindViewById<Button>(Resource.Id.button2);
            var button3 = FindViewById<Button>(Resource.Id.button3);
            var button4 = FindViewById<Button>(Resource.Id.button4);
            var button5 = FindViewById<Button>(Resource.Id.button5);
            var button6 = FindViewById<Button>(Resource.Id.button6);
            var button7 = FindViewById<Button>(Resource.Id.button7);
            var button8 = FindViewById<Button>(Resource.Id.button8);

            Button[] buttons = { button0, button1, button2, button3, button4, button5, button6, button7, button8 };

            for (int i = 0; i < (convoList.Length); i++)
            {
                if (i == 0)
                {
                    button0.Text = convoList[i];
                }
                if (i == 1)
                {
                    button1.Text = convoList[i];
                }
                if (i == 2)
                {
                    button2.Text = convoList[i];
                }
                if (i == 3)
                {
                    button3.Text = convoList[i];
                }
                if (i == 4)
                {
                    button4.Text = convoList[i];
                }
                if (i == 5)
                {
                    button5.Text = convoList[i];
                }
                if (i == 6)
                {
                    button6.Text = convoList[i];
                }
                if (i == 7)
                {
                    button7.Text = convoList[i];
                }
                if (i == 8)
                {
                    button8.Text = convoList[i];
                }
            }

            // TODO: Make this scrollable
            string[] myChatkeys = myChats.Keys.Select(x => x.ToString()).ToArray();
            for (int i = 0; i < myChats.Count; i++)
            {
                if (i >= 9) continue;
                buttons[i].Text = myChats[myChatkeys[i]].chatName;
            }


            // INPUT HANDLING
            // Button0 Click Event
            button0.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 1) return;
                messageScreen(myChats[myChatkeys[0]].chatName);
            };
            // Button1 Click Event
            button1.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 2) return;
                messageScreen(myChats[myChatkeys[1]].chatName);
            };
            // Button2 Click Event
            button2.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 3) return;
                messageScreen(myChats[myChatkeys[2]].chatName);
            };
            // Button3 Click Event
            button3.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 4) return;
                messageScreen(myChats[myChatkeys[3]].chatName);
            };
            // Button4 Click Event
            button4.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 5) return;
                messageScreen(myChats[myChatkeys[4]].chatName);
            };
            // Button5 Click Event
            button5.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 6) return;
                messageScreen(myChats[myChatkeys[5]].chatName);
            };
            // Button6 Click Event
            button6.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 7) return;
                messageScreen(myChats[myChatkeys[6]].chatName);
            };
            // Button7 Click Event
            button7.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 8) return;
                messageScreen(myChats[myChatkeys[7]].chatName);
            };
            // Button8 Click Event
            button8.Click += (sender, e) =>
            {
                // Send to messages screen
                if (myChatkeys.Length < 9) return;
                messageScreen(myChats[myChatkeys[8]].chatName);
            };
        }

        // LOGIN SCREEN
        void loginScreen()
        {
            SetContentView(Resource.Layout.login);
            currentView = "loginScreen";

            string username = "";
            string password = "";
            var nameEntry = FindViewById<EditText>(Resource.Id.nameEntry);
            var passEntry = FindViewById<EditText>(Resource.Id.passEntry);
            var loginButton = FindViewById<Button>(Resource.Id.loginButton);

            // INPUT HANDLING
            // Username Entry Field
            nameEntry.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                username = e.Text.ToString();
            };
            // Password Entry Field
            passEntry.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                password = e.Text.ToString();
            };
            // Login Button Click Event
            loginButton.Click += (sender, e) =>
            {
                // SEND LOGIN AND VERIFY

                // Pass conversation list and username to convoScreen
                signInToServer();
                //convoScreen(username);
            };

        }

        // CREATE CHAT SCREEN
        void createChatScreen()
        {
            SetContentView(Resource.Layout.createChat);
            currentView = "createChatScreen";

            var chatNameField = FindViewById<EditText>(Resource.Id.chatNameField);
            var createChatButton = FindViewById<Button>(Resource.Id.createChatButton);
            createChatButton.Click += (sender, e) =>
            {
                createChat(chatNameField.Text);
                // Wait for chat creation?
            };
            // Send back to conversation page
            var BackButton = FindViewById<Button>(Resource.Id.backButtonOnCreateChat);
            BackButton.Click += (sender, e) =>
            {
                convoScreen();
            };
        }

        // Startup screen
        void startupScreen()
        {
            SetContentView(Resource.Layout.startScreen);
            currentView = "startScreen";

            var loginButton = FindViewById<Button>(Resource.Id.loginButtonOnStartScreen);
            loginButton.Click += (sender, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(MainActivity));
                nextActivity.PutExtra("next", "convoScreen");
                //nextActivity.PutExtra("signOut", "true");
                StartActivity(nextActivity);

                //loginScreen();
            };

            var createAccountButton = FindViewById<Button>(Resource.Id.createAccountButton);
            createAccountButton.Click += (sender, e) =>
            {
                Intent nextActivity = new Intent(this, typeof(MainActivity));
                nextActivity.PutExtra("next", "createAccountScreen");
                nextActivity.PutExtra("signOut", "true");
                StartActivity(nextActivity);

                //createAccountScreen();
            }; 

            var restoreButton = FindViewById<Button>(Resource.Id.restoreAccountStartButton);
            restoreButton.Click += (sender, e) =>
            {
                //TODO: Sign in first
                Intent nextActivity = new Intent(this, typeof(MainActivity));
                nextActivity.PutExtra("next", "restoreAccountScreen");
                nextActivity.PutExtra("signOut", "true");
                StartActivity(nextActivity);

                //restoreAccountScreen();
            }; 
        }

        void restoreAccountScreen()
        {
            SetContentView(Resource.Layout.restoreAccount);
            currentView = "restoreAccount";
            var retstoreButton = FindViewById<Button>(Resource.Id.restoreButton);
            retstoreButton.Click += (sender, e) =>
            {
                restoreKeysNow = true;
                suppliedRestorePassword = FindViewById<EditText>(Resource.Id.restorePassword).Text;
                signInToServer();
            };
        }

        void backupAccountScreen()
        {
            SetContentView(Resource.Layout.backupKeys);
            currentView = "backupKeys";

            var backupKeysButton = FindViewById<Button>(Resource.Id.backupKeysButton);
            backupKeysButton.Click += (sender, e) =>
            {
                suppliedRestorePassword = FindViewById<EditText>(Resource.Id.backupPasswordField).Text;
                backupKeysToServer();
                convoScreen();
            };
        }

        void createAccountScreen()
        {
            SetContentView(Resource.Layout.createAccount);
            currentView = "startScreen";

            var createAcc = FindViewById<Button>(Resource.Id.createUserButton);
            createAcc.Click += (sender, e) =>
            {
                // GOOGLE SIGN IN CODE
                // TODO: Get access code
                // TODO: Remove when get login working
                string username = FindViewById<EditText>(Resource.Id.username).Text;

                createUser(username);
            };

            var backButton = FindViewById<Button>(Resource.Id.backButton);
            backButton.Click += (sender, e) =>
            {
                startupScreen();
            };

        }

        // Sign in calls : signInToServer() -> getAllUsers() -> getMyChats() -> getNewMessages() -> convoScreen()


        // Sends sign-in message to server to log IP w/socket
        void signInToServer()
        {
            string message = "{\"access_id\": \"" + this.access_id + "\"}";
            serverConnection.WriteMessage("signIn", message);
        }

        // Creates mainUser and goes to convo screen
        void signInToServerResponse(object sender, EventArgs e)
        {
            int messageIndex = serverConnection.unreadMessages.Count - 1;
            if (messageIndex < 0) return;
            JObject message = JObject.Parse(serverConnection.unreadMessages[messageIndex]);
            string type = serverConnection.interpretMessageType(message);

            if (type == "signedIn")
            {
                User newUser = new User();
                newUser.username = message[type]["username"].ToString();
                newUser.email = message[type]["email"].ToString();
                newUser.user_id = message[type]["id"].ToString();
                newUser.pubKey = message[type]["pubKey"].ToString();

                // Load storage. If not same user, reset
                loadChatsAndUsersFromStorage();
                if (mainUser == null || newUser.user_id != mainUser.user_id)
                {
                    // Deletes all storage
                    SecureStorageHelper ssHelper = new SecureStorageHelper();
                    ssHelper.RemoveAllItems();

                    // Loads new users and resets memory of users&chats
                    mainUser = newUser;
                    otherUsers = new Dictionary<string, User> { }; // username:user
                    usernameIdMatches = new Dictionary<string, string> { };  //user_id:username
                    myChats = new Dictionary<string, Chat> { };    // chatname:chat
                    chatNameMatches = new Dictionary<string, string> { };    // chat_id:chatname

                    // Creates a new public key and updates server
                    AsymmetricKeyHelper akh = new AsymmetricKeyHelper(myAsymKeyPairAlias + mainUser.username);
                    akh.CreateKey();
                    string pubKey = akh.GetSharablePublicKey();
                    updatePubKey(pubKey);
                }


                // Post sign-in activities
                getAllUsers();
                getMyChats();
            }

            if (type == "noAccount")
            {
                RunOnUiThread(() =>
                {
                    createAccountScreen();
                });
            }
        }

        // Sends getUsers request to server
        void getAllUsers()
        {
            string message = "{\"access_id\": \"" + this.access_id + "\"}";
            serverConnection.WriteMessage("getUsers", message);
        }

        // Adds all users to otherUsers dictionary
        void getAllUsersResponse(object sender, EventArgs e)
        {
            int messageIndex = serverConnection.unreadMessages.Count - 1;
            if (messageIndex < 0) return;
            JObject message = JObject.Parse(serverConnection.unreadMessages[messageIndex]);
            string type = serverConnection.interpretMessageType(message);

            if (type == "usersList"){

                // Resets lists
                otherUsers = new Dictionary<string, User> { }; // username:user
                usernameIdMatches = new Dictionary<string, string> { };  //user_id:username


                JArray usersArray = (JArray)message[type];
                int length = usersArray.Count;
                Console.WriteLine("Length: " + length.ToString());
                User aUser;
                for (int i = 0; i < length; i++)
                {
                    aUser = new User(message[type][i].ToString());
                    otherUsers[aUser.username] = aUser;
                    usernameIdMatches[aUser.user_id] = aUser.username;
                }
            }
        }

        // Sends request to get all user's chats
        void getMyChats()
        {
            string message = "{\"access_id\": \"" + this.access_id + "\"}";
            serverConnection.WriteMessage("getMyChats", message);
        }

        // Adds all users to otherUsers dictionary
        void getMyChatsResponse(object sender, EventArgs e)
        {
            int messageIndex = serverConnection.unreadMessages.Count - 1;
            if (messageIndex < 0) return;
            JObject message = JObject.Parse(serverConnection.unreadMessages[messageIndex]);
            string type = serverConnection.interpretMessageType(message);

            if (type == "myChatsList")
            {

                // Needs to compare with current list im memory?

                JArray chatsArray = (JArray)message[type];
                int length = chatsArray.Count;
                Console.WriteLine("Length: " + length.ToString());
                Chat aChat;

                var existingChatNames = myChats.Keys;

                // Parses chat. If not already saved, adds it
                for (int i = 0; i < length; i++)
                {
                    aChat = new Chat(message[type][i].ToString());
                    if (!existingChatNames.Contains<string>(aChat.chatName))
                    {
                        myChats[aChat.chatName] = aChat;
                        chatNameMatches[aChat.chatId] = aChat.chatName;
                    }
                }

                // Get new message for all chats
                foreach (KeyValuePair<string, Chat> entry in myChats) {
                    getNewMessages(entry.Value);
                }

                RunOnUiThread(() =>
                {
                    convoScreen();
                });

            }
        }

        // Requests new messages for a chat
        void getNewMessages(Chat requestingChat) 
        {
            int lastMessageIndex = requestingChat.messages.Count - 1;
            int lastMessageId = 0;
            if (lastMessageIndex != -1) { 
                lastMessageId = requestingChat.messages[lastMessageIndex].messageId;
            }
            string message = "{\"access_id\": \"" + this.access_id + "\", \"chatId\": \"" + requestingChat.chatId + "\", \"messageId\": \"" + lastMessageId.ToString() + "\"}";
            serverConnection.WriteMessage("messageGet", message);
        }

        // Adds new messages to appropriate chat objects, updates screen if neccessary
        void proccessNewMessages(object sender, EventArgs e)
        {
            int messageIndex = serverConnection.unreadMessages.Count - 1;
            if (messageIndex < 0) return;
            JObject message = JObject.Parse(serverConnection.unreadMessages[messageIndex]);
            string type = serverConnection.interpretMessageType(message);

            if (type == "receivedMessage")
            {
                JArray messagesArray = (JArray)message["receivedMessage"];
                int length = messagesArray.Count;
                Console.WriteLine("Length: " + length.ToString());
                WabbMessage newMessage;
                for (int i = 0; i < length; i++)
                {
                    newMessage = new WabbMessage(message["receivedMessage"][i].ToString());
                    string chatName = chatNameMatches[newMessage.chatId];
                    myChats[chatName].messages.Add(newMessage);

                    // If message is received while viewing the chat
                    if (activeChatId == newMessage.chatId && currentView == "messageScreen")
                    {
                        // Gets username unless its a new user since you logged on, then
                        // goes back to convo screen and gets new users.
                        string username = "";
                        try
                        {
                            username = usernameIdMatches[newMessage.user_id];
                        } catch (Exception)
                        {
                            Console.WriteLine("User did not exist...getting new list of users now");
                            getAllUsers();
                            convoScreen();
                            return;
                        }
                        string decryptedContents = myChats[chatNameMatches[newMessage.chatId]].decryptMessage(newMessage.messageContent);
                        string messageToAdd = username + "\n\t" + decryptedContents + "\n";


                        RunOnUiThread(() =>
                        {
                            FindViewById<TextView>(Resource.Id.messageDisplay).Text = FindViewById<TextView>(Resource.Id.messageDisplay).Text + messageToAdd;
                            var t = FindViewById<LinearLayout>(Resource.Id.linearLayout2);
                        });
                        // TODO: Scroll textbox down
                    }
                }                
            }
        }

        // Sends a message to the server to post in the chat
        void sendChatMessage(string chatId, string messageContent)
        {
            if (messageContent == "") return;
            // Encrypts message with chat's sym key
            messageContent = myChats[chatNameMatches[chatId]].encryptMessage(messageContent);
            string message = "{\"access_id\": \"" + this.access_id + "\", \"chatId\": \"" + chatId + "\", \"messageContent\": \"" + messageContent + "\"}";
            serverConnection.WriteMessage("messagePost", message);
        }

        void createChat(string chatName)
        {
            string message = "{\"access_id\": \"" + this.access_id + "\", \"chatName\": \"" + chatName + "\"}";
            serverConnection.WriteMessage("createChat", message);
        }

        // Adds newly created chat to our list
        void proccessNewChat(object sender, EventArgs e)
        {
            int messageIndex = serverConnection.unreadMessages.Count - 1;
            if (messageIndex < 0) return;
            JObject message = JObject.Parse(serverConnection.unreadMessages[messageIndex]);
            string type = serverConnection.interpretMessageType(message);

            if (type == "chatCreated")
            {
                // Add new chat to our list
                Chat aChat = new Chat(message[type].ToString());

                // Creates sym key
                aChat.createSymKey();

                myChats[aChat.chatName] = aChat;
                chatNameMatches[aChat.chatId] = aChat.chatName;

                // Invite Users
                string[] usersToInvite = FindViewById<EditText>(Resource.Id.chatInvites).Text.Split(',');
                for (int i = 0; i < usersToInvite.Length; i++)
                {
                    if (usersToInvite[i] == "") continue;
                    inviteUsersToChat(aChat.chatName, usersToInvite[i]);
                }


                RunOnUiThread(() =>
                {
                    convoScreen();
                });
            }

            if (type == "acceptedToChat")
            {
                Chat aChat = new Chat(message[type].ToString());
                string givenSymKey = message[type]["symKey"].ToString();

                // Decrypts encrypted symkey
                AsymmetricKeyHelper asymKeyHelper = new AsymmetricKeyHelper(myAsymKeyPairAlias + mainUser.username);
                var symKey = asymKeyHelper.DecryptDataFromString(givenSymKey);

                aChat.loadChatKey(symKey);
                myChats[aChat.chatName] = aChat;
                chatNameMatches[aChat.chatId] = aChat.chatName;
                getNewMessages(aChat);

                if (currentView == "convoScreen")
                {
                    RunOnUiThread(() =>
                    {
                        convoScreen();
                    });
                }
            }
        }

        void inviteUsersToChat(string chatName, string username)
        {
            string chatId = myChats[chatName].chatId;
            string joinerId = otherUsers[username].user_id;
            string joinerPubKey = otherUsers[username].pubKey;
            string symKeyEncrypted = myChats[chatName].getSharableKey();

            // User's pub key is used to encrypt symmetric key
            AsymmetricKeyHelper asymKeyHelper = new AsymmetricKeyHelper("otherUserKey");
            symKeyEncrypted = asymKeyHelper.EncryptWithAnotherPublicKey(symKeyEncrypted, joinerPubKey);

            string message = "{\"access_id\": \"" + this.access_id + "\", \"chatId\": \"" + chatId + "\", \"symKey\": \"" + symKeyEncrypted + "\", \"joinerId\": \"" + joinerId + "\"}";
            serverConnection.WriteMessage("allowUserToJoinChat", message);

        }

        void createUser(string username)
        {
            AsymmetricKeyHelper akh = new AsymmetricKeyHelper(myAsymKeyPairAlias + username);
            akh.CreateKey();
            string pubKey = akh.GetSharablePublicKey();

            string message = "{\"access_id\": \"" + this.access_id + "\", \"username\": \"" + username + "\", \"pubKey\": \"" + pubKey + "\"}";
            serverConnection.WriteMessage("createUser", message);
        }

        void proccessCreatedUser(object sender, EventArgs e)
        {
            int messageIndex = serverConnection.unreadMessages.Count - 1;
            if (messageIndex < 0) return;
            JObject message = JObject.Parse(serverConnection.unreadMessages[messageIndex]);
            string type = serverConnection.interpretMessageType(message);

            if (type == "userCreated")
            {
                access_id = message[type]["access_id"].ToString();
                signInToServer();
            }
        }

        void saveChatsAndUsersToStorage()
        {
            SecureStorageHelper storageHelper = new SecureStorageHelper();
            storageHelper.StoreItem<Dictionary<string, Chat>>("myChats", myChats);
            storageHelper.StoreItem<Dictionary<string, string>>("chatNameMatches", chatNameMatches);
            storageHelper.StoreItem<User>("mainUser", mainUser);
            // Not saving users incase pubkey is updated.
        }

        void loadChatsAndUsersFromStorage()
        {
            SecureStorageHelper storageHelper = new SecureStorageHelper();
            myChats = storageHelper.GetItem<Dictionary<string, Chat>>("myChats");
            chatNameMatches = storageHelper.GetItem<Dictionary<string, string>>("chatNameMatches");
            mainUser = storageHelper.GetItem<User>("mainUser");
        }

        void backupKeysToServer()
        {
            // Creates key and encrypts w/ password
            PasswordBasedKeyHelper passHelper = new PasswordBasedKeyHelper("restoration" + mainUser.user_id);
            passHelper.CreateKey(suppliedRestorePassword, mainUser.email);
            suppliedRestorePassword = "";

            // Creates dict of chatname:symkey
            Dictionary<string, string> keysBackup = new Dictionary<string, string>();
            var chatKeyValue = myChats.Keys.ToArray<string>();
            for (int i = 0; i < chatKeyValue.Length; i++)
            {
                if (!myChats[chatKeyValue[i]].hasKey()) continue;
                keysBackup[chatKeyValue[i]] = myChats[chatKeyValue[i]].getSharableKey();
            }

            // Serializes the keys and encrypts them
            var serializedKeyPairs = JsonConvert.SerializeObject(keysBackup);
            var encryptedSerializedKeyPairs = passHelper.EncryptDataToString(serializedKeyPairs);

            string message = "{\"access_id\": \"" + this.access_id + "\", \"username\": \"" + mainUser.username + "\", \"backupData\": \"" + encryptedSerializedKeyPairs + "\"}";
            serverConnection.WriteMessage("backupKeys", message);

        }

        void restoreKeysRequest()
        {
            string message = "{\"access_id\": \"" + this.access_id + "\"}";
            serverConnection.WriteMessage("restoreKeys", message);
            restoreKeysNow = false;
        }

        void restoreKeysResponse(object sender, EventArgs e)
        {
            int messageIndex = serverConnection.unreadMessages.Count - 1;
            if (messageIndex < 0) return;
            JObject message = JObject.Parse(serverConnection.unreadMessages[messageIndex]);
            string type = serverConnection.interpretMessageType(message);

            if (type == "restoreKeys")
            {
                // Gets data
                string encryptedSerializedKeys = message[type].ToString();

                // Creates password helper. suppliedRestorePassword must be supplied
                PasswordBasedKeyHelper passHelper = new PasswordBasedKeyHelper("restoration" + mainUser.user_id);
                passHelper.CreateKey(suppliedRestorePassword, mainUser.email);

                // Deletes from memory
                suppliedRestorePassword = "";

                // Deserialies and decryptes the keys
                string decryptedSerializedKeys = passHelper.DecryptDataFromString(encryptedSerializedKeys);
                Dictionary<string, string> chatsKeysPair = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedSerializedKeys);

                // Puts keys into chat objects
                var chatNames = chatsKeysPair.Keys.ToArray<string>();
                for (int i = 0; i < chatNames.Length; i++)
                {
                    var name = chatNames[i];
                    var symKey = chatsKeysPair[name];
                    myChats[name].loadChatKey(symKey);
                }

                // Saves to storage
                saveChatsAndUsersToStorage();
            }
        }

        void updatePubKey(string publicKey)
        {
            string message = "{\"access_id\": \"" + this.access_id + "\", \"pubKey\": \"" + publicKey + "\"}";
            serverConnection.WriteMessage("updateUserKey", message);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //SecureStorageHelper secStor = new SecureStorageHelper();
            //secStor.RemoveAllItems();
            //AsymmetricKeyHelper akh = new AsymmetricKeyHelper(myAsymKeyPairAlias + mainUser.username);
            //akh.DeleteKey();

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            var nextScreen = Intent.GetStringExtra("next");


            if (nextScreen != null)
            {
                Console.WriteLine("\nABOUT TO CONNECT\n");
                serverConnection = new TLSConnector();
                serverConnection.OnMessageReceived += new EventHandler(signInToServerResponse);
                serverConnection.OnMessageReceived += new EventHandler(getAllUsersResponse);
                serverConnection.OnMessageReceived += new EventHandler(getMyChatsResponse);
                serverConnection.OnMessageReceived += new EventHandler(proccessNewMessages);
                serverConnection.OnMessageReceived += new EventHandler(proccessNewChat);
                serverConnection.OnMessageReceived += new EventHandler(proccessCreatedUser);
                serverConnection.OnMessageReceived += new EventHandler(restoreKeysResponse);

                // Starts connection to server on new thread
                ThreadStart connectionThreadRef = new ThreadStart(serverConnection.Connect);
                Thread connectionThread = new Thread(connectionThreadRef);
                connectionThread.Start();
            }

            // Starts the login screen
            //loginScreen();

            access_id = Intent.GetStringExtra("token");
            if (nextScreen == "convoScreen")
            {
                Thread.Sleep(5000);
                signInToServer();
                return;
            }
            else if (nextScreen == "createAccountScreen")
            {
                createAccountScreen();
                return;
            } else if (nextScreen == "restoreAccountScreen")
            {
                restoreAccountScreen();
                return;
            }



            startupScreen();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}