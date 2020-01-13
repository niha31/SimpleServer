using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleClient
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SimpleClient simpleClient = new SimpleClient();
            if (simpleClient.Connect("127.0.0.1", 4444))
            {
                simpleClient.Run();
            }            
        }
    }
}
