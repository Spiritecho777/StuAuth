using MFA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ListViewItem = System.Windows.Controls.ListViewItem;

namespace StuAuth
{
    public partial class Import : Page
    {
        Main menu;
        public Import(Main window)
        {
            InitializeComponent();
            menu=window;

            Initialisation();
        }

        private void Initialisation()
        {
            int i = 0;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Fichiers texte (*.txt)|*.txt";
            ofd.Title = "Sélectionnez un fichier texte";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                string[] lignes = File.ReadAllLines(filePath);

                foreach (string line in lignes)
                {
                    ListOtp.Items.Add(line);
                }

                /*using StreamReader reader = new StreamReader(filePath);
                {
                    string[] lignes = File.ReadAllLines(filePath);

                    foreach (string line in lignes)
                    {
                        i++;

                        string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");

                        if (!Directory.Exists(appDirectory))
                        {
                            Directory.CreateDirectory(appDirectory);
                        }

                        string Savefile = System.IO.Path.Combine(appDirectory, "Account.dat");

                        using (StreamWriter sw = File.AppendText(Savefile))
                        {
                            sw.WriteLine(i + ";" + line);
                        }
                    }
                }*/
            }            
        }

        private void NewName(object sender, SelectionChangedEventArgs e) 
        {
            if (ListOtp.SelectedItem != null)
            {
                string? SelectedAccount = ListOtp.SelectedItem.ToString();
                AccountNameBox.Text = SelectedAccount;
            }
        }

        private void Modify_Click(object sender, RoutedEventArgs e) 
        {
            if(ListOtp.SelectedItem != null)
            {
                string? selectedItem = ListOtp.SelectedItem.ToString();
                int? selectedIndex =ListOtp.SelectedIndex;

                
            }
            
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
            NavigationService.GoBack();
            menu.UpdateList();
        }
    }
}
