using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
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
        List<string> unreadMessages = new List<string>();
        public event EventHandler OnMessageReceived;
        
        public TLSConnector(){
            this.OnMessageReceived += new EventHandler(DisplayMessage);
        }



        public void Connect() 
        {
            this.tcpClient.Connect("127.0.0.1", 8000);
            Console.WriteLine("Connected...");

            this.serverStream = this.tcpClient.GetStream();
            this.WriteMessage("First message sent from C# Client");

            // Read bytes
            this.serverStream = this.tcpClient.GetStream();
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
                    bytesRead = this.serverStream.Read(messageBytes, 0, 4096);
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
                this.WriteMessage("Thanks for message: " + receivedMessage);

                // Thread.Sleep(1000);

                if (this.unreadMessages.Count >= 10){
                    Console.WriteLine("\n\nReceived 10+ messages...");
                    this.unreadMessages.ForEach(Console.WriteLine);
                    this.tcpClient.Close();
                    break;
                }
            }
        }

        public void WriteMessage(string message){
            byte[] outStream = Encoding.ASCII.GetBytes(message);
            this.serverStream.Write(outStream, 0, outStream.Length);
            this.serverStream.Flush();
        }

        public void DisplayMessage(object sender, EventArgs e){
            Console.WriteLine("I received a message!!!");
        }
    }

    
}