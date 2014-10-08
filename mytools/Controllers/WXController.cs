using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using mytools.Web.Models;
using WeiXin;
using WeiXin.Message;
namespace mytools.Web.Controllers
{
    public class WXController : Controller
    {
        WeiXinApi weixin = new WeiXinApi();
        [HttpGet]
        public ActionResult API(string signature, string timestamp, string nonce, string echoStr)
        {
            var token = System.Configuration.ConfigurationManager.AppSettings["WXTOKEN"];
            return Content(weixin.Sign(signature, timestamp, nonce, echoStr, token));
        }
        [HttpPost]
        public ActionResult API(string signature, string timestamp, string nonce)
        {
            var token = System.Configuration.ConfigurationManager.AppSettings["WXTOKEN"];
            if (weixin.Check(signature, timestamp, nonce, token))
            {

                var msg = weixin.GetWeiXinMessage(this.HttpContext);
                var repmsg = new RepTextMessage() { ToUserName = msg.FromUserName, FromUserName = msg.ToUserName };
                try
                {
                    if (msg.MsgType == WeiXin.Message.MessageType.Text)
                    {
                        var cmd = ((RecTextMessage)msg).Content;
                        var ebbinghaus = new Ebbinghaus();
                        if (ebbinghaus.IsShell(cmd))
                        {
                            var result = ebbinghaus.Exec(cmd, msg.FromUserName);
                            repmsg.Data = new TextMsgData() { Content = result };
                            return Content(repmsg.ToString());
                        }
                    }
                    repmsg.Data = new TextMsgData() { Content = Ebbinghaus.help };
                    return Content(repmsg.ToString());
                }
                catch (Exception ex)
                {
                    repmsg.Data = new TextMsgData() { Content = ex.Message };
                    return Content(repmsg.ToString());
                }
            }
            return new EmptyResult();
        }
    }
}
