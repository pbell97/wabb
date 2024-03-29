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

namespace wabb.Utilities
{
    public class WabbMessage
    {
        public string access_id;
        public string chatId;
        public string messageContent;
        public string user_id;
        public int messageId;

        public WabbMessage(){

        }

        public WabbMessage(string JSON){
            JObject newMessage = JObject.Parse(JSON);
            if (newMessage["access_id"] != null) this.access_id = newMessage["access_id"].ToString();
            this.chatId = newMessage["chatId"].ToString();
            this.messageContent = newMessage["messageContent"].ToString();
            this.user_id = newMessage["user_id"].ToString();
            this.messageId = Int32.Parse(newMessage["messageId"].ToString());
        }

        public string createJSONString(){
            if (this.access_id != null){
                return "{" + $"\"access_id\": \"{this.access_id}\", \"chatId\": \"{this.chatId}\", \"messageContent\": \"{this.messageContent}\", \"user_id\": \"" + this.user_id + "\", \"messageId\": \"" + this.messageId + "\"}";
            } else {
                return "{" + $"\"chatId\": \"{this.chatId}\", \"messageContent\": \"{this.messageContent}\", \"user_id\": \"" + this.user_id + "\", \"messageId\": \"" + this.messageId + "\"}";
            }
        }


    }
}