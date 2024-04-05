using OtpNet;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace MFA
{
    public partial class SelectAccount : Page
    {
        private String accountName;
        private String OtpUri;
        private DispatcherTimer timer;

        public SelectAccount(String Name, String Otp)
        {
            InitializeComponent();
            accountName = Name;
            OtpUri = Otp;
            AccountName.Content = accountName;
            GenerateOtp();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void GenerateOtp()
        {
            string otpauthUri = OtpUri;

            var uri = new Uri(otpauthUri);
            var query = uri.Query.TrimStart('?');
            var queryParams = System.Web.HttpUtility.ParseQueryString(query);
            var secretBase32 = queryParams["secret"];

            var secretBytes = Base32Encoding.ToBytes(secretBase32);

            var totp = new OtpNet.Totp(secretBytes, step: 30);

            var otp = totp.ComputeTotp();

            MDP.Content = otp;

            var timeRemaining = 30 - (DateTime.UtcNow.Second % 30);
            TempsRestant.Content = timeRemaining;



            /*essai.Text = "https://edi-log.eu.itglue.com/";

            string javascriptCode2 = @"
                                var TFT = document.querySelectorAll('input[type=text]');
                                if (TFT.length > 0) {
                                    for (var i = 0; i < TFT.length; i++) {
                                        TFT[i].value = arguments[0];
                                    }
                                }";*/
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, (Object)MDP.Content);
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            GenerateOtp();
        }

    }
}
