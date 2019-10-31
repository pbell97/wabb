using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace tlsTesting
{
    public class Program
    {
        static void Main(String[] args){
            // string t = "{'access_id': 'a', 'chatId': 'Yep', 'messageContent': 'Content', 'user_id': 'TestUserID'}";
            // Message myMessage = new Message(t);
            // myMessage.access_id = "a";
            // myMessage.messageContent = "Content";
            // myMessage.chatId = "Yep";
            // Console.WriteLine(myMessage.createJSONString());



            TLSConnector con = new TLSConnector();

            ThreadStart connectionThreadRef = new ThreadStart(con.Connect);
            Thread connectionThread = new Thread(connectionThreadRef);
            connectionThread.Start();

            
            
        }

    }

    
}