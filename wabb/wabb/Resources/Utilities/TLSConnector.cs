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

namespace wabb.Utilities
{
    public class TLSConnector
    {
        TcpClient tcpClient = new TcpClient();
        NetworkStream serverStream = default(NetworkStream);
        SslStream sslStream;
        public List<string> unreadMessages = new List<string>();
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

            while (true)
            {
                try
                {
                    // Read up to 4096 bytes
                    Console.WriteLine("\nGetting bytesRead\n");
                    bytesRead = this.sslStream.Read(messageBytes, 0, 4096);
                    encoder = new ASCIIEncoding();
                    receivedMessage = encoder.GetString(messageBytes, 0, bytesRead).Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                    this.unreadMessages.Add(receivedMessage);
                    this.OnMessageReceived(this, new EventArgs());
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }


                //if (this.unreadMessages.Count >= 3)
                //{
                //    Console.WriteLine("\n\nReceived 10+ messages...");
                //    this.unreadMessages.ForEach(Console.WriteLine);
                //    this.tcpClient.Close();
                //    break;
                //}
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

        public string interpretMessageType(JObject message)
        {
            string type = message.Properties().Select(p => p.Name).ToList()[0];
            return type;
        }

        public void DisplayMessage(object sender, EventArgs e){
            Console.WriteLine("Got message: " + this.unreadMessages[0]);
            int messageIndex = this.unreadMessages.Count - 1;
            if (messageIndex < 0) return;
            JObject message = JObject.Parse(this.unreadMessages[messageIndex]);
            string type = this.interpretMessageType(message);
            Console.WriteLine("Type: " + type);

            if (type == "usersList"){
                //JArray usersArray = (JArray) message["usersList"];
                //int length = usersArray.Count;
                //Console.WriteLine("Length: " + length.ToString());
                //User aUser;
                //for (int i = 0; i < length; i++){
                //    aUser = new User(message["usersList"][i].ToString());
                //}
            }

            if (type == "receivedMessage"){
                //JArray messagesArray = (JArray)message["receivedMessage"];
                //int length = messagesArray.Count;
                //Console.WriteLine("Length: " + length.ToString());
                //WabbMessage myMessage;
                //for (int i = 0; i < length; i++)
                //{
                //    myMessage = new WabbMessage(message["receivedMessage"][i].ToString());
                //}
            }

            if (type == "userCreated"){
                //User myUser = new User(message["userCreated"].ToString());
            }

            if (type == "chatsList"){
                //JArray chatsArray = (JArray) message["chatsList"];
                //int length = chatsArray.Count;
                //Console.WriteLine("Length: " + length.ToString());
                //Chat aChat;
                //for (int i = 0; i < length; i++){
                //    aChat = new Chat(message["chatsList"][i].ToString());
                //}
            }

            if (type == "error")
            {
                Console.WriteLine("ERROR RECEIVED FROM SERVER: " + message["error"].ToString());
            }

            // Console.WriteLine(l.ToString());
            //this.unreadMessages.RemoveAt(0);
            Console.WriteLine();
        }
    }

    
}