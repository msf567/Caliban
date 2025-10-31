using System;
using System.Threading;
using System.Windows.Forms;

namespace Hooks
{
    internal class HooksProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Run(new HookForm());  
        }

       
    }
}