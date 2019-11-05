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
        private string symKeyAlias;
        public string[] users;
        public List<WabbMessage> messages;

        public Chat(){
            this.messages = new List<WabbMessage>();
        }

        public Chat(string JSON){
            JObject newChat = JObject.Parse(JSON);
            if (this.symKeyAlias != null) this.symKeyAlias = newChat["symKey"].ToString();
            this.chatId = newChat["chatId"].ToString();
            this.chatName = newChat["chatName"].ToString();
            this.users = newChat["users"].ToString().Split(',');

            this.messages = new List<WabbMessage>();
        }

        public string createJSONString(){
            return "{" + $"\"chatId\": \"{this.chatId}\", \"chatName\": \"{this.chatName}\"" + "}";
        }

        public void createSymKey()
        {
            symKeyAlias = chatName + "chat";
            SymmetricKeyHelper symHelper = new SymmetricKeyHelper(symKeyAlias);
            symHelper.CreateKey();
        }

        public string getSharableKey()
        {
            SymmetricKeyHelper symHelper = new SymmetricKeyHelper(symKeyAlias);
            return symHelper.GetKeyString();
        }

        public void loadChatKey(string chatSymKey)
        {
            SymmetricKeyHelper symHelper = new SymmetricKeyHelper(symKeyAlias);
            symHelper.LoadKey(chatSymKey);
        }

        public string encryptMessage(string messagePlaintext)
        {
            SymmetricKeyHelper symHelper = new SymmetricKeyHelper(symKeyAlias);
            return symHelper.EncryptDataToBytes(messagePlaintext).ToString();
        }

        public string decryptMessage(string messageEncrypted)
        {
            SymmetricKeyHelper symHelper = new SymmetricKeyHelper(symKeyAlias);
            return symHelper.DecryptData(messageEncrypted);
        }

    }
}