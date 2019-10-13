using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Threading;

using Newtonsoft;
using Newtonsoft.Json.Linq;


// Make a message buffer that it adds messages too when it receives them. 
// Have the while loop be in a seperate thread.
// Use C# events

namespace tlsTesting
{
    public class Message
    {
        public string access_id;
        public string chatId;
        public string messageContent;
        public string user_id;

        public Message(){

        }

        public Message(string JSON){
            JObject newMessage = JObject.Parse(JSON);
            this.access_id = newMessage["access_id"].ToString();
            this.chatId = newMessage["chatId"].ToString();
            this.messageContent = newMessage["messageContent"].ToString();
            this.user_id = newMessage["user_id"].ToString();
        }

        public string createJSONString(){
            return "{" + $"\"access_id\": \"{this.access_id}\", \"chatId\": \"{this.chatId}\", \"messageContent\": \"{this.messageContent}\"" + "}";
        }


    }
}