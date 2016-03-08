using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebTest.Infrastructure;

namespace WebTest.Models
{
    public class GaefaSignature
    {
        public string text { get; set; }

        private static String ToBase64(String input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("X2"); /* hex format */
            return sbinary;
        }

        public string GetSignature()
        {
            string key = GlobalVar.KEY;
            string text = GlobalVar.AGENCY_UID + key;

            HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            //hmac.Key = Encoding.UTF8.GetBytes("201649260749126qRGFxHC57");

            byte[] hashedData = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));

            string hash = ToBase64(ByteToString(hashedData));

            foreach (var item in hashedData)
            {
                System.Diagnostics.Debug.Write(item);
            }
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine(hash);

            return hash;
        }

        public GaefaSignature() { }
    }
}