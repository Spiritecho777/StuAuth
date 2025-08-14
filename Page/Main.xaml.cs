#region
using StuAuth.Classe;
using StuAuth.Page;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using Button = System.Windows.Controls.Button;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
#endregion

namespace StuAuth
{
    public partial class Main : System.Windows.Controls.Page
    {
        #region Variable
        private MainWindow windows;
        private List<string> AccountName = new List<string>();
        private List<string> OtpUri = new List<string>();
        private HttpServer server;
        private AccountManager accountManager = new AccountManager();
        public bool isServerRunning = false;
        public string name;
        #endregion

        public Main(MainWindow window)
        {
            InitializeComponent();
            //Choix de la langue
            Language.ItemsSource = new List<string> { "Br", "En", "Fr", "Ja" };
            switch (Properties.Settings.Default.LangCode)
            {
                case "Br": Language.SelectedIndex = 0; break;
                case "En": Language.SelectedIndex = 1; break;
                case "Fr": Language.SelectedIndex = 2; break;
                case "Ja": Language.SelectedIndex = 3; break;
                default: Language.SelectedIndex = 1; break;
            }
            windows = window;
            server = new HttpServer(this);
            UpdateFolderList();
        }

        #region Liste
        #region Dossier
        private void ListFolder()
        {
            AccountList.Items.Clear();

            var accountByFolder = AccountName
                .Zip(OtpUri, (name, uri) => new { Name = name, Uri = uri })
                .GroupBy(c => Path.GetDirectoryName(c.Name));

            foreach (var folder in accountByFolder)
            {
                Button button = new Button();
                button.Content = folder.Key ?? "No Folder";
                button.Style = FindResource("CustomButton") as Style;
                button.Click += OpenFolder;

                ListViewItem item = new ListViewItem();
                item.Content = button;

                AccountList.Items.Add(item);
            }
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;

            if (button != null)
            {
                string buttonName = button.Content.ToString();
                FolderName.Content = buttonName;
                UpdateAccountList(buttonName);
            }
        }

        public void UpdateFolderList()
        {
            AccountName.Clear();
            OtpUri.Clear();

            if (accountManager.FileExists())
            {
                Dictionary<string, int> occurrences = accountManager.CountFolderOccurrences();

                List<string> validLines = accountManager.GetValidLines(occurrences);

                foreach (string line in validLines)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length == 2)
                    {
                        AccountName.Add(parts[0]);
                        OtpUri.Add(parts[1]);
                    }
                }

                accountManager.WriteLines(validLines);
            }

            FolderName.Content = "";
            ListFolder();
        }

        #endregion

        #region Compte
        private void ListAccount()
        {
            AccountList.Items.Clear();

            var accountByFolder = AccountName
              .Zip(OtpUri, (name, uri) => new { Name = name, Uri = uri })
              .GroupBy(c => Path.GetDirectoryName(c.Name));

            foreach (var folder in accountByFolder)
            {
                foreach (var account in folder)
                {
                    if (Path.GetFileName(account.Name) != "")
                    {
                        Button button = new Button();
                        button.Content = Path.GetFileName(account.Name);
                        button.Click += AccountView;

                        ListViewItem item = new ListViewItem();
                        item.Content = button;
                        button.Style = FindResource("CustomButton") as Style;

                        AccountList.Items.Add(item);
                    }
                }
            }
        }

        private void AccountView(object sender, RoutedEventArgs e)
        {
            string AC;
            string OU;

            Button? button = sender as Button;

            if (button != null)
            {
                ListViewItem? item = FindAccountAncestor<ListViewItem>(button);

                if (item != null)
                {
                    int index=AccountList.Items.IndexOf(item);

                    AC = AccountName[index];
                    OU = OtpUri[index];
                    button.Style = FindResource("CustomButton") as Style;
                    windows.Page.Navigate(new SelectAccount(AC, OU));
                }
            }
        }

        private T? FindAccountAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        public void UpdateAccountList(string folderName)
        {
            AccountName.Clear();
            OtpUri.Clear();

            List<string> accounts = accountManager.GetAccountsByFolder(folderName);

            foreach (var account in accounts)
            {
                string[] parts = account.Split(';');
                if (parts.Length == 2)
                {
                    AccountName.Add(parts[0]);
                    OtpUri.Add(parts[1]);
                }
            }

            ListAccount();
        }
        #endregion
        #endregion

        #region Bouton
        private void Back_Click(object sender, RoutedEventArgs e) 
        {
            UpdateFolderList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
            if (windows != null) //Compte
            {
                string Folder = FolderName.Content.ToString();
                if (!string.IsNullOrEmpty(Folder))
                {
                    windows.Page.Navigate(new NewAccount(windows, this, Folder));
                }
                else //Dossier
                {
                    string FolderN = Microsoft.VisualBasic.Interaction.InputBox(loc["IntAdd"]);

                    if (string.IsNullOrWhiteSpace(FolderN))
                    {
                        MessageBox.Show(loc["Error"], loc["ErrorIntAdd"]);
                        return;
                    }

                    accountManager.Add(FolderN);
                    UpdateFolderList();
                }
            }
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
            string folder = FolderName.Content?.ToString();
            if (AccountList.SelectedItem is ListViewItem selectedItem && selectedItem.Content is Button accountButton)
            {
                string name = accountButton.Content.ToString();

                try
                {
                    if (!string.IsNullOrEmpty(folder))
                    {
                        if (accountManager.DeleteFolderOrAccount(name, isFolder: false))
                        {
                            MessageBox.Show(loc["IntDeleteAccount",name]);
                        }
                    }
                    else
                    {
                        bool deleted = accountManager.DeleteFolderOrAccount(name, isFolder: true, force: false);

                        if (!deleted)
                        {
                            var confirm = MessageBox.Show(loc["IntDeleteFolder1",name],
                                loc["IntDeleteFolder2"],
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning
                            );

                            if (confirm == MessageBoxResult.Yes)
                            {
                                if (accountManager.DeleteFolderOrAccount(name, isFolder: true, force: true))
                                {
                                    MessageBox.Show(loc["IntDeleteFolder3",name]);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(loc["IntDeleteFolder4", name]);
                        }
                    }

                    UpdateFolderList();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
            System.Windows.MessageBox.Show(loc["IntAppVersion"] + System.Windows.Forms.Application.ProductVersion);
        }

        private void rename_Click(object sender, RoutedEventArgs e)
        {
            var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
            string folder = FolderName.Content?.ToString();
            if (AccountList.SelectedItem is ListViewItem selectedItem && selectedItem.Content is Button accountButton)
            {
                string oldName = accountButton.Content.ToString();
                string newName = Microsoft.VisualBasic.Interaction.InputBox(loc["IntRename"]);

                if (!string.IsNullOrEmpty(newName))
                {
                    if (!string.IsNullOrEmpty(folder))
                    {
                        accountManager.RenameFolderOrAccount(oldName, newName, isFolder: false);
                    }
                    else
                    {
                        accountManager.RenameFolderOrAccount(oldName, newName, isFolder: true);
                    }

                    UpdateFolderList();
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
            ContextMenu contextMenu = new ContextMenu();

            MenuItem menuItem1 = new MenuItem();
            menuItem1.Header = loc["IntExport1"];
            menuItem1.Click += ExportToTexte;
            contextMenu.Items.Add(menuItem1);

            MenuItem menuItem2 = new MenuItem();
            menuItem2.Header = loc["IntExport2"];
            menuItem2.Click += ExportToQRCode;
            contextMenu.Items.Add(menuItem2);

            Button? button = sender as Button;
            if (button != null)
            {
                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
            }
        }

        private void Serveur_Connect(object sender, RoutedEventArgs e)
        {
            windows.Page.Navigate(new NetworkParameters(this,server,accountManager));
        }

        private void TimeSynchro(object sender, RoutedEventArgs e)
        {
            string startServiceCommand = "net start w32time";
            string resyncCommand = "w32tm /resync";

            ExecuteCommand(startServiceCommand,resyncCommand);
        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Language.SelectedItem is string langCode)
            {
                var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
                loc.Culture = new CultureInfo(langCode);

                Properties.Settings.Default.LangCode = langCode;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region Méthode
        private void ExportToTexte(object sender, RoutedEventArgs e)
        {
            var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
            bool isNotAllowed=false;
            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account_decrypted.dat");
            string[] lignes0 = File.ReadAllLines(filePath);

            foreach (string line0 in lignes0)
            {
                string[] part0 = line0.Split(';');
                if (part0[1] == "")
                {
                    MessageBox.Show(loc["ErrorExport"]);
                    isNotAllowed = true;
                    break;
                }
            }

            if (!isNotAllowed)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = loc["Txt1"];
                sfd.Title = loc["Txt2"];

                if (sfd.ShowDialog() == DialogResult.OK)
                {


                    if (File.Exists(filePath))
                    {
                        string[] lignes = File.ReadAllLines(filePath);

                        foreach (string line in lignes)
                        {
                            string[] part = line.Split(';');
                            using (StreamWriter sw = File.AppendText(sfd.FileName))
                            {
                                string exportline = part[1];

                                string[] part2 = exportline.Split("/");
                                int L = part2.Count();
                                if (L > 4)
                                {
                                    string namebis = part2[L-1];
                                    string[] part2bis= namebis.Split("?");
                                    name = part2[L-2] + "?" + part2bis[1];
                                }
                                else
                                {
                                    name = part2[L - 1];
                                }
                                string[] part3 = name.Split("?");
                                name = part3[0];
                                name = name.Replace(" ", "%20")
                                            .Replace("@", "%40")
                                            .Replace(":", "%3A");
                                exportline = part2[0] + "/" + part2[1] + "/" + part2[2] + "/" + name + "?" + part3[1];

                                sw.WriteLine(exportline);
                            }
                        }
                    }
                }
            }
        }

        private void ExportToQRCode(object sender, RoutedEventArgs e)
        {
            var loc = (Loc)System.Windows.Application.Current.Resources["Loc"];
            bool isNotAllowed = false;
            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account_decrypted.dat");
            string[] lignes0 = File.ReadAllLines(filePath);

            foreach (string line0 in lignes0)
            {
                string[] part0 = line0.Split(';');
                if (part0[1] == "")
                {
                    MessageBox.Show(loc["ErrorExport"]);
                    isNotAllowed = true;
                    break;
                }
            }

            if (!isNotAllowed)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "";
                sfd.Title = loc["ExportQR"];

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string? saveDirectory = System.IO.Path.GetDirectoryName(sfd.FileName) ?? string.Empty;
                    string folderName = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
                    string targetDirectory = System.IO.Path.Combine(saveDirectory, folderName);
                    Debug.WriteLine(targetDirectory);
                    Directory.CreateDirectory(targetDirectory);
                    if (!Directory.Exists(targetDirectory))
                    {
                        MessageBox.Show(loc["ErrorExportQR"],targetDirectory);
                        return;
                    }

                    if (File.Exists(filePath))
                    {
                        string[] lines = File.ReadAllLines(filePath);

                        foreach (string line in lines)
                        {
                            string[] part = line.Split(";");
                            string accountName = part[0];

                            if (accountName.Contains(":")) { accountName = accountName.Replace(":", " "); }
                            string exportline = part[1];

                            string[] part2 = exportline.Split("/");
                            int L = part2.Count();
                            if (L > 4)
                            {
                                string namebis = part2[L - 1];
                                string[] part2bis = namebis.Split("?");
                                name = part2[L - 2] + "?" + part2bis[1];
                            }
                            else
                            {
                                name = part2[L - 1];
                            }
                            string[] part3 = name.Split("?");
                            name = part3[0];
                            name = name.Replace(" ", "%20")
                                        .Replace("@", "%40")
                                        .Replace(":", "%3A");
                            exportline = part2[0] + "/" + part2[1] + "/" + part2[2] + "/" + name + "?" + part3[1];

                            BarcodeWriter writer = new BarcodeWriter()
                            {
                                Format = BarcodeFormat.QR_CODE,
                                Options = new EncodingOptions
                                {
                                    Height = 300,
                                    Width = 300
                                }
                            };
                            Bitmap qrCodeImage = writer.Write(exportline);
                            string[] part4 = accountName.Split("\\");
                            accountName = part4[1];

                            string qrCodeFileName = System.IO.Path.Combine(targetDirectory, $"{accountName}.png");
                            qrCodeImage.Save(qrCodeFileName);
                        }
                    }
                }
            }
        }

        private void ExecuteCommand(string command, string command2)
        {
            try {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"{command} ; {command2}",
                        Verb = "runas",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Debug.WriteLine("Synchronisation réussie");
                }
                else
                {
                    Debug.WriteLine("Synchronisation échoué");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        #endregion
    }
}