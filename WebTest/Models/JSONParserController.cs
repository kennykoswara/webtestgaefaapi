using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;


namespace WebTest.Models
{

    public class JSONParser
    {
        public string Url { get; set; }
        public string BaseUrlPackage = "http://gaefaapi.cubeout.com/api/values/"; //API LAMA
        //public string BaseUrlPackage = "http://gaefaclientdemo.cubeout.com/api/values/"; //API BARU
        //public string BaseUrlPackage = "http://192.168.1.24//GaefaUIAPI/api/values/";
        public string BaseUrlGetList { get; set; }
        public string BaseUrlGetTotal { get; set; }
        public string BaseUrlGetDetail { get; set; }
        public string BaseUrlPostPackage { get; set; }
        public string BaseUrlGetOrderDetailByOrderReference { get; set; }
        public string BaseUrlSavePassenger { get; set; }
        public string BaseUrlSaveTag { get; set; }

        public JSONParser() {
            BaseUrlGetList = BaseUrlPackage + "GetList";
            BaseUrlGetTotal = BaseUrlPackage + "GetTotalList";
            BaseUrlGetDetail = BaseUrlPackage + "GetDetail";
            BaseUrlPostPackage = BaseUrlPackage + "SaveOrder";
            BaseUrlGetOrderDetailByOrderReference = BaseUrlPackage + "GetOrderDetailByOrderReference";
            BaseUrlSavePassenger = BaseUrlPackage + "savePassenger";
            //BaseUrlSavePassenger = "http://192.168.1.24//GaefaUIAPI/api/values/savePassenger/";
            BaseUrlSaveTag = BaseUrlPackage + "savePackagePublishedTag";
        }

        public List<GaefaPackageJSON> GetGaefaPackageArray(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<GaefaPackageJSON>));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    List<GaefaPackageJSON> jsonResponse = objResponse as List<GaefaPackageJSON>;
                    return jsonResponse;
                }
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine("disini");
                Console.WriteLine(e.Message);
                return null;
            }

        }
        public GaefaPackageJSON GetGaefaPackage(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GaefaPackageJSON));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    GaefaPackageJSON jsonResponse = objResponse as GaefaPackageJSON;
                    return jsonResponse;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        public string GetTotalPackage(string requestUrl)
        {
            string responseText;
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;

                //request.Accept = "application/xrds+xml";  
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                WebHeaderCollection header = response.Headers;

                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                {
                    responseText = reader.ReadToEnd();
                }
                return (responseText == null) ? "1" : responseText;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "1";
            }
        }

        public bool? PostPackage(string requestUrl)
        {
            string responseText;
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;

                //request.Accept = "application/xrds+xml";  
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                WebHeaderCollection header = response.Headers;

                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                {
                    responseText = reader.ReadToEnd();
                }
                return (responseText == null) ? false : true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public GaefaOrderDetail GetOrderDetailByOrderReference(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GaefaOrderDetail));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    GaefaOrderDetail jsonResponse = objResponse as GaefaOrderDetail;
                    return jsonResponse;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
    }

}