using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace LSRS.FTP
{
    public class FtpWeb
    {
        public string ftpRemotePath;
        public string ftpUserID;
        public string ftpPassword;
        public string ftpURI;
        public string ftpPath;
        /// <summary>  
        /// 连接FTP  
        /// </summary>  
        /// <param name="FtpServerIP">FTP连接地址</param>  
        /// <param name="FtpRemotePath">指定FTP连接成功后的当前目录, 如果不指定即默认为根目录</param>  
        /// <param name="FtpUserID">用户名</param>  
        /// <param name="FtpPassword">密码</param>  
        public FtpWeb(string FtpURI, string FtpRemotePath, string FtpUserID, string FtpPassword)
        {
            //    ftpServerIP = FtpServerIP;
            ftpRemotePath = FtpRemotePath;
            ftpUserID = FtpUserID;
            ftpPassword = FtpPassword;
            // ftpURI = "ftp://" + ftpServerIP + "/" + ftpRemotePath + "/";
            //  ftpURI = "ftp://" + ftpServerIP + "/" ;
            ftpURI = FtpURI;
            ftpPath = ftpURI + ftpRemotePath + "/";
        }
        public FtpWeb()
        {
        }
        /// <summary>  
        /// 获取FTP文件列表包括文件夹  
        /// </summary>  
        /// <returns></returns>  
        public string[] GetAllList(string url, string userid, string password,ref string errMsg)
        {
            List<string> list = new List<string>();
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(new Uri(url));
            req.Credentials = new NetworkCredential(userid, password);
            req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            req.UseBinary = true;
            req.UsePassive = false;
            req.Timeout = int.MaxValue;
            try
            {
                using (FtpWebResponse res = (FtpWebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(res.GetResponseStream(), System.Text.Encoding.UTF8))//这里encoding.Default防止中文乱码
                    {
                        string s;
                        while ((s = sr.ReadLine()) != null)
                        {
                            list.Add(s);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
            }
            return list.ToArray();
        }
    }
}
