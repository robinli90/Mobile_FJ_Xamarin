
using System;

namespace Mobile_FJ
{
    public interface IFtpWebRequest
    {
        bool FTPWrite(string ftpFileName, string localFileName, string ftpPathOverride="");

        string FTPRead(string ftpFileName, string ftpPathOverride = "");
    }
}
