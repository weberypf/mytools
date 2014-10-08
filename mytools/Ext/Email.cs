using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace mytools.Web.Ext
{
    public class Email
    {
        public static bool Mail(string recevier,string subject,string content)
        {
            var username = System.Configuration.ConfigurationManager.AppSettings["MAILGUN_SMTP_LOGIN"];
            var pass = System.Configuration.ConfigurationManager.AppSettings["MAILGUN_SMTP_PASSWORD"];
            var host = System.Configuration.ConfigurationManager.AppSettings["MAILGUN_SMTP_SERVER"];
            var port = System.Configuration.ConfigurationManager.AppSettings["MAILGUN_SMTP_PORT"];

            var client = new System.Net.Mail.SmtpClient();
            client.Host = host;
            client.Port = Convert.ToInt32(port);
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.Credentials = new System.Net.NetworkCredential(username, pass);


            var msg = new System.Net.Mail.MailMessage();
            msg.Subject = subject;
            msg.Body = content;
            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            msg.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.High;
            msg.From = new MailAddress("53294245@qq.com", "peter", System.Text.Encoding.UTF8);
            msg.To.Add(recevier);

            try
            {
                client.Send(msg);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}