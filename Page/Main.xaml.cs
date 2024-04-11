using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using OtpNet;
using System.Windows.Markup;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using ListViewItem = System.Windows.Controls.ListViewItem;
using ZXing;
using ZXing.Windows.Compatibility;
using ZXing.Common;

namespace MFA
{
    public partial class Main : Page
    {
        private MainWindow windows;
        private List<string> AccountName = new List<string>();
        private List<string> OtpUri = new List<string>();

        public Main(MainWindow window)
        {
            InitializeComponent();
            windows = window;
            UpdateList();
        }

        #region Liste
        private void ListAccount()
        {
            AccountList.Items.Clear();

            for (int i = 0; i < AccountName.Count; i++)
            {
                Button button = new Button();
                button.Content = AccountName[i];
                button.Click += AccountView;

                ListViewItem item = new ListViewItem();
                item.Content = button;
                button.Style = FindResource("CustomButton") as Style;

                AccountList.Items.Add(item);
            }
        }

        private void AccountView(object sender, RoutedEventArgs e)
        {
            string AC;
            string OU;

            Button? button = sender as Button;

            if (button != null)
            {
                ListViewItem? item = FindAncestor<ListViewItem>(button);

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

        private T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
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

        public void UpdateList()
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

                    if (part.Length == 2)
                    {
                        AccountName.Add(part[0]);
                        OtpUri.Add(part[1]);
                    }
                }
            }
            ListAccount();
        }
        #endregion

        #region Bouton
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (windows != null)
            {
                windows.Page.Navigate(new NewAccount(windows, this));
            }
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

            if (File.Exists(filePath))
            {
                int selectedIndex = AccountList.SelectedIndex;
                if (selectedIndex >= 0) 
                {
                    AccountName.RemoveAt(selectedIndex);

                    List<string> lines = File.ReadAllLines(filePath).ToList();
                    lines.RemoveAt(selectedIndex);

                    File.WriteAllLines(filePath, lines);
                }
            }
            UpdateList();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Numero de version actuel : " + System.Windows.Forms.Application.ProductVersion);
        }

        private void rename_Click(object sender, RoutedEventArgs e)
        {
            if (AccountList.SelectedItem != null)
            {
                string newName = Microsoft.VisualBasic.Interaction.InputBox("Entrez le nouveau nom");

                if (!string.IsNullOrEmpty(newName))
                {
                    string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
                    string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

                    if (File.Exists(filePath))
                    {
                        int selectedIndex = AccountList.SelectedIndex;
                        if (selectedIndex >= 0)
                        {
                            List<string> lines = File.ReadAllLines(filePath).ToList();
                            string[] part = lines[selectedIndex].Split(';');
                            if (part.Length == 2)
                            {
                                part[0] = newName;
                                lines[selectedIndex] = string.Join(";", part);
                            }
                            string[] part1 = lines[selectedIndex].Split("/");
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
                                    lines[selectedIndex] = string.Join("/",part1);
                                }
                            }
                            

                            File.WriteAllLines(filePath, lines);
                        }
                    }
                    UpdateList();
                }
            }
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
        #endregion

        #region Export
        private void ExportToTexte(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Fichiers texte (*.txt)|*.txt";
            sfd.Title = "Sélectionnez un fichier texte";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
                string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

                if (File.Exists(filePath))
                {
                    string[] lignes = File.ReadAllLines(filePath);

                    foreach (string line in lignes)
                    {
                        string[] part = line.Split(';');
                        using (StreamWriter sw = File.AppendText(sfd.FileName))
                        {
                            string exportline = part[1];
                            sw.WriteLine(exportline);
                        }
                    }
                }
            }     
        }

        private void ExportToQRCode(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "";
            sfd.Title = "Enregistrer les QR codes";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string? saveDirectory = System.IO.Path.GetDirectoryName(sfd.FileName) ?? string.Empty;
                string folderName = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
                string targetDirectory = System.IO.Path.Combine(saveDirectory, folderName);

                Directory.CreateDirectory(targetDirectory);

                string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
                string filePath = System.IO.Path.Combine(appDirectory, "Account.dat");

                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);

                    foreach(string line in lines)
                    {
                        string[] part = line.Split(";");
                        string accountName = part[0];
                        string exportline = part[1];

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

                        string qrCodeFileName = System.IO.Path.Combine(targetDirectory, $"{accountName}.png");
                        qrCodeImage.Save(qrCodeFileName);
                    }
                }
            }
        }
        #endregion
    }
}

