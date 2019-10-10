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
            TLSConnector con = new TLSConnector();

            ThreadStart connectionThreadRef = new ThreadStart(con.Connect);
            Thread connectionThread = new Thread(connectionThreadRef);
            connectionThread.Start();

            Console.WriteLine("End of main");
        }
    }

    
}