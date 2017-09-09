using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace BizSparkSupport.MVC.Models
{
    public class SendEmail
    {
        public static void send(string Email, string Name, System.Web.HttpRequestBase Request, string activationCode, int? password, string Path)
        {
            using (StreamReader reader = System.IO.File.OpenText(System.Web.HttpContext.Current.Server.MapPath(Path))) // Path to your 
            {
                MailMessage mm = new MailMessage();
                mm.To.Add(new MailAddress(Email.Trim(), "Request"));
                mm.From = new MailAddress("DivSpark@outlook.com");
                mm.Subject = "Microsoft support team";
                // mm.BodyFormat = System.Web.Mail.MailFormat.Html;
                mm.IsBodyHtml = true;
                if (password == null)
                {
                    if ((Path == "~/email.html") || (Path == "~/Validate.html"))
                    {
                        mm.Body = reader.ReadToEnd().Replace("<%Scheme%>", Request.Url.Scheme)
                        .Replace("<%Authority%>", Request.Url.Authority)
                        .Replace("<%email%>", Email)
                        .Replace("<%Code%>", activationCode)
                        .Replace("<%name%>", Name);
                    }
                }
                else
                {
                    mm.Body = reader.ReadToEnd().Replace("<%Scheme%>", Request.Url.Scheme)
                    .Replace("<%Authority%>", Request.Url.Authority)
                    .Replace("<%email%>", Email)
                    .Replace("<%Code%>", activationCode)
                    .Replace("<%password%>", password.ToString())
                    .Replace("<%name%>", Name);
                }
                SmtpClient smcl = new SmtpClient();
                smcl.UseDefaultCredentials = false;
                smcl.EnableSsl = true;
                smcl.Host = "smtp-mail.outlook.com";
                smcl.Port = 587;
                smcl.DeliveryMethod = SmtpDeliveryMethod.Network;
                smcl.Credentials = new NetworkCredential("DivSpark@outlook.com", "Spark2017%");
                //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("DivSpark@outlook.com", "Spark2017%");
                //smcl.Credentials = credentials;

                smcl.Send(mm);


            }

        }
    }
}