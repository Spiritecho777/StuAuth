using OtpNet;
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
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace MFA
{
    public partial class SelectAccount : Page
    {
        private String accountName;
        private String OtpUri;

        public SelectAccount(String Name,String Otp)
        {
            InitializeComponent();
            accountName = Name;
            OtpUri = Otp;
            GenerateOtp();
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

            MDP.Content=otp;

            essai.Text = "https://accounts.google.com/v3/signin/challenge/totp?TL=AEzbmxwGvaQ1wRBBEC0Vp97gN32jSI2vDXYKLMfDmCwVykDkiEAdm-SYpAGrsSK6&checkConnection=youtube%3A290&checkedDomains=youtube&cid=2&continue=https%3A%2F%2Fmail.google.com%2Fmail%2F&ddm=0&dsh=S-184504961%3A1711031620385582&flowEntry=ServiceLogin&flowName=GlifWebSignIn&ifkv=ARZ0qKKB_KD9zeTZykGcYkVJJH5KoiELQo0LKOw-1WP1k0LmaMvPqGp3dr1xnaJglm0o0ApabzHboQ&pstMsg=1&rip=1&service=mail&theme=mn";

            /*if (!string.IsNullOrEmpty(essai.Text))
            {
                IWebDriver driver = new FirefoxDriver();

                driver.Navigate().GoToUrl(essai.Text);

                IWebElement searchBox = driver.FindElement(By.CssSelector("input[type='text']"));

                searchBox.SendKeys(MDP.Content.ToString());

                driver.Quit();
            }*/
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Copy_Click(object sender, RoutedEventArgs e) 
        {
            Clipboard.SetData(DataFormats.Text, (Object)MDP.Content);
        }
    }
}
