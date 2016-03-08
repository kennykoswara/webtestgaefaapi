using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebTest.Infrastructure;
using WebTest.PortalList;

namespace WebTest.Models
{

    public class GaefaPackageJSON
    {
        public int id { get; set; }
        public string name { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int minimumPack { get; set; }
        public int? duration { get; set; }
        public double pricePerPack { get; set; }
        public bool includeFlight { get; set; }
        public bool includeHotel { get; set; }
        public string data { get; set; }
        public string location { get; set; }
        public string note { get; set; }
    }

    public class GaefaPackage
    {
        public int id { get; set; }
        public string name { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int minimumPack { get; set; }
        public int? duration { get; set; }
        public double pricePerPack { get; set; }
        public bool includeFlight { get; set; }
        public bool includeHotel { get; set; }
        public Data data { get; set; }
        public string location { get; set; }
        public string note { get; set; }

        public GaefaPackage() { }
    }

    public class Data
    {
        public Day[] days { get; set; }
    }

    public class Day
    {
        public int dayNum { get; set; }
        public Event[] events { get; set; }
    }

    public class Event
    {
        public string category { get; set; }
        public string time { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }

    public class GaefaPackageInformation
    {
        public GaefaFilter filter { get; set; }
        public string GetListTotal()
        {
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            json.Url = json.BaseUrlGetTotal + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn;
            System.Diagnostics.Debug.WriteLine(json.GetTotalPackage(json.Url));
            return json.GetTotalPackage(json.Url);
        }

        public GaefaPackageInformation(GaefaFilter filter)
        {
            this.filter = filter;
        }

        public GaefaPackageInformation()
        {
            filter = new GaefaFilter();
        }
    }

    public class PostToGaefaDB
    {
        public bool? postStatus { get; set; }
        public PostToGaefaDB() { }
        public void Post(int package_id, string order_reference, int total_pack, DateTime date)
        {
            
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();

            /*
            using (var client = new HttpClient())
            {
                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("agency_uid", GlobalVar.AGENCY_UID));
                values.Add(new KeyValuePair<string, string>("package_id", package_id.ToString()));
                values.Add(new KeyValuePair<string, string>("signature", sign.GetSignature()));
                values.Add(new KeyValuePair<string, string>("order_reference", order_reference));
                values.Add(new KeyValuePair<string, string>("total_pack", total_pack.ToString()));
                values.Add(new KeyValuePair<string, string>("date", date.ToString()));

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(json.BaseUrlPostPackage, content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (responseString == "true") this.postStatus = true;
                else this.postStatus = false;
            }
            */

            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["agency_uid"] = GlobalVar.AGENCY_UID;
                data["package_id"] = package_id.ToString();
                data["signature"] = sign.GetSignature();
                data["order_reference"] = order_reference;
                data["total_pack"] = total_pack.ToString();
                data["date"] = date.ToString();

                var response = wb.UploadValues(json.BaseUrlPostPackage, "POST", data);
                
                if (System.Text.Encoding.Default.GetString(response) == "true") this.postStatus = true;
                else this.postStatus = false;
            }
            
        }
    }
}