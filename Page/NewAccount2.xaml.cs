using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using OtpNet;
using System.Buffers.Text;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using System.Security.Cryptography;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using Migration;


namespace MFA
{
    public partial class NewAccount2 : Page
    {
        private string otpauth;
        private string secret;
        private Main Menu;

        public NewAccount2(string OtpAuth,Main menu)
        {
            InitializeComponent();
            otpauth = OtpAuth;
            Menu = menu;

            if (otpauth.Contains("migration"))
            {
                int dataIndex = otpauth.IndexOf("data=") + "data=".Length;
                string base64Data = otpauth.Substring(dataIndex);

                byte[] bytes = Convert.FromBase64String(base64Data);
                ByteString payload = DecodeProtobuf(bytes);
                
                secret = Base32Encode(payload, false);
                otpauth = "otpauth://totp/?secret=" + secret + "&digits=6&period=30";
            }
        }

        public dynamic DecodeProtobuf(byte[] payload)
        {
            var parser = new MessageParser<Payload>(() => new Payload());
            var message = parser.ParseFrom(payload);
            ByteString secret = message.OtpParameters[0].Secret;
            return secret;
        }

        public static string Base32Encode(ByteString data, bool padding)
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
            if (padding)
            {
                while (result.Length % 8 != 0)
                {
                    result.Append("=");
                }
            }
            return result.ToString();
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
                    otpauth = part[0] + "/" + part[1] + "/" + part[2] + "/" + AccountName.Text + "/" + part[3];
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
