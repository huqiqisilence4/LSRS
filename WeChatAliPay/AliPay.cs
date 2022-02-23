using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Com.Alipay.Domain;
using Com.Alipay;
using WxPayAPI;
using Com.Alipay.Business;
using Com.Alipay.Model;
using System.Xml;
using System.Runtime.Serialization.Json;

namespace WeChatAliPayService.Common
{
    public class AliPay
    {
        /*
        IAlipayTradeService serviceClient = F2FBiz.CreateClientInstance(AliConfig.serverUrl, AliConfig.appId, AliConfig.merchant_private_key, AliConfig.version,
                     AliConfig.sign_type, AliConfig.alipay_public_key, AliConfig.charset);
        */

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 支付宝支付
        /// </summary>
        /// <param name="money">总金额</param>
        /// <param name="auth_code">授权码</param>
        /// <param name="out_trade_no">订单号</param>
        /// <returns></returns>
        public static string AliTradePay(string money, string auth_code, string out_trade_no,string oper, ref string errorMsg)
        {
            try
            {
                //log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.cfg.xml"));    //ASP.NET
                AlipayTradePayContentBuilder builder = BuildPayContent(money, auth_code, out_trade_no, oper);
                IAlipayTradeService serviceClient = F2FBiz.CreateClientInstance(AliConfig.serverUrl, AliConfig.appId, AliConfig.merchant_private_key, AliConfig.version,
                                                                                AliConfig.sign_type, AliConfig.alipay_public_key, AliConfig.charset);
                IDictionary<string, string> payParam = new Dictionary<string, string>();
                payParam.Add("out_trade_no", builder.out_trade_no); //订单号
                payParam.Add("seller_id", builder.seller_id);       //收款账号
                payParam.Add("auth_code", builder.auth_code);       //支付授权码,付款码
                payParam.Add("total_amount", builder.total_amount); //订单总金额
                payParam.Add("subject", builder.subject);           //订单名称
                payParam.Add("timeout_express", builder.timeout_express);   //自定义超时时间
                payParam.Add("store_id", builder.store_id);             //门店编号，很重要的参数，可以用作之后的营销
                payParam.Add("operator_id", builder.operator_id);       //操作员编号，很重要的参数，可以用作之后的营销
                payParam.Add("scene", builder.scene);               //支付场景
                payParam.Add("terminal_id", builder.terminal_id);   // 商户机具终端编号 
                // 发起支付参数记录
                LogXml(payParam,"支付");
                AlipayF2FPayResult payResult = serviceClient.tradePay(builder);
                errorMsg = payResult.response.Body;
                // 支付返回结果信息记录
                LogJsonConvertXml("支付宝支付返回结果", errorMsg, builder.out_trade_no);
                return payResult.Status.ToString();
            }
            catch (Exception ex)
            {
                return "FAILED";
            }
        }

        /// <summary>
        /// 构造支付请求数据
        /// </summary>
        /// <returns>请求数据集</returns>
        private static AlipayTradePayContentBuilder BuildPayContent(string money, string auth_code, string out_trade_no,string oper)
        {
            ////线上联调时，请输入真实的外部订单号。
            //string out_trade_no = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "0000" + (new Random()).Next(1, 10000).ToString();
            //扫码枪扫描到的用户手机钱包中的付款条码
            AlipayTradePayContentBuilder builder = new AlipayTradePayContentBuilder();

            //收款账号
            builder.seller_id = AliConfig.pid;
            //订单编号
            builder.out_trade_no = out_trade_no;
            //支付场景，无需修改
            builder.scene = "bar_code";
            //支付授权码,付款码
            builder.auth_code = auth_code;
            //订单总金额
            builder.total_amount = money;
            ////测试金额
            //builder.total_amount = "0.01";
            //参与优惠计算的金额
            //builder.discountable_amount = "";
            //不参与优惠计算的金额
            //builder.undiscountable_amount = "";
            //订单名称
            builder.subject = AliConfig.subject;
            //自定义超时时间
            builder.timeout_express = AliConfig.timeOut;
            //订单描述
            builder.body = AliConfig.body;
            //门店编号，很重要的参数，可以用作之后的营销
            builder.store_id = AliConfig.store_id;
            //操作员编号，很重要的参数，可以用作之后的营销
            builder.operator_id = oper;


            //传入商品信息详情
            List<GoodsInfo> gList = new List<GoodsInfo>();

            GoodsInfo goods = new GoodsInfo();
            goods.goods_id = AliConfig.goods_id;
            goods.goods_name = AliConfig.goods_name;
            goods.price = money;
            goods.quantity = "1";
            gList.Add(goods);
            builder.goods_detail = gList;

            //系统商接入可以填此参数用作返佣
            ExtendParams exParam = new ExtendParams();
            exParam.sys_service_provider_id = AliConfig.sys_service_provider_id;
            builder.extend_params = exParam;
            return builder;

        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="out_trade_no">退款订单号</param>
        /// <param name="out_request_no">退款交易订单号</param>
        /// <param name="money">退款金额</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static string AliTradeRefund(string out_trade_no, string out_request_no, string money,string refund_reason, ref string errorMsg)
        {
            try
            {
                //log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.cfg.xml"));    //ASP.NET
                AlipayTradeRefundContentBuilder builder = BuildContent(out_trade_no, out_request_no, money, refund_reason);
                IAlipayTradeService serviceClient = F2FBiz.CreateClientInstance(AliConfig.serverUrl, AliConfig.appId, AliConfig.merchant_private_key, AliConfig.version,
                    AliConfig.sign_type, AliConfig.alipay_public_key, AliConfig.charset);
                IDictionary<string, string> payParam = new Dictionary<string, string>();
                payParam.Add("out_trade_no", builder.out_trade_no); //订单号
                payParam.Add("out_request_no", builder.out_request_no);       //退款请求单号保持唯一性。
                payParam.Add("refund_amount", builder.refund_amount);       //退款金额
                payParam.Add("refund_reason", builder.refund_reason); // 退款原因
                LogXml(payParam,"退款");
                AlipayF2FRefundResult refundResult = serviceClient.tradeRefund(builder);
                errorMsg = refundResult.response.Body;
                LogJsonConvertXml("支付宝退款返回结果", errorMsg, out_trade_no);
                return nameof(refundResult.Status);
            }
            catch (Exception ex)
            {
                //Logger.Error("退款环节出现错误；\r\n交易订单号：" + out_request_no + "\r\n" + "订单号：" + out_trade_no + "\r\n" + ex.Message);
                return nameof(ResultEnum.FAILED);
            }
        }

        /// <summary>
        /// 构造退款请求数据
        /// </summary>
        /// <returns>请求数据集</returns>
        private static AlipayTradeRefundContentBuilder BuildContent(string out_trade_no, string out_request_no, string money, string refund_reason)
        {
            AlipayTradeRefundContentBuilder builder = new AlipayTradeRefundContentBuilder();

            // 支付宝交易号与商户网站订单号不能同时为空
            builder.out_trade_no = out_trade_no;

            // 退款请求单号保持唯一性。
            builder.out_request_no = out_request_no;

            // 退款金额
            builder.refund_amount = money;

            // 退款原因
            builder.refund_reason = refund_reason;

            return builder;

        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="out_trade_no">订单号</param>
        /// <param name="errorMsg">错误信息</param>
        /// <returns></returns>
        public static ResultEnum AliTradeQuery(string out_trade_no, ref string errorMsg)
        {
            try
            {
                //log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.cfg.xml"));    //ASP.NET
                IAlipayTradeService serviceClient = F2FBiz.CreateClientInstance(AliConfig.serverUrl, AliConfig.appId, AliConfig.merchant_private_key, AliConfig.version,
                                        AliConfig.sign_type, AliConfig.alipay_public_key, AliConfig.charset);

                //Logger.Debug("支付宝发起订单查询...\r\n订单号：" + out_trade_no);
                AlipayF2FQueryResult queryResult = serviceClient.tradeQuery(out_trade_no);
                errorMsg = queryResult.response.Body;
                LogJsonConvertXml("支付宝订单查询返回结果", errorMsg, out_trade_no);
                return queryResult.Status;
            }
            catch (Exception ex)
            {
                //Logger.Error("订单查询环节出现错误；订单号：" + out_trade_no+"\r\n" + ex.Message);
                return ResultEnum.FAILED;
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="payParam">参数列表</param>
        private static void LogXml(IDictionary<string, string> payParam,string type)
        {
            try
            {
                //log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.cfg.xml"));    //ASP.NET
                string str = "收到支付宝" + type + "请求:<?xml version=\"1.0\" encoding=\"utf-8\"?> \r\n";
                string service = "发起" + type + "请求";
                StringBuilder sbXml = new StringBuilder();
                sbXml.AppendLine("<root>\r\n    <header>\r\n        <service>" + service + "</service>\r\n    </header>");
                sbXml.AppendLine("    <parameters>");
                foreach (KeyValuePair<string, string> item in payParam)
                {
                    sbXml.AppendLine("        <" + item.Key + ">" + item.Value + "</" + item.Key + ">");
                }
                sbXml.AppendLine("    </parameters>\r\n<root>");
                //Logger.Debug(str + sbXml);
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// json转Xml
        /// </summary>
        /// <param name="json"></param>
        private static void LogJsonConvertXml(string type, string json,string orderNo)
        {
            try
            {
                //log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.cfg.xml"));    //ASP.NET
                XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), XmlDictionaryReaderQuotas.Max);
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?> \r\n" + doc.InnerXml.Replace("type=\"object\"", "").Replace("type=\"string\"", "").Replace("type=\"array\"", "").Replace(" >", ">");
                xml = xml.Replace("<root><alipay_trade_pay_response>", "<root><alipay_trade_pay_response><out_trade_no>" + orderNo + "</out_trade_no>");
                xml = xml.Replace("<root><alipay_trade_refund_response>", "<root><alipay_trade_refund_response><out_trade_no>" + orderNo + "</out_trade_no>");
                System.Xml.XmlDocument docXml = new System.Xml.XmlDocument();
                docXml.LoadXml(xml);
                System.IO.StringWriter sw = new System.IO.StringWriter();
                using (System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(sw))
                {
                    writer.Indentation = 4;  // the Indentation
                    writer.Formatting = System.Xml.Formatting.Indented;
                    docXml.WriteContentTo(writer);
                    writer.Close();
                }
                //Logger.Debug(type + sw);
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }
        }
    }
}
