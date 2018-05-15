﻿using MediaCleaner.APIClients;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MediaCleaner.Views
{
    /// <summary>
    /// Interaction logic for LoginEmby_password.xaml
    /// </summary>
    public partial class LoginEmby_password : Window
    {
        EmbyClient embyApi;
        TextBox usernameTB;
        PasswordBox passwordTB;
        TextBlock wrongpw;
        public bool LoginSuccessful = false;
        public string username = "";

        public LoginEmby_password(string username)
        {
            InitializeComponent();
            embyApi = new EmbyClient();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MediaCleaner.Resource." + "icon_running.ico"))
            {
                this.Icon = BitmapFrame.Create(stream);
            }

            usernameTB = (TextBox)this.FindName("uname");
            passwordTB = (PasswordBox)this.FindName("pw");
            wrongpw = (TextBlock)this.FindName("wpw");

            usernameTB.Text = username;
            passwordTB.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var embyaccesstoken = embyApi.getAccessToken(usernameTB.Text, passwordTB.Password);

            if (embyaccesstoken == "")
            {
                Log.Error("Trying to log in failed.");
                wrongpw.Visibility = Visibility.Visible;
            }
            else
            {
                Config.embyAccessToken = embyaccesstoken;
                LoginSuccessful = true;
                this.Close();
            }
        }
    }
}