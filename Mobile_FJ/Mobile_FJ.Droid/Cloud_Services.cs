using System;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;
using Android.Content;
using Android.Net;
using Mobile_FJ.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(CloudServices))]

namespace Mobile_FJ.Droid
{
    class CloudServices : IFtpWebRequest
    {
        private static string ftpUsername = "Guest";
        private static string ftpPassword = "robinisthebest";
        private static string ftpPath =
            @"ftp://robinli.asuscomm.com/Seagate_Backup_Plus_Drive/Personal%20Banker/Cloud_Sync/";



        /// <summary>
        /// Download Cloud Synced file based on filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>bool uploadFail</returns>
        public static bool FTPWrite(string ftpFileName, string localFilePath, string ftpPathOverride="")
        {

            Console.WriteLine("FTP Download start at " + DateTime.Now.TimeOfDay + " (path=" + ftpFileName + ")");

            if (ftpPathOverride.Length == 0)
            {
                ftpPath = @"ftp://robinli.asuscomm.com/Seagate_Backup_Plus_Drive/Personal%20Banker/Cloud_Sync/Sync/" +
                          ftpFileName;
            }
            else
            {
                ftpPath = ftpPathOverride;
            }

            bool Upload_Fail = false;

            // Delete file from FTP
            try
            {
                FtpWebRequest request = (FtpWebRequest) WebRequest.Create(ftpPath);
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse responseDel = (FtpWebResponse) request.GetResponse();
                Console.WriteLine("Delete status: {0}", responseDel.StatusDescription);
                responseDel.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FTP Delete error: " + ex.ToString());
            }
            // Login log to FTP server
            try
            {

                FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(ftpPath);
                requestDir.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                requestDir.Method = WebRequestMethods.Ftp.UploadFile;

                try
                {
                    // Copy the contents of the file to the request stream.  
                    StreamReader sourceStream = new StreamReader(localFilePath);
                    byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    sourceStream.Close();
                    requestDir.ContentLength = fileContents.Length;

                    Stream requestStream = requestDir.GetRequestStream();
                    requestStream.Write(fileContents, 0, fileContents.Length);
                    requestStream.Close();

                    FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();

                    Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                    response.Close();
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ez)
            {
                Console.WriteLine("FTP ERROR : " + ez.ToString());
                // FTP Error
            }

            Console.WriteLine("FTP Download Thread end at " + DateTime.Now.TimeOfDay);

            return Upload_Fail;
        }

        /// <summary>
        /// Read file from FTP Server and return string
        /// </summary>
        /// <param name="ftpFileName"></param>
        /// <returns></returns>
        public static string FTPRead(string ftpFileName, string ftpPathOverride = "")
        {
            // Enable path override
            if (ftpPathOverride.Length == 0)
            {
                ftpPath = @"ftp://robinli.asuscomm.com/Seagate_Backup_Plus_Drive/Personal%20Banker/Cloud_Sync/Sync/" +
                          ftpFileName;
            }
            else
            {
                ftpPath = ftpPathOverride + ftpFileName;
            }


            Console.WriteLine("FTP Reading Cloud start at " + DateTime.Now.TimeOfDay);

            TimeSpan Start_Time = DateTime.Now.TimeOfDay;

            // Login log to FTP server
            try
            {
                if (FTP_Check_File_Exists(ftpPath))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                        // try to download log from client
                        try
                        {
                            byte[] newFileData = client.DownloadData(ftpPath);
                            string fileString = System.Text.Encoding.UTF8.GetString(newFileData);
                            
                            return fileString;
                        }
                        catch (Exception ez)
                        {
                            Debug.WriteLine("FTP ERROR : " + ez.ToString());
                            return "";
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("File not found on FTP Server: " + ftpPath);
                }
            }
            catch (Exception ez)
            {
                Debug.WriteLine("FTP ERROR : " + ez.ToString());
                return "";
                // FTP Error
            }

            Console.WriteLine("FTP Read End Thread end at " + DateTime.Now.TimeOfDay);

            return "";
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="ftpPath"></param>
        /// <returns></returns>
        public static bool FTP_Check_File_Exists(string ftpPath)
        {

            #region Check if FTP Path exists
            var request = (FtpWebRequest)WebRequest.Create(ftpPath);
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode ==
                    FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
                }
            }

            return true;
            #endregion
        }


        #region Interface methods

        string IFtpWebRequest.FTPRead(string ftpFileName, string ftpPathOverride = "")
        {
            return FTPRead(ftpFileName, ftpPathOverride);
        }

        bool IFtpWebRequest.FTPWrite(string ftpFileName, string localFilePath, string ftpPathOverride = "")
        {
            return FTPWrite(ftpFileName, localFilePath, ftpPathOverride);
        }

        #endregion
    }
}
