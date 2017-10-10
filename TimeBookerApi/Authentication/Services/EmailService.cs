using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Net.Mime;

namespace TimeBookerApi.Authentication.Services
{
    public class EmailService : IIdentityMessageService
    {
        SmtpClient client;

        public EmailService()
        {
                client = new SmtpClient
                {
                    Host = "Smtp.Gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Properties.Settings.Default.Email, Properties.Settings.Default.Password)
                };
        }

        public Task SendAsync(IdentityMessage message)
        {
            MailMessage messageToSend;
            try
            {
                if (message.Subject == "Reset Password")
                {
                    messageToSend = new MailMessage
                    {
                        From = new MailAddress(Properties.Settings.Default.Email, "TimeBooker"),
                        To = { message.Destination },
                        Subject = message.Subject,
                        Body = message.Body
                    };
                    client.Send(messageToSend);
                }
                else
                {
                    #region formatter
                    string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Body);
                    string html = "Please confirm your account by clicking this link: <a href=\"" + message.Body + "\">link</a><br/>";

                    html += HttpUtility.HtmlEncode(@"Or copy the following link to the browser:" + message.Body);
                    #endregion

                    messageToSend = new MailMessage
                    {
                        From = new MailAddress(Properties.Settings.Default.Email, "TimeBooker"),
                        To = { message.Destination },
                        Subject = message.Subject,
                    };
                    messageToSend.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
                    messageToSend.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));
                    client.Send(messageToSend);
                }
            }
            catch (Exception e)
            {
                Console.Write("Error when trying to send ethe confirmation mail. " + e.InnerException);
            }
            return Task.FromResult(0);
        }
    }
}