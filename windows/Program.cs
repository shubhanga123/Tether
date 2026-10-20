using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tether
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    internal sealed class MainForm : Form
    {
        public MainForm()
        {
            Text = "Tether";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 300);
            BackColor = Color.FromArgb(31, 138, 76);

            Label message = new Label();
            message.Text = "Tether\n\nPC app is working\nStep 1 - version 0.1.0";
            message.ForeColor = Color.White;
            message.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            message.AutoSize = false;
            message.Dock = DockStyle.Fill;
            message.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(message);
        }
    }
}
