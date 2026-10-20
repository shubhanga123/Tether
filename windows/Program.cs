using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
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
        private readonly Label statusLabel;
        private readonly Label ipLabel;
        private readonly Label codeLabel;
        private TcpListener _listener;
        private X509Certificate2 _serverCertificate;
        private string _pairingCode;

        public MainForm()
        {
            Text = "Tether - PC";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(550, 380);
            BackColor = Color.FromArgb(24, 28, 36);

            var titleLabel = new Label
            {
                Text = "Tether PC",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };

            ipLabel = new Label
            {
                ForeColor = Color.FromArgb(180, 200, 220),
                Font = new Font("Segoe UI", 12f),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Finding IP Address..."
            };

            codeLabel = new Label
            {
                ForeColor = Color.FromArgb(255, 215, 0),
                Font = new Font("Consolas", 28f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "------"
            };

            statusLabel = new Label
            {
                ForeColor = Color.FromArgb(160, 175, 190),
                Font = new Font("Segoe UI", 14f, FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Waiting for phone connection..."
            };

            Controls.Add(statusLabel);
            Controls.Add(codeLabel);
            Controls.Add(ipLabel);
            Controls.Add(titleLabel);

            StartServer();
        }

        private async void StartServer()
        {
            try
            {
                string localIp = GetLocalIpAddress();
                ipLabel.Text = $"PC IP Address: {localIp}";

                _pairingCode = new Random().Next(100000, 999999).ToString();
                codeLabel.Text = $"Code: {_pairingCode}";

                _serverCertificate = CreateSelfSignedCertificate();
                _listener = new TcpListener(IPAddress.Any, 8443);
                _listener.Start();

                await Task.Run(AcceptLoop);
            }
            catch (Exception ex)
            {
                ipLabel.Text = "Error starting server";
                statusLabel.Text = ex.Message;
            }
        }

        private async Task AcceptLoop()
        {
            while (true)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClient(client));
                }
                catch
                {
                    break;
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using SslStream sslStream = new SslStream(client.GetStream(), false);
                sslStream.AuthenticateAsServer(_serverCertificate, false, System.Security.Authentication.SslProtocols.Tls12, false);

                using StreamReader reader = new StreamReader(sslStream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(sslStream, Encoding.UTF8) { AutoFlush = true };

                string line = reader.ReadLine();
                if (line != null && line.StartsWith("PAIR:"))
                {
                    string inputCode = line.Substring(5).Trim();
                    if (inputCode == _pairingCode)
                    {
                        writer.WriteLine("OK");
                        Invoke(new Action(() =>
                        {
                            statusLabel.Text = "Connected to Phone!";
                            statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
                            codeLabel.Text = "PAIRED";
                        }));
                    }
                    else
                    {
                        writer.WriteLine("ERROR: Bad Code");
                    }
                }
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    statusLabel.Text = $"Connection failed: {ex.Message}";
                }));
            }
        }

        private string GetLocalIpAddress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return "127.0.0.1";
        }

        private X509Certificate2 CreateSelfSignedCertificate()
        {
            using RSA rsa = RSA.Create(2048);
            var req = new CertificateRequest("CN=TetherPC", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var cert = req.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(5));
            return new X509Certificate2(cert.Export(X509ContentType.Pfx));
        }
    }
}
