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
using SendGrid;
using System.Net.Configuration;
using Microsoft.Azure.KeyVault;
using SendGrid.Helpers.Mail;

namespace TimeBookerApi.Authentication.Services
{
    //I'm using the IIdentityMessageService interface so i can register this as an EmailService to the UserManager.
    public class EmailService : IIdentityMessageService
    {
        string apiKey;
        SendGridClient gridClient;
        
        public EmailService()
        {
            apiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
            gridClient = new SendGridClient(apiKey);
        }

        /// <summary>
        /// Method to send the email. There is 2 possible outcomes.
        /// 1= Send a reset token if the Subject in IdentityMessage is "Reset Password".
        /// 2= Send a email confirmation link to the registered email.
        /// </summary>
        /// <param name="message">Pass a valid IdentityMessage. 
        /// Set Subject to "Reset Password" if it's a reset password token you want to send.</param>
        /// <returns></returns>
        public async Task SendAsync(IdentityMessage message)
        {
            Response response = null;
            try
            {
                if (message.Subject == "Reset Password")
                {
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress("support@timebooker.se", "Support"),
                        Subject = message.Subject,
                        PlainTextContent = message.Body,
                        HtmlContent = "<strong>" + message.Body + "</strong>"
                    };
                    msg.AddTo(new EmailAddress(message.Destination));
                    response = await gridClient.SendEmailAsync(msg);
                }
                else
                {
                    string text = string.Format("Click on this link to {0}: {1}", message.Subject, message.Body);
                    string html = "Confirm your email by clicking this link: <a href=\"" + message.Body + "\">Confirm Email</a><br/>";

                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress("support@timebooker.se", "Support"),
                        Subject = message.Subject,
                        PlainTextContent = text,
                        HtmlContent = html
                    };
                    msg.AddTo(message.Destination);
                    response = await gridClient.SendEmailAsync(msg);
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write(ex.Message + "Email Response:" + response);
            }
        }
    }
}