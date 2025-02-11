using StuAuth.Classe;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;


namespace StuAuth
{
    public partial class NewAccount2 : System.Windows.Controls.Page
    {
        private string otpauth;
        private Main Menu;
        private string fName;

        public NewAccount2(string OtpAuth, Main menu,string folderName)
        {
            InitializeComponent();
            fName = folderName;
            otpauth = OtpAuth;
            Menu = menu;
        }

        private void SaveNewAccount(object sender, EventArgs e) 
        {
            if (!string.IsNullOrEmpty(AccountName.Text))
            {
                string[] part = otpauth.Split('/');
                otpauth = part[0] + "/" + part[1] + "/" + part[2] + "/" + AccountName.Text + "/" + part[3];
                string line = fName + "\\" + AccountName.Text + ";" + otpauth;
                AccountManager accountManager = new AccountManager();
                accountManager.AddAccount(line);

                NavigationService.GoBack();
                NavigationService.GoBack();
                Menu.UpdateFolderList();
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
