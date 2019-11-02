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
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        TLSConnector serverConnection;
        string access_id = "ya29.ImGvB2mgWUfgN6L8qWBkmK_9_Suj7GEAhl1u9i_2msGTQDbjDOkTk8uF7Ah3H6KbMpKxHmclVRbjxP1KFxa147cG5hyEUC6m6jAEAyr5dE71K_3I9g9-GsJp2OTCZNxNZpLb";
        string[] convoList = { "Patrick", "Spencer", "Kohler", "Dylan", "Jonathan", "Empty Chat", "Empty Chat", "Empty Chat", "Empty Chat" };
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
            


            // Build conversation string from message senders/receivers and their conversation
            for (int i = 0; i < (messageListLen); i++)
            {
                if (i % 2 == 0)
                {
                    messages = messages + Conversation[i] + "\n\t";
                }
                else
                {
                    messages = messages + Conversation[i] + "\n";
                }
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
                //string sendMessage = newMessage;
                //messages = messages + Sender + "\n\t" + newMessage + "\n";
                //FindViewById<TextView>(Resource.Id.messageDisplay).Text = messages;

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
        }

        // Conversation Screen
        void convoScreen(string username = null)
        {
            if (username == null) username = this.mainUser.username;

            SetContentView(Resource.Layout.conversations);
            currentView = "convoScreen";


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
                this.mainUser = new User();
                mainUser.username = message[type]["username"].ToString();
                mainUser.email = message[type]["email"].ToString();
                mainUser.user_id = message[type]["id"].ToString();
                mainUser.pubKey = message[type]["pubKey"].ToString();

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
                JArray chatsArray = (JArray)message[type];
                int length = chatsArray.Count;
                Console.WriteLine("Length: " + length.ToString());
                Chat aChat;
                for (int i = 0; i < length; i++)
                {
                    aChat = new Chat(message[type][i].ToString());
                    myChats[aChat.chatName] = aChat;
                    chatNameMatches[aChat.chatId] = aChat.chatName;
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
                        string username = usernameIdMatches[myChats[chatName].messages[i].user_id];
                        string messageToAdd = username + "\n\t" + newMessage.messageContent + "\n";
                        FindViewById<TextView>(Resource.Id.messageDisplay).Text = FindViewById<TextView>(Resource.Id.messageDisplay).Text + messageToAdd;
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

            // Starts connection to server on new thread
            ThreadStart connectionThreadRef = new ThreadStart(serverConnection.Connect);
            Thread connectionThread = new Thread(connectionThreadRef);
            connectionThread.Start();

            // Starts the login screen
            loginScreen();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}