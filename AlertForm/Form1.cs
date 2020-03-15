using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DotNet.Utilities;
using ZtTools;
using System.IO;

namespace LSRS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// FTP对象
        /// </summary>
        FTPHelper ftp;

        /// <summary>
        /// 服务器版本号
        /// </summary>
        private string _newVersion;

        /// <summary>
        /// 当前版本号
        /// </summary>
        private string _currVersion;

        /// <summary>
        /// 弹窗链接
        /// </summary>
        private string _alertLink = string.Empty;

        /// <summary>
        /// 定时检查更新是否需要弹窗
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                ftp.GotoDirectory("弹窗广告", true);
                if (ftp.DownloadDecode(Application.StartupPath + "\\Temp", "advVersion.ini"))
                {
                    // 获取本地版本号
                    var iniCur = new IniClass(Application.StartupPath + @"\version.ini");
                    _currVersion = iniCur.IniReadValue("Update", "version");
                    // 获取服务器版本号
                    var iniNew = new IniClass(Application.StartupPath + @"\Temp\advVersion.ini");
                    _newVersion = iniNew.IniReadValue("Update", "version");
                    // 计算版本号
                    int CurrStr = Convert.ToInt32(_currVersion.Replace(".", "").Trim());
                    int NewStr = Convert.ToInt32(_newVersion.Replace(".", "").Trim());
                    if (CurrStr < NewStr)
                    {
                        ftp.GotoDirectory("Image", true);
                        byte[] head = new byte[] { 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6 };
                        ftp.Download(Application.StartupPath + "\\Temp", "弹窗.png", head);
                        // 更新本地版本号
                        iniCur.IniWriteValue("Update", "version", _newVersion);
                        _alertLink = iniNew.IniReadValue("Update", "alertLink");
                        Stream stream = ftp.FileToStream(Application.StartupPath + "\\Temp\\弹窗.png");
                        FrmAlert alert = new FrmAlert(stream, _alertLink);
                        alert.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 加载窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            ftp = new FTPHelper("byw3132430001.my3w.com/FTPServer", "", "byw3132430001", "000000mmm...");
        }
    }
}
