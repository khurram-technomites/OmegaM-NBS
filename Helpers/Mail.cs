using NowBuySell.Data.Infrastructure;
using NowBuySell.Service;
using NowBuySell.Web.Helpers.Routing;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web;
using static NowBuySell.Web.Helpers.Enumerations.Enumeration;

namespace NowBuySell.Web.Helpers
{
    public class Mail
    {
        private string NetworkEmail { get; set; }
        private string NetworkPassword { get; set; }
        private string FromEmail { get; set; }
        private string DisplayName { get; set; }
        private int Port { get; set; }
        private string Host { get; set; }
        private bool EnableSsl { get; set; }


        private readonly IEmailService _emailservice;
        private readonly IUnitOfWork _unitOfWork;

        public Mail(IEmailService emailservice, IUnitOfWork unitOfWork)
        {
            this._emailservice = emailservice;
            this._unitOfWork = unitOfWork;

            var EmailSetting = _emailservice.GetDefaultEmailSetting();
            if (EmailSetting != null)
            {
                NetworkEmail = EmailSetting.EmailAddress;
                NetworkPassword = EmailSetting.Password;
                // FromEmail = EmailSetting.UserID;
                DisplayName = "NowBuySell";
                Port = Convert.ToInt16(EmailSetting.Port);
                Host = EmailSetting.Host;
                EnableSsl = EmailSetting.EnableSSL.Equals("true") ? true : false;
            }
        }

        public bool SendMail(string email, string subject, string body)
        {
            try
            {
                // Credentials
                var credentials = new NetworkCredential(NetworkEmail, NetworkPassword);

                // Mail message
                var mail = new MailMessage()
                {
                    From = new MailAddress(FromEmail, DisplayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(new MailAddress(email));

                // Smtp client
                var client = new SmtpClient()
                {
                    Port = Port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = Host,
                    EnableSsl = EnableSsl,
                    Credentials = credentials
                };

                // Send it...         
                client.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendMail(string email, string subject, string body, AlternateView alternate, bool isImage = false)
        {
            try
            {
                // Credentials
                var credentials = new NetworkCredential(NetworkEmail, NetworkPassword);

                // Mail message
                var mail = new MailMessage()
                {
                    From = new MailAddress(FromEmail, DisplayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(new MailAddress(email));
                if (alternate != null)
                    mail.AlternateViews.Add(alternate);

                // Smtp client
                var client = new SmtpClient()
                {
                    Port = Port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = Host,
                    EnableSsl = false,
                    Credentials = credentials
                };

                // Send it...         
                client.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendMail(string Email, EmailTemplate emailTemplate, EmailLang emailLang, Int64 userId = -1)
        {
            try
            {
                string subject = string.Empty;
                string body = string.Empty;

                GetEmailContent(emailTemplate, emailLang, ref subject, ref body, userId);

                if (SendMail(Email, subject, body, null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool GetEmailContent(EmailTemplate EmailTemplate, EmailLang emailLang, ref string subject, ref string body, Int64 userId = -1)
        {
            try
            {
                switch (EmailTemplate)
                {
                    case EmailTemplate.ForgotPassword:
                        subject = "Forgot Password.";
                        string CustomerToken = Guid.NewGuid() + "-" + Convert.ToString(userId);
                        body = "Recover your account:<a href=" + CustomURL.GetFormatedURL("Account/ResetPassword?user=" + CustomerToken) + ">click here</a>";

                        break;

                    case EmailTemplate.Feedback:
                        subject = "Coffee Cafe";
                        body = "";
                        break;

                    default:
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendForgotPasswordMail(long userId, string username, string Email, string url)
        {
            try
            {
                string subject = "NowBuySell | Account Recovery ";
                string body = string.Empty;

                using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/Assets/EmailTemplates/ForgotPassword.html")))
                {
                    body = reader.ReadToEnd();
                }

                body = body.Replace("{Url}", url);
                body = body.Replace("{UserName}", username);

                if (SendMail(Email, subject, body))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}