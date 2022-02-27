using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WxPayAPI;

namespace WeChatAliPayService.Common
{
    public class WeChatPay
    {
        /// <summary>
        ///  微信支付
        /// </summary>
        /// <param name="money">总金额</param>
        /// <param name="auth_code">授权码</param>
        /// <param name="out_trade_no">订单号</param>
        /// <returns>支付结果</returns>
        public static string WeChatMicroPay(string money,string auth_code,string out_trade_no)
        {
            //string result = MicroPay.Run("车票", (Convert.ToDouble(money) * 100).ToString(), auth_code, out_trade_no);
            //测试
            string result = MicroPay.Run("车票", "1", auth_code);
            return result;
        }

        public static string WeChatNativePay(string productId, int money)
        {
            //string result = NativePay.GetPrePayUrl(productId);
            string result = NativePay.GetPayUrl(productId, money);
            return result;
        }

        /// <summary>
        ///  订单查询
        /// </summary>
        /// <param name="transaction_id">微信订单号</param>
        /// <param name="out_trade_no">商户订单号</param>
        /// <returns>查询结果</returns>
        public static string WeChatOrderQuery(string transaction_id, string out_trade_no)
        {
            Log.Info("OrderQuery", "OrderQuery is processing...");

            WxPayData data = new WxPayData();
            if (!string.IsNullOrEmpty(transaction_id))//如果微信订单号存在，则以微信订单号为准
            {
                data.SetValue("transaction_id", transaction_id);
            }
            else//微信订单号不存在，才根据商户订单号去查单
            {
                data.SetValue("out_trade_no", out_trade_no);
            }

            WxPayData result = WxPayApi.OrderQuery(data);//提交订单查询请求给API，接收返回数据

            Log.Info("OrderQuery", "OrderQuery process complete, result : " + result.ToXml());
            return result.ToPrintStr();
        }

        /// <summary>
        ///  申请退款
        /// </summary>
        /// <param name="transaction_id">微信订单号</param>
        /// <param name="out_trade_no">商户订单号</param>
        /// <param name="total_fee">订单总金额</param>
        /// <param name="refund_fee">退款金额</param>
        /// <param name="MCHID">操作员，默认为商户号</param>
        /// <returns></returns>
        public static string WeChatRefund(string transaction_id, string out_trade_no, string total_fee, string refund_fee)
        {
            Log.Info("Refund", "Refund is processing...");

            WxPayData data = new WxPayData();
            if (!string.IsNullOrEmpty(transaction_id))//微信订单号存在的条件下，则已微信订单号为准
            {
                data.SetValue("transaction_id", transaction_id);
            }
            else//微信订单号不存在，才根据商户订单号去退款
            {
                data.SetValue("out_trade_no", out_trade_no);
            }

            //data.SetValue("total_fee", int.Parse(total_fee));//订单总金额
            //data.SetValue("refund_fee", int.Parse(refund_fee));//退款金额
            data.SetValue("total_fee", 1);//订单总金额
            data.SetValue("refund_fee", 1);//退款金额
            data.SetValue("out_refund_no", WxPayApi.GenerateOutTradeNo());//随机生成商户退款单号
            data.SetValue("op_user_id", WxPayConfig.Config().GetMchID());//操作员，默认为商户号

            WxPayData result = WxPayApi.Refund(data);//提交退款申请给API，接收返回数据

            Log.Info("Refund", "Refund process complete, result : " + result.ToXml());
            return result.ToPrintStr();
        }
    }
}
