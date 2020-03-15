using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DotNet.Utilities;
using System.Threading;
using System.IO;
using DevExpress.XtraTreeList.Nodes;
using LSRS.FTP;
using DevExpress.Spreadsheet;
using System.Management;
using ZtTools;
using ESucess;

namespace LSRS
{
    public partial class FrmLSRS : XtraForm
    {
        public FrmLSRS()
        {
            InitializeComponent();
        }

        /// <summary>
        /// FTP对象
        /// </summary>
        FTPHelper ftp;

        /// <summary>
        /// 
        /// </summary>
        FtpWeb ftpWeb = null;
        /// <summary>
        /// 服务器版本号
        /// </summary>
        private string _newVersion;
        /// <summary>
        /// 当前版本号
        /// </summary>
        private string _currVersion;

        /// <summary>
        /// 加载窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmLSRS_Load(object sender, EventArgs e)
        {
            try
            {
                ftp = new FTPHelper("byw3132430001.my3w.com", "", "byw3132430001", "000000mmm...");
                barStaticItem1.Caption = "服务器连接成功";
                
                // 目录树
                DownFileInfo();
                treeView1.ExpandAll();
                // 广告位文件下载
                ftp.GotoDirectory("Image", true);
                byte[] head = new byte[] { 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6 };
                ftp.Download(Application.StartupPath + "\\Temp", "广告.jpg",head);
                Stream stream = ftp.FileToStream(Application.StartupPath + "\\Temp\\广告.jpg");
                barLargeButtonItem1.ImageOptions.Image = Image.FromStream(stream);
                Thread thread = new Thread(InitInfo);
                thread.IsBackground = true;
                thread.Start();

                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                barStaticItem1.Caption = "服务器连接失败";
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDefaultPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                xtraTabControl1.SelectedTabPage = tabPageWeb;
                Uri strUrl = new Uri("http://192.168.8.238/dgflow/Login.aspx?flag=1&Redir=false");
                webBrowser1.Url = strUrl;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 联系客服
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnContactService_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                xtraTabControl1.SelectedTabPage = tabPageWeb;
                Uri strUrl = new Uri("http://192.168.8.238/dgflow/Login.aspx?flag=1&Redir=false");
                webBrowser1.Url = strUrl;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReg_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                xtraTabControl1.SelectedTabPage = tabPageWeb;
                Uri strUrl = new Uri("http://192.168.8.238/dgflow/Login.aspx?flag=1&Redir=false");
                webBrowser1.Url = strUrl;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 初始化目录树
        /// </summary>
        private void DownFileInfo()
        {
            try
            {
                TreeNode preNode = treeView1.Nodes["Node0"];
                preNode.Nodes.Clear();
                ftpWeb = new FtpWeb("byw3132430001.my3w.com", null, "byw3132430001", "000000mmm...");//实际应用
                ftpWeb.ftpPath = "ftp://" + "byw3132430001.my3w.com" + "/" + "目录树" + "/";
                GetDir(ftpWeb.ftpPath, ftpWeb.ftpUserID, ftpWeb.ftpPassword, preNode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 获取目录
        /// </summary>
        /// <param name="ftpPath"></param>
        /// <param name="ftpUserID"></param>
        /// <param name="ftpPassword"></param>
        /// <param name="preNode"></param>
        private void GetDir(string ftpPath, string ftpUserID, string ftpPassword, TreeNode preNode)
        {
            string[] listArray = ftpWeb.GetAllList(ftpPath, ftpUserID, ftpPassword);
            TreeNode TempNode = preNode;
            List<string> dirList = new List<string>();
            List<string> fileList = new List<string>();
            if (listArray.Length != 0)
            {
                for (int i = 0; i < listArray.Length; i++)//去除文件
                {
                    if ((listArray[i].Remove(29)).Contains("<DIR>"))//判断是否是文件夹,截取前29位信息,如果包含<DIR>则是文件夹
                    {
                        dirList.Add(listArray[i].Remove(0, 39));
                    }
                    else
                        fileList.Add(listArray[i].Remove(0, 39));
                }
                if (dirList.Count != 0)
                {
                    foreach (string dir in dirList)
                    {
                        TreeNode node = new TreeNode(dir);
                        node.Tag = "DIR";
                        TempNode.Nodes.Add(node);
                        preNode = node;
                        GetDir(ftpPath + dir + "/", ftpUserID, ftpPassword, preNode);
                    }
                }
                if (fileList.Count > 0)
                {
                    foreach (string file in fileList)
                    {
                        TreeNode node = new TreeNode(file);
                        node.Tag = "FILE";
                        TempNode.Nodes.Add(node);
                        preNode = TempNode.Nodes[file];
                    }
                }
            }
        }

        /// <summary>
        /// 双击打开相应的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (e.Node.Tag.ToString() == "FILE")
                {
                    string filePath = Application.StartupPath + "\\" + e.Node.FullPath;
                    FileInfo fileInfo = new FileInfo(filePath);

                    //文件夹路径不存在创建文件夹路径
                    if (!fileInfo.Directory.Exists)
                        fileInfo.Directory.Create();
                    // 文件不存在 下载文件
                    //if (!fileInfo.Exists || fileInfo.Length == 0)
                    {
                        // 切换路径
                        ftp.GotoDirectory(e.Node.Parent.FullPath, true);
                        //下载文件
                        if (e.Node.Text.Contains(".txt"))
                            ftp.DownloadTXT(Application.StartupPath + "\\" + e.Node.Parent.FullPath, e.Node.Text);
                        else
                        {
                            if (e.Node.Text.Contains(".pdf"))
                            {
                                ftp.DownloadPDF(Application.StartupPath + "\\" + e.Node.Parent.FullPath, e.Node.Text);
                            }
                            else
                            {
                                byte[] head = new byte[] { 0x00, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
                                ftp.Download(Application.StartupPath + "\\" + e.Node.Parent.FullPath, e.Node.Text, head);
                            } 
                        }
                    }
                    // 根据文件类型选择不同的打开方式
                    fileInfo = new FileInfo(filePath);
                    string fileExt = fileInfo.Extension; // 文件后缀名
                    switch (fileExt)
                    {
                        case ".xlsx":
                            // 打开文件
                            xtraTabControl1.SelectedTabPage = tabPageExcel;
                            IWorkbook workbookxlsx = spreadsheetControl1.Document;
                            Stream streamxlsx = ftp.FileToStream(filePath);
                            if (streamxlsx != null)
                                workbookxlsx.LoadDocument(streamxlsx, DocumentFormat.Xlsx);
                            else
                                MessageBox.Show("文件打开失败");
                            break;
                        case ".xls":
                            // 打开文件
                            xtraTabControl1.SelectedTabPage = tabPageExcel;
                            IWorkbook workbook = spreadsheetControl1.Document;
                            Stream streamxls = ftp.FileToStream(filePath);
                            if (streamxls != null)
                                workbook.LoadDocument(streamxls, DocumentFormat.Xls);
                            else
                                MessageBox.Show("文件打开失败");
                            break;
                        case ".doc":
                            xtraTabControl1.SelectedTabPage = tabPageWord;
                            Stream streamdoc = ftp.FileToStream(filePath);
                            if (streamdoc != null)
                                richEditControl1.LoadDocument(streamdoc, DevExpress.XtraRichEdit.DocumentFormat.Doc);
                            else
                                MessageBox.Show("文件打开失败");
                            break;
                        case ".docx":
                            xtraTabControl1.SelectedTabPage = tabPageWord;
                            Stream streamdocx = ftp.FileToStream(filePath);
                            if (streamdocx != null)
                                richEditControl1.LoadDocument(streamdocx, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
                            else
                                MessageBox.Show("文件打开失败");
                            break;
                        case ".pdf":
                            xtraTabControl1.SelectedTabPage = tabPagePdf;
                            pdfViewer1.LoadDocument(filePath);
                            break;
                        case ".txt":
                            xtraTabControl1.SelectedTabPage = tabPageText;
                            txtInfo.Text = "";
                            string str = File.ReadAllText(filePath, Encoding.Default);
                            string data = Encrypt.DecryptDES(str, "12345678");
                            txtInfo.Text = data;
                            break;
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
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                SearchNode(treeView1.Nodes[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="node"></param>
        private void SearchNode(TreeNode node)
        {
            if (node.Text.Contains(txtSearchText.Text.Trim()))
            {
                treeView1.SelectedNode = node;
                node.BackColor = Color.FromArgb(51,153,255);
                return;
            }
            else
                node.BackColor = Color.White;
            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    SearchNode(n);
                }
            }
            else
            {
                if (node.Text.Contains(txtSearchText.Text.Trim()))
                {
                    treeView1.SelectedNode = node;
                    node.BackColor = Color.FromArgb(51, 153, 255);
                    return;
                }
                else
                    node.BackColor = Color.White;
            }
        }

        /// <summary>获取机器指纹</summary>
        /// <returns>机器指纹</returns>
        private string GetFingerprint()
        {
            string fingerprint = string.Empty;
            //获取CPU序列号
            ManagementObjectSearcher MOSProcessor = new ManagementObjectSearcher("Select ProcessorID From Win32_Processor");
            ManagementObjectCollection OperatingSystems = MOSProcessor.Get();
            foreach (var OperatingSystem in OperatingSystems)
            {//获取CPU序列号
                fingerprint += OperatingSystem["ProcessorID"].ToString();
            }
            //获取网络适配器MAC地址
            //ManagementObjectSearcher MOSMACAddress = new ManagementObjectSearcher("Select * From Win32_NetworkAdapterConfiguration");
            //ManagementObjectCollection MACAddresses = MOSMACAddress.Get();
            //foreach (var MACAddress in MACAddresses)
            //{//获取网卡MAC地址
            //    if (!(bool)MACAddress["IPEnabled"]) continue;
            //    fingerprint += MACAddress["MACAddress"].ToString().Replace(":", "");
            //}
            return fingerprint;
        }

        /// <summary>
        ///  初始化信息 上传客户端信息（唯一标识符 IP地址 等）(统计安装人数)
        /// </summary>
        private void InitInfo()
        {
            string fileName = GetFingerprint();
            List<string> ClientInfo=new List<string>();
            string filePath = Application.StartupPath + "\\Temp\\" + fileName+".txt";
            FileInfo fileInfo = new FileInfo(filePath);

            //文件夹路径不存在创建文件夹路径
            if (!fileInfo.Exists)
                fileInfo.Create();
            ClientInfo.Add(fileName);
            ZtTools.Tools.WriteToTxt(filePath, ClientInfo);
            ftp.GotoDirectory("Upload", true);
            ftp.Upload(filePath);
        }

        #region 弃用
        bool loadDrives = false;

        string path = string.Empty;

        private void treeList1_VirtualTreeGetChildNodes(object sender, DevExpress.XtraTreeList.VirtualTreeGetChildNodesInfo e)
        {
            Cursor current = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            if (!loadDrives)
            {
                ftp.GotoDirectory("AAA", false);
                string[] roots = ftp.GetFilesDicList();
                e.Children = roots;
                loadDrives = true;
            }
            else
            {
                try
                {
                    if (!isExpand)
                    {
                        if (e.Node.ToString().Contains(" "))
                        {
                            path = e.Node.ToString().Substring(39, e.Node.ToString().Length - 39);
                        }
                        else
                            path = e.Node.ToString();
                    }
                    else
                        path.Remove(path.LastIndexOf('\\'), 1);
                    ftp.GotoDirectory("AAA\\" + path, true);
                    string[] dirs = ftp.GetFilesDetailList();
                    string[] files = ftp.GetFileList("");

                    //文件夹为空
                    if (dirs[0] == "")
                    {
                        string[] arr = new string[files.Length];
                        files.CopyTo(arr, 0);
                        e.Children = arr;
                    }
                    else
                    {
                        if (files[0] == "")
                        {
                            string[] arr = new string[dirs.Length];
                            dirs.CopyTo(arr, 0);
                            e.Children = arr;
                        }
                        else
                        {
                            string[] arr = new string[dirs.Length + files.Length];
                            dirs.CopyTo(arr, 0);
                            files.CopyTo(arr, dirs.Length);
                            e.Children = arr;
                        }
                    }
                }
                catch { e.Children = new object[] { }; }
            }
            Cursor.Current = current;
        }

        private void treeList1_VirtualTreeGetCellValue(object sender, DevExpress.XtraTreeList.VirtualTreeGetCellValueInfo e)
        {
            try
            {
                //if (!string.IsNullOrEmpty(e.Node.ToString()))
                //{
                //    if (e.Node.ToString().Contains(" "))
                //    {
                //        if (e.Node.ToString().Contains("<DIR>"))
                //        {
                //            DirectoryInfo di = new DirectoryInfo(e.Node.ToString().Substring(39, e.Node.ToString().Length - 39));
                //            if (e.Column == colName)
                //                e.CellData = di.Name;
                //        }
                //        else
                //        {
                //            if (e.Column == colName)
                //                e.CellData = e.Node.ToString().Substring(39, e.Node.ToString().Length - 39);
                //        }
                //    }
                //    else
                //    {
                //        if (e.Node.ToString().Contains("<DIR>"))
                //        {
                //            DirectoryInfo di = new DirectoryInfo(e.Node.ToString());
                //            if (e.Column == colName)
                //                e.CellData = di.Name;
                //        }
                //        else
                //        {
                //            if (e.Column == colName)
                //                e.CellData = e.Node.ToString();
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        private void GetNodeFullName(TreeListNode node)
        {
            if (node.ParentNode != null)
                GetNodeFullName(node.ParentNode);
            else
            {
                path += node.GetDisplayText(0) + "\\";
            }
        }

        bool isExpand = false;
        private void treeList1_BeforeExpand(object sender, DevExpress.XtraTreeList.BeforeExpandEventArgs e)
        {
            isExpand = true;
            path = string.Empty;
            GetNodeFullName(e.Node);
        }
        #endregion

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DownFileInfo();
            treeView1.ExpandAll();
        }

        /// <summary>
        /// 广告位链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barLargeButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            xtraTabControl1.SelectedTabPage = tabPageWeb;
            Uri strUrl = new Uri("http://192.168.8.238/dgflow/Login.aspx?flag=1&Redir=false");
            webBrowser1.Url = strUrl;
        }

        /// <summary>
        /// 弹屏功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                ftp.GotoDirectory("弹屏文件", true);
                if (ftp.DownloadDecode(Application.StartupPath + "\\弹屏文件", "version.ini"))
                {
                    // 获取本地版本号
                    var iniCur = new IniClass(Application.StartupPath + @"\Version.ini");
                    _currVersion=iniCur.IniReadValue("Update", "version");
                    // 获取服务器版本号
                    var iniNew = new IniClass(Application.StartupPath + @"\弹屏文件\version.ini");
                    _newVersion = iniNew.IniReadValue("Update", "version");
                    // 计算版本号
                    int CurrStr = Convert.ToInt32(_currVersion.Replace(".", "").Trim());
                    int NewStr = Convert.ToInt32(_newVersion.Replace(".", "").Trim());
                    if (CurrStr < NewStr)
                    {
                        // 更新本地版本号
                        iniCur.IniWriteValue("Update", "version", _newVersion);
                        List<string> alert = new List<string>();
                        ZtTools.Tools.ReadFileByLine(Application.StartupPath + "\\弹屏文件\\AlertText.txt", out alert);
                        StringBuilder sb = new StringBuilder();
                        foreach (string str in alert)
                            sb.AppendLine(str);
                        alertControl1.Show(this, "温馨提示", sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("505"))
                {
                    MessageBox.Show(ex.Message);
                    ZtTools.Tools.Error(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 弹框点击链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void alertControl1_AlertClick(object sender, DevExpress.XtraBars.Alerter.AlertClickEventArgs e)
        {
            string text = e.Info.Text;
            System.Diagnostics.Process.Start("http://192.168.8.238/dgflow/Login.aspx?flag=1&Redir=false");
        }
    }
}
