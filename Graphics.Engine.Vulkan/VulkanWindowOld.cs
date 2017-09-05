using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace Graphics.Engine
{
    // https://blogs.msdn.microsoft.com/rickhos/2005/03/30/the-ideal-system-windows-forms-3d-gameloop-take-15/
    // https://gamedev.stackexchange.com/questions/67651/what-is-the-standard-c-windows-forms-game-loop
    public partial class VulkanWindowOld : Form
    {
        private readonly Action _actionFrame;
        public VulkanWindowOld(Action actionFrame)
        {
            _actionFrame = actionFrame;
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        public static extern Int32 SendNotifyMessage(IntPtr hWnd, Int32 msg, IntPtr wParam, IntPtr lParam);

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x000F)
            {
                _actionFrame();
                SendNotifyMessage(this.Handle, 0x000F, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}