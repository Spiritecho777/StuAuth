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
        private HttpServer server;
        public MainWindow()
        {
            InitializeComponent();
            Page.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            Page.Navigate(new Main(this));

            server = new HttpServer(this);
            server.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            server.Stop();
        }

        public void NewAccount(string OtpAuth,Main menu)
        {
            Page.NavigationService.Navigate(new NewAccount2(OtpAuth,menu));
        }

        public void ImportM(List<string> Account, Main menu)
        {
            Page.NavigationService.Navigate(new Import(Account,menu));
        }
    }
}