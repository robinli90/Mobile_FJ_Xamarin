using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mobile_FJ.Objects_and_Classes;
using Xamarin.Forms;

namespace Mobile_FJ
{
    public partial class CodeVerification : ContentPage
    {
        public CodeVerification()
        {
            InitializeComponent();
        }

        private void ValidateLogin_OnClick(object sender, EventArgs e)
        {
            // Pull FTP Code
            string ftpPathOverride =
                @"ftp://robinli.asuscomm.com/Seagate_Backup_Plus_Drive/Personal%20Banker/Cloud_Sync/Authentication/";
                      ;

            string internalCode = "";

            try
            {
                List<string> lines = new List<string>();
                // Copy local repository to FTP Server
                if (Device.OS == TargetPlatform.Android || Device.OS == TargetPlatform.iOS)
                {
                    // Read from FTP File
                    //string[] lines = GetFileContent(emailAddress + SyncFileName)
                    lines = AESGCM.SimpleDecryptWithPassword(DependencyService.Get<IFtpWebRequest>().FTPRead(Global.GetSettingsValue("SYNC_EMAIL").ToLower() + "_auth.pbf", ftpPathOverride), Global.AESGCMKey)
                        .Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToList();
                }

                

                internalCode = lines[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (codeTextBox.Text == internalCode)
            {
                DisplayAlert("Account validated!",
                    "Your account has been verified and connected with desktop application", "Proceed");
                // Accept login
                Global.SetSettingsValue("EMAIL_VALIDATED", "1");
                Global.isLoggedIn = true;
                Navigation.PushModalAsync(new MainPage());
            }
            else
            {
                DisplayAlert("Error",
                    "Incorrect authentication code", "OK");
            }
        }
    }
}
