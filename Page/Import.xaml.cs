using MFA;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;
using CheckBox = System.Windows.Controls.CheckBox;

namespace StuAuth
{
    public partial class Import : Page
    {
        Main menu;
        public Import(Main window)
        {
            InitializeComponent();
            menu = window;

            Initialisation();
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

                        CheckBox? checkBox = new CheckBox();
                        checkBox.Content = name + ";" + line;
                        ListOtp.Items.Add(checkBox);
                    }
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
                    if (line != null)
                    {
                        string[] part = line.Split(';');

                        if (part.Length == 2)
                        {
                            try
                            {
                                var uri = new Uri(part[1]);

                                string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
                                if (!Directory.Exists(appDirectory))
                                {
                                    Directory.CreateDirectory(appDirectory);
                                }
                                string Savefile = System.IO.Path.Combine(appDirectory, "Account.dat");
                                using (StreamWriter sw = File.AppendText(Savefile))
                                {
                                    sw.WriteLine(line);
                                }
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
            menu.UpdateList();
        }

        private void Back_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
