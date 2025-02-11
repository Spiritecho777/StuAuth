using StuAuth.Classe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StuAuth.Page
{
    public partial class NetworkParameters : System.Windows.Controls.Page
    {
        private Main main;
        private HttpServer server;
        private List<string> listIPA = new List<string>();
        private List<string> listIPS = new List<string>();
        private bool IsIPA = false;
        private bool IsIPS = false;
        private string IPApplication = Properties.Settings.Default.IPApplication;
        private string IPServeur = Properties.Settings.Default.IPServeur;

        public NetworkParameters(Main main,HttpServer server)
        {
            InitializeComponent();
            this.main = main;
            this.server = server;
            if (!main.isServerRunning)
            {
                Serv.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                Serv.Background = new SolidColorBrush(Colors.Green);
            }
        }

        private void Serveur(object sender, EventArgs e)
        {
            if (!main.isServerRunning)
            {
                main.ServeurConnect.Background = new SolidColorBrush(Colors.Green);
                Serv.Background = new SolidColorBrush(Colors.Green);
                server.Start(IPServ.Text);
                main.isServerRunning = true;
                NavigationService.GoBack();
            }
            else
            {
                server.Stop();
                server = null;
                main.ServeurConnect.Background = new SolidColorBrush(Colors.Red);
                Serv.Background = new SolidColorBrush(Colors.Red);
                main.isServerRunning = false;
                NavigationService.GoBack();
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Synchro_Click(object sender, EventArgs e)
        {

        }

        private void RegisterIPA(object sender, EventArgs e)
        {
            if (IPApp.SelectedItem != null)
            {
                IPApplication = IPApp.SelectedItem.ToString();
                Properties.Settings.Default.IPApplication = IPApplication;
                Properties.Settings.Default.Save();
            }
        }
        private void RegisterIPS(object sender, EventArgs e)
        {
            if (IPServ.SelectedItem != null)
            {
                IPServeur = IPServ.SelectedItem.ToString();
                Properties.Settings.Default.IPApplication = IPServeur;
                Properties.Settings.Default.Save();
            }
        }

        #region Scan reseau
        private bool IsValidIPAddress(string ipAddress)
        {
            string pattern = @"^(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                             + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
                             + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)$";
            return Regex.IsMatch(ipAddress, pattern);
        }

        public async Task ScanNetworkAsync(string subnet)
        {
            List<Task> tasks = new List<Task>();

            for (int i = 1; i < 255; i++)
            {
                string ip = $"{subnet}.{i}";
                tasks.Add(PingHost(ip));
            }

            await Task.WhenAll(tasks);
        }

        private async Task PingHost(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ipAddress, 500);

                    if (reply.Status == IPStatus.Success)
                    {
                        string hostName = GetHostName(ipAddress);

                        Dispatcher.Invoke(() =>
                        {
                            if (IsIPA)
                            {
                                listIPA.Add(ipAddress);
                                IsIPA = false;
                            }
                            if (IsIPS)
                            {
                                listIPS.Add(ipAddress);
                                IsIPS = false;
                            }
                        });
                    }
                }
            }
            catch
            {
                Debug.WriteLine($"Erreur lors du ping de {ipAddress}");
            }
        }

        private static string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
                return hostEntry.HostName;
            }
            catch
            {
                return "Nom d'hôte introuvable";
            }
        }
        #endregion
    }
}


//il faut terminer le copier coller avec la version smartphone