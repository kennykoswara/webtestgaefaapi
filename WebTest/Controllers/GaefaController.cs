using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using System.Web;
using System.Web.Mvc;
using WebTest.Models;
using System.Globalization;
using WebTest.Infrastructure;
using System.Text;
using System.Data.Entity.Validation;
using System.IO;
using System.Data.Entity.Infrastructure;
using WebTest.Security;
using PayPal.PayPalAPIInterfaceService.Model;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using System.Net.Mail;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PagedList;
using WebTest.PortalList;
using System.Data.Entity;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace WebTest.Controllers
{

    public class GaefaController : Controller
    {
        public class GaefaMultipleModel
        {
            public List<gaefa_book_info> booking;
            public List<GaefaPackage> gaefaPackage;
        }
        
        public class GaefaDetail
        {
            public gaefa_book_info booking;
            public GaefaPackage gaefaPackage;
        }

        public class BookingAndTransfer
        {
            public GaefaDetail booking;
            public gaefa_transfer_info transfer;
        }

        public class BookingAndPayPal
        {
            public GaefaDetail booking;
            public gaefa_paypal_info paypal;
        }

        public class BookCodeAndEmail
        {
            public string email;
            public string bookCode;
        }

        public class GaefaDetailNew
        {
            public gaefa_book_new booking;
            public GaefaPackage gaefaPackage;
            public gaefa_pic_info picInfo;
        }

        public class BookingAndTransferNew
        {
            public GaefaDetailNew booking;
            public gaefa_transfer_new transfer;
            public gaefa_pic_info picInfo;
        }

        public class GaefaMultipleModelNew
        {
            public List<gaefa_book_new> booking;
            public List<GaefaPackage> gaefaPackage;
        }

        public class BookingAndPayPalNew
        {
            public GaefaDetailNew booking;
            public gaefa_paypal_new paypal;
            public gaefa_pic_info picInfo;
        }

        testEntities3 db = new testEntities3();

        public ActionResult GetModule(string partialName)
        {
            return PartialView("~/Views/Partial/" + partialName);
        }

        // GET: Gaefa
        public ActionResult Index()
        {
            return View("List");
        }

        public ActionResult Error() //Global Error Page
        {
            return View();
        }

        public ActionResult FindBooking() //To find booking with email and bookcode
        {
            return View();
        }
        
        public ActionResult List(string keyword = "", bool? flight = null, bool? inn = null, GaefaPackageSort.sort_type? sortType = null, GaefaPackageSort.sort_mode? sortMode = null, int? page = null, string tag = "") //To see list of all package
        {
            GaefaSignature sign = new GaefaSignature();
            int pageNumber = (page ?? 1);
            GaefaPagination pagination = new GaefaPagination()
            {
                limit = 6,
            };
            pagination.start = (pageNumber - 1) * pagination.limit;
            GaefaFilter filter = new GaefaFilter()
            {
                titleOrLocation = keyword,
                include_flight = flight,
                include_inn = inn,
                tag = tag,
            };
            GaefaPackageSort sort = new GaefaPackageSort()
            {
                sortMode = (sortMode ?? GaefaPackageSort.sort_mode.DESCENDING),
                sortType = (sortType ?? GaefaPackageSort.sort_type.cdate),
            };
            JSONParser json = new JSONParser();
            json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode + "&tag=" + filter.tag;
            System.Diagnostics.Debug.WriteLine(json.Url);
            //string url = GaefaPackageUrl();
            List<GaefaPackageJSON> ListOfGaefaPackageJSON = json.GetGaefaPackageArray(json.Url);
            List<GaefaPackage> ListOfGaefaPackage = new List<GaefaPackage>();

            GaefaPackageInformation info = new GaefaPackageInformation(filter);

            if (ListOfGaefaPackageJSON == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                for (int i = 0; i < ListOfGaefaPackageJSON.Count; i++)
                {
                    ListOfGaefaPackage.Add(new GaefaPackage
                    {
                        id = ListOfGaefaPackageJSON[i].id,
                        data = JsonConvert.DeserializeObject<Data>(ListOfGaefaPackageJSON[i].data),
                        duration = ListOfGaefaPackageJSON[i].duration,
                        includeFlight = ListOfGaefaPackageJSON[i].includeFlight,
                        includeHotel = ListOfGaefaPackageJSON[i].includeHotel,
                        location = ListOfGaefaPackageJSON[i].location,
                        name = ListOfGaefaPackageJSON[i].name,
                        note = ListOfGaefaPackageJSON[i].note,
                        priceAdult = ListOfGaefaPackageJSON[i].priceAdult,
                        priceChild = ListOfGaefaPackageJSON[i].priceChild,
                        priceChildNoBed = ListOfGaefaPackageJSON[i].priceChildNoBed,
                        rangedDate = ListOfGaefaPackageJSON[i].rangedDate,
                        selectedDate = ListOfGaefaPackageJSON[i].selectedDate,
                    });

                }

                ViewBag.PackageAmount = int.Parse(info.GetListTotal());
                ViewBag.LimitPerPage = pagination.limit;
                ViewBag.filter = filter;
                ViewBag.pagination = pagination;
                ViewBag.sort = sort;
                ViewBag.keyword = keyword;
                return View("List", ListOfGaefaPackage.ToList());
            }
        }

        public ActionResult Detail(int? id) //To see the detail of the package
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return RedirectToAction("Error", "Gaefa");
            }
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + id + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
            GaefaPackage package;

            if (packageJSON == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Error", "Gaefa");
            }
            

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                name = packageJSON.name,
                note = packageJSON.note,
                priceAdult = packageJSON.priceAdult,
                priceChild = packageJSON.priceChild,
                priceChildNoBed = packageJSON.priceChildNoBed,
                rangedDate = packageJSON.rangedDate,
                selectedDate = packageJSON.selectedDate,
            };

            return View(package);
        }

        public ActionResult Checkout(int? id) //To checkout page of the package
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + id + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
            GaefaPackage package;

            if (packageJSON == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Error", "Gaefa");
            }

            if(packageJSON.selectedDate != null)
            {
                foreach(var item in packageJSON.selectedDate.dateList)
                {
                    DateTime tempDT_date = DateTime.ParseExact(item.date.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    item.date = tempDT_date.ToString("yyyy-MM-dd");
                }
            }

            if(packageJSON.rangedDate != null)
            {
                if(packageJSON.rangedDate.start_date != null)
                {
                    DateTime tempDT_startDate = DateTime.ParseExact(packageJSON.rangedDate.start_date.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    packageJSON.rangedDate.start_date = tempDT_startDate.ToString("yyyy-MM-dd");
                }

                if (packageJSON.rangedDate.end_date != null)
                {
                    DateTime tempDT_endDate = DateTime.ParseExact(packageJSON.rangedDate.end_date.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    packageJSON.rangedDate.end_date = tempDT_endDate.ToString("yyyy-MM-dd");
                }
            }
            
            
            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                name = packageJSON.name,
                note = packageJSON.note,
                priceAdult = packageJSON.priceAdult,
                priceChild = packageJSON.priceChild,
                priceChildNoBed = packageJSON.priceChildNoBed,
                rangedDate = packageJSON.rangedDate,
                selectedDate = packageJSON.selectedDate,
            };
            

            /*DUMMY DATA
            Event eventDetail = new Event
            {
                title = "a",
                category = "b",
                description = "c",
                time = "d",
            };

            Event[] events = new Event[1];
            events[0] = eventDetail;
            
            Day day = new Day
            {
                dayNum = 1,
                events = events,
            };

            Day[] days = new Day[1];
            days[0] = day;

            Data data = new Data {
                days = days,
            };

            GaefaPackage package = new GaefaPackage
            {
                id = 1,
                data = data,
                duration = 1,
                includeFlight = true,
                includeHotel = false,
                location = "Singapore",
                minimumPack = 2,
                name = "aaa",
                note = "kappa",
                priceAdult = 12.50,
                priceChild = 10.50,
                priceChildNoBed = 8.00,
                rangedDate = null,
                selectedDate = null,
            };
            */
            return View(package);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(int tourID, string payopt, string picName, string picAddress, string picTelCode, string picTelephone, string picEmail, string note, DateTime tourDate, int peopleCount, string couponDisc, int adult = 0, int child = 0, int childNoBed = 0) //To process the payment ($0, Transfer, or PayPal)
        {
            if(peopleCount < 1)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            decimal totalPrice;
            decimal originalPrice;
            int discPercentage;
            decimal discNotPercentage;
            int discFlag = -1;
            int discType = -1;
            string couponDiscX;

            bool dateFlag = false;
            int indexSelectedDate = -1;

            var query_coupon = from c in db.gaefa_coupon where c.couponCode == couponDisc select c;
            var query_promo = from p in db.gaefa_promo where p.promoCode == couponDisc select p;

            if (couponDisc == null || couponDisc == "")
            {
                couponDiscX = null;
                discPercentage = 0;
                discNotPercentage = 0;
            }
            else
            {
                if(query_coupon.FirstOrDefault() == null && query_promo.FirstOrDefault() == null)
                {
                    couponDiscX = null;
                    discPercentage = 0;
                    discNotPercentage = 0;
                }
                else if(query_coupon.FirstOrDefault() != null && query_promo.FirstOrDefault() == null)
                {
                    couponDiscX = couponDisc.ToUpper();
                    discType = 0;
                    if(query_coupon.FirstOrDefault().status == true)
                    {
                        discPercentage = query_coupon.FirstOrDefault().discPercentage ?? 0;
                        discNotPercentage = query_coupon.FirstOrDefault().discPrice ?? 0;
                    }
                    else
                    {
                        discPercentage = 0;
                        discNotPercentage = 0;
                    }

                    if(discPercentage == 0)
                    {
                        discFlag = 1;
                    }
                    else if(discNotPercentage == 0)
                    {
                        discFlag = 0;
                    }
                }
                else //if promo
                {
                    couponDiscX = couponDisc.ToUpper();
                    discType = 1;
                    if (query_promo.FirstOrDefault().used < query_promo.FirstOrDefault().amount)
                    {
                        discPercentage = query_promo.FirstOrDefault().discPercentage ?? 0;
                        discNotPercentage = query_promo.FirstOrDefault().discPrice ?? 0;
                    }
                    else
                    {
                        discPercentage = 0;
                        discNotPercentage = 0;
                    }

                    if (discPercentage == 0)
                    {
                        discFlag = 1;
                    }
                    else if (discNotPercentage == 0)
                    {
                        discFlag = 0;
                    }
                }
            }

            
            var bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            var orderReference = DateTime.Now.ToString("yyyymmddhhmmss") + "-" + BasicHelper.getRandomString(GlobalVar.ORDER_REFERENCE_LENGTH);
            

            var checkBookCode = from b in db.gaefa_book_new where b.bookCode == bookCode select b;
            while (checkBookCode.FirstOrDefault() != null)
            {
                bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            }

            var checkOrderReference = from oR in db.gaefa_book_new where oR.orderReference == orderReference select oR;
            while (checkOrderReference.FirstOrDefault() != null)
            {
                orderReference = DateTime.Now.ToString("yyyymmddhhmmss") + "-" + BasicHelper.getRandomString(GlobalVar.ORDER_REFERENCE_LENGTH);
            }

            if (adult + child + childNoBed == 0)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                GaefaSignature sign = new GaefaSignature();
                JSONParser json = new JSONParser();
                //json.Url = json.UrlPackage + id;
                json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + tourID + "&signature=" + sign.GetSignature();
                GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);


                if (packageJSON != null) //if (packageJSON == null) 
                {
                    if (packageJSON.selectedDate != null)
                    {
                        for (int i = 0; i < packageJSON.selectedDate.dateList.Count; i++)
                        {
                            DateTime tempDT_date = DateTime.ParseExact(packageJSON.selectedDate.dateList[i].date.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            if (tourDate == tempDT_date)
                            {
                                indexSelectedDate = i;
                                dateFlag = true;
                                break;
                            }
                            else
                            {
                                indexSelectedDate = -1;
                                dateFlag = false;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if(packageJSON.rangedDate.start_date != null)
                        {
                            DateTime tempDT_start = DateTime.ParseExact(packageJSON.rangedDate.start_date.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            if (tourDate < tempDT_start)
                            {
                                dateFlag = false;
                            }
                            else
                            {
                                dateFlag = true;
                            }
                        }
                        
                        if(packageJSON.rangedDate.end_date != null)
                        {
                            DateTime tempDT_end = DateTime.ParseExact(packageJSON.rangedDate.end_date.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            if (tourDate > tempDT_end)
                            {
                                dateFlag = false;
                            }
                        }

                        switch ((int)tourDate.DayOfWeek)
                        {
                            case 0:
                                if (!packageJSON.rangedDate.is_sunday) dateFlag = false;
                                break;
                            case 1:
                                if (!packageJSON.rangedDate.is_monday) dateFlag = false;
                                break;
                            case 2:
                                if (!packageJSON.rangedDate.is_tuesday) dateFlag = false;
                                break;
                            case 3:
                                if (!packageJSON.rangedDate.is_wednesday) dateFlag = false;
                                break;
                            case 4:
                                if (!packageJSON.rangedDate.is_thursday) dateFlag = false;
                                break;
                            case 5:
                                if (!packageJSON.rangedDate.is_friday) dateFlag = false;
                                break;
                            case 6:
                                if (!packageJSON.rangedDate.is_saturday) dateFlag = false;
                                break;
                        }
                    }

                    if (!dateFlag) return RedirectToAction("Error", "Gaefa");


                    double priceTimesPeople = (packageJSON.priceAdult * adult) + (packageJSON.priceChild * child) + (packageJSON.priceChildNoBed * childNoBed);
                    //double priceTimesPeople = (12.5 * adult) + (10.5 * child) + (8 * childNoBed);

                    if (discType == 0)
                    {
                        if(query_coupon.FirstOrDefault().packMin != null && query_coupon.FirstOrDefault().priceMin != null)
                        {
                            if (adult + child + childNoBed < query_coupon.FirstOrDefault().packMin && decimal.Round((decimal)(priceTimesPeople), 2, MidpointRounding.AwayFromZero) < query_coupon.FirstOrDefault().priceMin)
                            {
                                return RedirectToAction("Error", "Gaefa");
                            }
                        }
                        else if(query_coupon.FirstOrDefault().packMin != null && query_coupon.FirstOrDefault().priceMin == null)
                        {
                            if (adult + child + childNoBed < query_coupon.FirstOrDefault().packMin)
                            {
                                return RedirectToAction("Error", "Gaefa");
                            }
                        }
                        else
                        {
                            if (decimal.Round((decimal)(priceTimesPeople), 2, MidpointRounding.AwayFromZero) < query_coupon.FirstOrDefault().priceMin)
                            {
                                return RedirectToAction("Error", "Gaefa");
                            }
                        }
                    }
                    else if(discType == 1)
                    {
                        if (query_promo.FirstOrDefault().packMin != null && query_promo.FirstOrDefault().priceMin != null)
                        {
                            if (adult + child + childNoBed < query_promo.FirstOrDefault().packMin && decimal.Round((decimal)(priceTimesPeople), 2, MidpointRounding.AwayFromZero) < query_promo.FirstOrDefault().priceMin)
                            {
                                return RedirectToAction("Error", "Gaefa");
                            }
                        }
                        else if (query_promo.FirstOrDefault().packMin != null && query_promo.FirstOrDefault().priceMin == null)
                        {
                            if (adult + child + childNoBed < query_promo.FirstOrDefault().packMin)
                            {
                                return RedirectToAction("Error", "Gaefa");
                            }
                        }
                        else
                        {
                            if (decimal.Round((decimal)(priceTimesPeople), 2, MidpointRounding.AwayFromZero) < query_promo.FirstOrDefault().priceMin)
                            {
                                return RedirectToAction("Error", "Gaefa");
                            }
                        }
                    }

                    if(packageJSON.selectedDate != null)
                    {
                        if ((adult + child + childNoBed) < 1) return RedirectToAction("Error", "Gaefa");
                        if ((adult + child + childNoBed) > packageJSON.selectedDate.dateList[indexSelectedDate].total_maximum_passenger) return RedirectToAction("Error", "Gaefa");
                    }
                    else
                    {
                        if ((adult + child + childNoBed) < packageJSON.rangedDate.minimum_pack) return RedirectToAction("Error", "Gaefa");
                        if ((adult + child + childNoBed) > packageJSON.rangedDate.maximum_pack) return RedirectToAction("Error", "Gaefa");
                    }

                    originalPrice = decimal.Round((decimal)(priceTimesPeople), 2, MidpointRounding.AwayFromZero);

                    if (discFlag == 0)
                    {
                        totalPrice = decimal.Round(originalPrice - (originalPrice * discPercentage / 100), 2, MidpointRounding.AwayFromZero);
                    }
                    else if(discFlag == 1)
                    {
                        totalPrice = decimal.Round(originalPrice - discNotPercentage, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        totalPrice = originalPrice;
                    }

                    if (totalPrice <= (decimal)0.00)
                    {
                        string promoCode;
                        string couponCode;
                        if (discType == 0)
                        {
                            couponCode = couponDiscX;
                            promoCode = null;
                        }
                        else if (discType == 1)
                        {
                            couponCode = null;
                            promoCode = couponDiscX;
                        }
                        else
                        {
                            couponCode = null;
                            promoCode = null;
                        }

                        gaefa_book_new a = new gaefa_book_new
                        {
                            bookCode = bookCode,
                            tourID = tourID,
                            email = picEmail.Trim(),
                            dateOrder = DateTime.Now,
                            dateToGo = tourDate,
                            orderReference = orderReference,
                            paymentMethod = "Zero",
                            status = "Paid",
                            adultCount = adult,
                            childCount = child,
                            childNoBedCount = childNoBed,
                            passengerAmount = adult + child + childNoBed,
                            totalPrice = (decimal)0.00,
                            note = note.Replace("\r\n", "%0D%0A"),
                            couponCode = couponCode,
                            promoCode = promoCode,
                            postedToGaefa = false,
                            postedPassenger = false,
                        };
                        
                        gaefa_pic_info p = new gaefa_pic_info
                        {
                            bookCode = bookCode,
                            email = picEmail.Trim(),
                            address = picAddress.Trim(),
                            name = picName,
                            telephone = "+" + picTelCode.ToString() + picTelephone.ToString(),
                        };

                        if (couponDiscX != null)
                        {
                            if (discType == 0)
                            {
                                var coupon_query = from c in db.gaefa_coupon where c.couponCode == couponDiscX select c;
                                coupon_query.FirstOrDefault().status = false;
                            }
                            else
                            {
                                var promo_query = from pr in db.gaefa_promo where pr.promoCode == couponDiscX select pr;
                                promo_query.FirstOrDefault().used += 1;
                            }
                        }

                        PostToGaefaDB PostToGaefa = new PostToGaefaDB();
                        PostToGaefa.Post(tourID, a.orderReference, a.adultCount, a.childCount, a.childNoBedCount, a.dateToGo, a.note);

                        if (PostToGaefa.postStatus == false)
                        {
                            a.postedToGaefa = false;
                            var query = from u in db.user_info select u;
                            var subjectPostFail = "An error has occured when posting to Gaefa from WebTest";
                            var bodyPostFail = "<p>Hi, admin.</p>";
                            bodyPostFail += "<p>There is an error detected when the system wanted to post tour package with booking code <b>" + bookCode + "</b> to Gaefa</p>";
                            bodyPostFail += "<p>In order to synchronize the order on your database with Gaefa's database, you have to login as admin at your website and press <b>Sync</b> button at the homepage.</p>";
                            bodyPostFail += "<br/>";
                            bodyPostFail += "<p>Sorry for the incovenience.</p>";
                            BasicHelper.sendEmail(query.FirstOrDefault().email, subjectPostFail, bodyPostFail);
                        }
                        else
                        {
                            a.postedToGaefa = true;
                        }

                        if (ModelState.IsValid)
                        {
                            db.gaefa_book_new.Add(a);
                            db.gaefa_pic_info.Add(p);
                            db.SaveChanges();

                            var subject = "Checkout Confirmation";
                            var body = "<p>Hi, you have just checkout tour package to " + packageJSON.location + " with booking code: <b>" + bookCode + "</b>.</p>";
                            body += "<p>In order to be able to use your tour ticket to " + packageJSON.location + ", you have to confirm your booking first.</p>";
                            body += "<p>Please head to our 'Find Booking' section to find your booking details via our website or you may click this <a href='" + Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("FindBooking", "Gaefa") + "'>link</a> to head over there.</p>";
                            body += "<p>You will have to input your email and booking code to confirm.</p>";
                            BasicHelper.sendEmail(picEmail.Trim(), subject, body);
                        }
                        else
                        {
                            return RedirectToAction("Error", "Gaefa");
                        }

                        return RedirectToAction("ZeroPayment","Gaefa", new { bookingCode = bookCode });
                    }

                    if (payopt == "PayPal")
                    {
                        return RedirectToAction("PayPalPayment", "Gaefa", new { destination = packageJSON.location, tourID = tourID, packageQuantity = adult + child + childNoBed, adult = adult, child = child, childNoBed = childNoBed, bookingCode = bookCode, orderReference = orderReference, email = picEmail.Trim(), tourDate = tourDate, totalPrice = totalPrice, picName = picName, picAddress = picAddress.Trim(), picTelNumber = picTelCode.ToString() + picTelephone.ToString(), discCode = couponDiscX, note = note.Replace("\r\n", "%0D%0A"), originalPrice = originalPrice, discType = discType, discFlag = discFlag });
                        //return RedirectToAction("PayPalPayment", "Gaefa", new { destination = "a", tourID = tourID, packageQuantity = adult + child + childNoBed, adult = adult, child = child, childNoBed = childNoBed, bookingCode = bookCode, orderReference = orderReference, email = picEmail.Trim(), tourDate = tourDate, totalPrice = totalPrice, picName = picName, picAddress = picAddress.Trim(), picTelNumber = picTelCode.ToString() + picTelephone.ToString(), discCode = couponDiscX, note = note.Replace("\r\n", "%0D%0A"), originalPrice = originalPrice, discType = discType, discFlag = discFlag });
                    }
                    else
                    {
                        string promoCode;
                        string couponCode;
                        if (discType == 0)
                        {
                            couponCode = couponDiscX;
                            promoCode = null;
                        }
                        else if (discType == 1)
                        {
                            couponCode = null;
                            promoCode = couponDiscX;
                        }
                        else
                        {
                            couponCode = null;
                            promoCode = null;
                        }

                        gaefa_book_new a = new gaefa_book_new
                        {
                            bookCode = bookCode,
                            tourID = tourID,
                            email = picEmail.Trim(),
                            dateOrder = DateTime.Now,
                            dateToGo = tourDate,
                            orderReference = orderReference,
                            paymentMethod = payopt,
                            status = "Unpaid",
                            adultCount = adult,
                            childCount = child,
                            childNoBedCount = childNoBed,
                            passengerAmount = adult + child + childNoBed,
                            totalPrice = totalPrice,
                            note = note.Replace("\r\n", "%0D%0A"),
                            couponCode = couponCode,
                            promoCode = promoCode,
                            postedToGaefa = false,
                            postedPassenger = false,
                        };


                        gaefa_pic_info p = new gaefa_pic_info
                        {
                            bookCode = bookCode,
                            email = picEmail.Trim(),
                            address = picAddress.Trim(),
                            name = picName,
                            telephone = "+" + picTelCode.ToString() + picTelephone.ToString(),
                        };

                        if (couponDiscX != null)
                        {
                            if (discType == 0)
                            {
                                var coupon_query = from c in db.gaefa_coupon where c.couponCode == couponDiscX select c;
                                coupon_query.FirstOrDefault().status = false;
                            }
                            else
                            {
                                var promo_query = from pr in db.gaefa_promo where pr.promoCode == couponDiscX select pr;
                                promo_query.FirstOrDefault().used += 1;
                            }
                        }


                        if (ModelState.IsValid)
                        {
                            db.gaefa_book_new.Add(a);
                            db.gaefa_pic_info.Add(p);
                            db.SaveChanges();

                            var subject = "Checkout Confirmation";
                            var body = "<p>Hi, you have just checkout tour package to " + packageJSON.location + " with booking code: <b>" + bookCode + "</b>.</p>";
                            body += "<p>In order to be able to use your tour ticket to " + packageJSON.location + ", you have to confirm your booking first.</p>";
                            body += "<p>Please head to our 'Find Booking' section to find your booking details via our website or you may click this <a href='" + Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("FindBooking", "Gaefa") + "'>link</a> to head over there.</p>";
                            body += "<p>You will have to input your email and booking code to confirm.</p>";
                            BasicHelper.sendEmail(picEmail.Trim(), subject, body);
                        }
                        else
                        {
                            return RedirectToAction("Error", "Gaefa");
                        }

                        return RedirectToAction("AfterTransferCheckout", "Gaefa", new { bookCode = bookCode, email = a.email });
                    }
                }
                else
                {
                    return RedirectToAction("Error", "Gaefa");
                }
            }

        }
        

        public ActionResult AfterTransferCheckout(string bookCode, string email) //Page to show after checkout using transfer method
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookCode && b.email == email select b;
            if (query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                if (query.FirstOrDefault().status != "Unpaid")
                {
                    return RedirectToAction("Error", "Gaefa");
                }
                else
                {
                    return View(model: new BookCodeAndEmail { bookCode = bookCode, email = email });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailWithoutPayment(string bookingCode) //Page that shows detail of Unpaid booking
        {
            if (bookingCode == "")
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                //return RedirectToAction("Index", "Tour");
                return RedirectToAction("Error", "Gaefa");
            }

            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;

            if(query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            string couponCode = query.FirstOrDefault().couponCode;
            string promoCode = query.FirstOrDefault().promoCode;

            if (query.FirstOrDefault() == null)
            {
                //TempData["book"] = "<script>alert('Booking not found');</script>";
                //return RedirectToAction("Index", "Tour");
                return RedirectToAction("Error", "Gaefa");
            }

            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + query.FirstOrDefault().tourID + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
            GaefaPackage package;

            if (packageJSON == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Error", "Gaefa");
            }

            var picInfo = from p in db.gaefa_pic_info where p.bookCode == bookingCode select p;
            var coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;
            var promo = from pr in db.gaefa_promo where pr.promoCode == promoCode select pr;

            int discFlag = -1;
            int discType = -1;
            int discPercentage;
            decimal discNotPercentage;
            decimal discPrice;

            if (coupon.FirstOrDefault() == null && promo.FirstOrDefault() == null)
            {
                discPercentage = 0;
                discNotPercentage = 0;
            }
            else if (coupon.FirstOrDefault() != null && promo.FirstOrDefault() == null)
            {
                discType = 0;
                discPercentage = coupon.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = coupon.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }
            else //if promo
            {
                discType = 1;
                discPercentage = promo.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = promo.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }

            var originalPrice = (packageJSON.priceAdult * query.FirstOrDefault().adultCount) + (packageJSON.priceChild * query.FirstOrDefault().childCount) + (packageJSON.priceChildNoBed * query.FirstOrDefault().childNoBedCount);

            if (discFlag == 0)
            {
                discPrice = (decimal)originalPrice * discPercentage / 100;
                ViewBag.DiscAmount = discPercentage;
            }
            else if (discFlag == 1)
            {
                discPrice = discNotPercentage;
            }
            else
            {
                discPrice = 0;
            }

            ViewBag.DiscType = discType;
            ViewBag.DiscFlag = discFlag;
            ViewBag.OriginalPrice = Math.Round(originalPrice, 2, MidpointRounding.AwayFromZero);
            ViewBag.DiscPrice = Math.Round(discPrice, 2, MidpointRounding.AwayFromZero);

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");
            
            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                name = packageJSON.name,
                note = packageJSON.note,
                priceAdult = packageJSON.priceAdult,
                priceChild = packageJSON.priceChild,
                priceChildNoBed = packageJSON.priceChildNoBed,
                rangedDate = packageJSON.rangedDate,
                selectedDate = packageJSON.selectedDate,
            };

            return View("DetailWithoutPayment", new GaefaDetailNew() { booking = query.FirstOrDefault(), gaefaPackage = package, picInfo = picInfo.FirstOrDefault() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer(string bookingCode) //Page to fill detail of transfer
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;

            if (query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            string couponCode = query.FirstOrDefault().couponCode;
            string promoCode = query.FirstOrDefault().promoCode;

            if (query.FirstOrDefault() == null || query.FirstOrDefault().status == "Waiting" || query.FirstOrDefault().status == "Paid")
            {
                //return RedirectToAction("Index", "Tour");
                return RedirectToAction("Error", "Gaefa");
            }

            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + query.FirstOrDefault().tourID + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
            GaefaPackage package;

            if (packageJSON == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Error", "Gaefa");
            }

            var picInfo = from p in db.gaefa_pic_info where p.bookCode == bookingCode select p;
            var coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;
            var promo = from pr in db.gaefa_promo where pr.promoCode == promoCode select pr;

            int discFlag = -1;
            int discType = -1;
            int discPercentage;
            decimal discNotPercentage;
            decimal discPrice;

            if (coupon.FirstOrDefault() == null && promo.FirstOrDefault() == null)
            {
                discPercentage = 0;
                discNotPercentage = 0;
            }
            else if (coupon.FirstOrDefault() != null && promo.FirstOrDefault() == null)
            {
                discType = 0;
                discPercentage = coupon.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = coupon.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }
            else //if promo
            {
                discType = 1;
                discPercentage = promo.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = promo.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }
            

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                name = packageJSON.name,
                note = packageJSON.note,
                priceAdult = packageJSON.priceAdult,
                priceChild = packageJSON.priceChild,
                priceChildNoBed = packageJSON.priceChildNoBed,
                rangedDate = packageJSON.rangedDate,
                selectedDate = packageJSON.selectedDate,
            };

            var originalPrice = (packageJSON.priceAdult * query.FirstOrDefault().adultCount) + (packageJSON.priceChild * query.FirstOrDefault().childCount) + (packageJSON.priceChildNoBed * query.FirstOrDefault().childNoBedCount);
            //var originalPrice = packageJSON.pricePerPack * query.FirstOrDefault().passengerAmount;

            if (discFlag == 0)
            {
                discPrice = (decimal)originalPrice * discPercentage / 100;
                ViewBag.DiscAmount = discPercentage;
            }
            else if(discFlag == 1)
            {
                discPrice = discNotPercentage;
            }
            else
            {
                discPrice = 0;
            }

            ViewBag.DiscType = discType;
            ViewBag.DiscFlag = discFlag;
            ViewBag.OriginalPrice = Math.Round(originalPrice, 2, MidpointRounding.AwayFromZero);
            ViewBag.DiscPrice = Math.Round(discPrice, 2, MidpointRounding.AwayFromZero);

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            if (query.FirstOrDefault().status == "Disapproved") ViewBag.Disapproved = true;
            else ViewBag.Disapproved = false;

            return View("Transfer", new GaefaDetailNew() { booking = query.FirstOrDefault(), gaefaPackage = package, picInfo = picInfo.FirstOrDefault() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPaymentTransfer(string bookCode, DateTime date, string sender, decimal price, string bank, string bankFrom, string account, HttpPostedFileBase receipt) //Process transfer detail
        {
            string path;
            System.Diagnostics.Debug.WriteLine(receipt);
            if (receipt != null && receipt.ContentLength > 0)
            {
                var fileName = Path.GetFileName(receipt.FileName);
                var extension = Path.GetExtension(fileName);
                var newFileName = bookCode + extension;
                path = Request.Url.GetLeftPart(UriPartial.Authority) + Path.Combine("/Images/Receipts/", newFileName);
                var pathWithRoot = Path.Combine(Server.MapPath("~/Images/Receipts/"), newFileName);
                receipt.SaveAs(pathWithRoot);
            }
            else
            {
                path = null;
            }

            var query = from b in db.gaefa_book_new where b.bookCode == bookCode select b;

            gaefa_transfer_new x = new gaefa_transfer_new
            {
                bookCode = bookCode,
                paymentDate = date,
                approveDate = null,
                senderName = sender.Trim(),
                accountNumber = account,
                fromBank = bankFrom,
                amountTransferred = price,
                bankName = bank,
                email = query.FirstOrDefault().email.Trim(),
                receipt = path,
            };

            if (ModelState.IsValid)
            {
                db.gaefa_transfer_new.Add(x);
                query.FirstOrDefault().status = "Waiting";
                db.SaveChanges();
                var subject = "Please wait for the admin to approve your booking.";
                var body = "<p>Hi, please wait for the approval of the admin for your tour package with booking Code: <b>" + bookCode + "</b>.";
                body += "<p>You will be sent an email by our admin after we have approve your payment.</p>";
                body += "<br/>";
                body += "<p>Thank you for using our services.</p>";
                body += "<p>Please come back again.</p>";
                BasicHelper.sendEmail(x.email, subject, body);
            }

            ViewBag.AfterTransfer = true;
            //return RedirectToAction("AfterTransferConfirmation", "Gaefa");
            return View("FindBooking");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailWithPayment(string bookingCode) //Page to show detail of Waiting and Paid booking
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;
            var transfer = from t in db.gaefa_transfer_new where t.bookCode == bookingCode select t;

            if (query.FirstOrDefault() == null || transfer.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + query.FirstOrDefault().tourID + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
            GaefaPackage package;

            if (packageJSON == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Error", "Gaefa");
            }

            string couponCode = query.FirstOrDefault().couponCode;
            string promoCode = query.FirstOrDefault().promoCode;

            var picInfo = from p in db.gaefa_pic_info where p.bookCode == bookingCode select p;
            var coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;
            var promo = from pr in db.gaefa_promo where pr.promoCode == promoCode select pr;

            int discFlag = -1;
            int discType = -1;
            int discPercentage;
            decimal discNotPercentage;
            decimal discPrice;

            if (coupon.FirstOrDefault() == null && promo.FirstOrDefault() == null)
            {
                discPercentage = 0;
                discNotPercentage = 0;
            }
            else if (coupon.FirstOrDefault() != null && promo.FirstOrDefault() == null)
            {
                discType = 0;
                discPercentage = coupon.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = coupon.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }
            else //if promo
            {
                discType = 1;
                discPercentage = promo.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = promo.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }
            var originalPrice = (packageJSON.priceAdult * query.FirstOrDefault().adultCount) + (packageJSON.priceChild * query.FirstOrDefault().childCount) + (packageJSON.priceChildNoBed * query.FirstOrDefault().childNoBedCount);
            //var originalPrice = packageJSON.pricePerPack * query.FirstOrDefault().passengerAmount;

            if (discFlag == 0)
            {
                discPrice = (decimal)originalPrice * discPercentage / 100;
                ViewBag.DiscAmount = discPercentage;
            }
            else if (discFlag == 1)
            {
                discPrice = discNotPercentage;
            }
            else
            {
                discPrice = 0;
            }

            ViewBag.DiscType = discType;
            ViewBag.DiscFlag = discFlag;
            ViewBag.OriginalPrice = Math.Round(originalPrice, 2, MidpointRounding.AwayFromZero);
            ViewBag.DiscPrice = Math.Round(discPrice, 2, MidpointRounding.AwayFromZero);

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");
            
            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                name = packageJSON.name,
                note = packageJSON.note,
                priceAdult = packageJSON.priceAdult,
                priceChild = packageJSON.priceChild,
                priceChildNoBed = packageJSON.priceChildNoBed,
                rangedDate = packageJSON.rangedDate,
                selectedDate = packageJSON.selectedDate,
            };


            return View(new BookingAndTransferNew { booking = new GaefaDetailNew { booking = query.FirstOrDefault(), gaefaPackage = package }, transfer = transfer.FirstOrDefault(), picInfo = picInfo.FirstOrDefault() });
        }
        
        [SessionExpire]
        public ActionResult Approve() //Page to show list of payment needed to be approved by admin
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                GaefaSignature sign = new GaefaSignature();
                GaefaFilter filter = new GaefaFilter()
                {
                    titleOrLocation = "",
                    include_flight = null,
                    include_inn = null,
                    tag = "",
                };
                GaefaPackageInformation info = new GaefaPackageInformation(filter);
                GaefaPagination pagination = new GaefaPagination()
                {
                    limit = int.Parse(info.GetListTotal()),
                    start = 0,
                };
                GaefaPackageSort sort = new GaefaPackageSort()
                {
                    sortMode = GaefaPackageSort.sort_mode.DESCENDING,
                    sortType = GaefaPackageSort.sort_type.cdate,
                };
                JSONParser json = new JSONParser();
                json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode + "&tag=" + filter.tag;
                System.Diagnostics.Debug.WriteLine(json.Url);
                //string url = GaefaPackageUrl();
                List<GaefaPackageJSON> ListOfGaefaPackageJSON = json.GetGaefaPackageArray(json.Url);
                List<GaefaPackage> ListOfGaefaPackage = new List<GaefaPackage>();

                if (ListOfGaefaPackageJSON == null)
                {
                    return RedirectToAction("Error", "Gaefa");
                }
                else
                {
                    for (int i = 0; i < ListOfGaefaPackageJSON.Count; i++)
                    {

                        ListOfGaefaPackage.Add(new GaefaPackage
                        {
                            id = ListOfGaefaPackageJSON[i].id,
                            data = JsonConvert.DeserializeObject<Data>(ListOfGaefaPackageJSON[i].data),
                            duration = ListOfGaefaPackageJSON[i].duration,
                            includeFlight = ListOfGaefaPackageJSON[i].includeFlight,
                            includeHotel = ListOfGaefaPackageJSON[i].includeHotel,
                            location = ListOfGaefaPackageJSON[i].location,
                            name = ListOfGaefaPackageJSON[i].name,
                            note = ListOfGaefaPackageJSON[i].note,
                            priceAdult = ListOfGaefaPackageJSON[i].priceAdult,
                            priceChild = ListOfGaefaPackageJSON[i].priceChild,
                            priceChildNoBed = ListOfGaefaPackageJSON[i].priceChildNoBed,
                            rangedDate = ListOfGaefaPackageJSON[i].rangedDate,
                            selectedDate = ListOfGaefaPackageJSON[i].selectedDate,
                        });
                    }

                    DateTime DateNow = DateTime.Now.Date;
                    var userID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());

                    var queryBook = from b in db.gaefa_book_new where b.status == "Waiting" select b;

                    return View(new GaefaMultipleModelNew() { booking = queryBook.ToList(), gaefaPackage = ListOfGaefaPackage });
                }
            }
            else
            {
                //return RedirectToAction("Index", "Home");
                return RedirectToAction("Error", "Gaefa");
            }
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovePayment(string bookCode, string approveChoice) //Process approving payment
        {
            //if(Request.Form["approveButton"] != null)
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            var query = from b in db.gaefa_book_new where b.bookCode == bookCode select b;
            var transfer = from t in db.gaefa_transfer_new where t.bookCode == bookCode select t;
            if (approveChoice == "Approve")
            {
                //TempData["Approved"] = "<script>alert('Payment Approved.');</script>";
                query.FirstOrDefault().status = "Paid";
                transfer.FirstOrDefault().approveDate = DateTime.Now;

                
                PostToGaefaDB PostToGaefa = new PostToGaefaDB();
                PostToGaefa.Post(query.FirstOrDefault().tourID, query.FirstOrDefault().orderReference, query.FirstOrDefault().adultCount, query.FirstOrDefault().childCount, query.FirstOrDefault().childNoBedCount, query.FirstOrDefault().dateToGo, query.FirstOrDefault().note);

                if (PostToGaefa.postStatus == true)
                {
                    query.FirstOrDefault().postedToGaefa = true;
                    db.SaveChanges();

                    var subject = "Payment Approved";
                    var body = "<p>Hi, your payment for booking code: <b>" + bookCode + "</b> has been approved by our admin.</p>";
                    body += "<p>You can check the details by using our 'Find Booking' section to find your booking details via our website or you may click this <a href='" + Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("FindBooking", "Gaefa") + "'>link</a> to head over there.</p>";
                    body += "<p>You will then have to input your email and booking code to check the details.</p>";
                    body += "<br/>";
                    body += "<p>Thank you for using our services.</p>";
                    BasicHelper.sendEmail(query.FirstOrDefault().email, subject, body);
                    //return RedirectToAction("ApproveList", "Booking");
                    return Json(new { status = "approve" });
                }
                else
                {
                    return Json(new { status = "error" });
                }
                
            }
            else
            {
                //TempData["Approved"] = "<script>alert('Payment Disapproved.');</script>";
                query.FirstOrDefault().status = "Disapproved";
                db.gaefa_transfer_new.Remove(transfer.FirstOrDefault());
                db.SaveChanges();

                var subject = "Payment Disapproved";
                var body = "<p>Hi, it seems your payment for booking code: <b>" + bookCode + "</b> has been disapproved by our admin.</p>";
                body += "<p>You can re-confirm your payment by using our 'Find Booking' section to find your booking details via our website or you may click this <a href='" + Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("FindBooking", "Gaefa") + "'>link</a> to head over there.</p>";
                body += "<p>You will then have to input your email and booking code in order to reconfirm your payment.</p>";
                body += "<br/>";
                body += "<p>Sorry for the inconvenience.</p>";
                BasicHelper.sendEmail(query.FirstOrDefault().email, subject, body);
                //return RedirectToAction("ApproveList", "Booking");
                return Json(new { status = "disapprove" });
            }
        }
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelBooking(string bookingCode) //Function to cancel booking that is still not paid
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;
            if (query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            var picInfo = from i in db.gaefa_pic_info where i.bookCode == bookingCode select i;
            if (picInfo.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            if (query.FirstOrDefault().status == "Paid") return RedirectToAction("Error", "Gaefa");
            db.gaefa_pic_info.Remove(picInfo.FirstOrDefault());
            db.gaefa_book_new.Remove(query.FirstOrDefault());
            db.SaveChanges();

            var subject = "Booking Canceled";
            var body = "<p>Hi, You have just canceled your booking with booking code <b>" + bookingCode + "</b></p>";
            body += "<br/>";
            body += "<p>Thank you for using our services.</p>";
            BasicHelper.sendEmail(query.FirstOrDefault().email, subject, body);

            return Json(new { status = "success" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DisapprovedTransfer(string bookingCode) //Not used anymore
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;
            if(query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            string couponCode = query.FirstOrDefault().couponCode;
            string promoCode = query.FirstOrDefault().promoCode;


            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + query.FirstOrDefault().tourID + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
            GaefaPackage package;

            if (packageJSON == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Error", "Gaefa");
            }
            

            var picInfo = from p in db.gaefa_pic_info where p.bookCode == bookingCode select p;
            var coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;
            var promo = from pr in db.gaefa_promo where pr.promoCode == promoCode select pr;

            int discFlag = -1;
            int discType = -1;
            int discPercentage;
            decimal discNotPercentage;
            decimal discPrice;

            if (coupon.FirstOrDefault() == null && promo.FirstOrDefault() == null)
            {
                discPercentage = 0;
                discNotPercentage = 0;
            }
            else if (coupon.FirstOrDefault() != null && promo.FirstOrDefault() == null)
            {
                discType = 0;
                discPercentage = coupon.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = coupon.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }
            else //if promo
            {
                discType = 1;
                discPercentage = promo.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = promo.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }
            
            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                name = packageJSON.name,
                note = packageJSON.note,
                priceAdult = packageJSON.priceAdult,
                priceChild = packageJSON.priceChild,
                priceChildNoBed = packageJSON.priceChildNoBed,
                rangedDate = packageJSON.rangedDate,
                selectedDate = packageJSON.selectedDate,
            };

            var originalPrice = (packageJSON.priceAdult * query.FirstOrDefault().adultCount) + (packageJSON.priceChild * query.FirstOrDefault().childCount) + (packageJSON.priceChildNoBed * query.FirstOrDefault().childNoBedCount);
            //var originalPrice = packageJSON.pricePerPack * query.FirstOrDefault().passengerAmount;

            if (discFlag == 0)
            {
                discPrice = (decimal)originalPrice * discPercentage / 100;
                ViewBag.DiscAmount = discPercentage;
            }
            else if (discFlag == 1)
            {
                discPrice = discNotPercentage;
            }
            else
            {
                discPrice = 0;
            }

            ViewBag.DiscType = discType;
            ViewBag.DiscFlag = discFlag;
            ViewBag.OriginalPrice = Math.Round(originalPrice, 2, MidpointRounding.AwayFromZero);
            ViewBag.DiscPrice = Math.Round(discPrice, 2, MidpointRounding.AwayFromZero);

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            return View(model: new GaefaDetailNew() { booking = query.FirstOrDefault(), gaefaPackage = package, picInfo = picInfo.FirstOrDefault() });
        }

        //PAYPAL SECTION//////////////////////////////////////////////

        
        public ActionResult PayPalPayment(string destination, int packageQuantity, string bookingCode, int tourID, string orderReference, string email, string discCode, string note, DateTime tourDate, decimal totalPrice, string picName, string picAddress, string picTelNumber, int adult, int child, int childNoBed, decimal originalPrice, int discType, int discFlag) //Execute PayPal Payment
        {
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + tourID + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);

            if (packageJSON == null) //if (packageJSON != null)
            {
                //return HttpNotFound();
                return RedirectToAction("Error", "Gaefa");
            }
            
            int discPercentage;
            decimal discNotPercentage;
            decimal discPrice = (decimal)0.00;
            string couponCodeX = discCode;
            PayPalExpressCheckout paypal = new PayPalExpressCheckout();
            paypal.RETURN_URL = Request.Url.GetLeftPart(UriPartial.Authority) + "/Gaefa/PayPalSuccess?bookCode=" + bookingCode + "&quantity=" + packageQuantity + "&tourID=" + tourID + "&orderReference=" + orderReference + "&email=" + email + "&tourDate=" + tourDate + "&picName=" + picName + "&picAddress=" + picAddress + "&picTelNumber=" + picTelNumber + "&adult=" + adult + "&child=" + child + "&childNoBed=" + childNoBed + "&discCode=" + discCode + "&discType=" + discType + "&note=" + note;
            paypal.CANCEL_URL = Request.Url.GetLeftPart(UriPartial.Authority) + "/Gaefa/PayPalCancel/";
            
            if(discCode == "" || discCode == null)
            {
                discCode = null;
                discPercentage = 0;
                discPrice = (decimal)0.00;
            }
            else
            {
                if(discType == 0)
                {
                    var query_coupon = from c in db.gaefa_coupon where c.couponCode == discCode select c;

                    if (query_coupon.FirstOrDefault() == null)
                    {
                        discPercentage = 0;
                        discNotPercentage = 0;
                        discPrice = (decimal)0.00;
                        couponCodeX = null;
                    }
                    else
                    {
                        if (query_coupon.FirstOrDefault().status == true)
                        {
                            discPercentage = query_coupon.FirstOrDefault().discPercentage ?? 0;
                            discNotPercentage = query_coupon.FirstOrDefault().discPrice ?? 0;
                        }
                        else
                        {
                            discPercentage = 0;
                            discNotPercentage = 0;
                            couponCodeX = null;
                        }
                    }
                }
                else if(discType == 1)
                {
                    var query_promo = from pr in db.gaefa_promo where pr.promoCode == discCode select pr;

                    if (query_promo.FirstOrDefault() == null)
                    {
                        discPercentage = 0;
                        discNotPercentage = 0;
                        discPrice = (decimal)0.00;
                        couponCodeX = null;
                    }
                    else
                    {
                        if (query_promo.FirstOrDefault().used < query_promo.FirstOrDefault().amount)
                        {
                            discPercentage = query_promo.FirstOrDefault().discPercentage ?? 0;
                            discNotPercentage = query_promo.FirstOrDefault().discPrice ?? 0;
                        }
                        else
                        {
                            discPercentage = 0;
                            discNotPercentage = 0;
                            couponCodeX = null;
                        }
                    }
                }
                else
                {
                    discPercentage = 0;
                    discNotPercentage = 0;
                }

                if (discFlag == 0)
                {
                    discPrice = (decimal)originalPrice * discPercentage / 100;
                }
                else if (discFlag == 1)
                {
                    discPrice = discNotPercentage;
                }
                else
                {
                    discPrice = 0;
                }

            }

            
            if (paypal.SetExpressCheckout(destination, packageJSON.priceAdult, packageJSON.priceChild, packageJSON.priceChildNoBed, adult, child, childNoBed, couponCodeX, discPrice, discFlag, discPercentage, discType))
            {
                return Redirect(PayPalExpressCheckout.PayPalExpressCheckoutURL + paypal.TOKEN);
            }
            else
            {
                return RedirectToAction("PayPalFailed", "Gaefa");
            }
            
            
        }

        public ActionResult PayPalFailed() //Page to show if paypal failed
        {
            return View();
        }

        public ActionResult PayPalCancel() //Page to show if user cancel paypal payment
        {
            return View();
        }
        

        public ActionResult PayPalSuccess(string bookCode, int quantity, int adult, int child, int childNoBed, int tourID, string orderReference, string email, DateTime tourDate, string picAddress, string picName, string picTelNumber, string discCode, string note, string token, string PayerID, int discType) //Page to show after successful paypal payment
        {
            PayPalExpressCheckout paypal = new PayPalExpressCheckout();
            GetExpressCheckoutDetailsResponseType p = paypal.GetExpressCheckout(token);
            DoExpressCheckoutPaymentResponseType d;

            if (discCode != "")
            {
                var coupon_query = from c in db.gaefa_coupon where c.couponCode == discCode select c;
                var promo_query = from pr in db.gaefa_promo where pr.promoCode == discCode select pr;

                if (coupon_query.FirstOrDefault() == null && promo_query.FirstOrDefault() == null)
                {
                    return RedirectToAction("PayPalFailed", "Gaefa");
                }
                else if(coupon_query.FirstOrDefault() != null && promo_query.FirstOrDefault() == null)
                {
                    if (!coupon_query.FirstOrDefault().status) return RedirectToAction("PayPalFailed", "Gaefa");
                }
                else if(coupon_query.FirstOrDefault() == null && promo_query.FirstOrDefault() != null)
                {
                    if (promo_query.FirstOrDefault().used >= promo_query.FirstOrDefault().amount && promo_query.FirstOrDefault().amount != -1) return RedirectToAction("PayPalFailed", "Gaefa");
                }
            }

            d = paypal.DoExpressCheckout();

            if (d.Ack.ToString() == "SUCCESS")
            {
                GaefaSignature sign = new GaefaSignature();
                JSONParser json = new JSONParser();
                //json.Url = json.UrlPackage + id;
                json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + tourID + "&signature=" + sign.GetSignature();
                GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);

                if (packageJSON == null)//if(packageJSON != null)
                {
                    return RedirectToAction("Error", "Gaefa");
                }
                else
                {
                    string promoCode;
                    string couponCode;
                    if (discType == 0)
                    {
                        couponCode = discCode;
                        promoCode = null;
                    }
                    else if (discType == 1)
                    {
                        couponCode = null;
                        promoCode = discCode;
                    }
                    else
                    {
                        couponCode = null;
                        promoCode = null;
                    }

                    gaefa_book_new a = new gaefa_book_new
                    {
                        orderReference = orderReference,
                        email = email.Trim(),
                        bookCode = bookCode,
                        dateToGo = tourDate,
                        tourID = tourID,
                        paymentMethod = "PayPal",
                        adultCount = adult,
                        childCount = child,
                        childNoBedCount = childNoBed,
                        passengerAmount = quantity,
                        status = "Paid",
                        totalPrice = decimal.Parse(p.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].OrderTotal.value),
                        dateOrder = DateTime.Now,
                        couponCode = couponCode,
                        promoCode = promoCode,
                        note = note,
                        postedToGaefa = false,
                        postedPassenger = false,
                    };
                    
                    gaefa_paypal_new pp = new gaefa_paypal_new
                    {
                        bookCode = bookCode,
                        paypalAddress = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.Street1 + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.CityName + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.StateOrProvince + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.CountryName + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.PostalCode,
                        paypalAmount = decimal.Parse(p.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].OrderTotal.value),
                        paypalDateAndTime = (d.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].PaymentDate),
                        paypalPayerID = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID,
                        paypalTransactionID = d.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID,
                        paypalName = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerName.FirstName + " " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerName.LastName,
                        //paypalNote = p.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].NoteText,
                    };

                    if (discCode != "")
                    {
                        if (discType == 0)
                        {
                            var coupon_query = from c in db.gaefa_coupon where c.couponCode == discCode select c;
                            coupon_query.FirstOrDefault().status = false;
                        }
                        else
                        {
                            var promo_query = from pr in db.gaefa_promo where pr.promoCode == discCode select pr;
                            promo_query.FirstOrDefault().used += 1;
                        }
                    }

                    gaefa_pic_info info = new gaefa_pic_info
                    {
                        bookCode = bookCode,
                        email = email.Trim(),
                        address = picAddress,
                        name = picName,
                        telephone = "+" + picTelNumber,
                    };

                    if (ModelState.IsValid)
                    {
                        db.gaefa_book_new.Add(a);
                        db.gaefa_paypal_new.Add(pp);
                        db.gaefa_pic_info.Add(info);

                        db.SaveChanges();


                        PostToGaefaDB PostToGaefa = new PostToGaefaDB();
                        PostToGaefa.Post(tourID, orderReference, adult, child, childNoBed, tourDate, note);

                        if (PostToGaefa.postStatus == false)
                        {
                            var query = from u in db.user_info select u;
                            var subjectPostFail = "An error has occured when posting to Gaefa from WebTest";
                            var bodyPostFail = "<p>Hi, admin.</p>";
                            bodyPostFail += "<p>There is an error detected when the system wanted to post tour package with booking code <b>" + bookCode + "</b> to Gaefa</p>";
                            bodyPostFail += "<p>In order to synchronize the order on your database with Gaefa's database, you have to login as admin at your website and press <b>Sync</b> button at the homepage.</p>";
                            bodyPostFail += "<br/>";
                            bodyPostFail += "<p>Sorry for the incovenience.</p>";
                            BasicHelper.sendEmail(query.FirstOrDefault().email, subjectPostFail, bodyPostFail);
                        }
                        else
                        {
                            a.postedToGaefa = true;
                            db.SaveChanges();
                        }


                        var subject = "Payment Success. Your tour package is ready.";
                        var body = "<p>Hi, you have just made a succesful paypal payment for tour package to " + packageJSON.location + " with booking code <b>" + bookCode + "</b></p>";
                        body += "<p>You can now view your package detail via our 'Find Booking' section on our website or you may click this <a href='" + Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("FindBooking", "Gaefa") + "'>link</a> to head over there.</p>";
                        body += "<p>You will then have to input your email and booking code in order to view your package detail.</p>";
                        body += "<br/>";
                        body += "<p>Thank you for using our service.</p>";
                        BasicHelper.sendEmail(a.email, subject, body);

                    };
                    return View(model: bookCode);
                }
            }
            else
            {
                return RedirectToAction("Error", "Gaefa");
            }
            
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayPalDetail(string bookingCode) //Page to show about the detail of paid paypal payment
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;
            var paypal = from p in db.gaefa_paypal_new where p.bookCode == bookingCode select p;
            

            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + query.FirstOrDefault().tourID + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
            GaefaPackage package;

            if (packageJSON == null)
            {
                //return HttpNotFound();
                return RedirectToAction("Error", "Gaefa");
            }

            string couponCode = query.FirstOrDefault().couponCode;
            string promoCode = query.FirstOrDefault().promoCode;

            var picInfo = from p in db.gaefa_pic_info where p.bookCode == bookingCode select p;
            var coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;
            var promo = from pr in db.gaefa_promo where pr.promoCode == promoCode select pr;

            int discFlag = -1;
            int discType = -1;
            int discPercentage;
            decimal discNotPercentage;
            decimal discPrice;

            if (coupon.FirstOrDefault() == null && promo.FirstOrDefault() == null)
            {
                discPercentage = 0;
                discNotPercentage = 0;
            }
            else if (coupon.FirstOrDefault() != null && promo.FirstOrDefault() == null)
            {
                discType = 0;
                discPercentage = coupon.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = coupon.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }
            else //if promo
            {
                discType = 1;
                discPercentage = promo.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = promo.FirstOrDefault().discPrice ?? 0;

                if (discPercentage == 0)
                {
                    discFlag = 1;
                }
                else if (discNotPercentage == 0)
                {
                    discFlag = 0;
                }
            }

            var originalPrice = (packageJSON.priceAdult * query.FirstOrDefault().adultCount) + (packageJSON.priceChild * query.FirstOrDefault().childCount) + (packageJSON.priceChildNoBed * query.FirstOrDefault().childNoBedCount);
            //var originalPrice = packageJSON.pricePerPack * query.FirstOrDefault().passengerAmount;

            if (discFlag == 0)
            {
                discPrice = (decimal)originalPrice * discPercentage / 100;
                ViewBag.DiscAmount = discPercentage;
            }
            else if (discFlag == 1)
            {
                discPrice = discNotPercentage;
            }
            else
            {
                discPrice = 0;
            }

            ViewBag.DiscType = discType;
            ViewBag.DiscFlag = discFlag;
            ViewBag.OriginalPrice = Math.Round(originalPrice, 2, MidpointRounding.AwayFromZero);
            ViewBag.DiscPrice = Math.Round(discPrice, 2, MidpointRounding.AwayFromZero);
            double discountedPrice = originalPrice - decimal.ToDouble(discPrice);
            ViewBag.Tax = Math.Round(((discountedPrice + 0.3) * 1000 / 961) - discountedPrice, 2, MidpointRounding.AwayFromZero);

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");
            

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                name = packageJSON.name,
                note = packageJSON.note,
                priceAdult = packageJSON.priceAdult,
                priceChild = packageJSON.priceChild,
                priceChildNoBed = packageJSON.priceChildNoBed,
                rangedDate = packageJSON.rangedDate,
                selectedDate = packageJSON.selectedDate,
            };

            //var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime.Substring(0, 10), "yyyy-MM-dd-hh:mm:ss", CultureInfo.InvariantCulture).AddHours(7)).ToString();
            //var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime, "yyyy-MM-ddThh:mm:ssZ", CultureInfo.InvariantCulture)).ToString();
            var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime, "yyyy-MM-ddThh:mm:ssZ", CultureInfo.InvariantCulture)).ToString("dd/MM/yyyy - HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            System.Diagnostics.Debug.WriteLine(paypalDateCultureInvariant);
            //paypal.FirstOrDefault().paypalDateAndTime = paypalDateCultureInvariant.Substring(2, 2) + "/" + paypalDateCultureInvariant.Substring(0, 1) + "/" + paypalDateCultureInvariant.Substring(5, 4) + " - " + paypal.FirstOrDefault().paypalDateAndTime.Substring(11, 8) + " (GMT)";
            paypal.FirstOrDefault().paypalDateAndTime = paypalDateCultureInvariant;

            return View(new BookingAndPayPalNew { booking = new GaefaDetailNew { booking = query.FirstOrDefault(), gaefaPackage = package }, paypal = paypal.FirstOrDefault(), picInfo = picInfo.FirstOrDefault() });
        }

        //END OF PAYPAL SECTION//////////////////////////////////////

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FindBooking(string email, string bookingCode) //Process find booking
        {
            var query_book = from b in db.gaefa_book_new where b.email == email && b.bookCode == bookingCode select b;

            if (query_book.FirstOrDefault() == null)
            {
                return Json(new { status = "failed", detail = "null" });
            }
            else
            {
                GaefaSignature sign = new GaefaSignature();
                JSONParser json = new JSONParser();
                //json.Url = json.UrlPackage + id;
                json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + query_book.FirstOrDefault().tourID + "&signature=" + sign.GetSignature();
                GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);

                if(packageJSON == null)
                {
                    return RedirectToAction("Error", "Gaefa");
                }

                return Json(new { status = "success", detail = packageJSON, bookCode = query_book.FirstOrDefault().bookCode, bookStatus = query_book.FirstOrDefault().status, method = query_book.FirstOrDefault().paymentMethod, date = query_book.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy") });
            }
        }
        
        
        [SessionExpire]
        public ActionResult Sold() //Page to show list of sold package
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                var queryBook = from b in db.gaefa_book_new where b.status == "Paid" select b;
                List<int> id = new List<int>();
                List<GaefaPackageJSON> packageJSONList = new List<GaefaPackageJSON>();
                List<GaefaPackage> packageList = new List<GaefaPackage>();
                GaefaPackageJSON tempPackage;

                queryBook.ToList().ForEach(x => id.Add(x.tourID));

                id = id.Distinct().ToList();

                GaefaSignature sign = new GaefaSignature();
                JSONParser json = new JSONParser();

                id.ForEach(x =>
                {
                    json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + x + "&signature=" + sign.GetSignature();
                    tempPackage = json.GetGaefaPackage(json.Url);
                    if (tempPackage != null)
                    {
                        packageJSONList.Add(tempPackage);
                    }
                });

                if(packageJSONList.Count == 0)
                {
                }
                else
                {
                    packageJSONList.ForEach(x =>
                    {
                        packageList.Add(new GaefaPackage
                        {
                            id = x.id,
                            data = JsonConvert.DeserializeObject<Data>(x.data),
                            duration = x.duration,
                            includeFlight = x.includeFlight,
                            includeHotel = x.includeHotel,
                            location = x.location,
                            name = x.name,
                            note = x.note,
                            priceAdult = x.priceAdult,
                            priceChild = x.priceChild,
                            priceChildNoBed = x.priceChildNoBed,
                            rangedDate = x.rangedDate,
                            selectedDate = x.selectedDate,
                        });
                    });
                }

                return View(new GaefaMultipleModelNew() { booking = queryBook.ToList(), gaefaPackage = packageList });

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SyncWithGaefa() //Sync package with Gaefa if the preceding post error
        {
            var query_un_posted = from p in db.gaefa_book_new where p.postedToGaefa == false && p.status == "Paid" select p;
            
            int package_id;
            string order_reference;
            int adult_pack, child_pack, child_nobed_pack;
            DateTime date;
            string note;

            int countFailed = 0;
            int UnpostedCount = query_un_posted.Count();

            PostToGaefaDB PostToGaefa = new PostToGaefaDB();

            if(UnpostedCount == 0 || query_un_posted == null)
            {
                return Json(new { status = "failed", message = "none" });
            }
            else
            {
                IQueryable<gaefa_book_new> query_book;
                foreach (var item in query_un_posted.ToList())
                {
                    query_book = from b in db.gaefa_book_new where b.bookCode == item.bookCode select b;

                    package_id = query_book.FirstOrDefault().tourID;
                    order_reference = query_book.FirstOrDefault().orderReference;
                    adult_pack = query_book.FirstOrDefault().adultCount;
                    child_pack = query_book.FirstOrDefault().childCount;
                    child_nobed_pack = query_book.FirstOrDefault().childNoBedCount;
                    date = query_book.FirstOrDefault().dateToGo;
                    note = query_book.FirstOrDefault().note;

                    PostToGaefa.Post(package_id, order_reference, adult_pack, child_pack, child_nobed_pack, date, note);

                    if(PostToGaefa.postStatus == true)
                    {
                        item.postedToGaefa = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        countFailed += 1;
                    }
                }

                var countSuccess = UnpostedCount - countFailed;
                if (countFailed > 0)
                {
                    if (UnpostedCount == 1 || countFailed == UnpostedCount)
                    {
                        return Json(new { status = "failed", message = "none", count = countSuccess });
                    }
                    else
                    {
                        return Json(new { status = "good", message = "half", count = countSuccess }); ;
                    }
                }
                else
                {
                    return Json(new { status = "success", message = "all", count = countSuccess });
                }
            }
        }
        

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetPackage(int id)
        {
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + id + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
            GaefaPackage package;

            if(packageJSON == null)
            {
                return Json(new { detail = "error"});
            }
            else
            {

                package = new GaefaPackage
                {
                    id = packageJSON.id,
                    data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                    duration = packageJSON.duration,
                    includeFlight = packageJSON.includeFlight,
                    includeHotel = packageJSON.includeHotel,
                    location = packageJSON.location,
                    name = packageJSON.name,
                    note = packageJSON.note,
                    priceAdult = packageJSON.priceAdult,
                    priceChild = packageJSON.priceChild,
                    priceChildNoBed = packageJSON.priceChildNoBed,
                    rangedDate = packageJSON.rangedDate,
                    selectedDate = packageJSON.selectedDate,
                };

                return Json(new { detail = package });
            }
            
        }
        
        [HttpGet]
        public ActionResult CheckCoupon(string couponCode) //check coupon code inputted by user
        {
            var query_coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;

            if(!query_coupon.Any())
            {
                return Json(new { status = "not found", coupon = "null", flag = -1, type = 0 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (query_coupon.FirstOrDefault().status)
                {
                    if(DateTime.Now.Date >= query_coupon.FirstOrDefault().availableDate && DateTime.Now.Date <= query_coupon.FirstOrDefault().expiryDate)
                    {
                        if(query_coupon.FirstOrDefault().discPercentage == null)
                        {
                            if(query_coupon.FirstOrDefault().discPrice < (decimal)0.00)
                            {
                                return Json(new { status = "discount amount error", coupon = "null", flag = -1, type = 0 }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { status = "success", discPercentage = query_coupon.FirstOrDefault().discPercentage, discPrice = query_coupon.FirstOrDefault().discPrice, packMin = query_coupon.FirstOrDefault().packMin, priceMin = query_coupon.FirstOrDefault().priceMin, flag = 1, type = 0 }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            if(query_coupon.FirstOrDefault().discPercentage < 0)
                            {
                                return Json(new { status = "discount amount error", coupon = "null", flag = -1, type = 0 }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { status = "success", discPercentage = query_coupon.FirstOrDefault().discPercentage, discPrice = query_coupon.FirstOrDefault().discPrice, packMin = query_coupon.FirstOrDefault().packMin, priceMin = query_coupon.FirstOrDefault().priceMin, flag = 0, type = 0 }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json(new { status = "date error", coupon = "null", type = 0 }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = "used", coupon = "null", type = 0 }, JsonRequestBehavior.AllowGet);
                }
                
            }
        }

        [HttpGet]
        public ActionResult CheckPromo(string promoCode) //check promo code inputted by user
        {
            var query_promo = from c in db.gaefa_promo where c.promoCode.Equals(promoCode, StringComparison.Ordinal) select c;

            if (!query_promo.Any())
            {
                return Json(new { status = "not found", promo = "null", flag = -1, type = 1 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (query_promo.FirstOrDefault().used < query_promo.FirstOrDefault().amount || query_promo.FirstOrDefault().amount == -1)
                {
                    if (DateTime.Now.Date >= query_promo.FirstOrDefault().availableDate && DateTime.Now.Date <= query_promo.FirstOrDefault().expiryDate)
                    {
                        if (query_promo.FirstOrDefault().discPercentage == null)
                        {
                            if (query_promo.FirstOrDefault().discPrice < (decimal)0.00)
                            {
                                return Json(new { status = "discount amount error", promo = "null", flag = -1, type = 1 }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { status = "success", discPercentage = query_promo.FirstOrDefault().discPercentage, discPrice = query_promo.FirstOrDefault().discPrice, packMin = query_promo.FirstOrDefault().packMin, priceMin = query_promo.FirstOrDefault().priceMin, flag = 1, type = 1 }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            if (query_promo.FirstOrDefault().discPercentage < 0)
                            {
                                return Json(new { status = "discount amount error", promo = "null", flag = -1, type = 1 }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { status = "success", discPercentage = query_promo.FirstOrDefault().discPercentage, discPrice = query_promo.FirstOrDefault().discPrice, packMin = query_promo.FirstOrDefault().packMin, priceMin = query_promo.FirstOrDefault().priceMin, flag = 0, type = 1 }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json(new { status = "date error", promo = "null", type = 1 }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = "used", promo = "null", type = 1  }, JsonRequestBehavior.AllowGet);
                }

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckCodeType(string code) //process if code is coupon or promo
        {
            var query_coupon = from c in db.gaefa_coupon where c.couponCode == code select c;
            var query_promo = from p in db.gaefa_promo where p.promoCode == code select p;

            if(query_coupon.FirstOrDefault() == null && query_promo.FirstOrDefault() != null)
            {
                return RedirectToAction("CheckPromo", "Gaefa", new { promoCode = code } );
                //return Json(new { type = 1 });
            }
            else if(query_coupon.FirstOrDefault() != null && query_promo.FirstOrDefault() == null)
            {
                return RedirectToAction("CheckCoupon", "Gaefa", new { couponCode = code } );
                //return Json(new { type = 0 });
            }
            else
            {
                return Json(new { type = -1 });
            }
        }

        [SessionExpire]
        public ActionResult GetDetailByReferenceView() //Show page to get detail by passing order reference to Gaefa
        {
            List<string> orderReferencesList = new List<string>();
            var query_book = from b in db.gaefa_book_new select b;

            query_book.ToList().ForEach(x =>
            {
                if(!x.postedToGaefa) { }
                else
                {
                    orderReferencesList.Add(x.orderReference);
                }
            });

            return View("GetDetailByReference", model: orderReferencesList);
        }

        [SessionExpire]
        public ActionResult GetDetailByReference(string orderReference) //Function To get detail by passing order_reference to Gaefa
        {
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetOrderDetailByOrderReference + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&order_reference=" + orderReference;
            GaefaOrderDetail detailJSON = json.GetOrderDetailByOrderReference(json.Url);

            if(detailJSON == null)
            {
                return Json(new { status = "error", detail = "null" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = "success", detail = detailJSON }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ZeroPayment(string bookingCode) //Page to view if checkout price $0 (if price < discount) NOT GOOD
        {
            return View(model: bookingCode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ZeroPaymentDetail(string bookingCode) //Page to show detail of $0 price booking
        {
            var query_book = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;
            IQueryable<gaefa_pic_info> query_pic;

            if(query_book.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                int tourID = query_book.FirstOrDefault().tourID;
                query_pic = from p in db.gaefa_pic_info where p.bookCode == bookingCode select p;

                string couponCode = query_book.FirstOrDefault().couponCode;
                string promoCode = query_book.FirstOrDefault().promoCode;

                if(query_pic.FirstOrDefault() == null)
                {
                    return RedirectToAction("Error","Gaefa");
                }
                else
                {
                    GaefaSignature sign = new GaefaSignature();
                    JSONParser json = new JSONParser();
                    //json.Url = json.UrlPackage + id;
                    json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + tourID + "&signature=" + sign.GetSignature();
                    GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);
                    GaefaPackage package;

                    if (packageJSON == null)
                    {
                        //return HttpNotFound();
                        return RedirectToAction("Error", "Gaefa");
                    }

                    package = new GaefaPackage
                    {
                        id = packageJSON.id,
                        data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                        duration = packageJSON.duration,
                        includeFlight = packageJSON.includeFlight,
                        includeHotel = packageJSON.includeHotel,
                        location = packageJSON.location,
                        name = packageJSON.name,
                        note = packageJSON.note,
                        priceAdult = packageJSON.priceAdult,
                        priceChild = packageJSON.priceChild,
                        priceChildNoBed = packageJSON.priceChildNoBed,
                        rangedDate = packageJSON.rangedDate,
                        selectedDate = packageJSON.selectedDate,
                    };

                    var coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;
                    var promo = from pr in db.gaefa_promo where pr.promoCode == promoCode select pr;

                    int discFlag = -1;
                    int discType = -1;
                    int discPercentage;
                    decimal discNotPercentage;
                    decimal discPrice;

                    if (coupon.FirstOrDefault() == null && promo.FirstOrDefault() == null)
                    {
                        discPercentage = 0;
                        discNotPercentage = 0;
                    }
                    else if (coupon.FirstOrDefault() != null && promo.FirstOrDefault() == null)
                    {
                        discType = 0;
                        discPercentage = coupon.FirstOrDefault().discPercentage ?? 0;
                        discNotPercentage = coupon.FirstOrDefault().discPrice ?? 0;

                        if (discPercentage == 0)
                        {
                            discFlag = 1;
                        }
                        else if (discNotPercentage == 0)
                        {
                            discFlag = 0;
                        }
                    }
                    else //if promo
                    {
                        discType = 1;
                        discPercentage = promo.FirstOrDefault().discPercentage ?? 0;
                        discNotPercentage = promo.FirstOrDefault().discPrice ?? 0;

                        if (discPercentage == 0)
                        {
                            discFlag = 1;
                        }
                        else if (discNotPercentage == 0)
                        {
                            discFlag = 0;
                        }
                    }

                    var originalPrice = (packageJSON.priceAdult * query_book.FirstOrDefault().adultCount) + (packageJSON.priceChild * query_book.FirstOrDefault().childCount) + (packageJSON.priceChildNoBed * query_book.FirstOrDefault().childNoBedCount);
                    //var originalPrice = packageJSON.pricePerPack * query.FirstOrDefault().passengerAmount;

                    if (discFlag == 0)
                    {
                        discPrice = (decimal)originalPrice * discPercentage / 100;
                        ViewBag.DiscAmount = discPercentage;
                    }
                    else if (discFlag == 1)
                    {
                        discPrice = discNotPercentage;
                    }
                    else
                    {
                        discPrice = 0;
                    }

                    ViewBag.DiscType = discType;
                    ViewBag.DiscFlag = discFlag;
                    ViewBag.OriginalPrice = Math.Round(originalPrice, 2, MidpointRounding.AwayFromZero);
                    ViewBag.DiscPrice = Math.Round(discPrice, 2, MidpointRounding.AwayFromZero);

                    ViewBag.DateToGo = query_book.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

                    return View(model: new GaefaDetailNew { booking = query_book.FirstOrDefault(), gaefaPackage = package, picInfo = query_pic.FirstOrDefault() });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavePassengerDetails(string bookingCode) //View to add passenger details
        {
            var query_book = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;

            if (query_book.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                ViewBag.AdultAmount = query_book.FirstOrDefault().adultCount;
                ViewBag.ChildAmount = query_book.FirstOrDefault().childCount;
                ViewBag.ChildNoBedAmount = query_book.FirstOrDefault().childNoBedCount;
            }

            ViewBag.BookCode = bookingCode;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostSavePassengerDetails(string bookCode) //Function to save passenger detail to Gaefa -- Still failed
        {
            var queryBook = from b in db.gaefa_book_new where b.bookCode == bookCode select b;

            if (queryBook.FirstOrDefault() == null) return RedirectToAction("Error", "Gaefa");

            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();

            json.Url = json.BaseUrlGetOrderDetailByOrderReference + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&order_reference=" + queryBook.FirstOrDefault().orderReference;
            GaefaOrderDetail detailJSON = json.GetOrderDetailByOrderReference(json.Url);

            if (detailJSON == null) return RedirectToAction("Error", "Gaefa");

            string[] adultNames = Request.Form.GetValues("adultName");
            string[] childNames = Request.Form.GetValues("childName");
            string[] childNoBedNames = Request.Form.GetValues("childNoBedName");

            string[] adultRemarks = Request.Form.GetValues("adultRemarks");
            string[] childRemarks = Request.Form.GetValues("childRemarks");
            string[] childNoBedRemarks = Request.Form.GetValues("childNoBedRemarks");

            List<PassengerList> passengerList = new List<PassengerList>();

            if(adultNames != null)
            {
                for (int i = 0; i < adultNames.Length; i++)
                {
                    PassengerList pass = new PassengerList
                    {
                        name = adultNames[i],
                        remarks = adultRemarks[i],
                    };
                    passengerList.Add(pass);
                }
            }
            
            if(childNames != null)
            {
                for (int i = 0; i < childNames.Length; i++)
                {
                    PassengerList pass = new PassengerList
                    {
                        name = childNames[i],
                        remarks = childRemarks[i],
                    };
                    passengerList.Add(pass);
                }
            }
            
            if(childNoBedNames != null)
            {
                for (int i = 0; i < childNoBedNames.Length; i++)
                {
                    PassengerList pass = new PassengerList
                    {
                        name = childNoBedNames[i],
                        remarks = childNoBedRemarks[i],
                    };
                    passengerList.Add(pass);
                }
            }
            PostPassengerDetails postPass = new PostPassengerDetails();

            postPassenger p = new postPassenger
            {
                agency_uid = GlobalVar.AGENCY_UID,
                order_reference = queryBook.FirstOrDefault().orderReference,
                signature = sign.GetSignature(),
                passengerList = passengerList,
            };

            postPass.Post(p);

            if (postPass.postStatus == true)
            {
                queryBook.FirstOrDefault().postedPassenger = true;
                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                }
                return View();
            }
            else return RedirectToAction("Error", "Gaefa");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePassengerDetails(string bookingCode) //View to return to update passenger details - Not finished because save passenger details still failed
        {
            var query_book = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;

            if(query_book.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                ViewBag.AdultAmount = query_book.FirstOrDefault().adultCount;
                ViewBag.ChildAmount = query_book.FirstOrDefault().childCount;
                ViewBag.ChildNoBedAmount = query_book.FirstOrDefault().childNoBedCount;
            }
            
            return View();
        }

        [SessionExpire]
        public ActionResult TagMaker() //Page to let admin make tag
        {
            GaefaSignature sign = new GaefaSignature();
            int pageNumber = 1;
            GaefaPagination pagination = new GaefaPagination()
            {
                limit = 100000,
            };
            pagination.start = (pageNumber - 1) * pagination.limit;
            GaefaFilter filter = new GaefaFilter()
            {
                titleOrLocation = "",
                include_flight = null,
                include_inn = null,
                tag = "",
            };
            GaefaPackageSort sort = new GaefaPackageSort()
            {
                sortMode = GaefaPackageSort.sort_mode.DESCENDING,
                sortType = GaefaPackageSort.sort_type.cdate,
            };
            JSONParser json = new JSONParser();
            json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode + "&tag=" + filter.tag;
            System.Diagnostics.Debug.WriteLine(json.Url);
            //string url = GaefaPackageUrl();
            List<GaefaPackageJSON> ListOfGaefaPackageJSON = json.GetGaefaPackageArray(json.Url);
            List<int> idList = new List<int>();

            if (ListOfGaefaPackageJSON == null) return RedirectToAction("Error", "Gaefa");

            ListOfGaefaPackageJSON.ForEach(x =>
            {
                idList.Add(x.id);
            });

            var tag = from t in db.gaefa_tag select t.tag;
            ViewBag.TagList = tag.ToList();
            return View(model: idList.OrderBy(x => x));
        }

        [SessionExpire]
        public ActionResult getTicketList(int? id) //Function to get list of package or ticket for tag maker view
        {

            var tag = from t in db.gaefa_tag select t.tag;
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return Json(new { status = "error", detail = "null", tagList = "null", allTag = tag.ToList() });
            }
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + id + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);

            if (packageJSON == null)
            {
                //return HttpNotFound();
                return Json(new { status = "error", detail = "null", tagList = "null", allTag = tag.ToList() });
            }
            
            if(packageJSON.tag != null)
            {
                string[] tagList = packageJSON.tag.Split(',');
                for(int i = 0; i < tagList.Length; i++)
                {
                    tagList[i] = BasicHelper.ToTitleCase(tagList[i]);
                }
                return Json(new { status = "success", detail = packageJSON, tagList = tagList, allTag = tag.ToList() });
            }
            else
            {
                return Json(new { status = "success", detail = packageJSON, tagList = "null", allTag = tag.ToList() });
            }
            
        }

        [SessionExpire]
        public ActionResult addTag(string tagName) //function to add new tag
        {
            //tagName = BasicHelper.ToFirstLetterCapital(tagName.ToLower().Trim());
            tagName = BasicHelper.ToTitleCase(tagName.ToLower().Trim());
            var tag_query = from t in db.gaefa_tag where t.tag == tagName select t;

            if(tag_query.FirstOrDefault() == null)
            {
                gaefa_tag a = new gaefa_tag {
                    tag = tagName,
                };

                if (ModelState.IsValid)
                {
                    db.gaefa_tag.Add(a);
                    db.SaveChanges();
                }

                return Json(new { status = "success" });
            }
            else
            {
                return Json(new { status = "exist" });
            }
        }
        
        [SessionExpire]
        public ActionResult BindTag(int id, string tagName) //function to bind tag to a package
        {
            tagName = BasicHelper.ToTitleCase(tagName.Trim());

            PostTag PostTag = new PostTag();
            PostTag.Post(id, tagName);

            if (PostTag.postStatus == true)
            {
                return Json(new { status = "success" });
            }
            else
            {
                return Json(new { status = "error" });
            }
        }

        [SessionExpire]
        public ActionResult removeTag(string tagName) //function to delete tag
        {
            var tag = from t in db.gaefa_tag where t.tag == tagName select t;

            GaefaSignature sign = new GaefaSignature();
            int pageNumber = 1;
            GaefaPagination pagination = new GaefaPagination()
            {
                limit = 100000,
            };
            pagination.start = (pageNumber - 1) * pagination.limit;
            GaefaFilter filter = new GaefaFilter()
            {
                titleOrLocation = "",
                include_flight = null,
                include_inn = null,
                tag = "",
            };
            GaefaPackageSort sort = new GaefaPackageSort()
            {
                sortMode = GaefaPackageSort.sort_mode.DESCENDING,
                sortType = GaefaPackageSort.sort_type.cdate,
            };
            JSONParser json = new JSONParser();
            json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode + "&tag=" + filter.tag;
            System.Diagnostics.Debug.WriteLine(json.Url);
            //string url = GaefaPackageUrl();
            List<GaefaPackageJSON> ListOfGaefaPackageJSON = json.GetGaefaPackageArray(json.Url);

            if (tag.FirstOrDefault() == null || ListOfGaefaPackageJSON == null)
            {
                return Json(new { status = "error" });
            }
            else
            {
                foreach(var item in ListOfGaefaPackageJSON)
                {
                    if (item.tag == null) continue;
                    if (item.tag.Contains(tagName, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] tempTag = item.tag.Split(',');
                        List<string> tempTagList = new List<string>(tempTag);
                        tempTagList.Remove(tagName);
                        tempTag = tempTagList.ToArray();
                        item.tag = String.Join(",", tempTag);

                        PostTag PostTag = new PostTag();
                        PostTag.Post(item.id, item.tag);
                    }
                    else
                    {
                        continue;
                    }
                }

                db.gaefa_tag.Remove(tag.FirstOrDefault());
                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                }
                return Json(new { status = "success" });
            }
        }

        [SessionExpire]
        public ActionResult GetTaglist() //function to get list of all created tags
        {
            var tag = from t in db.gaefa_tag select t.tag;

            return Json(new { tagList = tag.ToList() });
        }
    }
}