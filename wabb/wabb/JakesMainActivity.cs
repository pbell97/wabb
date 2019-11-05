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

namespace Chat_UI
{
    //[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        TLSConnector serverConnection;
        string access_id = "ya29.ImGvB1v-AyXAA-zSrKxGiVhMZm960BiVEL84xfaMZi5bvMAHSNMtnJQ-H08Knm1PfflQsDHLjo-jCMXkEnbH6KBpMeMJlt9L8uZKgtH_Vo7nRA6S5auFGiSZB7o29ZGOCgtr";
        string[] convoList = { "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat" };
        User mainUser;
        Dictionary<string, User> otherUsers = new Dictionary<string, User> { }; // username:user
        Dictionary<string, string> usernameIdMatches = new Dictionary<string, string> { };  //user_id:username
        Dictionary<string, Chat> myChats = new Dictionary<string, Chat> { };    // chatname:chat
        Dictionary<string, string> chatNameMatches = new Dictionary<string, string> { };    // chat_id:chatname

        string activeChatName = "";
        string activeChatId = "";
        string currentView = "";

        // MESSAGE SCREEN
        void messageScreen(string chatName)
        {
            string Sender = "SENDER";
            string Recvr = "RECVR";
            SetContentView(Resource.Layout.messages);
            currentView = "messageScreen";


            activeChatName = chatName;
            activeChatId = myChats[chatName].chatId;

            // INSERTION OF MESSAGE CONTENTS
            string[] Conversation = new string[]{ Sender, "Kill me please lol", Recvr, "Nah fam sorry I am busy" };
            string messages = "";
            int messageListLen = Conversation.Length;

            int numOfMessages = myChats[chatName].messages.Count;
            for (int i = 0; i < numOfMessages; i++)
            {
                string username = usernameIdMatches[myChats[chatName].messages[i].user_id];
                string messageToAdd = username + "\n\t" + myChats[chatName].messages[i].messageContent + "\n";
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
                convoScreen(Sender);
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

            SetContentView(Resource.Layout.conversations);
            currentView = "convoScreen";

            // Saves new 
            saveChatsAndUsersToStorage();


            var createChatButton = FindViewById<Button>(Resource.Id.createNewChatButton);
            createChatButton.Click += (sender, e) =>
            {
                createChatScreen();
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
                messageScreen(myChats[myChatkeys[0]].chatName);
            };
            // Button1 Click Event
            button1.Click += (sender, e) =>
            {
                // Send to messages screen
                messageScreen(myChats[myChatkeys[1]].chatName);
            };
            // Button2 Click Event
            button2.Click += (sender, e) =>
            {
                // Send to messages screen
                messageScreen(myChats[myChatkeys[2]].chatName);
            };
            // Button3 Click Event
            button3.Click += (sender, e) =>
            {
                // Send to messages screen
                messageScreen(myChats[myChatkeys[3]].chatName);
            };
            // Button4 Click Event
            button4.Click += (sender, e) =>
            {
                // Send to messages screen
                messageScreen(myChats[myChatkeys[4]].chatName);
            };
            // Button5 Click Event
            button5.Click += (sender, e) =>
            {
                // Send to messages screen
                messageScreen(myChats[myChatkeys[5]].chatName);
            };
            // Button6 Click Event
            button6.Click += (sender, e) =>
            {
                // Send to messages screen
                messageScreen(myChats[myChatkeys[6]].chatName);
            };
            // Button7 Click Event
            button7.Click += (sender, e) =>
            {
                // Send to messages screen
                messageScreen(myChats[myChatkeys[7]].chatName);
            };
            // Button8 Click Event
            button8.Click += (sender, e) =>
            {
                // Send to messages screen
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

            var loginButton = FindViewById<Button>(Resource.Id.loginButton);
            loginButton.Click += (sender, e) =>
            {
                loginScreen();
            };

            var createAccountButton = FindViewById<Button>(Resource.Id.createAccountButton);
            createAccountButton.Click += (sender, e) =>
            {
                createAccountScreen();
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
                access_id = "ya29.ImCvB6VbczgIvH8uP3FVXS7fPuzGyf9M3LLzbDfF2yUBVcy2hKLMkvNumUZNyRVnKSqm-E4kAPNND1zTkWZKvoCY-ew8FsBj0ywd0APSWrv4KscyeScTp9xLLGUvabVThS0";

                string username = FindViewById<EditText>(Resource.Id.username).Text;
                string restorationPassword = FindViewById<EditText>(Resource.Id.restorationPassword).Text;

                // TODO: generate pubkey
                string pubKey = "testPubKey";


                createUser(username, pubKey);
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
                    mainUser = newUser;
                    otherUsers = new Dictionary<string, User> { }; // username:user
                    usernameIdMatches = new Dictionary<string, string> { };  //user_id:username
                    myChats = new Dictionary<string, Chat> { };    // chatname:chat
                    chatNameMatches = new Dictionary<string, string> { };    // chat_id:chatname
                }


                // Post sign-in activities
                getAllUsers();
                getMyChats();
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

                // Needs to compare with current list saved in memory

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
                        string username = usernameIdMatches[newMessage.user_id];
                        string messageToAdd = username + "\n\t" + newMessage.messageContent + "\n";
                        FindViewById<TextView>(Resource.Id.messageDisplay).Text = FindViewById<TextView>(Resource.Id.messageDisplay).Text + messageToAdd;
                        // TODO: Scroll textbox down
                    }
                }                
            }
        }

        // Sends a message to the server to post in the chat
        void sendChatMessage(string chatId, string messageContent)
        {
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

                // TODO: CREATE SYMKEY
                aChat.createSymKey();

                myChats[aChat.chatName] = aChat;
                chatNameMatches[aChat.chatId] = aChat.chatName;

                // Invite Users
                string[] usersToInvite = FindViewById<EditText>(Resource.Id.chatInvites).Text.Split(',');
                for (int i = 0; i < usersToInvite.Length; i++)
                {
                    inviteUsersToChat(aChat.chatName, usersToInvite[i]);
                }

                
                

                RunOnUiThread(() =>
                {
                    convoScreen();
                });
            }
        }

        void inviteUsersToChat(string chatName, string username)
        {
            string chatId = myChats[chatName].chatId;
            string joinerId = otherUsers[username].user_id;
            string joinerPubKey = otherUsers[username].pubKey;
            string symKeyEncrypted = myChats[chatName].getSharableKey();

            // TODO: Encrypt symkey with pub key

            string message = "{\"access_id\": \"" + this.access_id + "\", \"chatId\": \"" + chatId + "\", \"symKey\": \"" + symKeyEncrypted + "\", \"joinerId\": \"" + joinerId + "\"}";
            serverConnection.WriteMessage("allowUserToJoinChat", message);

        }

        void createUser(string username)
        {
            AsymmetricKeyHelper akh = new AsymmetricKeyHelper("myKeyPair");
            akh.CreateKey();
            string pubKey = akh.GetPublicKeyString();

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
            storageHelper.StoreItem<Dictionary<string, User>>("otherUsers", otherUsers);
            storageHelper.StoreItem<Dictionary<string, string>>("usernameIdMatches", usernameIdMatches);
            storageHelper.StoreItem<Dictionary<string, Chat>>("myChats", myChats);
            storageHelper.StoreItem<Dictionary<string, string>>("chatNameMatches", chatNameMatches);
            storageHelper.StoreItem<User>("mainUser", mainUser);

        }

        void loadChatsAndUsersFromStorage()
        {
            SecureStorageHelper storageHelper = new SecureStorageHelper();
            otherUsers = storageHelper.GetItem<Dictionary<string, User>>("otherUsers");
            usernameIdMatches = storageHelper.GetItem<Dictionary<string, string>>("usernameIdMatches");
            myChats = storageHelper.GetItem<Dictionary<string, Chat>>("myChats");
            chatNameMatches = storageHelper.GetItem<Dictionary<string, string>>("chatNameMatches");
            mainUser = storageHelper.GetItem<User>("mainUser");
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            //this.convoList 
            serverConnection = new TLSConnector();
            serverConnection.OnMessageReceived += new EventHandler(signInToServerResponse); 
            serverConnection.OnMessageReceived += new EventHandler(getAllUsersResponse);
            serverConnection.OnMessageReceived += new EventHandler(getMyChatsResponse);
            serverConnection.OnMessageReceived += new EventHandler(proccessNewMessages); 
            serverConnection.OnMessageReceived += new EventHandler(proccessNewChat); 
            serverConnection.OnMessageReceived += new EventHandler(proccessCreatedUser); 

            // Starts connection to server on new thread
            ThreadStart connectionThreadRef = new ThreadStart(serverConnection.Connect);
            Thread connectionThread = new Thread(connectionThreadRef);
            connectionThread.Start();

            // Starts the login screen
            //loginScreen();
            startupScreen();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}