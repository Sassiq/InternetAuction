using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace OnlineAuctionProject.Repository
{
    public class Email
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public Email(string to, string subject, string body)
        {
            this.To = to;
            this.Subject = subject;
            this.Body = body;
        }
        public void Send()
        {
            //Creating an email
            MailMessage email = new MailMessage();
            email.From = new MailAddress("no-reply@onlineauctions.com");
            email.To.Add(new MailAddress(To));
            email.Subject = Subject;
            email.Body = Body.Replace("\n", "<br />");
            email.IsBodyHtml = true;
            //Sending the email
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.gmail.com";
            System.Net.NetworkCredential networkCredential = new System.Net.NetworkCredential();
            networkCredential.UserName = "onlineauctionsreply@gmail.com";
            networkCredential.Password = "SharifQasrawi@123";
            smtpClient.UseDefaultCredentials = true;
            smtpClient.Credentials = networkCredential;
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.Send(email);
        }
    }
}