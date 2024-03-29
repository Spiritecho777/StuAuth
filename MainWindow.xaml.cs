using System.Windows;
using System.Windows.Navigation;
using StuAuth;

namespace MFA
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Page.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            Page.Navigate(new Main(this));
        }

        public void NewAccount(string OtpAuth,Main menu)
        {
            Page.NavigationService.Navigate(new NewAccount2(OtpAuth,menu));
        }

        public void ImportM(Main menu)
        {
            Page.NavigationService.Navigate(new Import(menu));
        }
    }
}