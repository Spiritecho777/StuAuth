﻿using System.Text;
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
using StuAuth;
using ZXing;

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