using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using WeiXin.Message;
namespace WeiXin
{
    public class WeiXinApi
    {
        #region 验证服务URL
        public string Sign(string signature, string timestamp, string nonce, string echoStr, string token)
        {
            if (Check(signature, timestamp, nonce, token))
            {
                return echoStr;
            }
            else
            {
                return string.Empty;
            }
        }
        public bool IsSignRequest(HttpContextBase context)
        {
            return "GET".Equals(context.Request.HttpMethod);
        }
        public bool Check(string signature, string timestamp, string nonce,string token)
        {
            var vs = new[] { timestamp, nonce, token }.OrderBy(s => s);
            var str = string.Join("", vs);
            var copu = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "SHA1");
            return copu.Equals(signature, StringComparison.CurrentCultureIgnoreCase);
        }
        #endregion


        /// <summary>
        /// 获取微信发来的消息
        /// </summary>
        /// <returns></returns>
        public ReceiveMessage GetWeiXinMessage(HttpContextBase context)
        {
            var result = string.Empty;
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                result = reader.ReadToEnd();
            }

            return ReceiveMessage.Parse(result);
        }
    }
}
