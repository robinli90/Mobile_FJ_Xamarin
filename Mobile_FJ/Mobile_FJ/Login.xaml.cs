using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mobile_FJ.Objects_and_Classes;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;

namespace Mobile_FJ
{
    public partial class Login : ContentPage
    {

        public Login()
        {
            InitializeComponent();

            if (Global.GetSettingsValue("SYNC_EMAIL").Length > 0)
            {
                emailBox.Text = Global.GetSettingsValue("SYNC_EMAIL");

                if (!Global.isOnWifi()) // only try on WiFi (most data will block this)
                {
                    emailBox.IsEnabled = false;
                }
            }
        }

        private void LoginButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                if (emailBox != null && emailBox.Text.Length > 0 && emailBox.Text.Contains("@") &&
                    emailBox.Text.Contains(".") && Global.GetSettingsValue("EMAIL_VALIDATED") == "1" && emailBox.Text.ToLower() == Global.GetSettingsValue("SYNC_EMAIL"))
                {
                    Global.SetSettingsValue("SYNC_EMAIL", emailBox.Text);
                    Global.isLoggedIn = true;
                    Navigation.PushModalAsync(new MainPage());
                }
                else if (emailBox != null && emailBox.Text.Length > 0 && emailBox.Text.Contains("@") &&
                         emailBox.Text.Contains(".") && (Global.GetSettingsValue("EMAIL_VALIDATED") != "1" || emailBox.Text.ToLower() != Global.GetSettingsValue("SYNC_EMAIL")))
                {
                    DisplayAlert("Authentication required", "Please login to the desktop application and generate an authentication code to continue. Go to File > Mobile Services > Settings > 'Generate Mobile Code'", "OK");
                    Global.SetSettingsValue("SYNC_EMAIL", emailBox.Text);
                    Navigation.PushModalAsync(Global.VerificationPage);
                }
                else
                {
                    DisplayAlert("Email error", "The email provided seems to be invalid", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
