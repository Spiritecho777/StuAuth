using StuAuth.Classe;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Resources;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace StuAuth.Page
{
    public partial class NetworkParameters : System.Windows.Controls.Page
    {
        private Main main;
        private HttpServer server;
        private AccountManager accountManager;
        private ObservableCollection<string> listIPA = new ObservableCollection<string>();
        private string IPApplication = Properties.Settings.Default.IPApplication;

        public NetworkParameters(Main main,HttpServer server, AccountManager accountManager)
        {
            InitializeComponent();
            var loc = (Loc)Application.Current.Resources["Loc"];
            this.main = main;
            this.server = server;
            this.accountManager = accountManager;
            IPApp.ItemsSource = listIPA;
            IPApp.Text = IPApplication;

            LoadNetworkInterfaces();
            ListInterface.SelectedIndex = Properties.Settings.Default.InterfaceSelect;

            if (ListInterface.SelectedItem is NetworkInterfaceInfo selectedInterface)
            {
                string ipAdress = GetLocalIPAddress(selectedInterface.Name);
                IPServ.Content = ipAdress;
            }
            else
            {
                IPServ.Content = loc["IntNP5"];
            }

            if (!main.isServerRunning)
            {
                Serv.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                Serv.Background = new SolidColorBrush(Colors.Green);
            }
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

        #region Control
        private void Serveur(object sender, EventArgs e)
        {
            if (!main.isServerRunning)
            {
                main.ServeurConnect.Background = new SolidColorBrush(Colors.Green);
                Serv.Background = new SolidColorBrush(Colors.Green);
                server.Start(IPServ.Content.ToString());
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

        private async void Synchro_Click(object sender, EventArgs e)
        {
            var loc = (Loc)Application.Current.Resources["Loc"];
            MessageBoxResult answer = MessageBox.Show(loc["IntNP1"],
            loc["Synchronization"],
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question
            );
            if (answer == MessageBoxResult.Yes)
            {
                if (!string.IsNullOrWhiteSpace(IPApplication) && IsHostReachable(IPApplication))
                {
                    string response = await SendRequestAsync("/", "GET");
                    if (response != null)
                    {
                        try
                        {
                            Back.IsEnabled = false;
                            Serv.IsEnabled = false;
                            Synchro.IsEnabled = false;
                            LoadingProgressBar.Visibility = Visibility.Visible;
                            LoadingProgressBar.Value = 0;

                            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(response);

                            if (data == null || !data.ContainsKey("Accounts") || !data.ContainsKey("Folder"))
                            {
                                Console.WriteLine(" Erreur: Données manquantes.");
                                return;
                            }

                            string[] accounts = data["Accounts"].Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] folders = data["Folder"].Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                            string formattedAccounts;
                            int totalAccounts = accounts.Length;

                            List<string> comptesExistants = accountManager.GetAllOtpUri();

                            for (int i = 0; i < totalAccounts; i++)
                            {
                                string folder = i < folders.Length ? folders[i].Trim() : "Uncategorized";
                                string otpUri = accounts[i].Trim();

                                bool alreadyExists = comptesExistants.Any(ligne => otpUri.Contains(ligne));

                                if (!alreadyExists)
                                {
                                    string label = ExtractLabelFromOTP(otpUri);
                                    formattedAccounts = ($"{folder}\\{label};{otpUri}");

                                    await Task.Run(() => accountManager.AddAccount(formattedAccounts));
                                } 

                                Dispatcher.Invoke(() =>
                                {
                                    LoadingProgressBar.Value = ((double)(i + 1) / totalAccounts) * 100;
                                });
                                await Task.Delay(100);
                            }

                            await Task.Delay(1000);
                            Synchro.IsEnabled = true;
                            Serv.IsEnabled = true;
                            Back.IsEnabled = true;
                            LoadingProgressBar.Visibility = Visibility.Hidden;
                            NavigationService.GoBack();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Erreur de parsing JSON: " + ex.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(loc["IntNP2"], loc["Error"]);
                }
            }
        }

        private async void AppNetworkChanged(object sender, KeyEventArgs e)
        {
            var loc = (Loc)Application.Current.Resources["Loc"];
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(SubApp.Text) && IsValidIPAddress(SubApp.Text))
                {
                    listIPA.Clear();
                    await ScanNetworkAsync(SubApp.Text);
                }
                else
                {
                    MessageBox.Show(loc["IntNP3"], loc["Error"]);
                }
            }
        }

        private void ChangedInterface(object sender, EventArgs e)
        {
            if (ListInterface.SelectedItem is NetworkInterfaceInfo selectedInterface)
            {
                string ipAddress = GetLocalIPAddress(selectedInterface.Name);
                IPServ.Content = ipAddress;
                Properties.Settings.Default.InterfaceSelect = ListInterface.SelectedIndex;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region Reseau
        private string GetLocalIPAddress(string interfaceName)
        {
            var loc = (Loc)Application.Current.Resources["Loc"];
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.Name != interfaceName || ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(addr.Address))
                    {
                        return addr.Address.ToString();
                    }
                }
            }

            return loc["IntNP6"];
        }

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

            List<string> arpDevices = GetAllConnectedDevices(subnet);
            foreach (string ip in arpDevices)
            {
                if(!listIPA.Contains(ip))
                {
                    listIPA.Add(ip);
                }
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
                            listIPA.Add(ipAddress);
                        });
                    }
                }
            }
            catch
            {
                Debug.WriteLine($"Erreur lors du ping de {ipAddress}");
            }
        }
        
        private List<string> GetAllConnectedDevices(string subnet)
        {
            List<string> devices = new List<string>();

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "arp";
                p.StartInfo.Arguments = "-a";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                string pattern = @"(\d+\.\d+\.\d+\.\d+)\s+([a-fA-F0-9:-]+)";
                foreach (Match match in Regex.Matches(output, pattern))
                {
                    string ip = match.Groups[1].Value;

                    if (ip.StartsWith(subnet + "."))
                    {
                        devices.Add(ip);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erreur lors de la récupération des appareils via ARP : " + ex.Message);
            }

            return devices;
        }

        private async Task<string> SendRequestAsync(string endpoint, string method, string jsonData = null)
        {
            var loc = (Loc)Application.Current.Resources["Loc"];
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri($"http://{IPApplication}:19755/");

                    HttpResponseMessage response = null;

                    if (method == "GET")
                    {
                        response = await client.GetAsync(endpoint);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show(loc["IntNP4"], loc["Error"]);
                    return null;
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"Requête expirée : {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur inattendue : {ex.Message}");
                    return null;
                }
            }
        }

        static string ExtractLabelFromOTP(string otpUri)
        {
            var match = Regex.Match(otpUri, @"otpauth://totp/([^?]+)");
            return match.Success ? match.Groups[1].Value.Replace("/", "") : "Unknown";
        }

        private bool IsHostReachable(string ip)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(ip, 1000);
                    return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private void LoadNetworkInterfaces()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .Select(ni => new NetworkInterfaceInfo
                {
                    Name = ni.Name,
                    Description = ni.Description
                })
                .ToList();

            ListInterface.ItemsSource = interfaces;
        }
        #endregion
    }

    public class NetworkInterfaceInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"{Name} - {Description}";
    }

}