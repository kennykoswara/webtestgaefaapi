using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebTest.Infrastructure
{
    public class BasicHelper
    {
        private static Random random;
        private static readonly string CAPITAL = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string LETTER = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string NUMBER = "0123456789";

        public static string getRandomString(int total_length)
        {
            return getRandomString(total_length, CAPITAL + LETTER + NUMBER);
        }

        public static string getRandomString(int total_length, string char_set)
        {
            if (random == null) random = new Random();

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < total_length; i++)
            {
                result.Append(char_set[random.Next(0, char_set.Length - 1)]);
            }

            return result.ToString();
        }

        public static string hashPassword(string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] originalBytes = ASCIIEncoding.Default.GetBytes(password);
            Byte[] encodedBytes = md5.ComputeHash(originalBytes);
            password = BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
            return password;
        }

        public static void sendEmail(string email, string subject, string body)
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress(email));  // replace with valid value 
            message.From = new MailAddress(GlobalVar.WEB_EMAIL_GMAIL);  // replace with valid value
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = GlobalVar.WEB_EMAIL_GMAIL,  // replace with valid value
                    Password = GlobalVar.WEB_EMAIL_PASSWORD,  // replace with valid value
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
        }
    }
}