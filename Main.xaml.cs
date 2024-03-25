using System;
using System.IO;
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

namespace MFA
{
    public partial class Main : Page
    {
        private MainWindow windows ;
        private List<string> AccountName = new List<string>();
        private List<string> OtpUri = new List<string>();

        public Main(MainWindow window)
        {
            InitializeComponent();
            windows = window;

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

                AccountList.Items.Add(item);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (windows != null)
            {
                windows.Page.Navigate(new NewAccount(windows,this));
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

                            File.WriteAllLines(filePath, lines);
                        }
                    }
                    UpdateList();
                }
            }
        }
    }
}

