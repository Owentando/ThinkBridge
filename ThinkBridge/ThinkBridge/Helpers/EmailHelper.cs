using System;
using System.Net;
using System.Net.Mail;

namespace ThinkBridge.Helpers
{
    public static class EmailHelper
    {
        private static bool MAIL_ENABLED = true;
        private static string MAIL_SERVER = "smtp.gmail.com";
        private static int MAIL_PORT = 587;
        private static bool MAIL_USE_TLS = true;
        private static bool MAIL_USE_SSL = false;
        private static string MAIL_USERNAME = "mindpulse.notifications@gmail.com";
        private static string MAIL_PASSWORD = "oqippflpbvdajqqe";
        private static string MAIL_DEFAULT_SENDER = "mindpulse.notifications@gmail.com";

        public static void SendEmail(string toEmail, string subject, string body)
        {
            if (!MAIL_ENABLED)
                return;

            try
            {
                var smtp = new SmtpClient(MAIL_SERVER, MAIL_PORT)
                {
                    Credentials = new NetworkCredential(MAIL_USERNAME, MAIL_PASSWORD),
                    EnableSsl = MAIL_USE_TLS || MAIL_USE_SSL
                };

                var message = new MailMessage
                {
                    From = new MailAddress(MAIL_DEFAULT_SENDER),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                message.To.Add(toEmail);

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email error: " + ex.Message);
            }
        }
    }
}