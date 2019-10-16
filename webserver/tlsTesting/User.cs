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


namespace tlsTesting
{
    public class User
    {
        public string access_id;
        public string username;
        public string user_id;

        public User(){

        }

        public User(string JSON){
            JObject newMessage = JObject.Parse(JSON);
            this.access_id = newMessage["response"]["access_id"].ToString();
            this.username = newMessage["response"]["username"].ToString();
            this.user_id = newMessage["response"]["user_id"].ToString();
        }

        public string createJSONString(){
            return "{" + $"\"access_id\": \"{this.access_id}\", \"username\": \"{this.username}\", \"user_id\": \"{user_id}\"" + "}";
        }

    }
}