using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Threading;

using Newtonsoft.Json.Linq;


// Make a message buffer that it adds messages too when it receives them. 
// Have the while loop be in a seperate thread.
// Use C# events

namespace tlsTesting
{
    public class TLSConnector
    {
        TcpClient tcpClient = new TcpClient();
        NetworkStream serverStream = default(NetworkStream);
        SslStream sslStream;
        List<string> unreadMessages = new List<string>();
        public event EventHandler OnMessageReceived;
        
        public TLSConnector(){
            this.OnMessageReceived += new EventHandler(DisplayMessage);
        }

        public void Connect() 
        {
            this.tcpClient.Connect("www.patricksproj.codes", 443);
            Console.WriteLine("Connected...");
            this.sslStream = new SslStream(this.tcpClient.GetStream(), false, null);
            sslStream.AuthenticateAsClient("www.patricksproj.codes");
            
            // Read bytes
            byte[] messageBytes = new byte[4096];
            int bytesRead;
            bytesRead = 0;
            string receivedMessage = "";
            ASCIIEncoding encoder;

            User myUser = new User();
            myUser.username = "PatrickBell";
            // MsState email
            myUser.access_id = "ya29.ImGiByDWZN1ry1S3qwf3C2_jcmtI7RhbBZo8fXsRwZ23dUo1Kl2bUg_CHO4QzwSFj135_zaUjSlBKozOc976HVqzPwLiDo31-h6Jl84pStTBfRaAuZJ8tXZj-4GEFSsuhK8f";
            // Gmail below
            // myUser.access_id = "ya29.Il-bB3YQJzIemDXlWBtnXg2MRlrs0inhqcXr1e0XlhFnhvCn2ClPQh_1eefAEY4wY3bOaeRXxbjriLvTNq38ZBADs0UDsNp5r0Vg1D0GD_wPaklYs7jUI6V9ZBErkjSqzg";
            
            Message myMessage = new Message();
            myMessage.access_id = myUser.access_id;
            myMessage.chatId = "-1060544732";
            myMessage.messageContent = "This is a message sent from the first client";
            this.WriteMessage("signIn", "{\"access_id\": \"" + myMessage.access_id + "\"}");
            // this.WriteMessage("createUser", "{\"access_id\": \"" + myMessage.access_id + "\", \"username\": \"" + "PatricksGmail" + "\"}");
            // this.WriteMessage("messageGet", "{\"access_id\": \"" + myMessage.access_id + "\", \"chatId\": \"" + "-1060544732" + "\", \"messageId\": \"" + "10" + "\"}");
            // this.WriteMessage("createChat", "{\"access_id\": \"" + myMessage.access_id + "\", \"chatName\": \"" + "myChat" + "\"}");
            // this.WriteMessage("joinChat", "{\"access_id\": \"" + myMessage.access_id + "\", \"chatId\": \"" + "2136482312" + "\", \"pubKey\": \"" + "pubKeyGoesHere" + "\"}");
            // this.WriteMessage("allowUserToJoinChat", "{\"access_id\": \"" + myMessage.access_id + "\", \"chatId\": \"" + "2136482312" + "\", \"symKey\": \"" + "symKeyGoesHere" + "\", \"joinerId\": \"" + "113467843674295288430" + "\"}");
            // this.WriteMessage("messagePost", myMessage.createJSONString());

            while (true)
            {
                try
                {
                    // Read up to 4096 bytes
                    Console.WriteLine("Getting bytesRead");
                    bytesRead = this.sslStream.Read(messageBytes, 0, 4096);
                    Console.WriteLine("Here1");
                    encoder = new ASCIIEncoding();
                    receivedMessage = encoder.GetString(messageBytes, 0, bytesRead).Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                    Console.WriteLine("Here2");
                    this.unreadMessages.Add(receivedMessage);
                    Console.WriteLine("Here3");
                    this.OnMessageReceived(this, new EventArgs());
                    Console.WriteLine("Here4");
                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }


                if (this.unreadMessages.Count >= 3){
                    Console.WriteLine("\n\nReceived 10+ messages...");
                    this.unreadMessages.ForEach(Console.WriteLine);
                    this.tcpClient.Close();
                    break;
                }
            }
        }

        public void WriteMessage(string messageType, string message){
            if (message[0] != '"' && message[0] != '[' && message[0] != '{'){
                message = "\"" + message + "\"";
            }
            string outMessage = "{" +$"\"type\": \"{messageType}\" , \"content\": {message}" + "}";
            byte[] outStream = Encoding.ASCII.GetBytes(outMessage);
            this.sslStream.Write(outStream, 0, outStream.Length);
            this.sslStream.Flush();
        }

        public void DisplayMessage(object sender, EventArgs e){
            Console.WriteLine("Here5");
            Console.WriteLine("Got message: " + this.unreadMessages[0]);
            Console.WriteLine("Here6");            
            this.unreadMessages.RemoveAt(0);
            Console.WriteLine("Here7");
        }
    }

    
}