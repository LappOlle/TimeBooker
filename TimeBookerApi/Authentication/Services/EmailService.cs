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
    //I'm using the IIdentityMessageService interface so i can register this as an EmailService to the UserManager.
    public class EmailService : IIdentityMessageService
    {
        SmtpClient client;

        public EmailService()
        {
            //Setting up SmtpClient for use of my gmail account.
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

        /// <summary>
        /// Method to send the email. There is 2 possible outcomes.
        /// 1= Send a reset token if the Subject in IdentityMessage is "Reset Password".
        /// 2= Send a email confirmation link to the registered email.
        /// </summary>
        /// <param name="message">Pass a valid IdentityMessage. 
        /// Set Subject to "Reset Password" if it's a reset password token you want to send.</param>
        /// <returns></returns>
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
                    string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Body);
                    string html = "Please confirm your account by clicking this link: <a href=\"" + message.Body + "\">link</a><br/>";
                    html += HttpUtility.HtmlEncode(@"or copy the following link to the browser:" + message.Body);

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
                Console.Write(e.InnerException);
            }
            return Task.FromResult(0);
        }
    }
}