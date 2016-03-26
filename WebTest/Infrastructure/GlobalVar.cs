using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebTest.Infrastructure
{
    public class GlobalVar
    {
        public static readonly string SESSION_NAME = "name";
        public static readonly string SESSION_ID = "id";
        public static readonly string SESSION_EMAIL = "email";
        public static string BASE_URL = "http://localhost:55929/";

        public static readonly int ORDER_REFERENCE_LENGTH = 6;

        public static string AGENCY_UID = "201620180820190dJKcRCfIu"; //Gaefa Agency UID for this travel agency
        public static string KEY = "20162018082019BX1DFgTfaa"; //Gaefa Key for this travel agency

        public static string EMAIL_GAEFA = "jegul95@gmail.com";
        public static string EMAIL_WEB = "jegul955@gmail.com";

        public static string PAYPAL_NVP_API_USERNAME = "jegul95-facilitator_api1.gmail.com"; //PayPal NVP API Username
        public static string PAYPAL_NVP_API_PASSWORD = "G6DQDKKWNLKGJR8V"; //Paypal NVP API Password
        public static string PAYPAL_NVP_API_SIGNATURE = "AI.L38LvsFfXTK029ExJw8E0rVDrAHtbTSybrG.DFpxS0RcbKirnMnlK"; //PayPal NVP API Signature

        public static string WEB_EMAIL_GMAIL = "webtestjegul@gmail.com"; //gmail email to send every email to user
        public static string WEB_EMAIL_PASSWORD = "konyakujelly"; //the password
    }
}