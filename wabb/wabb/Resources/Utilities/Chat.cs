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


namespace wabb.Utilities
{
    public class Chat
    {
        public string chatId;
        public string chatName;
        public string symKey;
        public string[] users;

        public Chat(){

        }

        public Chat(string JSON){
            JObject newChat = JObject.Parse(JSON);
            if (this.symKey != null) this.symKey = newChat["symKey"].ToString();
            this.chatId = newChat["chatId"].ToString();
            this.chatName = newChat["chatName"].ToString();
            this.users = newChat["users"].ToString().Split(',');
        }

        public string createJSONString(){
            return "{" + $"\"chatId\": \"{this.chatId}\", \"chatName\": \"{this.chatName}\"" + "}";
        }

    }
}