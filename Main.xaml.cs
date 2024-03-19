using System;
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

namespace MFA
{
    public partial class Main : Page
    {
        private MainWindow windows ;
        public Main(MainWindow window)
        {
            InitializeComponent();
            windows = window;
            //GenerateOtp();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (windows != null)
            {
                windows.Page.Navigate(new NewAccount(windows));
            }
        }

        public void GenerateOtp()
        {
            //string otpauthUri = "otpauth://totp/Google%3Apoubelletest109%40gmail.com?secret=gopbvckkpsixmpfefithgvclpwzs3brc&issuer=Google";
            string otpauthUri="";

            var uri = new Uri(otpauthUri);
            var query = uri.Query.TrimStart('?');
            var queryParams = System.Web.HttpUtility.ParseQueryString(query);
            var secretBase32 = queryParams["secret"];

            var secretBytes = Base32Encoding.ToBytes(secretBase32);

            var totp = new OtpNet.Totp(secretBytes, step: 30);

            var otp = totp.ComputeTotp();

            //OtpCode.Text = otp.ToString();
        }
    }
}

