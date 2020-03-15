using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.IO;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Diagnostics;


namespace InstallCompenent
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        /// <summary>
        /// 应用程序入口
        /// </summary>
        public static void Main()
        {
        }

        public Installer()
        {
            InitializeComponent();
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            Assembly asm = Assembly.GetExecutingAssembly();
            string path = asm.Location.Remove(asm.Location.LastIndexOf("\\"));
            // 设置开机自启动
            //string StartupPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonStartup);
            //System.IO.File.Copy(path + "\\AlertForm\\AlertForm.exe.lnk", StartupPath + @"\AlertForm.exe.lnk", true);
            SetAutoStartup(path + "\\AlertForm\\AlertForm.exe");
            //启动弹窗程序
            System.Diagnostics.Process.Start(path + "\\AlertForm\\AlertForm.exe");
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="nPath"></param>
        /// <param name="nWriteStrList"></param>
        private void WriteToTxt(String nPath, string nWriteStrList)
        {
            StreamWriter nSw = null;
            try
            {
                nSw = new StreamWriter(nPath, false, System.Text.Encoding.Default);
                nSw.WriteLine(nWriteStrList);
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

        /// <summary>
        ///  向目标路径创建指定文件的快捷方式
        /// </summary>
        /// <param name="directory">目标目录</param>
        /// <param name="shortcutName">快捷方式名字</param>
        /// <param name="targetPath">文件完全路径</param>
        /// <param name="description">描述</param>
        /// <param name="iconLocation">图标地址</param>
        /// <returns>成功或失败</returns>
        private bool CreateShortcut(string directory, string shortcutName, string targetPath, string description = null, string iconLocation = null)
        {
            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);                         //目录不存在则创建
                //添加引用 Com 中搜索 Windows Script Host Object Model
                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));          //合成路径
                WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);    //创建快捷方式对象
                shortcut.TargetPath = targetPath;                                                               //指定目标路径
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);                                  //设置起始位置
                shortcut.WindowStyle = 1;                                                                       //设置运行方式，默认为常规窗口
                shortcut.Description = description;                                                             //设置备注
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;    //设置图标路径
                shortcut.Save();                                                                                //保存快捷方式
                return true;
            }
            catch (Exception ex)
            {
                string temp = ex.Message;
                temp = "";
            }
            return false;
        }

        private void SetAutoStartup(string strAssName)
        {
            //此方法把启动项加载到注册表中
            //获得应用程序名
            string ShortFileName = "";
            RegistryKey rgkRun = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rgkRun == null)
            {
                rgkRun = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            }
            rgkRun.SetValue(ShortFileName, strAssName);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
        }
    }
}
