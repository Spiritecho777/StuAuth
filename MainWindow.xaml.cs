using System.IO;
using System.Windows;
using System.Windows.Navigation;
using StuAuth;
using Newtonsoft.Json;
using System.Text;
using System.ComponentModel;
using StuAuth.Classe;
using System.Configuration;

namespace StuAuth
{
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            Page.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            Page.Navigate(new Main(this));
        }

        public void NewAccount(string OtpAuth,Main menu,string folderName)
        {
            Page.NavigationService.Navigate(new NewAccount2(OtpAuth,menu,folderName));
        }

        public void ImportM(List<string> Account, Main menu)
        {
            Page.NavigationService.Navigate(new Import(Account,menu));
        }
    }
}