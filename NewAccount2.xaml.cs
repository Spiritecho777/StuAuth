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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    sw.WriteLine(AccountName.Text + ";" + otpauth); 
                }

                //StreamWriter sw = new StreamWriter(Savefile);
                //sw.WriteLine(AccountName.Text + ";" + otpauth);
                //sw.Close();

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
