using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ESucess;

namespace EncodeFile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 浏览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = folderBrowserDialog1.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEncode_Click(object sender, EventArgs e)
        {
            try
            {
                splashScreenManager1.ShowWaitForm();
                EncodeFile(this.textBox1.Text);
                splashScreenManager1.CloseWaitForm();
                MessageBox.Show("加密完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void EncodeFile(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);//创建文件夹信息类实例
            FileInfo[] fileInfo = directoryInfo.GetFiles();
            DirectoryInfo[] dirInfo = directoryInfo.GetDirectories();

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
            // 文件夹
            if (dirList.Count != 0)
            {
                foreach (string dir in dirList)
                {
                    // 写入行数据
                    EncodeFile(path +"\\"+ dir + "/");
                }
            }
            if (fileList.Count > 0)
            {
                foreach (string file in fileList)
                {
                    string fileType = file.Split('.')[1];
                    FileStream fileStream = null;
                    FileStream outputStream = null;
                    int bufferSize = 2048;
                    byte[] buffer = new byte[bufferSize];
                    int readCount = 0;
                    switch (fileType)
                    {
                        case "docx":
                        case "doc":
                        case "xlsx":
                        case "xls":
                            fileStream = new FileStream(path + "\\" + file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                            outputStream = new FileStream(path + "\\" + file + ".tmp", FileMode.Create);
                            readCount = fileStream.Read(buffer, 0, bufferSize);
                            byte[] head = new byte[] { 0x00, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
                            outputStream.Write(head, 0, 6);
                            while (readCount > 0)
                            {
                                outputStream.Write(buffer, 0, readCount);
                                readCount = fileStream.Read(buffer, 0, bufferSize);
                            }
                            fileStream.Close();
                            outputStream.Close();
                            File.Delete(path + "\\" + file);
                            File.Move(path + "\\" + file + ".tmp", path + "\\" + file);
                            break;
                        case "txt":
                            string str = File.ReadAllText(path + "\\" + file, Encoding.Default);
                            string data = Encrypt.EncryptDES(str, "12345678");
                            File.WriteAllText(path + "\\" + file, data);
                            break;
                        case "pdf":
                        case "html":
                        case "htm":
                            fileStream = new FileStream(path + "\\" + file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                            outputStream = new FileStream(path + "\\" + file + ".tmp", FileMode.Create);
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
                            File.Delete(path + "\\" + file);
                            File.Move(path + "\\" + file + ".tmp", path + "\\" + file);
                            break;
                        default:
                            break;
                    }

                }
            }
        }
    }
}
