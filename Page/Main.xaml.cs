#region
using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using ListViewItem = System.Windows.Controls.ListViewItem;
using ZXing;
using ZXing.Windows.Compatibility;
using ZXing.Common;
using Newtonsoft.Json;
using System.Text;
using StuAuth.Classe;
using System.Diagnostics;
using System;
using System.Windows.Shapes;
using Path = System.IO.Path;
using MessageBox = System.Windows.MessageBox;
#endregion

namespace StuAuth
{
    public partial class Main : Page
    {
        #region Variable
        private MainWindow windows;
        private List<string> AccountName = new List<string>();
        private List<string> OtpUri = new List<string>();
        private HttpServer server;
        public bool isServerRunning = false;
        
        private List<int> accountsIndexMapping = new List<int>();
        #endregion

        public Main(MainWindow window)
        {
            InitializeComponent();
            windows = window;
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

            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

            if (File.Exists(filePath))
            {
                string[] lignes = File.ReadAllLines(filePath);

                Dictionary<string, int> occurrences = new Dictionary<string, int>();

                foreach (string line in lignes)
                {
                    string[] part = line.Split(';');
                    if (part.Length == 2)
                    {
                        string[] part1 = part[0].Split('\\');
                        string key = part1[0];

                        if (occurrences.ContainsKey(key))
                        {
                            occurrences[key]++;
                        }
                        else
                        {
                            occurrences[key] = 1;
                        }
                    }
                }

                List<string> lignesValides = new List<string>();

                foreach (string line in lignes)
                {
                    string[] part = line.Split(';');

                    if (part.Length == 2)
                    {
                        string[] part1 = part[0].Split('\\');
                        string key = part1[0];
                        if (part[1] != "" || occurrences[key] == 1)
                        {
                            AccountName.Add(part[0]);
                            OtpUri.Add(part[1]);

                            lignesValides.Add(line);
                        }
                    }
                }
                File.WriteAllLines(filePath, lignesValides);
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

            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

            if (File.Exists(filePath))
            {
                string[] lignes = File.ReadAllLines(filePath);

                foreach (string line in lignes)
                {
                    string[] part = line.Split(';');
                    string[] part2 = part[0].Split("\\");
                    if (part.Length == 2)
                    {
                        if (part2[0] == folderName)
                        {
                            AccountName.Add(part[0]);
                            OtpUri.Add(part[1]);
                        }
                    }
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
            if (windows != null) //Compte
            {
                string Folder = FolderName.Content.ToString();
                if (!string.IsNullOrEmpty(Folder))
                {
                    windows.Page.Navigate(new NewAccount(windows, this, Folder));
                }
                else //Dossier
                {
                    string FolderN = Microsoft.VisualBasic.Interaction.InputBox("Entrez le nom du dossier");

                    if (!string.IsNullOrEmpty(FolderN))
                    {
                        string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
                        string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

                        if (Directory.Exists(appDirectory))
                        {
                            if (File.Exists(filePath))
                            {
                                List<string> line = File.ReadAllLines(filePath).ToList();

                                line.Add($"{FolderN}\\;");
                                File.WriteAllLines(filePath, line);
                            }
                            else
                            {
                                List<string> line = new List<string>();

                                line.Add($"{FolderN}\\;");
                                File.WriteAllLines(filePath, line);
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(appDirectory);
                            if (File.Exists(filePath))
                            {
                                List<string> line = File.ReadAllLines(filePath).ToList();

                                line.Add($"{FolderN}\\;");
                                File.WriteAllLines(filePath, line);
                            }
                            else
                            {
                                List<string> line = new List<string>();

                                line.Add($"{FolderN}\\;");
                                File.WriteAllLines(filePath, line);
                            }
                        }
                    }
                    UpdateFolderList();
                }
            }
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

            if (File.Exists(filePath)) //Compte
            {
                string Folder = FolderName.Content.ToString();
                if (!string.IsNullOrEmpty(Folder))
                {
                    if (AccountList.SelectedItem != null)
                    {
                        ListViewItem selectedItem = AccountList.SelectedItem as ListViewItem;
                        if (selectedItem.Content is Button accountButton)
                        {
                            string Name = accountButton.Content.ToString();

                            List<string> line = File.ReadAllLines(filePath).ToList();
                            for (int i = line.Count - 1; i >= 0; i--)
                            {
                                List<string> lines = File.ReadAllLines(filePath).ToList();
                                string[] part = lines[i].Split(';');
                                if (part.Length == 2)
                                {
                                    string[] part4 = part[0].Split("\\");
                                    if (part4[1] == Name)
                                    {
                                        List<string> line2 = File.ReadAllLines(filePath).ToList();
                                        line2.RemoveAt(i);

                                        File.WriteAllLines(filePath, line2);
                                    }
                                }
                            }
                        }
                    }
                }
                else //Dossier
                {
                    ListViewItem selectedItem = AccountList.SelectedItem as ListViewItem;
                    if (selectedItem.Content is Button accountButton)
                    {
                        string Name = accountButton.Content.ToString();

                        List<string> line = File.ReadAllLines(filePath).ToList();
                        for (int i = 0; i < line.Count; i++)
                        {
                            List<string> lines = File.ReadAllLines(filePath).ToList();
                            string[] part = lines[i].Split('\\');
                            if (part.Length == 2)
                            {
                                if (part[0] == Name)
                                {
                                    if (part[1].Count() > 1)
                                    {
                                        System.Windows.MessageBox.Show("Il y a des compte de présent dans ce dossier");
                                        break;
                                    }
                                    else
                                    {
                                        List<string> line2 = File.ReadAllLines(filePath).ToList();
                                        line2.RemoveAt(i);

                                        File.WriteAllLines(filePath, line2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            UpdateFolderList();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Numero de version actuel : " + System.Windows.Forms.Application.ProductVersion);
        }

        private void rename_Click(object sender, RoutedEventArgs e)
        {
            string Folder = FolderName.Content.ToString();
            if (!string.IsNullOrEmpty(Folder)) //Compte
            {
                if (AccountList.SelectedItem != null)
                {
                    ListViewItem selectedItem = AccountList.SelectedItem as ListViewItem;
                    if (selectedItem.Content is Button accountButton)
                    {
                        string oldName = accountButton.Content.ToString();

                        string newName = Microsoft.VisualBasic.Interaction.InputBox("Entrez le nouveau nom");

                        if (!string.IsNullOrEmpty(newName))
                        {
                            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
                            string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

                            if (File.Exists(filePath))
                            {
                                List<string> line = File.ReadAllLines(filePath).ToList();
                                for (int i = 0; i < line.Count; i++)
                                {
                                    List<string> lines = File.ReadAllLines(filePath).ToList();
                                    string[] part = lines[i].Split(';');
                                    if (part.Length == 2)
                                    {
                                        string[] part4 = part[0].Split("\\");
                                        if (part4[1] == oldName)
                                        {
                                            part4[1] = newName;
                                            string updatePath = string.Join("\\", part4);
                                            part[0] = updatePath;
                                            lines[i] = string.Join(";", part);

                                            string[] part1 = lines[i].Split("/");
                                            if (part1.Length > 0)
                                            {
                                                string lastpart = part1[3];
                                                string[] part3 = lastpart.Split("?");
                                                if (part3.Length == 2)
                                                {
                                                    if (newName.Contains(" "))
                                                    {
                                                        newName = newName.Replace(" ", "%20");
                                                    }
                                                    if (newName.Contains("@"))
                                                    {
                                                        newName = newName.Replace("@", "%40");
                                                    }
                                                    part3[0] = newName;
                                                    lastpart = string.Join("?", part3);
                                                    part1[3] = lastpart;
                                                    lines[i] = string.Join("/", part1);
                                                }
                                            }
                                        }
                                        File.WriteAllLines(filePath, lines);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else //Dossier
            {
                if (AccountList.SelectedItem != null)
                {
                    ListViewItem selectedItem = AccountList.SelectedItem as ListViewItem;
                    if (selectedItem.Content is Button accountButton)
                    {
                        string oldName = accountButton.Content.ToString();

                        string newName = Microsoft.VisualBasic.Interaction.InputBox("Entrez le nouveau nom");

                        if (!string.IsNullOrEmpty(newName))
                        {
                            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
                            string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

                            if (File.Exists(filePath))
                            {
                                List<string> line = File.ReadAllLines(filePath).ToList();
                                for (int i = 0; i < line.Count; i++)
                                {
                                    List<string> lines = File.ReadAllLines(filePath).ToList();
                                    string[] part = lines[i].Split('\\');
                                    if (part.Length == 2 && part[0] == oldName)
                                    {
                                        part[0] = newName;
                                        lines[i] = string.Join("\\", part);
                                    }
                                    File.WriteAllLines(filePath, lines);
                                }
                            }
                        }
                    }
                }
            }
            UpdateFolderList();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();

            MenuItem menuItem1 = new MenuItem();
            menuItem1.Header = "Exporter vers fichier texte";
            menuItem1.Click += ExportToTexte;
            contextMenu.Items.Add(menuItem1);

            MenuItem menuItem2 = new MenuItem();
            menuItem2.Header = "Exporter vers fichier QRCode";
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
            if (!isServerRunning)
            {
                ServeurConnect.Background = new SolidColorBrush(Colors.Green);
                server = new HttpServer(this);
                server.Start();
                isServerRunning = true;
            }
            else
            {
                server.Stop();
                server = null;
                ServeurConnect.Background = new SolidColorBrush(Colors.Red);
                isServerRunning = false;
            }
        }

        private void TimeSynchro(object sender, RoutedEventArgs e)
        {
            string startServiceCommand = "net start w32time";
            string resyncCommand = "w32tm /resync";

            ExecuteCommand(startServiceCommand,resyncCommand);
        }
        #endregion

        #region Méthode
        private void ExportToTexte(object sender, RoutedEventArgs e)
        {
            bool isNotAllowed=false;
            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");
            string[] lignes0 = File.ReadAllLines(filePath);

            foreach (string line0 in lignes0)
            {
                string[] part0 = line0.Split(';');
                if (part0[1] == "")
                {
                    MessageBox.Show("Il y a un dossier vide veuillez le supprimer avant l'export");
                    isNotAllowed = true;
                    break;
                }
            }

            if (!isNotAllowed)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Fichiers texte (*.txt)|*.txt";
                sfd.Title = "Sélectionnez un fichier texte";

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
                                string name = part2[3];
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
            bool isNotAllowed = false;
            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");
            string[] lignes0 = File.ReadAllLines(filePath);

            foreach (string line0 in lignes0)
            {
                string[] part0 = line0.Split(';');
                if (part0[1] == "")
                {
                    MessageBox.Show("Il y a un dossier vide veuillez le supprimer avant l'export");
                    isNotAllowed = true;
                    break;
                }
            }

            if (!isNotAllowed)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "";
                sfd.Title = "Enregistrer les QR codes";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string? saveDirectory = System.IO.Path.GetDirectoryName(sfd.FileName) ?? string.Empty;
                    string folderName = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
                    string targetDirectory = System.IO.Path.Combine(saveDirectory, folderName);
                    Debug.WriteLine(targetDirectory);
                    Directory.CreateDirectory(targetDirectory);
                    if (!Directory.Exists(targetDirectory))
                    {
                        MessageBox.Show($"Le répertoire {targetDirectory} n'a pas pu être créé.");
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
                            string name = part2[3];
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

/*
Bug sur les export
permettre la modification du chemin du fichier de compte
chiffrement du fichier de compte
Création d'une appli mobile
*/