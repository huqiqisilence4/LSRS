using Com.Alipay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WxPayAPI;

namespace WeChatAliPay
{
    public class LoadConfig
    {
        static string WeChatAliPayPath = AppDomain.CurrentDomain.BaseDirectory + "WeChatAliPay.config.xml";

        public static string ReadWeChatConfig()
        {
            if (AliConfig.appId != null && AliConfig.appId != "")
            {
                return "";
            }
            string result = string.Empty;
            try
            {
                XmlDocument mode = new XmlDocument();
                mode.Load(WeChatAliPayPath);
                XmlNodeList top = mode.DocumentElement.ChildNodes;
                foreach (XmlElement element in top)
                {
                    if (element.Name.ToLower() == "wechatalipay")
                    {
                        foreach (XmlNode el in element.ChildNodes)
                        {
                            if (el.Attributes != null)
                            {
                                switch (el.Attributes["key"].Value)
                                {
                                    // 微信参数
                                    case "WeChatMicroPayUrl":
                                        WxPayConfig.WeChatMicroPayUrl = el.Attributes["value"].Value;
                                        break;
                                    case "WeChatOrderQueryUrl":
                                        WxPayConfig.WeChatOrderQueryUrl = el.Attributes["value"].Value;
                                        break;
                                    case "WeChatCloseOrderUrl":
                                        WxPayConfig.WeChatCloseOrderUrl = el.Attributes["value"].Value;
                                        break;
                                    case "WeChatPayAPIKey":
                                        WxPayConfig.WeChatPayAPIKey = el.Attributes["value"].Value;
                                        break;
                                    case "WeChatAppid":
                                        WxPayConfig.WeChatAppid = el.Attributes["value"].Value;
                                        break;
                                    case "WeChatMchid":
                                        WxPayConfig.WeChatMchid = el.Attributes["value"].Value;
                                        break;
                                    //case "WeChatDeviceInfo":
                                    //    RC.ClientDesktop.Common.Application.Instance.WeChatCloseOrderUrl = el.Attributes["value"].Value;
                                    //    break;
                                    //case "WeChatTimeOut":
                                    //    WxPayConfig.TimeOut = el.Attributes["value"].Value;
                                    //    break;
                                    //case "Body":
                                    //    WxPayConfig.Body = el.Attributes["value"].Value;
                                    //    break;
                                    // 支付宝参数
                                    case "AliPayGateway":
                                        AliConfig.serverUrl = el.Attributes["value"].Value;
                                        break;
                                    case "AliPayAppId":
                                        AliConfig.appId = el.Attributes["value"].Value;
                                        break;
                                    case "AliPayAppPrivateKey":
                                        AliConfig.merchant_private_key = el.Attributes["value"].Value;
                                        break;
                                    case "AliPayPublicKey":
                                        AliConfig.alipay_public_key = el.Attributes["value"].Value;
                                        break;
                                    case "AliPayPid":
                                        AliConfig.pid = el.Attributes["value"].Value;
                                        break;
                                    case "AliPayAppPublicKey":
                                        AliConfig.merchant_public_key = el.Attributes["value"].Value;
                                        break;
                                    case "AliTimeOut":
                                        AliConfig.timeOut = el.Attributes["value"].Value;
                                        break;
                                    case "AliSubJect":
                                        AliConfig.subject = el.Attributes["value"].Value;
                                        break;
                                    case "Body":
                                        AliConfig.body = el.Attributes["value"].Value;
                                        break;
                                    case "store_id":
                                        AliConfig.store_id = el.Attributes["value"].Value;
                                        break;
                                    case "operator_id":
                                        AliConfig.operator_id = el.Attributes["value"].Value;
                                        break;
                                    case "goods_id":
                                        AliConfig.goods_id = el.Attributes["value"].Value;
                                        break;
                                    case "goods_name":
                                        AliConfig.goods_name = el.Attributes["value"].Value;
                                        break;
                                    case "sys_service_provider_id":
                                        AliConfig.sys_service_provider_id = el.Attributes["value"].Value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
