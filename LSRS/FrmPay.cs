using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThoughtWorks.QRCode.Codec;
using WeChatAliPayService.Common;

namespace LSRS
{
    public partial class FrmPay : Form
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payMoney">支付金额 单位为分</param>
        public FrmPay(int payMoney)
        {
            InitializeComponent();
            this._payMoney = payMoney;
            this.labPayMoney.Text += payMoney;
            WeChatAliPay.LoadConfig.ReadWeChatConfig();
        }

        int num = 0;

        string out_trade_no = string.Empty;
        /// <summary>
        /// 支付金额
        /// </summary>
        private int _payMoney = 0;

        /// <summary>
        /// 加载窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmPay_Load(object sender, EventArgs e)
        {
            if (this._payMoney < 0)
            {
                MessageBox.Show("支付金额必须大于0");
                return;
            }
            out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmss" + (new Random()).Next(1, 10000).ToString().PadLeft(5, '0'));
            string wechatLink = WeChatPay.WeChatNativePay(out_trade_no, this._payMoney);
            if (!string.IsNullOrEmpty(wechatLink))
                pictureBox1.Image = WeChatGenerateQRCode(wechatLink);
            string aliLink = AliPay.AliTradePrecreate(out_trade_no, "0.01", "测试");
            if (!string.IsNullOrEmpty(aliLink))
                pictureBox4.Image = AliGenerateQRCode(aliLink);
            Task.Factory.StartNew(() =>
            {
                if (string.Compare(AliPay.LoopQuery(out_trade_no), "10000", true) == 0)
                {
                    this.DialogResult = DialogResult.OK;
                }
            });
        }

        /// <summary>
        /// 生成二维码并展示到页面上
        /// </summary>
        /// <param name="precreateResult">二维码串</param>
        private Bitmap AliGenerateQRCode(string aliLink)
        {
            //打印出 preResponse.QrCode 对应的条码
            Bitmap bt;
            string enCodeString = aliLink;
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;
            qrCodeEncoder.QRCodeScale = 3;
            qrCodeEncoder.QRCodeVersion = 8;
            bt = qrCodeEncoder.Encode(enCodeString, Encoding.UTF8);
            return bt;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="link">要生成二维码的字符串</param>
        /// <returns></returns>
        private Bitmap WeChatGenerateQRCode(string link)
        {
            Bitmap bmp = null;
            try
            {
                QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                qrCodeEncoder.QRCodeScale = 4;
                qrCodeEncoder.QRCodeVersion = 0;
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                bmp = qrCodeEncoder.Encode(link);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Invalid version !");
            }
            return bmp;
        }
    }
}
