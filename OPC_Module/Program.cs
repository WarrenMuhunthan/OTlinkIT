using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPC_Module;

namespace OPC_Module
{
    class Program
    {
        static void Main(string[] args)
        {
            OPCClient oPCClient = new OPCClient();
            oPCClient.ConnectToServer();
            callopc();
        }

        public static void callopc()
        {
            Console.ReadLine();
        }
    }
}
