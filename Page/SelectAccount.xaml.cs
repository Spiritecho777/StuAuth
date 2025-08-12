using OtpNet;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace StuAuth
{
    public partial class SelectAccount : System.Windows.Controls.Page
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
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public static string Base32Encode(byte[] data)
        {
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            StringBuilder result = new StringBuilder((data.Length * 8 + 4) / 5);

            int buffer = data[0];
            int next = 1;
            int bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < 5)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xFF;
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = 5 - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }

                int index = 0x1F & (buffer >> (bitsLeft - 5));
                bitsLeft -= 5;
                result.Append(base32Chars[index]);
            }
            return result.ToString();
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
