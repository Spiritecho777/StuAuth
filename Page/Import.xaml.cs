using MFA;
using StuAuth.Classe;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;
using CheckBox = System.Windows.Controls.CheckBox;

namespace StuAuth
{
    public partial class Import : System.Windows.Controls.Page
    {
        Main menu;
        List<string> Account = new List<string>();
        string fName;
        public Import(List<string> AccountList, Main window, string folderName)
        {
            InitializeComponent();
            menu = window;
            fName = folderName;

            if (AccountList.Count != 0)
            {
                Account = AccountList;
                ImportGoogle();
            }
            else
            {
                Initialisation();
            }
        }

        private void Initialisation()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Fichiers texte (*.txt)|*.txt";
            ofd.Title = "Sélectionnez un fichier texte";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                string[] lignes = File.ReadAllLines(filePath);

                foreach (string line in lignes)
                {
                    if (line.StartsWith("otpauth:"))
                    {
                        string[] part = line.Split('/');
                        string name = part[3];
                        part = name.Split("?");
                        name = part[0];
                        name = name.Replace("%20", "")
                                    .Replace("%40", "@")
                                    .Replace("%3A", ":");

                        CheckBox? checkBox = new CheckBox();
                        checkBox.Content = name + ";" + line;
                        ListOtp.Items.Add(checkBox);
                    }
                }
            }
        }

        private void ImportGoogle()
        {
            foreach (string line in Account)
            {
                if (line.StartsWith("otpauth:"))
                {
                    string[] part = line.Split('/');
                    string name = part[3];
                    part = name.Split("?");
                    name = part[0];
                    if (name.Contains("%20"))
                    {
                        name = name.Replace("%20", " ");
                    }
                    if (name.Contains("%40"))
                    {
                        name = name.Replace("%40", "@");
                    }

                    CheckBox? checkBox = new CheckBox();
                    checkBox.Content = name + ";" + line;
                    ListOtp.Items.Add(checkBox);
                }
            }
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            foreach (CheckBox checkBox in ListOtp.Items)
            {
                if (checkBox.IsChecked == true)
                {
                    string? line = checkBox.Content.ToString();
                    line = $"{fName}\\{line}";
                    if (line != null)
                    {
                        string[] part = line.Split(';');

                        if (part.Length == 2)
                        {
                            try
                            {
                                var uri = new Uri(part[1]);

                                AccountManager accountManager = new AccountManager();
                                accountManager.AddAccount(line);
                            }
                            catch
                            {
                                System.Windows.MessageBox.Show("Il y a une erreur dans votre fichier d'export veuillez vérifier et recommencer");
                            }
                        }
                    }
                }
            }

            NavigationService.GoBack();
            NavigationService.GoBack();
            menu.UpdateFolderList();
        }

        private void Back_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
