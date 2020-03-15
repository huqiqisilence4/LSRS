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
using DevExpress.XtraBars;
using System.Security.AccessControl;
using DevExpress.XtraTab;

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
        /// 弹窗链接
        /// </summary>
        private string _alertLink = string.Empty;

        /// <summary>
        /// 广告位链接
        /// </summary>
        private string _alertAdvertLink = string.Empty;

        /// <summary>
        /// 目录树数据源
        /// </summary>
        private DataTable _dt = new DataTable();

        /// <summary>
        /// 筛选条件
        /// </summary>
        private string _filter = string.Empty;

        /// <summary>
        /// 加载窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmLSRS_Load(object sender, EventArgs e)
        {
            try
            {
                ftp = new FTPHelper("byw3132430001.my3w.com/FTPServer", "", "byw3132430001", "000000mmm...");
                //ftp = new FTPHelper("192.168.8.20", "", "Administrator", "ZT101977");

                DataColumn ID = new DataColumn("ID", Type.GetType("System.Int32"));
                DataColumn ParentID = new DataColumn("ParentID", Type.GetType("System.Int32"));
                DataColumn Name = new DataColumn("Name", Type.GetType("System.String"));
                DataColumn FullPath = new DataColumn("FullPath", Type.GetType("System.String"));
                DataColumn FileType = new DataColumn("Type", Type.GetType("System.String"));
                _dt.Columns.Add(ID);
                _dt.Columns.Add(ParentID);
                _dt.Columns.Add(Name);
                _dt.Columns.Add(FullPath);
                _dt.Columns.Add(FileType);

                treeList1.Appearance.SelectedRow.BackColor = Color.FromArgb(30, 0, 0, 240);
                treeList1.Appearance.FocusedRow.BackColor = Color.FromArgb(60, 0, 0, 240);

                // 目录树
                IniClass iniCur = null;
                if (CheckUpdate(ref iniCur))
                {
                    if (DialogResult.Yes == MessageBox.Show("检测到有新的版本，是否需要更新。", "温馨提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        // 更新本地版本号
                        iniCur.IniWriteValue("Update", "version", _newVersion);
                        splashScreenManager1.ShowWaitForm();
                        // 从服务器下载数据更新目录树
                        UpdateTreeByServer();
                        //string updateFile = iniCur.IniReadValue("Update", "UpdateFile");
                        //UpdateTreeByServer(updateFile);
                    }
                    else
                    {
                        // 从本地加载数据 更新目录树
                        splashScreenManager1.ShowWaitForm();
                        UpdateTreeByLocal();
                    }
                }
                else
                {
                    // 从本地加载数据 更新目录树
                    splashScreenManager1.ShowWaitForm();
                    UpdateTreeByLocal();
                }
                this.treeList1.Nodes[0].Expanded = true;
                btnLargeDefaultPage_ItemClick(null, null);
                //treeView1.ExpandAll();

                // 显示首页
                Thread thread = new Thread(InitInfo);
                thread.IsBackground = true;
                thread.Start();
                barStaticItem1.Caption = "服务器连接成功";
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
            }
            catch (Exception ex)
            {
                barStaticItem1.Caption = "服务器连接失败";
                MessageBox.Show("服务器加载数据异常，请重新启动客户端程序"+ex.ToString());
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLargeDefaultPage_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.ShowWaitForm();
                ftp.GotoDirectory("弹窗广告", true);
                if (ftp.DownloadDecode(Application.StartupPath + "\\Temp", "advVersion.ini"))
                {
                    // 获取服务器版本号
                    var iniNew = new IniClass(Application.StartupPath + @"\Temp\advVersion.ini");
                    string defaultPageLink = iniNew.IniReadValue("Update", "DefaultPageLink");
                    xtraTabControl1.SelectedTabPage = tabPageWeb;
                    Uri strUrl = new Uri(defaultPageLink);
                    webBrowser1.Url = null;
                    webBrowser1.Url = strUrl;
                }
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
            }
            catch (Exception ex)
            {
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
                //MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 联系客服
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLargeContactService_ItemClick(object sender, ItemClickEventArgs e)
        {
            string contactServiceLink = string.Empty;
            try
            {
                if (!splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.ShowWaitForm();
                ftp.GotoDirectory("弹窗广告", true);
                if (ftp.DownloadDecode(Application.StartupPath + "\\Temp", "advVersion.ini"))
                {
                    // 获取服务器版本号
                    var iniNew = new IniClass(Application.StartupPath + @"\Temp\advVersion.ini");
                    contactServiceLink = iniNew.IniReadValue("Update", "ContactServiceLink");
                    xtraTabControl1.SelectedTabPage = tabPageWebQQ;
                    Uri strUrl = new Uri(contactServiceLink);
                    webBrowser2.Navigate(contactServiceLink);
                }
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
            }
            catch (Exception ex)
            {
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
                tabPageWebQQ.Controls.Remove(webBrowser2);
                WebBrowser webBrowser = new WebBrowser();
                webBrowser.Name = "webBrowser2";
                webBrowser.Dock = DockStyle.Fill;
                tabPageWebQQ.Controls.Add(webBrowser);
                webBrowser.Navigate(contactServiceLink);
                //MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLargeReg_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.ShowWaitForm();
                ftp.GotoDirectory("弹窗广告", true);
                if (ftp.DownloadDecode(Application.StartupPath + "\\Temp", "advVersion.ini"))
                {
                    // 获取服务器版本号
                    var iniNew = new IniClass(Application.StartupPath + @"\Temp\advVersion.ini");
                    string regLink= iniNew.IniReadValue("Update", "RegLink");
                    xtraTabControl1.SelectedTabPage = tabPageWeb;
                    Uri strUrl = new Uri(regLink);
                    webBrowser1.Url = strUrl;
                }
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
            }
            catch (Exception ex)
            {
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        ///  从服务器更新 目录树
        /// </summary>
        private void UpdateTreeByServer()
        {
            DataTable dt=_dt.Copy();
            _dt.Rows.Clear();
            DataRow drroot = _dt.NewRow();
            drroot[0] = 0;
            drroot[1] = -1;
            drroot[2] = "目录树";
            drroot[3] = "";
            _dt.Rows.Add(drroot);
            id = 1;
            // 从服务器更新 下载目录树
            ftpWeb = new FtpWeb("byw3132430001.my3w.com", "", "byw3132430001", "000000mmm...");//实际应用
            ftpWeb.ftpPath = "ftp://" + "byw3132430001.my3w.com/FTPServer" + "/" + "目录树" + "/";
            //ftpWeb = new FtpWeb("192.168.8.20", "", "Administrator", "ZT101977");//实际应用
            //ftpWeb.ftpPath = "ftp://" + "192.168.8.20" + "/" + "目录树" + "/";
            //GetDir(ftpWeb.ftpPath, ftpWeb.ftpUserID, ftpWeb.ftpPassword, preNode);
            errMsg=string.Empty;
            GetDir(ftpWeb.ftpPath, ftpWeb.ftpUserID, ftpWeb.ftpPassword, 0);
            if (string.IsNullOrEmpty(errMsg))
            {
                barStaticItem1.Caption = "服务器连接成功";
                this.treeList1.DataSource = null;
                this.treeList1.DataSource = _dt;
            }
            else
            {
                MessageBox.Show("更新数据失败,请重启客户端更新数据。", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ZtTools.Tools.Error(errMsg);
                UpdateTreeByLocal();
            }
        }

        /// <summary>
        ///  从服务器更新 目录树
        /// </summary>
        private void UpdateTreeByServer(string filePath)
        {
            ftp.GotoDirectory(filePath, true);
            //switch()
        }

        /// <summary>
        /// 从本地加载目录树
        /// </summary>
        private void UpdateTreeByLocal()
        {
            _dt.Rows.Clear();
            DataRow drroot = _dt.NewRow();
            drroot[0] = 0;
            drroot[1] = -1;
            drroot[2] = "目录树";
            drroot[3] = "";
            _dt.Rows.Add(drroot);
            id = 1;
            GetDir(Application.StartupPath + "\\目录树\\", 0);
            this.treeList1.DataSource = _dt;
        }

        /// <summary>
        /// 初始化目录树
        /// </summary>
        private void DownFileInfo()
        {
            try
            {
                // 检查是否有更新（没有更新读取本地目录树启动程序 有更新提示用户是否需要更新）
                //TreeNode preNode = treeView1.Nodes["Node0"];
                //preNode.Nodes.Clear();

                //_dt.Rows.Clear();
                //DataRow drroot = _dt.NewRow();
                //drroot[0] = 0;
                //drroot[1] = -1;
                //drroot[2] = "目录树";
                //drroot[3] = "";
                //_dt.Rows.Add(drroot);


                //if (CheckUpdate())
                //{
                    
                //    // 关闭进度条显示
                //}
                //else
                //{
                //    splashScreenManager1.ShowWaitForm();
                //    // 从本地文件更新 读取本地目录树
                //    //GetDir(Application.StartupPath + "\\目录树\\", preNode);
                //    GetDir(Application.StartupPath + "\\目录树\\", 0);
                //    this.treeList1.DataSource = _dt;
                //    Thread.Sleep(1000);
                //}
                //if (splashScreenManager1.IsSplashFormVisible)
                //    splashScreenManager1.CloseWaitForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        string errMsg = string.Empty;
        int id = 1;

        /// <summary>
        /// 获取目录（从服务器）
        /// </summary>
        /// <param name="ftpPath"></param>
        /// <param name="ftpUserID"></param>
        /// <param name="ftpPassword"></param>
        /// <param name="preNode"></param>
        private void GetDir(string ftpPath, string ftpUserID, string ftpPassword, TreeNode preNode)
        {
            string[] listArray = ftpWeb.GetAllList(ftpPath, ftpUserID, ftpPassword, ref errMsg);
            if (!string.IsNullOrEmpty(errMsg))
                return;
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
                        // 创建该文件夹到本地
                        string filePath = Application.StartupPath + "\\" + node.FullPath;
                        DirectoryInfo dirInfo = new DirectoryInfo(filePath);
                        //文件夹路径不存在创建文件夹路径
                        if (!dirInfo.Exists)
                            dirInfo.Create();
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
                        // 下载该文件到本地
                        DownLoadDirTree(node);
                    }
                }
            }
        }

        /// <summary>
        /// 获取目录（从服务器）
        /// </summary>
        /// <param name="ftpPath"></param>
        /// <param name="ftpUserID"></param>
        /// <param name="ftpPassword"></param>
        /// <param name="preNode"></param>
        private void GetDir(string ftpPath, string ftpUserID, string ftpPassword, int parentID)
        {
            string[] listArray = ftpWeb.GetAllList(ftpPath, ftpUserID, ftpPassword, ref errMsg);
            if (!string.IsNullOrEmpty(errMsg))
                return;
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
                        // 创建该文件夹到本地
                        int index = ftpPath.LastIndexOf("树")+2;
                        string filePath = Application.StartupPath + "\\目录树\\" + ftpPath.Substring(index, ftpPath.Length-index) + dir;
                        DirectoryInfo dirInfo = new DirectoryInfo(filePath);
                        //文件夹路径不存在创建文件夹路径
                        if (!dirInfo.Exists)
                            dirInfo.Create();
                        // 写入行数据
                        if (!_filter.Contains(dir))
                        {
                            DataRow dr = _dt.NewRow();
                            dr[0] = id++;
                            dr[1] = parentID;
                            int dirValue = 0;
                            if (int.TryParse(dir.Substring(0, 2), out dirValue))
                                dr[2] = dir;
                            else if (int.TryParse(dir.Substring(0, 1), out dirValue))
                                dr[2] = "0" + dir;
                            else
                                dr[2] = dir;
                            dr[3] = filePath;
                            dr[4] = "DIR";
                            _dt.Rows.Add(dr);
                        }
                        // Next
                        GetDir(ftpPath + dir + "/", ftpUserID, ftpPassword, id-1);
                    }
                }
                if (fileList.Count > 0)
                {
                    foreach (string file in fileList)
                    {
                        int index = ftpPath.LastIndexOf("树") + 2;
                        // 下载该文件到本地
                        DownLoadDirTree(file, ftpPath, Application.StartupPath + "\\目录树\\" + ftpPath.Substring(index, ftpPath.Length - index));
                        string fileType=file.Split('.')[1];
                        if (!fileType.Contains("jpg") && !fileType.Contains("png") && !fileType.Contains("bmp") && !fileType.Contains("gif"))
                        {
                            // 写入行数据
                            DataRow dr = _dt.NewRow();
                            dr[0] = id++;
                            dr[1] = parentID;
                            int dirValue = 0;
                            if (int.TryParse(file.Substring(0, 2), out dirValue))
                                dr[2] = file.Split('.')[0];
                            else
                                if (int.TryParse(file.Substring(0, 1), out dirValue))
                                    dr[2] = "0" + file.Split('.')[0];
                                else
                                    dr[2] = file.Split('.')[0];
                            dr[3] = Application.StartupPath + "\\目录树\\" + ftpPath.Substring(index, ftpPath.Length - index) + file;
                            dr[4] = "FILE";
                            _dt.Rows.Add(dr);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取目录（从本地）
        /// </summary>
        /// <param name="ftpPath"></param>
        /// <param name="ftpUserID"></param>
        /// <param name="ftpPassword"></param>
        /// <param name="preNode"></param>
        private void GetDir(string path, TreeNode preNode)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);//创建文件夹信息类实例
            FileInfo[] fileInfo = directoryInfo.GetFiles();
            DirectoryInfo[] dirInfo = directoryInfo.GetDirectories();

            TreeNode TempNode = preNode;
            List<string> dirList = new List<string>();
            List<string> fileList = new List<string>();

            foreach (FileInfo file in fileInfo)
            {
                fileList.Add(file.Name);
            }

            foreach (DirectoryInfo dir in dirInfo)
            {
                dirList.Add(dir.Name);
            }

            if (dirList.Count != 0)
            {
                foreach (string dir in dirList)
                {
                    TreeNode node = new TreeNode(dir);
                    node.Tag = "DIR";
                    TempNode.Nodes.Add(node);
                    preNode = node;
                    GetDir(path + dir + "/", preNode);
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

        /// <summary>
        /// 获取目录（从本地）
        /// </summary>
        /// <param name="ftpPath"></param>
        /// <param name="ftpUserID"></param>
        /// <param name="ftpPassword"></param>
        /// <param name="preNode"></param>
        private void GetDir(string path, int parentID)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);//创建文件夹信息类实例
            FileInfo[] fileInfo = directoryInfo.GetFiles();
            DirectoryInfo[] dirInfo = directoryInfo.GetDirectories();

            //Array.Sort(fileInfo, (x1, x2) => x1.Name.Substring(0,2).PadLeft(10, '0').CompareTo(x2.Name.Substring(0,2).PadLeft(10, '0')));

            List<string> dirList = new List<string>();
            List<string> fileList = new List<string>();

            foreach (FileInfo file in fileInfo)
            {
                fileList.Add(file.Name);
            }

            foreach (DirectoryInfo dir in dirInfo)
            {
                dirList.Add(dir.Name);
            }

            if (dirList.Count != 0)
            {
                foreach (string dir in dirList)
                {
                    //TreeNode node = new TreeNode(dir);
                    //node.Tag = "DIR";
                    //TempNode.Nodes.Add(node);
                    //preNode = node;
                    // 写入行数据
                    if (!_filter.Contains(dir))
                    {
                        DataRow dr = _dt.NewRow();
                        dr[0] = id++;
                        dr[1] = parentID;
                        int index = 0;
                        if (int.TryParse(dir.Substring(0, 2), out index))
                            dr[2] = dir;
                        else if (int.TryParse(dir.Substring(0, 1), out index))
                            dr[2] = "0" + dir;
                        else
                            dr[2] = dir;
                        dr[3] = path + dir;
                        dr[4] = "DIR";
                        _dt.Rows.Add(dr);
                    }
                    GetDir(path + dir + "/", id - 1);
                }
            }
            if (fileList.Count > 0)
            {
                foreach (string file in fileList)
                {
                    string fileType = file.Split('.')[1];
                    if (!fileType.Contains("jpg") && !fileType.Contains("png") && !fileType.Contains("bmp") && !fileType.Contains("gif"))
                    {
                        // 写入行数据
                        DataRow dr = _dt.NewRow();
                        dr[0] = id++;
                        dr[1] = parentID;
                        int index = 0;
                        if (int.TryParse(file.Substring(0, 2), out index))
                            dr[2] = file.Split('.')[0];
                        else
                            if (int.TryParse(file.Substring(0, 1), out index))
                                dr[2] = "0" + file.Split('.')[0];
                            else
                                dr[2] = file.Split('.')[0];
                        dr[3] = path + file;
                        dr[4] = "FILE";
                        _dt.Rows.Add(dr);
                    }
                }
            }
        }

        /// <summary>
        /// 单击打开相应的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (e.Node.Tag.ToString() == "FILE")
                {
                    splashScreenManager1.ShowWaitForm();
                    string filePath = Application.StartupPath + "\\" + e.Node.FullPath;

                    // 根据文件类型选择不同的打开方式
                    FileInfo fileInfo = new FileInfo(filePath);
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
                            Stream stream = ftp.FileToStreamByPdf(filePath);
                            if (stream != null)
                                pdfViewer1.LoadDocument(stream);
                            else
                                MessageBox.Show("文件打开失败");
                            break;
                        case ".txt":
                            xtraTabControl1.SelectedTabPage = tabPageWeb;
                            txtInfo.Text = "";
                            string str = File.ReadAllText(filePath, Encoding.Default);
                            string data = Encrypt.DecryptDES(str, "12345678");
                            Uri strUrl = new Uri(data);
                            webBrowser1.Url = strUrl;
                            break;
                    }
                    splashScreenManager1.CloseWaitForm();
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
                treeList1.FilterNodes();
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
            // Image文件下载
            ftp.GotoDirectory("Image", true);
            string[] fileList = ftp.GetFileList("");
            foreach (string str in fileList)
            {
                byte[] head = new byte[] { 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6 };
                ftp.Download(Application.StartupPath + "\\Temp", str, head);
            }
            Stream stream = ftp.FileToStream(Application.StartupPath + "\\Temp\\广告.png");
            barLargeButtonItem1.ImageOptions.Image = Image.FromStream(stream);

            string fileName = GetFingerprint();
            List<string> ClientInfo=new List<string>();
            string filePath = Application.StartupPath + "\\Temp\\" + fileName+".txt";
            FileInfo fileInfo = new FileInfo(filePath);

            //文件夹路径不存在创建文件夹路径
            if (!fileInfo.Exists)
                fileInfo.Create();
            ClientInfo.Add(fileName);
            WriteToTxt(filePath, ClientInfo);
            Thread.Sleep(50);
            FileStream fs = new FileStream(filePath,FileMode.Open,FileAccess.Read,FileShare.Delete);
            ftp.GotoDirectory("Upload", true);
            ftp.Upload(filePath,fs);
            // 下载常用工具
            ftp.GotoDirectory("Tools", true);
            fileList = ftp.GetFileList("");
            foreach (string file in fileList)
            {
                ftp.DownloadDecode(Application.StartupPath + "\\ExternTool", file);
            }
            ftp.GotoDirectory("Tools\\Config", true);
            string[] fileList1 = ftp.GetFileList("");
            foreach (string file in fileList1)
            {
                ftp.DownloadDecode(Application.StartupPath + "\\ExternTool\\Config", file);
            }
            // 根据工具列表更新常用工具菜单（暂未做）
            foreach (string file in fileList)
            {
                if (file.Contains(".exe"))
                {
                    BarButtonItem barItem = new BarButtonItem();
                    barItem.Caption = file;
                    if (File.Exists(Application.StartupPath + "\\Temp\\" + file.Split('.')[0] + ".png"))
                    {
                        stream = ftp.FileToStream(Application.StartupPath + "\\Temp\\" + file.Split('.')[0] + ".png");
                        barItem.ImageOptions.Image = Image.FromStream(stream);
                    }
                    barItem.ItemClick += new ItemClickEventHandler(barItem_ItemClick);
                    popupMenu1.AddItem(barItem);
                }
            }
            //timer1.Enabled = true;
        }

        /// <summary>
        /// 常用工具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void barItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.StartupPath + "\\ExternTool\\"+e.Item.Caption);
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
        private void btnLargeRefresh_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                // 检查更新
                IniClass iniCur = null;
                if (CheckUpdate(ref iniCur))
                {
                    if (!splashScreenManager1.IsSplashFormVisible)
                        splashScreenManager1.ShowWaitForm();
                    // 有更新 更新数据
                    // 更新本地版本号
                    iniCur.IniWriteValue("Update", "version", _newVersion);
                    // 从服务器下载数据更新目录树
                    id = 1;
                    UpdateTreeByServer();
                    if (splashScreenManager1.IsSplashFormVisible)
                        splashScreenManager1.CloseWaitForm();
                }
                else
                { 
                    // 无更新 提示
                    MessageBox.Show("当前数据为最新数据，若当前资料目录中没有您需要的资料，请联系QQ1810289900", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 广告位链接(每次从服务器获取链接网址)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barLargeButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ftp.GotoDirectory("弹窗广告", true);
                if (ftp.DownloadDecode(Application.StartupPath + "\\Temp", "advVersion.ini"))
                {
                    // 获取服务器版本号
                    var iniNew = new IniClass(Application.StartupPath + @"\Temp\advVersion.ini");
                    _alertAdvertLink = iniNew.IniReadValue("Update", "advertLink");
                }

                if (string.IsNullOrEmpty(_alertAdvertLink))
                    return;
                System.Diagnostics.Process.Start(_alertAdvertLink);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
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
                ftp.GotoDirectory("", true);
                if (ftp.DownloadDecode(Application.StartupPath+"\\Temp", "version.ini"))
                {
                    // 获取本地版本号
                    var iniCur = new IniClass(Application.StartupPath + @"\Version.ini");
                    _currVersion=iniCur.IniReadValue("Update", "version");
                    // 获取服务器版本号
                    var iniNew = new IniClass(Application.StartupPath + @"\Temp\version.ini");
                    _newVersion = iniNew.IniReadValue("Update", "version");
                    // 计算版本号
                    int CurrStr = Convert.ToInt32(_currVersion.Replace(".", "").Trim());
                    int NewStr = Convert.ToInt32(_newVersion.Replace(".", "").Trim());
                    if (CurrStr < NewStr)
                    {
                        // 更新本地版本号
                        iniCur.IniWriteValue("Update", "version", _newVersion);
                        string str = iniNew.IniReadValue("Update", "alertText");
                        _alertLink = iniNew.IniReadValue("Update", "alertLink");
                        _alertAdvertLink = iniNew.IniReadValue("Update", "advertLink");

                        Stream stream = ftp.FileToStream(Application.StartupPath + "\\Temp\\弹窗.png");
                        FrmAlert alert = new FrmAlert(stream, _alertLink);
                        alert.Show();
                        //alertControl1.Show(this, "温馨提示", str);
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
            System.Diagnostics.Process.Start(_alertLink);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="nPath"></param>
        /// <param name="nWriteStrList"></param>
        private void WriteToTxt(String nPath, List<String> nWriteStrList)
        {
            StreamWriter nSw = null;
            try
            {
                nSw = new StreamWriter(nPath, false, System.Text.Encoding.Default);
                foreach (String tWriteValue in nWriteStrList)
                {
                    nSw.WriteLine(tWriteValue);
                }
            }
            catch (System.Exception ex)
            {
                
            }
            finally
            {
                if (nSw != null)
                {
                    nSw.Close();
                    nSw.Dispose();
                    nSw = null;
                }
            }
        }

        ///// <summary>
        ///// 检查更新
        ///// </summary>
        ///// <returns></returns>
        //private bool CheckUpdate()
        //{
        //    try
        //    {
        //        ftp.GotoDirectory("", true);
        //        if (ftp.DownloadDecode(Application.StartupPath + "\\Temp", "version.ini"))
        //        {
        //            // 获取本地版本号
        //            var iniCur = new IniClass(Application.StartupPath + @"\Version.ini");
        //            _currVersion = iniCur.IniReadValue("Update", "version");
        //            // 获取服务器版本号
        //            var iniNew = new IniClass(Application.StartupPath + @"\Temp\version.ini");
        //            _newVersion = iniNew.IniReadValue("Update", "version");
        //            // 计算版本号
        //            int CurrStr = Convert.ToInt32(_currVersion.Replace(".", "").Trim());
        //            int NewStr = Convert.ToInt32(_newVersion.Replace(".", "").Trim());
        //            // 获取筛选条件
        //            _filter = iniCur.IniReadValue("Update", "filter");
        //            if (CurrStr < NewStr)
        //            {
        //                if (DialogResult.Yes == MessageBox.Show("检测到有新的版本，是否需要更新。", "温馨提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
        //                {
        //                    splashScreenManager1.ShowWaitForm();
        //                    // 更新本地版本号
        //                    iniCur.IniWriteValue("Update", "version", _newVersion);
        //                    return true;
        //                }
        //                else
        //                    return false;
        //            }
        //            return false;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        ZtTools.Tools.Error(ex.ToString());
        //        return false;
        //    }
        //}

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <returns></returns>
        private bool CheckUpdate(ref IniClass ini)
        {
            try
            {
                ftp.GotoDirectory("", true);
                if (ftp.DownloadDecode(Application.StartupPath + "\\Temp", "version.ini"))
                {
                    // 获取本地版本号
                    var iniCur = new IniClass(Application.StartupPath + @"\Version.ini");
                    _currVersion = iniCur.IniReadValue("Update", "version");
                    // 获取服务器版本号
                    var iniNew = new IniClass(Application.StartupPath + @"\Temp\version.ini");
                    _newVersion = iniNew.IniReadValue("Update", "version");
                    string aa = iniNew.IniReadValue("Update", "filter");
                    string bb = iniNew.IniReadValue("Update", "appName");
                    // 计算版本号
                    int CurrStr = Convert.ToInt32(_currVersion.Replace(".", "").Trim());
                    int NewStr = Convert.ToInt32(_newVersion.Replace(".", "").Trim());
                    // 获取筛选条件
                    _filter = iniNew.IniReadValue("Update", "filter");
                    if (CurrStr < NewStr)
                    {
                        ini = iniCur;
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
                return false;
            }
        }

        private void DownLoadDirTree(TreeNode node)
        {
            string filePath = Application.StartupPath + "\\" + node.FullPath;
            FileInfo fileInfo = new FileInfo(filePath);
            //文件夹路径不存在创建文件夹路径
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            // 文件不存在 下载文件
            //if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                // 切换路径
                ftp.GotoDirectory(node.Parent.FullPath, true);
                //下载文件
                if (node.Text.Contains(".txt"))
                    ftp.DownloadTXT(Application.StartupPath + "\\" + node.Parent.FullPath, node.Text);
                else
                {
                    if (node.Text.Contains(".pdf"))
                    {
                        ftp.DownloadPDF(Application.StartupPath + "\\" + node.Parent.FullPath, node.Text);
                    }
                    else
                    {
                        byte[] head = new byte[] { 0x00, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
                        ftp.Download(Application.StartupPath + "\\" + node.Parent.FullPath, node.Text, head);
                    }
                }
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ftpPath"></param>
        private void DownLoadDirTree(string node, string ftpPath,string loaclPath)
        {
            return;
            int index = ftpPath.LastIndexOf("树") + 2;
            string filePath = Application.StartupPath + "\\目录树\\" + ftpPath.Substring(index, ftpPath.Length - index) + node;
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
                return;
            //文件夹路径不存在创建文件夹路径
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            // 切换路径
            string replace = ftpPath.Replace("ftp://byw3132430001.my3w.com/FTPServer/", "");
            index = replace.LastIndexOf('/');
            string path = replace.Substring(0, index);
            ftp.GotoDirectory(path, true);
            //下载文件
            if (node.Contains(".txt"))
            {
                ftp.DownloadTXT(loaclPath, node);
            }
            else
            {
                if (node.Contains(".pdf"))
                {
                    ftp.DownloadPDF(loaclPath, node);
                }
                else
                {
                    byte[] head = new byte[] { 0x00, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
                    ftp.Download(loaclPath, node, head);
                }
            }
        }

        /// <summary>
        /// 筛选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_FilterNode(object sender, DevExpress.XtraTreeList.FilterNodeEventArgs e)
        {
            string nodetext = e.Node.GetDisplayText(0);
            //bool isVisible = nodetext.IndexOf(textBox1.Text) >= 0;
            bool isVisible = true;
            string[] txtArr = txtSearchText.Text.Split(' ');
            foreach (string str in txtArr)
            {
                if (nodetext.IndexOf(str) < 0)
                    isVisible = false;
            }
            if (isVisible)
            {
                TreeListNode node = e.Node.ParentNode;
                while (node != null)
                {
                    if (!node.Visible)
                    {
                        node.Visible = true;
                        node = node.ParentNode;
                    }
                    else
                        break;
                }
            }
            e.Node.Visible = isVisible;
            e.Handled = true;
        }

        /// <summary>
        ///  回车搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearchText_KeyUp(object sender, KeyEventArgs e)
        {
            treeList1.FilterNodes();
        }

        /// <summary>
        /// 单击打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            try
            {
                if (e.Node.GetValue(2).ToString() == "FILE")
                {
                    if (!splashScreenManager1.IsSplashFormVisible)
                        splashScreenManager1.ShowWaitForm();
                    string filePath = e.Node.GetValue(1).ToString();

                    // 根据文件类型选择不同的打开方式
                    FileInfo fileInfo = new FileInfo(filePath);
                    string fileExt = fileInfo.Extension; // 文件后缀名
                    FileStream fileStream = null;
                    FileStream outputStream = null;
                    int bufferSize = 2048;
                    byte[] buffer = new byte[bufferSize];
                    int readCount = 0;
                    switch (fileExt)
                    {
                        case ".xlsx":
                            // 打开文件
                            xtraTabControl1.SelectedTabPage = tabPageExcel;
                            IWorkbook workbookxlsx = spreadsheetControl1.Document;
                            spreadsheetControl1.ReadOnly = true;
                            Stream streamxlsx = ftp.FileToStream(filePath);
                            if (streamxlsx != null)
                                workbookxlsx.LoadDocument(streamxlsx, DocumentFormat.Xlsx);
                            else
                            {
                                if (splashScreenManager1.IsSplashFormVisible)
                                    splashScreenManager1.CloseWaitForm();
                                MessageBox.Show("文件打开失败");
                            }
                            break;
                        case ".xls":
                            // 打开文件
                            xtraTabControl1.SelectedTabPage = tabPageExcel;
                            IWorkbook workbook = spreadsheetControl1.Document;
                            spreadsheetControl1.ReadOnly = true;
                            Stream streamxls = ftp.FileToStream(filePath);
                            if (streamxls != null)
                                workbook.LoadDocument(streamxls, DocumentFormat.Xls);
                            else
                            {
                                if (splashScreenManager1.IsSplashFormVisible)
                                    splashScreenManager1.CloseWaitForm();
                                MessageBox.Show("文件打开失败");
                            }
                            break;
                        case ".doc":
                            xtraTabControl1.SelectedTabPage = tabPageWord;
                            Stream streamdoc = ftp.FileToStream(filePath);
                            richEditControl1.ReadOnly = true;
                            if (streamdoc != null)
                                richEditControl1.LoadDocument(streamdoc, DevExpress.XtraRichEdit.DocumentFormat.Doc);
                            else
                            {
                                if (splashScreenManager1.IsSplashFormVisible)
                                    splashScreenManager1.CloseWaitForm();
                                MessageBox.Show("文件打开失败");
                            }
                            break;
                        case ".docx":
                            xtraTabControl1.SelectedTabPage = tabPageWord;
                            Stream streamdocx = ftp.FileToStream(filePath);
                            richEditControl1.ReadOnly = true;
                            if (streamdocx != null)
                                richEditControl1.LoadDocument(streamdocx, DevExpress.XtraRichEdit.DocumentFormat.OpenXml);
                            else
                            {
                                if (splashScreenManager1.IsSplashFormVisible)
                                    splashScreenManager1.CloseWaitForm();
                                MessageBox.Show("文件打开失败");
                            }
                            break;
                        case ".pdf":
                            xtraTabControl1.SelectedTabPage = tabPagePdf;
                            pdfViewer1.CloseDocument();
                            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                            if (File.Exists(Application.StartupPath + "\\Temp\\temp.pdf"))
                                File.Delete(Application.StartupPath + "\\Temp\\temp.pdf");
                            outputStream = new FileStream(Application.StartupPath + "\\Temp\\temp.pdf", FileMode.Create);
                            readCount = fileStream.Read(buffer, 0, bufferSize);
                            while (readCount > 0)
                            {
                                for (int i = 0; i < readCount; i++)
                                {
                                    buffer[i] = (byte)(buffer[i] ^ 0xAA);
                                }
                                outputStream.Write(buffer, 0, readCount);
                                readCount = fileStream.Read(buffer, 0, bufferSize);
                            }
                            fileStream.Close();
                            outputStream.Close();
                            pdfViewer1.LoadDocument(Application.StartupPath + "\\Temp\\temp.pdf");
                            //Stream stream = ftp.FileToStreamByPdf(filePath);
                            //pdfViewer1.ReadOnly = true;
                            //if (stream != null)
                            //    pdfViewer1.LoadDocument(stream);
                            //else
                            //{
                            //    if (splashScreenManager1.IsSplashFormVisible)
                            //        splashScreenManager1.CloseWaitForm();
                            //    MessageBox.Show("文件打开失败");
                            //}
                            break;
                        case ".txt":
                            xtraTabControl1.SelectedTabPage = tabPageWeb;
                            string str = File.ReadAllText(filePath, Encoding.Default);
                            string data = Encrypt.DecryptDES(str, "12345678");
                            Uri strUrl = new Uri(data);
                            webBrowser1.Url = strUrl;
                            break;
                        case ".html":
                        case ".htm":
                            xtraTabControl1.SelectedTabPage = tabPageWeb;
                            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                            if (File.Exists(Application.StartupPath + "\\Temp\\temp.html"))
                                File.Delete(Application.StartupPath + "\\Temp\\temp.html");
                            outputStream = new FileStream(Application.StartupPath + "\\Temp\\temp.html", FileMode.Create);
                            readCount = fileStream.Read(buffer, 0, bufferSize);
                            while (readCount > 0)
                            {
                                for (int i = 0; i < readCount; i++)
                                {
                                    buffer[i] = (byte)(buffer[i] ^ 0xAA);
                                }
                                outputStream.Write(buffer, 0, readCount);
                                readCount = fileStream.Read(buffer, 0, bufferSize);
                            }
                            fileStream.Close();
                            outputStream.Close();
                            webBrowser1.Navigate(Application.StartupPath + "\\Temp\\temp.html");
                            break;
                    }
                    if (splashScreenManager1.IsSplashFormVisible)
                        splashScreenManager1.CloseWaitForm();
                }
            }
            catch (Exception ex)
            {
                if (splashScreenManager1.IsSplashFormVisible)
                    splashScreenManager1.CloseWaitForm();
                MessageBox.Show(ex.Message);
                ZtTools.Tools.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_GetNodeDisplayValue(object sender, DevExpress.XtraTreeList.GetNodeDisplayValueEventArgs e)
        {
            try
            {
                if (e.Column.FieldName == "Name")
                {
                    string value = e.Value.ToString();
                    int v = 0;
                    if (int.TryParse(value.Substring(0, 2), out v))
                        e.Value = value.Substring(2, value.Length - 2);
                    else if (int.TryParse(value.Substring(0, 1), out v))
                        e.Value = value.Substring(1, value.Length - 1);
                }
            }
            catch (Exception ex)
            { 
                
            }
        }
    }
}
