using Newtonsoft.Json;
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

    public class GaefaOrderDetail
    {
        public int orderId { get; set; }
        public int package_publish_id { get; set; }
        public string orderReference { get; set; }
        public string date { get; set; }
        public int pack_adult { get; set; }
        public int pack_child { get; set; }
        public int pack_child_nobed { get; set; }
        public decimal price_adult { get; set; }
        public decimal price_child { get; set; }
        public decimal price_child_nobed { get; set; }
        public string note { get; set; }
    }

    public class GaefaPackageJSON
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? duration { get; set; }
        public double priceAdult { get; set; }
        public double priceChild { get; set; }
        public double priceChildNoBed { get; set; }
        public RangedDate rangedDate { get; set; }
        public SelectedDate selectedDate { get; set; }
        public bool includeFlight { get; set; }
        public bool includeHotel { get; set; }
        public string data { get; set; }
        public string location { get; set; }
        public string note { get; set; }
        public string tag { get; set; }
    }

    public class GaefaPackage
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? duration { get; set; }
        public double priceAdult { get; set; }
        public double priceChild { get; set; }
        public double priceChildNoBed { get; set; }
        public RangedDate rangedDate { get; set; }
        public SelectedDate selectedDate { get; set; }
        public bool includeFlight { get; set; }
        public bool includeHotel { get; set; }
        public Data data { get; set; }
        public string location { get; set; }
        public string note { get; set; }
        public string tag { get; set; }

        public GaefaPackage() { }
    }

    public class RangedDate
    {
        public string cdate { get; set; }
        public string end_date { get; set; }
        public bool is_friday { get; set; }
        public bool is_monday { get; set; }
        public bool is_saturday { get; set; }
        public bool is_sunday { get; set; }
        public bool is_thursday { get; set; }
        public bool is_tuesday { get; set; }
        public bool is_wednesday { get; set; }
        public int maximum_pack { get; set; }
        public string mdate { get; set; }
        public int minimum_pack { get; set; }
        public string start_date { get; set; }
    }

    public class SelectedDate
    {
        public List<DateList> dateList { get; set; }
    }

    public class DateList
    {
        public string cdate { get; set; }
        public string date { get; set; }
        public string mdate { get; set; }
        public int total_maximum_passenger { get; set; }
    }

    public class postPassenger
    {
        public string order_reference { get; set; }
        public string agency_uid { get; set; }
        public string signature { get; set; }
        public List<PassengerList> passengerList;
    }

    public class PassengerList
    {
        public string name { get; set; }
        public string remarks { get; set; }
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
            json.Url = json.BaseUrlGetTotal + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&tag=" + filter.tag;
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

    public class PostToGaefaDB //For Posting package to Gaefa
    {
        public bool? postStatus { get; set; }
        public PostToGaefaDB() { }
        public void Post(int package_id, string order_reference, int adult_pack, int child_pack, int child_nobed_pack, DateTime date, string note)
        {
            
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();

            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["agency_uid"] = GlobalVar.AGENCY_UID;
                data["package_id"] = package_id.ToString();
                data["signature"] = sign.GetSignature();
                data["order_reference"] = order_reference;
                data["note"] = note;
                data["pack_adult"] = adult_pack.ToString();
                data["pack_child"] = child_pack.ToString();
                data["pack_child_nobed"] = child_nobed_pack.ToString();
                data["date"] = date.ToString();

                System.Diagnostics.Debug.WriteLine(data);
                var response = wb.UploadValues(json.BaseUrlPostPackage, "POST", data);
                
                if (System.Text.Encoding.Default.GetString(response) == "true") this.postStatus = true;
                else this.postStatus = false;
            }
            
        }
    }

    public class PostPassengerDetails //For Posting Passenger Details To Gaefa
    {
        public bool? postStatus { get; set; }
        public PostPassengerDetails() { }
        public void Post(postPassenger postPassenger)
        {

            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();

            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();

                data["postPassenger"] = JsonConvert.SerializeObject(postPassenger);
                System.Diagnostics.Debug.WriteLine(data["postPassenger"]);

                System.Diagnostics.Debug.WriteLine(data);
                try
                {
                    var response = wb.UploadValues(json.BaseUrlSavePassenger, "POST", data);

                    if (System.Text.Encoding.Default.GetString(response) == "true") this.postStatus = true;
                    else this.postStatus = false;
                }
                catch
                {
                    this.postStatus = false;
                }
            }

        }
    }

    public class PostTag //For Adding Tag to Package
    {
        public bool? postStatus { get; set; }
        public PostTag() { }
        public void Post(int packageID, string tagName)
        {

            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();

            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();

                data["agency_uid"] = GlobalVar.AGENCY_UID;
                data["package_published_id"] = packageID.ToString();
                data["signature"] = sign.GetSignature();
                data["tag"] = tagName;
                
                var response = wb.UploadValues(json.BaseUrlSaveTag, "POST", data);

                if (System.Text.Encoding.Default.GetString(response) == "true") this.postStatus = true;
                else this.postStatus = false;
            }

        }
    }
}