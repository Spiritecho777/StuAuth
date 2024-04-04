using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MFA
{
    public partial class NewAccount2 : Page
    {
        private string otpauth;
        private Main Menu;
        public NewAccount2(string OtpAuth,Main menu)
        {
            InitializeComponent();
            otpauth = OtpAuth;
            Menu = menu;
        }

        private void SaveNewAccount(object sender, EventArgs e) 
        {
            if (!string.IsNullOrEmpty(AccountName.Text))
            {
                string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");

                if (!Directory.Exists(appDirectory))
                {
                    Directory.CreateDirectory(appDirectory);
                }

                string Savefile = System.IO.Path.Combine(appDirectory, "Account.dat");

                using (StreamWriter sw = File.AppendText(Savefile))
                {
                    string[] part = otpauth.Split('/');
                    otpauth = part[0] + "/" + part[1] + "/" + part[2] + "/" + AccountName.Text + part[3];
                    sw.WriteLine(AccountName.Text + ";" + otpauth); 
                }

                NavigationService.GoBack();
                NavigationService.GoBack();
                Menu.UpdateList();
            }
            else
            {
                MessageBox.Show("Veuillez entrez un nom de compte");
            }
        }

        private void Back(object sender, EventArgs e) 
        {
            NavigationService.GoBack();
        }
    }
}
