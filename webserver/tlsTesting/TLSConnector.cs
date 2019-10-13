using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Threading;


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
            Thread.Sleep(3000);
            this.sslStream = new SslStream(this.tcpClient.GetStream(), false, null);
            sslStream.AuthenticateAsClient("www.patricksproj.codes");
            Thread.Sleep(3000);
            
            string t = "{'access_id': 'a', 'chatId': 'Yep', 'messageContent': 'Content', 'user_id': 'TestUserID'}";
            Message myMessage = new Message(t);

            this.WriteMessage("message", myMessage.createJSONString());
            Thread.Sleep(1000);

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
                    Console.WriteLine("Getting bytesRead");
                    bytesRead = this.sslStream.Read(messageBytes, 0, 4096);
                    encoder = new ASCIIEncoding();
                    receivedMessage = encoder.GetString(messageBytes, 0, bytesRead);
                    this.unreadMessages.Add(receivedMessage);
                    this.OnMessageReceived(this, new EventArgs()); 
                }
                catch
                {
                    Console.WriteLine("Caught socket error");
                }

                // Send response
                this.WriteMessage("other", "stuff");

                // Thread.Sleep(1000);

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
            Console.WriteLine("I received a message!!!");
        }
    }

    
}