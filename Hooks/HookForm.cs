using System;
using System.Windows.Forms;
using Caliban.Core.Transport;

namespace Hooks
{
    public partial class HookForm : Form
    {
        UserActivityHook actHook;
        private HooksClient client;
        public HookForm()
        {
            InitializeComponent();
            client = new HooksClient("Hooks");
        }

        private void HookForm_Load(object sender, EventArgs e)
        {
            actHook = new UserActivityHook(); // crate an instance with global hooks
            // hang on events
            actHook.OnMouseActivity += new MouseEventHandler(MouseMoved);
            actHook.KeyDown += new KeyEventHandler(MyKeyDown);
            actHook.KeyPress += new KeyPressEventHandler(MyKeyPress);
            actHook.KeyUp += new KeyEventHandler(MyKeyUp);
            actHook.Start();
        }

        private void HookForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            actHook.Stop();
        }

        private static void MyKeyUp(object _sender, KeyEventArgs _e)
        {
            Console.WriteLine("KeyUp - " + _e.KeyData);
        }

        private static void MyKeyPress(object _sender, KeyPressEventArgs _e)
        {
            Console.WriteLine("KeyPress - " + _e.KeyChar);
        }

        private static void MyKeyDown(object _sender, KeyEventArgs _e)
        {
            Console.WriteLine("KeyDown 	- " + _e.KeyData);
        }

        private static void MouseMoved(object _sender, MouseEventArgs _e)
        {
            if (_e.Clicks > 0) Console.WriteLine("MouseButton 	- " + _e.Button);
        }
    }
}
