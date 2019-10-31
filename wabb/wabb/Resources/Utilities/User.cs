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
    public class User
    {
        public string access_id;
        public string username;
        public string user_id;
        public string email;
        public string pubKey;

        public User(){

        }

        public User(string JSON){
            JObject newMessage = JObject.Parse(JSON);
            if (newMessage["access_id"] != null) this.access_id = newMessage["access_id"].ToString();
            this.username = newMessage["username"].ToString();
            this.user_id = newMessage["id"].ToString();
            this.email = newMessage["email"].ToString();
            this.pubKey = newMessage["pubKey"].ToString();
        }

        public string createJSONString(){
            return "{" + $"\"access_id\": \"{this.access_id}\", \"username\": \"{this.username}\", \"user_id\": \"{user_id}\", \"email\": \"{this.email}\", \"pubKey\": \"{this.pubKey}\" "+ "}";
        }

    }
}