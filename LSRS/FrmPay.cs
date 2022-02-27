using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThoughtWorks.QRCode.Codec;
using WeChatAliPayService.Common;

namespace LSRS
{
    public partial class FrmPay : Form
    {
        public FrmPay(float payMoney)
        {
            InitializeComponent();
            this._payMoney = payMoney;
            this.labPayMoney.Text += payMoney;
            WeChatAliPay.LoadConfig.ReadWeChatConfig();
        }

        int num = 0;

        private float _payMoney = 0;

        private void FrmPay_Load(object sender, EventArgs e)
        {
            string out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmss" + (num++).ToString().PadLeft(5, '0'));
            string link = WeChatPay.WeChatNativePay(out_trade_no);
            pictureBox1.Image = GenerateQRCode(link);
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="link">要生成二维码的字符串</param>
        /// <returns></returns>
        private Bitmap GenerateQRCode(string link)
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
