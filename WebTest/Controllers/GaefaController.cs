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
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult FindBooking()
        {
            return View();
        }


        public ActionResult List(string keyword = "", bool? flight = null, bool? inn = null, GaefaPackageSort.sort_type? sortType = null, GaefaPackageSort.sort_mode? sortMode = null, int? page = null)
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
            };
            GaefaPackageSort sort = new GaefaPackageSort()
            {
                sortMode = (sortMode ?? GaefaPackageSort.sort_mode.DESCENDING),
                sortType = (sortType ?? GaefaPackageSort.sort_type.cdate),
            };
            JSONParser json = new JSONParser();
            json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode;
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
                    System.Diagnostics.Debug.WriteLine(ListOfGaefaPackageJSON[i].id + "-" + ListOfGaefaPackageJSON[i].name);

                    DateTime tempDT_start = DateTime.ParseExact(ListOfGaefaPackageJSON[i].startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    ListOfGaefaPackageJSON[i].startDate = tempDT_start.ToString("dd/MM/yyyy");
                    if (ListOfGaefaPackageJSON[i].endDate != null)
                    {
                        DateTime tempDT_end = DateTime.ParseExact(ListOfGaefaPackageJSON[i].endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        ListOfGaefaPackageJSON[i].endDate = tempDT_end.ToString("dd/MM/yyyy");
                    }

                    ListOfGaefaPackage.Add(new GaefaPackage
                    {
                        id = ListOfGaefaPackageJSON[i].id,
                        data = JsonConvert.DeserializeObject<Data>(ListOfGaefaPackageJSON[i].data),
                        duration = ListOfGaefaPackageJSON[i].duration,
                        endDate = ListOfGaefaPackageJSON[i].endDate,
                        includeFlight = ListOfGaefaPackageJSON[i].includeFlight,
                        includeHotel = ListOfGaefaPackageJSON[i].includeHotel,
                        location = ListOfGaefaPackageJSON[i].location,
                        minimumPack = ListOfGaefaPackageJSON[i].minimumPack,
                        name = ListOfGaefaPackageJSON[i].name,
                        note = ListOfGaefaPackageJSON[i].note,
                        pricePerPack = ListOfGaefaPackageJSON[i].pricePerPack,
                        startDate = ListOfGaefaPackageJSON[i].startDate,
                    });

                }

                ViewBag.PackageAmount = int.Parse(info.GetListTotal());
                ViewBag.LimitPerPage = pagination.limit;
                ViewBag.filter = filter;
                ViewBag.pagination = pagination;
                ViewBag.sort = sort;
                //return View("List", ListOfGaefaPackage.ToPagedList(pageNumber, ListOfGaefaPackageJSON.Count));
                //return View("List", ListOfGaefaPackage.ToPagedList(pageNumber, pagination.limit));
                return View("List", ListOfGaefaPackage.ToList());
            }


        }

        public ActionResult Detail(int? id)
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

            DateTime tempDT_start = DateTime.ParseExact(packageJSON.startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            packageJSON.startDate = tempDT_start.ToString("dd/MM/yyyy");
            if (packageJSON.endDate != null)
            {
                DateTime tempDT_end = DateTime.ParseExact(packageJSON.endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                packageJSON.endDate = tempDT_end.ToString("dd/MM/yyyy");
            }
            //tour_info t = db.tour_info.Find(id);
            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };

            return View(package);
        }

        public ActionResult Checkout(int? id)
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
            
            if (packageJSON.endDate != null)
            {
                DateTime tempDT_end = DateTime.ParseExact(packageJSON.endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                packageJSON.endDate = tempDT_end.ToString("yyyy-MM-dd");
            }

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };

            return View(package);
        }

        /*NO CHILD/ADULT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(int tourID, string payopt, string email, string note, DateTime tourDate, decimal totalPrice)
        {
            var bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            var orderReference = DateTime.Now.ToString("yyyymmddhhmmss") + "-" + BasicHelper.getRandomString(GlobalVar.ORDER_REFERENCE_LENGTH);

            var checkBookCode = from b in db.gaefa_book_info where b.bookCode == bookCode select b;
            while (checkBookCode.FirstOrDefault() != null)
            {
                bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            }

            var checkOrderReference = from oR in db.gaefa_book_info where oR.orderReference == orderReference select oR;
            while (checkOrderReference.FirstOrDefault() != null)
            {
                orderReference = DateTime.Now.ToString("yyyymmddhhmmss") + "-" + BasicHelper.getRandomString(GlobalVar.ORDER_REFERENCE_LENGTH);
            }

            string[] passenger = Request.Form.GetValues("passenger");
            for (int i = 0; i < passenger.Length; i++)
            {
                passenger[i] = passenger[i].Trim();
            }
            var passengerList = string.Join(";", passenger);
            //var passengerList = passenger.Split(',').ToArray();

            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            //json.Url = json.UrlPackage + id;
            json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + tourID + "&signature=" + sign.GetSignature();
            GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);


            if (packageJSON != null)
            {
                if (payopt == "PayPal")
                {
                    Response.Clear();

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<!DOCTYPE html>");
                    sb.Append("<html>");
                    sb.Append("<head>");
                    sb.Append("<title>PayPalPayment</title>");
                    sb.Append("</head>");
                    sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
                    sb.AppendFormat("<form name='form' action='{0}' method='post'>", Url.Action("PayPalPayment", "Gaefa", new { destination = packageJSON.location, price = packageJSON.pricePerPack, packageQuantity = passenger.Length, bookingCode = bookCode, passengers = passengerList, tourID = tourID, orderReference = orderReference, email = email.Trim(), tourDate = tourDate, totalPrice = totalPrice }));

                    // Other params go here
                    sb.Append("</form>");
                    sb.Append("</body>");
                    sb.Append("</html>");

                    Response.Write(sb.ToString());

                    Response.End();

                    //Will not be executed, just for return statement
                    return View();

                    //return RedirectToAction("PayPalPayment", "PayPal", new { destination = destinationQuery.FirstOrDefault(), price = priceQuery.FirstOrDefault(), packageQuantity = passenger.Length });
                }
                else
                {
                    gaefa_book_info a = new gaefa_book_info
                    {
                        bookCode = bookCode,
                        tourID = tourID,
                        email = email.Trim(),
                        dateOrder = DateTime.Now,
                        dateToGo = tourDate,
                        orderReference = orderReference,
                        paymentMethod = payopt,
                        passenger = passengerList,
                        status = "Unpaid",
                        totalPrice = totalPrice,
                    };

                    if (ModelState.IsValid)
                    {
                        db.gaefa_book_info.Add(a);
                        db.SaveChanges();

                        var subject = "Checkout Confirmation";
                        var body = "<p>Hi, you have just checkout tour package to " + packageJSON.location + " with booking code: <b>" + bookCode + "</b>.</p>";
                        body += "<p>In order to be able to use your tour ticket to " + packageJSON.location + ", you have to confirm your booking first.</p>";
                        body += "<p>Please head to our 'Find Booking' section to find your booking details via our website or you may click this <a href='" + Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("FindBooking", "Gaefa") + "'>link</a> to head over there.</p>";
                        body += "<p>You will have to input your email and booking code to confirm.</p>";
                        BasicHelper.sendEmail(email.Trim(), subject, body);
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
        */

        /* WITH ADULT CHILD, without PIC detail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(int tourID, string payopt, string email, string note, DateTime tourDate, decimal totalPrice)
        {
            var bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            var orderReference = DateTime.Now.ToString("yyyymmddhhmmss") + "-" + BasicHelper.getRandomString(GlobalVar.ORDER_REFERENCE_LENGTH);

            string adultList;
            string childList;

            int childAmount;
            int adultAmount;

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

            string[] adult = Request.Form.GetValues("adult");
            string[] child = Request.Form.GetValues("child");

            if(adult != null)
            {
                for (int i = 0; i < adult.Length; i++)
                {
                    adult[i] = adult[i].Trim();
                }
                adultList = string.Join(";", adult);
                adultAmount = adult.Length;
            }
            else
            {
                adultList = null;
                adultAmount = 0;
            }
            

            if (child != null)
            {
                for (int i = 0; i < child.Length; i++)
                {
                    child[i] = child[i].Trim();
                }
                childList = string.Join(";", child);
                childAmount = child.Length;
            }
            else
            {
                childList = null;
                childAmount = 0;
            }
            
            if(adultAmount + childAmount == 0)
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
                

                if (packageJSON != null)
                {
                    if((adultAmount + childAmount) < packageJSON.minimumPack)
                    {
                        return RedirectToAction("Error", "Gaefa");
                    }
                    else
                    {
                        if (payopt == "PayPal")
                        {
                            Response.Clear();

                            StringBuilder sb = new StringBuilder();
                            sb.Append("<!DOCTYPE html>");
                            sb.Append("<html>");
                            sb.Append("<head>");
                            sb.Append("<title>PayPalPayment</title>");
                            sb.Append("</head>");
                            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
                            sb.AppendFormat("<form name='form' action='{0}' method='post'>", Url.Action("PayPalPayment", "Gaefa", new { destination = packageJSON.location, price = packageJSON.pricePerPack, packageQuantity = adult.Length + child.Length, bookingCode = bookCode, adult = adultList, child = childList, tourID = tourID, orderReference = orderReference, email = email.Trim(), tourDate = tourDate, totalPrice = totalPrice }));

                            // Other params go here
                            sb.Append("</form>");
                            sb.Append("</body>");
                            sb.Append("</html>");

                            Response.Write(sb.ToString());

                            Response.End();

                            //Will not be executed, just for return statement
                            return View();

                            //return RedirectToAction("PayPalPayment", "PayPal", new { destination = destinationQuery.FirstOrDefault(), price = priceQuery.FirstOrDefault(), packageQuantity = passenger.Length });
                        }
                        else
                        {
                            gaefa_book_new a = new gaefa_book_new
                            {
                                bookCode = bookCode,
                                tourID = tourID,
                                email = email.Trim(),
                                dateOrder = DateTime.Now,
                                dateToGo = tourDate,
                                orderReference = orderReference,
                                paymentMethod = payopt,
                                adult = adultList,
                                child = childList,
                                status = "Unpaid",
                                passengerAmount = adultAmount + childAmount,
                                totalPrice = totalPrice,
                            };

                            if (ModelState.IsValid)
                            {
                                db.gaefa_book_new.Add(a);
                                db.SaveChanges();

                                var subject = "Checkout Confirmation";
                                var body = "<p>Hi, you have just checkout tour package to " + packageJSON.location + " with booking code: <b>" + bookCode + "</b>.</p>";
                                body += "<p>In order to be able to use your tour ticket to " + packageJSON.location + ", you have to confirm your booking first.</p>";
                                body += "<p>Please head to our 'Find Booking' section to find your booking details via our website or you may click this <a href='" + Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("FindBooking", "Gaefa") + "'>link</a> to head over there.</p>";
                                body += "<p>You will have to input your email and booking code to confirm.</p>";
                                BasicHelper.sendEmail(email.Trim(), subject, body);
                            }
                            else
                            {
                                return RedirectToAction("Error", "Gaefa");
                            }

                            return RedirectToAction("AfterTransferCheckout", "Gaefa", new { bookCode = bookCode, email = a.email });
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Error", "Gaefa");
                }
            }
            
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(int tourID, string payopt, string picName, string picAddress, string picTelCode, string picTelephone, string picEmail, string note, DateTime tourDate, int peopleCount, string couponDisc, int adult = 0, int child = 0)
        {
            if(peopleCount < 1)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            decimal totalPrice;
            decimal originalPrice;
            int discPercentage;
            decimal discNotPercentage;

            if (couponDisc == "" || couponDisc == null)
            {
                couponDisc = null;
                discPercentage = 0;
            }
            else
            {
                var query_coupon = from c in db.gaefa_coupon where c.couponCode == couponDisc select c;
                if(query_coupon.FirstOrDefault() == null)
                {
                    discPercentage = 0;
                }
                else
                {
                    if(query_coupon.FirstOrDefault().status == true)
                    {
                        discPercentage = query_coupon.FirstOrDefault().discPercentage ?? 0;
                        discNotPercentage = query_coupon.FirstOrDefault().discPrice ?? 0;
                    }
                    else
                    {
                        discPercentage = 0;
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

            if (adult + child == 0)
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


                if (packageJSON != null)
                {
                    if ((adult + child) < packageJSON.minimumPack)
                    {
                        return RedirectToAction("Error", "Gaefa");
                    }
                    else
                    {
                        originalPrice = decimal.Round((decimal)(packageJSON.pricePerPack * peopleCount), 2, MidpointRounding.AwayFromZero);
                        totalPrice = decimal.Round(originalPrice - (originalPrice * discPercentage / 100), 2, MidpointRounding.AwayFromZero);

                        if (payopt == "PayPal")
                        {
                            Response.Clear();

                            StringBuilder sb = new StringBuilder();
                            sb.Append("<!DOCTYPE html>");
                            sb.Append("<html>");
                            sb.Append("<head>");
                            sb.Append("<title>PayPalPayment</title>");
                            sb.Append("</head>");
                            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
                            sb.AppendFormat("<form name='form' action='{0}' method='post'>", Url.Action("PayPalPayment", "Gaefa", new { destination = packageJSON.location, price = packageJSON.pricePerPack, packageQuantity = adult + child, adult = adult, child = child, bookingCode = bookCode, tourID = tourID, orderReference = orderReference, email = picEmail.Trim(), tourDate = tourDate, totalPrice = totalPrice, picName = picName, picAddress = picAddress.Trim(), picTelNumber = picTelCode.ToString() + picTelephone.ToString(), couponCode = couponDisc, note = note.Replace("\r\n","__newline__") }));

                            // Other params go here
                            sb.Append("</form>");
                            sb.Append("</body>");
                            sb.Append("</html>");

                            Response.Write(sb.ToString());

                            Response.End();

                            //Will not be executed, just for return statement
                            return View();

                            //return RedirectToAction("PayPalPayment", "PayPal", new { destination = destinationQuery.FirstOrDefault(), price = priceQuery.FirstOrDefault(), packageQuantity = passenger.Length });
                        }
                        else
                        {
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
                                passengerAmount = adult + child,
                                totalPrice = totalPrice,
                                note = note.Replace("\r\n","<br/>"),
                                couponCode = couponDisc,
                            };

                            gaefa_pic_info p = new gaefa_pic_info
                            {
                                bookCode = bookCode,
                                email = picEmail.Trim(),
                                address = picAddress.Trim(),
                                name = picName,
                                telephone = "+" + picTelCode.ToString() + picTelephone.ToString(),
                            };

                            if(couponDisc != null)
                            {
                                var coupon_query = from c in db.gaefa_coupon where c.couponCode == couponDisc select c;

                                if(coupon_query.FirstOrDefault() == null)
                                {
                                    return RedirectToAction("Error", "Gaefa");
                                }
                                else
                                {
                                    coupon_query.FirstOrDefault().status = false;
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
                }
                else
                {
                    return RedirectToAction("Error", "Gaefa");
                }
            }

        }


        /*NO CHILD ADULT
        public ActionResult AfterTransferCheckout(string bookCode, string email)
        {
            var query = from b in db.gaefa_book_info where b.bookCode == bookCode && b.email == email select b;
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
        */

        public ActionResult AfterTransferCheckout(string bookCode, string email)
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

        /*
        [SessionExpire]
        public ActionResult Pending()
        {
            GaefaSignature sign = new GaefaSignature();
            GaefaFilter filter = new GaefaFilter()
            {
                titleOrLocation = "",
                include_flight = null,
                include_inn = null,
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
            json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode;
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
                    DateTime tempDT_start = DateTime.ParseExact(ListOfGaefaPackageJSON[i].startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    ListOfGaefaPackageJSON[i].startDate = tempDT_start.ToString("dd/MM/yyyy");
                    if (ListOfGaefaPackageJSON[i].endDate != null)
                    {
                        DateTime tempDT_end = DateTime.ParseExact(ListOfGaefaPackageJSON[i].endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        ListOfGaefaPackageJSON[i].endDate = tempDT_end.ToString("dd/MM/yyyy");
                    }

                    ListOfGaefaPackage.Add(new GaefaPackage
                    {
                        id = ListOfGaefaPackageJSON[i].id,
                        data = JsonConvert.DeserializeObject<Data>(ListOfGaefaPackageJSON[i].data),
                        duration = ListOfGaefaPackageJSON[i].duration,
                        endDate = ListOfGaefaPackageJSON[i].endDate,
                        includeFlight = ListOfGaefaPackageJSON[i].includeFlight,
                        includeHotel = ListOfGaefaPackageJSON[i].includeHotel,
                        location = ListOfGaefaPackageJSON[i].location,
                        minimumPack = ListOfGaefaPackageJSON[i].minimumPack,
                        name = ListOfGaefaPackageJSON[i].name,
                        note = ListOfGaefaPackageJSON[i].note,
                        pricePerPack = ListOfGaefaPackageJSON[i].pricePerPack,
                        startDate = ListOfGaefaPackageJSON[i].startDate,
                    });
                }

                DateTime DateNow = DateTime.Now.Date;
                var userID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());

                var queryBook = from b in db.gaefa_book_info join u in db.user_info on b.buyerID equals u.id where u.id == userID && b.status != "Paid" select b;

                ViewBag.NeedConfirm = queryBook.Where(b => b.status == "Unpaid").Count();
                ViewBag.WaitApproval = queryBook.Where(b => b.status == "Waiting").Count();
                ViewBag.Disapproved = queryBook.Where(b => b.status == "Disapproved").Count();
                

                return View(new GaefaMultipleModel() { booking = queryBook.ToList(), gaefaPackage = ListOfGaefaPackage });
            }
        }
        */

        /*NO ADULT CHILD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailWithoutPayment(string bookingCode)
        {
            if (bookingCode == "")
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                //return RedirectToAction("Index", "Tour");
                return RedirectToAction("Error", "Gaefa");
            }

            var query = from b in db.gaefa_book_info where b.bookCode == bookingCode select b;

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

            ///COMMENT THIS
            DateTime tempDT_start = DateTime.ParseExact(packageJSON.startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            packageJSON.startDate = tempDT_start.ToString("dd/MM/yyyy");
            if (packageJSON.endDate != null)
            {
                DateTime tempDT_end = DateTime.ParseExact(packageJSON.endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                packageJSON.endDate = tempDT_end.ToString("dd/MM/yyyy");
            }
            ///COMMENT THIS

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };

            return View("DetailWithoutPayment", new GaefaDetail() { booking = query.FirstOrDefault(), gaefaPackage = package });
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailWithoutPayment(string bookingCode)
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

            int discFlag = 0;
            int discPercentage;
            decimal discPrice;

            if (coupon.FirstOrDefault() != null)
            {
                if(coupon.FirstOrDefault().discPercentage != null)
                {
                    discFlag = 0;
                    ViewBag.DiscAmount = coupon.FirstOrDefault().discPercentage;
                }
                else
                {
                    discFlag = 1;
                    ViewBag.DiscAmount = coupon.FirstOrDefault().discPrice;
                }

            }
            else
            {
                ViewBag.DiscAmount = 0;
            }

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };

            var originalPrice = package.pricePerPack * query.FirstOrDefault().passengerAmount;
            ViewBag.DiscFlag = discFlag;
            ViewBag.OriginalPrice = originalPrice;

            return View("DetailWithoutPayment", new GaefaDetailNew() { booking = query.FirstOrDefault(), gaefaPackage = package, picInfo = picInfo.FirstOrDefault() });
        }

        /*NO ADULT CHILD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer(string bookingCode)
        {
            var query = from b in db.gaefa_book_info where b.bookCode == bookingCode select b;

            if (query.FirstOrDefault() == null || query.FirstOrDefault().status != "Unpaid")
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

            DateTime tempDT_start = DateTime.ParseExact(packageJSON.startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            packageJSON.startDate = tempDT_start.ToString("dd/MM/yyyy");
            if (packageJSON.endDate != null)
            {
                DateTime tempDT_end = DateTime.ParseExact(packageJSON.endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                packageJSON.endDate = tempDT_end.ToString("dd/MM/yyyy");
            }

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };

            return View("Transfer", new GaefaDetail() { booking = query.FirstOrDefault(), gaefaPackage = package });
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer(string bookingCode)
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;

            if (query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            string couponCode = query.FirstOrDefault().couponCode;

            if (query.FirstOrDefault() == null || query.FirstOrDefault().status != "Unpaid")
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

            int discFlag = 0;
            int discPercentage;
            decimal discNotPercentage;
            decimal discPrice;

            if (coupon.FirstOrDefault() != null)
            {
                if(coupon.FirstOrDefault().discPercentage != null)
                {
                    discFlag = 0;
                    ViewBag.DiscAmount = coupon.FirstOrDefault().discPercentage;
                }
                else
                {
                    discFlag = 1;
                    ViewBag.DiscAmount = coupon.FirstOrDefault().discPrice;
                }
                discPercentage = coupon.FirstOrDefault().discPercentage ?? 0;
                discNotPercentage = coupon.FirstOrDefault().discPrice ?? 0;
            }
            else
            {
                discFlag = 2;
                discPercentage = 0;
                discNotPercentage = 0;
                ViewBag.DiscAmount = 0;
            }

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };

            var originalPrice = package.pricePerPack * query.FirstOrDefault().passengerAmount;

            if (discFlag == 0)
            {
                discPrice = (decimal)originalPrice * discPercentage / 100;
            }
            else if(discFlag == 1)
            {
                discPrice = discNotPercentage;
            }
            else
            {
                discPrice = 0;
            }
            
            ViewBag.DiscFlag = discFlag;
            ViewBag.OriginalPrice = originalPrice;
            ViewBag.DiscAmount = discPrice;

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            return View("Transfer", new GaefaDetailNew() { booking = query.FirstOrDefault(), gaefaPackage = package, picInfo = picInfo.FirstOrDefault() });
        }

        /*NO ADULT CHILD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPaymentTransfer(string bookCode, DateTime date, string sender, decimal price, string bank, string bankFrom, string account, string email, HttpPostedFileBase receipt)
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

            gaefa_transfer_info x = new gaefa_transfer_info
            {
                bookCode = bookCode,
                paymentDate = date,
                approveDate = null,
                senderName = sender.Trim(),
                accountNumber = account,
                fromBank = bankFrom,
                amountTransferred = price,
                bankName = bank,
                email = email.Trim(),
                receipt = path,
            };

            if (ModelState.IsValid)
            {
                db.gaefa_transfer_info.Add(x);
                var query = from b in db.gaefa_book_info where b.bookCode == bookCode select b;
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

            return RedirectToAction("AfterTransferConfirmation", "Gaefa");
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPaymentTransfer(string bookCode, DateTime date, string sender, decimal price, string bank, string bankFrom, string account, string email, HttpPostedFileBase receipt)
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
                email = email.Trim(),
                receipt = path,
            };

            if (ModelState.IsValid)
            {
                db.gaefa_transfer_new.Add(x);
                var query = from b in db.gaefa_book_new where b.bookCode == bookCode select b;
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

            return RedirectToAction("AfterTransferConfirmation", "Gaefa");
        }

        /*NO ADULT CHILD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailWithPayment(string bookingCode)
        {
            var query = from b in db.gaefa_book_info where b.bookCode == bookingCode select b;
            var transfer = from t in db.gaefa_transfer_info where t.bookCode == bookingCode select t;

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

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };


            return View(new BookingAndTransfer { booking = new GaefaDetail { booking = query.FirstOrDefault(), gaefaPackage = package }, transfer = transfer.FirstOrDefault() });
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailWithPayment(string bookingCode)
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;
            var transfer = from t in db.gaefa_transfer_new where t.bookCode == bookingCode select t;
            string couponCode = query.FirstOrDefault().couponCode;

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

            var picInfo = from p in db.gaefa_pic_info where p.bookCode == bookingCode select p;
            var coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;

            if (coupon.FirstOrDefault() != null)
            {
                ViewBag.DiscAmount = coupon.FirstOrDefault().discPercentage;
            }
            else
            {
                ViewBag.DiscAmount = 0;
            }

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };


            return View(new BookingAndTransferNew { booking = new GaefaDetailNew { booking = query.FirstOrDefault(), gaefaPackage = package }, transfer = transfer.FirstOrDefault(), picInfo = picInfo.FirstOrDefault() });
        }

        /*NO ADULT CHILD
        [SessionExpire]
        public ActionResult Approve()
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                GaefaSignature sign = new GaefaSignature();
                GaefaFilter filter = new GaefaFilter()
                {
                    titleOrLocation = "",
                    include_flight = null,
                    include_inn = null,
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
                json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode;
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
                        DateTime tempDT_start = DateTime.ParseExact(ListOfGaefaPackageJSON[i].startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        ListOfGaefaPackageJSON[i].startDate = tempDT_start.ToString("dd/MM/yyyy");
                        if (ListOfGaefaPackageJSON[i].endDate != null)
                        {
                            DateTime tempDT_end = DateTime.ParseExact(ListOfGaefaPackageJSON[i].endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            ListOfGaefaPackageJSON[i].endDate = tempDT_end.ToString("dd/MM/yyyy");
                        }

                        ListOfGaefaPackage.Add(new GaefaPackage
                        {
                            id = ListOfGaefaPackageJSON[i].id,
                            data = JsonConvert.DeserializeObject<Data>(ListOfGaefaPackageJSON[i].data),
                            duration = ListOfGaefaPackageJSON[i].duration,
                            endDate = ListOfGaefaPackageJSON[i].endDate,
                            includeFlight = ListOfGaefaPackageJSON[i].includeFlight,
                            includeHotel = ListOfGaefaPackageJSON[i].includeHotel,
                            location = ListOfGaefaPackageJSON[i].location,
                            minimumPack = ListOfGaefaPackageJSON[i].minimumPack,
                            name = ListOfGaefaPackageJSON[i].name,
                            note = ListOfGaefaPackageJSON[i].note,
                            pricePerPack = ListOfGaefaPackageJSON[i].pricePerPack,
                            startDate = ListOfGaefaPackageJSON[i].startDate,
                        });
                    }

                    DateTime DateNow = DateTime.Now.Date;
                    var userID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());

                    var queryBook = from b in db.gaefa_book_info where b.status == "Waiting" select b;

                    return View(new GaefaMultipleModel() { booking = queryBook.ToList(), gaefaPackage = ListOfGaefaPackage });
                }
            }
            else
            {
                //return RedirectToAction("Index", "Home");
                return RedirectToAction("Error", "Gaefa");
            }
        }
        */

        [SessionExpire]
        public ActionResult Approve()
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                GaefaSignature sign = new GaefaSignature();
                GaefaFilter filter = new GaefaFilter()
                {
                    titleOrLocation = "",
                    include_flight = null,
                    include_inn = null,
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
                json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode;
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
                        DateTime tempDT_start = DateTime.ParseExact(ListOfGaefaPackageJSON[i].startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        ListOfGaefaPackageJSON[i].startDate = tempDT_start.ToString("dd/MM/yyyy");
                        if (ListOfGaefaPackageJSON[i].endDate != null)
                        {
                            DateTime tempDT_end = DateTime.ParseExact(ListOfGaefaPackageJSON[i].endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            ListOfGaefaPackageJSON[i].endDate = tempDT_end.ToString("dd/MM/yyyy");
                        }

                        ListOfGaefaPackage.Add(new GaefaPackage
                        {
                            id = ListOfGaefaPackageJSON[i].id,
                            data = JsonConvert.DeserializeObject<Data>(ListOfGaefaPackageJSON[i].data),
                            duration = ListOfGaefaPackageJSON[i].duration,
                            endDate = ListOfGaefaPackageJSON[i].endDate,
                            includeFlight = ListOfGaefaPackageJSON[i].includeFlight,
                            includeHotel = ListOfGaefaPackageJSON[i].includeHotel,
                            location = ListOfGaefaPackageJSON[i].location,
                            minimumPack = ListOfGaefaPackageJSON[i].minimumPack,
                            name = ListOfGaefaPackageJSON[i].name,
                            note = ListOfGaefaPackageJSON[i].note,
                            pricePerPack = ListOfGaefaPackageJSON[i].pricePerPack,
                            startDate = ListOfGaefaPackageJSON[i].startDate,
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


        /*NO ADULT CHILD
        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovePayment(string bookCode, string approveChoice)
        {
            //if(Request.Form["approveButton"] != null)
            GaefaSignature sign = new GaefaSignature();
            JSONParser json = new JSONParser();
            var query = from b in db.gaefa_book_info where b.bookCode == bookCode select b;
            var transfer = from t in db.gaefa_transfer_info where t.bookCode == bookCode select t;
            if (approveChoice == "Approve")
            {
                //TempData["Approved"] = "<script>alert('Payment Approved.');</script>";
                query.FirstOrDefault().status = "Paid";
                transfer.FirstOrDefault().approveDate = DateTime.Now;


                PostToGaefaDB PostToGaefa = new PostToGaefaDB();
                PostToGaefa.Post(query.FirstOrDefault().tourID, query.FirstOrDefault().orderReference, query.FirstOrDefault().passenger.Split(';').Length, query.FirstOrDefault().dateToGo);

                if (PostToGaefa.postStatus == true)
                {
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
                db.gaefa_transfer_info.Remove(transfer.FirstOrDefault());
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
        */

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovePayment(string bookCode, string approveChoice)
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
                PostToGaefa.Post(query.FirstOrDefault().tourID, query.FirstOrDefault().orderReference, query.FirstOrDefault().passengerAmount, query.FirstOrDefault().dateToGo);

                if (PostToGaefa.postStatus == true)
                {
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

        /*
        [SessionExpire]
        public ActionResult Purchased()
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                GaefaSignature sign = new GaefaSignature();
                GaefaFilter filter = new GaefaFilter()
                {
                    titleOrLocation = "",
                    include_flight = null,
                    include_inn = null,
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
                json.Url = json.BaseUrlGetList + "?agency_uid=" + GlobalVar.AGENCY_UID + "&signature=" + sign.GetSignature() + "&start=" + pagination.start + "&limit=" + pagination.limit + "&keyword=" + filter.titleOrLocation + "&include_flight=" + filter.include_flight + "&include_inn=" + filter.include_inn + "&sort_type=" + (int)sort.sortType + "&sort_mode=" + (int)sort.sortMode;
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
                        DateTime tempDT_start = DateTime.ParseExact(ListOfGaefaPackageJSON[i].startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        ListOfGaefaPackageJSON[i].startDate = tempDT_start.ToString("dd/MM/yyyy");
                        if (ListOfGaefaPackageJSON[i].endDate != null)
                        {
                            DateTime tempDT_end = DateTime.ParseExact(ListOfGaefaPackageJSON[i].endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            ListOfGaefaPackageJSON[i].endDate = tempDT_end.ToString("dd/MM/yyyy");
                        }

                        ListOfGaefaPackage.Add(new GaefaPackage
                        {
                            id = ListOfGaefaPackageJSON[i].id,
                            data = JsonConvert.DeserializeObject<Data>(ListOfGaefaPackageJSON[i].data),
                            duration = ListOfGaefaPackageJSON[i].duration,
                            endDate = ListOfGaefaPackageJSON[i].endDate,
                            includeFlight = ListOfGaefaPackageJSON[i].includeFlight,
                            includeHotel = ListOfGaefaPackageJSON[i].includeHotel,
                            location = ListOfGaefaPackageJSON[i].location,
                            minimumPack = ListOfGaefaPackageJSON[i].minimumPack,
                            name = ListOfGaefaPackageJSON[i].name,
                            note = ListOfGaefaPackageJSON[i].note,
                            pricePerPack = ListOfGaefaPackageJSON[i].pricePerPack,
                            startDate = ListOfGaefaPackageJSON[i].startDate,
                        });
                    }

                    DateTime DateNow = DateTime.Now.Date;
                    var userID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());

                    var queryBook = from b in db.gaefa_book_info join u in db.user_info on b.buyerID equals u.id where u.id == userID && b.status == "Paid" select b;

                    return View(new GaefaMultipleModel() { booking = queryBook.ToList(), gaefaPackage = ListOfGaefaPackage });
                }

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
        */

        /*NO ADULT CHILD
        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelBooking(string bookingCode)
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                var query = from b in db.gaefa_book_info where b.bookCode == bookingCode select b;
                db.gaefa_book_info.Remove(query.FirstOrDefault());
                db.SaveChanges();
                return RedirectToAction("Pending", "Gaefa");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        */
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelBooking(string bookingCode)
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
            db.gaefa_pic_info.Remove(picInfo.FirstOrDefault());
            db.gaefa_book_new.Remove(query.FirstOrDefault());
            db.SaveChanges();
            return Json(new { status = "success" });
        }

        /*NO ADULT CHILD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DisapprovedTransfer(string bookingCode)
        {
            var query = from b in db.gaefa_book_info where b.bookCode == bookingCode select b;

            if (query.FirstOrDefault() == null)
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

            DateTime tempDT_start = DateTime.ParseExact(packageJSON.startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            packageJSON.startDate = tempDT_start.ToString("dd/MM/yyyy");
            if (packageJSON.endDate != null)
            {
                DateTime tempDT_end = DateTime.ParseExact(packageJSON.endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                packageJSON.endDate = tempDT_end.ToString("dd/MM/yyyy");
            }

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };

            return View(model: new GaefaDetail() { booking = query.FirstOrDefault(), gaefaPackage = package });
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DisapprovedTransfer(string bookingCode)
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;
            if(query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }

            string couponCode = query.FirstOrDefault().couponCode;

            if (query.FirstOrDefault() == null)
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

            /*
            DateTime tempDT_start = DateTime.ParseExact(packageJSON.startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            packageJSON.startDate = tempDT_start.ToString("dd/MM/yyyy");
            if (packageJSON.endDate != null)
            {
                DateTime tempDT_end = DateTime.ParseExact(packageJSON.endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                packageJSON.endDate = tempDT_end.ToString("dd/MM/yyyy");
            }
            */

            var picInfo = from p in db.gaefa_pic_info where p.bookCode == bookingCode select p;
            var coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;

            if (coupon.FirstOrDefault() != null)
            {
                ViewBag.DiscAmount = coupon.FirstOrDefault().discPercentage;
            }
            else
            {
                ViewBag.DiscAmount = 0;
            }

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            return View(model: new GaefaDetailNew() { booking = query.FirstOrDefault(), gaefaPackage = package, picInfo = picInfo.FirstOrDefault() });
        }

        //PAYPAL SECTION//////////////////////////////////////////////
        /*NO ADULT CHILD
        [HttpPost]
        public ActionResult PayPalPayment(string destination, decimal price, int packageQuantity, string bookingCode, string passengers, int tourID, string orderReference, string email, string note, DateTime tourDate, decimal totalPrice)
        {

            PayPalExpressCheckout paypal = new PayPalExpressCheckout();
            paypal.RETURN_URL = GlobalVar.BASE_URL + "Gaefa/PayPalSuccess?bookCode=" + bookingCode + "&passengers=" + passengers + "&tourID=" + tourID + "&orderReference=" + orderReference + "&email=" + email + "&tourDate=" + tourDate + "&totalPrice=" + totalPrice;
            paypal.CANCEL_URL = GlobalVar.BASE_URL + "Gaefa/PayPalCancel/";
            if (paypal.SetExpressCheckout(destination, price, packageQuantity, totalPrice))
            {
                return Redirect(PayPalExpressCheckout.PayPalExpressCheckoutURL + paypal.TOKEN);
            }
            else
            {
                return RedirectToAction("PayPalFailed", "Gaefa");
            }

        }
        */

        [HttpPost]
        public ActionResult PayPalPayment(string destination, decimal price, int packageQuantity, string bookingCode, int tourID, string orderReference, string email, string couponCode, string note, DateTime tourDate, decimal totalPrice, string picName, string picAddress, string picTelNumber, int adult, int child)
        {
            int discPercentage;
            PayPalExpressCheckout paypal = new PayPalExpressCheckout();
            paypal.RETURN_URL = Request.Url.GetLeftPart(UriPartial.Authority) + "/Gaefa/PayPalSuccess?bookCode=" + bookingCode + "&quantity=" + packageQuantity + "&tourID=" + tourID + "&orderReference=" + orderReference + "&email=" + email + "&tourDate=" + tourDate + "&picName=" + picName + "&picAddress=" + picAddress + "&picTelNumber=" + picTelNumber + "&adult=" + adult + "&child=" + child + "&couponCode=" + couponCode + "&note=" + note;
            paypal.CANCEL_URL = Request.Url.GetLeftPart(UriPartial.Authority) + "/Gaefa/PayPalCancel/";
            
            if(couponCode == "" || couponCode == null)
            {
                couponCode = null;
                discPercentage = 0;
            }
            else
            {
                var query_coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;
                if(query_coupon.FirstOrDefault() == null)
                {
                    return RedirectToAction("Error", "Gaefa");
                }
                else
                {
                    discPercentage = query_coupon.FirstOrDefault().discPercentage ?? 0;
                }
            }

            if (paypal.SetExpressCheckout(destination, price, packageQuantity, couponCode, discPercentage))
            {
                return Redirect(PayPalExpressCheckout.PayPalExpressCheckoutURL + paypal.TOKEN);
            }
            else
            {
                return RedirectToAction("PayPalFailed", "Gaefa");
            }

        }

        public ActionResult PayPalFailed()
        {
            return View();
        }

        public ActionResult PayPalCancel()
        {
            return View();
        }

        /*NO ADULT CHILD
        public ActionResult PayPalSuccess(string bookCode, string passengers, int tourID, string orderReference, string email, DateTime tourDate, decimal totalPrice, string token, string PayerID)
        {
            PayPalExpressCheckout paypal = new PayPalExpressCheckout();
            GetExpressCheckoutDetailsResponseType p = paypal.GetExpressCheckout(token);
            DoExpressCheckoutPaymentResponseType d = paypal.DoExpressCheckout();
            

            if (d.Ack.ToString() == "SUCCESS")
            {
                GaefaSignature sign = new GaefaSignature();
                JSONParser json = new JSONParser();
                //json.Url = json.UrlPackage + id;
                json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + tourID + "&signature=" + sign.GetSignature();
                GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);

                if (packageJSON == null)
                {
                    return RedirectToAction("Error", "Gaefa");
                }
                else
                {
                    gaefa_book_info a = new gaefa_book_info
                    {
                        orderReference = orderReference,
                        email = email.Trim(),
                        bookCode = bookCode,
                        dateToGo = tourDate,
                        tourID = tourID,
                        paymentMethod = "PayPal",
                        passenger = passengers,
                        status = "Paid",
                        totalPrice = totalPrice,
                        dateOrder = DateTime.Now,
                    };

                    gaefa_paypal_info pp = new gaefa_paypal_info
                    {
                        bookCode = bookCode,
                        paypalAddress = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.Street1 + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.CityName + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.StateOrProvince + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.CountryName + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.PostalCode,
                        paypalAmount = decimal.Parse(p.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].OrderTotal.value),
                        paypalDateAndTime = (d.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].PaymentDate),
                        paypalPayerID = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID,
                        paypalTransactionID = d.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID,
                        paypalName = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerName.FirstName + " " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerName.LastName,
                        paypalNote = p.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].NoteText,
                        postedToGaefa = false,
                    };

                    if (ModelState.IsValid)
                    {
                        db.gaefa_book_info.Add(a);
                        db.gaefa_paypal_info.Add(pp);

                        db.SaveChanges();

                        
                        PostToGaefaDB PostToGaefa = new PostToGaefaDB();
                        PostToGaefa.Post(tourID, orderReference, passengers.Split(';').Length, tourDate);

                        if(PostToGaefa.postStatus == false)
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
                            pp.postedToGaefa = true;
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
        */

        public ActionResult PayPalSuccess(string bookCode, int quantity, int adult, int child, int tourID, string orderReference, string email, DateTime tourDate, string picAddress, string picName, string picTelNumber, string couponCode, string note, string token, string PayerID)
        {
            PayPalExpressCheckout paypal = new PayPalExpressCheckout();
            GetExpressCheckoutDetailsResponseType p = paypal.GetExpressCheckout(token);
            DoExpressCheckoutPaymentResponseType d = paypal.DoExpressCheckout();

            string coupon = null;

            if(couponCode != "")
            {
                coupon = couponCode;
            }

            if (d.Ack.ToString() == "SUCCESS")
            {
                GaefaSignature sign = new GaefaSignature();
                JSONParser json = new JSONParser();
                //json.Url = json.UrlPackage + id;
                json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + tourID + "&signature=" + sign.GetSignature();
                GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);

                if (packageJSON == null)
                {
                    return RedirectToAction("Error", "Gaefa");
                }
                else
                {
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
                        passengerAmount = quantity,
                        status = "Paid",
                        totalPrice = decimal.Parse(p.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].OrderTotal.value),
                        dateOrder = DateTime.Now,
                        couponCode = coupon,
                        note = note.Replace("__newline__","<br/>"),
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
                        postedToGaefa = false,
                    };

                    if (coupon != null)
                    {
                        var coupon_query = from c in db.gaefa_coupon where c.couponCode == coupon select c;
                        
                        if(coupon_query.FirstOrDefault() == null)
                        {
                            return RedirectToAction("Error", "Gaefa");
                        }
                        else
                        {
                            coupon_query.FirstOrDefault().status = false;
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
                        PostToGaefa.Post(tourID, orderReference, quantity, tourDate);

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
                            pp.postedToGaefa = true;
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

        /*NO ADULT CHILD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayPalDetail(string bookingCode)
        {
            var query = from b in db.gaefa_book_info where b.bookCode == bookingCode select b;
            var paypal = from p in db.gaefa_paypal_info where p.bookCode == bookingCode select p;

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

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };
            //var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime.Substring(0, 10), "yyyy-MM-dd-hh:mm:ss", CultureInfo.InvariantCulture).AddHours(7)).ToString();
            //var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime, "yyyy-MM-ddThh:mm:ssZ", CultureInfo.InvariantCulture)).ToString();
            var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime, "yyyy-MM-ddThh:mm:ssZ", CultureInfo.InvariantCulture)).ToString("dd/MM/yyyy - HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            System.Diagnostics.Debug.WriteLine(paypalDateCultureInvariant);
            //paypal.FirstOrDefault().paypalDateAndTime = paypalDateCultureInvariant.Substring(2, 2) + "/" + paypalDateCultureInvariant.Substring(0, 1) + "/" + paypalDateCultureInvariant.Substring(5, 4) + " - " + paypal.FirstOrDefault().paypalDateAndTime.Substring(11, 8) + " (GMT)";
            paypal.FirstOrDefault().paypalDateAndTime = paypalDateCultureInvariant + " (GMT + 7)";

            var originalPrice = package.pricePerPack * query.FirstOrDefault().passenger.Split(';').Length;
            ViewBag.OriginalPrice = originalPrice;
            ViewBag.Tax = paypal.FirstOrDefault().paypalAmount - Convert.ToDecimal(originalPrice);

            return View(new BookingAndPayPal { booking = new GaefaDetail { booking = query.FirstOrDefault(), gaefaPackage = package }, paypal = paypal.FirstOrDefault() });
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayPalDetail(string bookingCode)
        {
            var query = from b in db.gaefa_book_new where b.bookCode == bookingCode select b;
            var paypal = from p in db.gaefa_paypal_new where p.bookCode == bookingCode select p;
            string couponCode = query.FirstOrDefault().couponCode;

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

            int discPercentage;

            if (coupon.FirstOrDefault() != null)
            {
                ViewBag.DiscAmount = coupon.FirstOrDefault().discPercentage;
                discPercentage = coupon.FirstOrDefault().discPercentage ?? 0;
            }
            else
            {
                ViewBag.DiscAmount = 0;
                discPercentage = 0;
            }

            ViewBag.DateToGo = query.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy");

            package = new GaefaPackage
            {
                id = packageJSON.id,
                data = JsonConvert.DeserializeObject<Data>(packageJSON.data),
                duration = packageJSON.duration,
                endDate = packageJSON.endDate,
                includeFlight = packageJSON.includeFlight,
                includeHotel = packageJSON.includeHotel,
                location = packageJSON.location,
                minimumPack = packageJSON.minimumPack,
                name = packageJSON.name,
                note = packageJSON.note,
                pricePerPack = packageJSON.pricePerPack,
                startDate = packageJSON.startDate,
            };
            //var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime.Substring(0, 10), "yyyy-MM-dd-hh:mm:ss", CultureInfo.InvariantCulture).AddHours(7)).ToString();
            //var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime, "yyyy-MM-ddThh:mm:ssZ", CultureInfo.InvariantCulture)).ToString();
            var paypalDateCultureInvariant = (DateTime.ParseExact(paypal.FirstOrDefault().paypalDateAndTime, "yyyy-MM-ddThh:mm:ssZ", CultureInfo.InvariantCulture)).ToString("dd/MM/yyyy - HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            System.Diagnostics.Debug.WriteLine(paypalDateCultureInvariant);
            //paypal.FirstOrDefault().paypalDateAndTime = paypalDateCultureInvariant.Substring(2, 2) + "/" + paypalDateCultureInvariant.Substring(0, 1) + "/" + paypalDateCultureInvariant.Substring(5, 4) + " - " + paypal.FirstOrDefault().paypalDateAndTime.Substring(11, 8) + " (GMT)";
            paypal.FirstOrDefault().paypalDateAndTime = paypalDateCultureInvariant;

            var originalPrice = package.pricePerPack * query.FirstOrDefault().passengerAmount;
            double discPrice;
            if(couponCode == null)
            {
                discPrice = 0;
            }
            else
            {
                discPrice = originalPrice * discPercentage / 100;
            }
            ViewBag.OriginalPrice = originalPrice - discPrice;
            ViewBag.Tax = paypal.FirstOrDefault().paypalAmount - Convert.ToDecimal(originalPrice - discPrice);

            return View(new BookingAndPayPalNew { booking = new GaefaDetailNew { booking = query.FirstOrDefault(), gaefaPackage = package }, paypal = paypal.FirstOrDefault(), picInfo = picInfo.FirstOrDefault() });
        }

        //END OF PAYPAL SECTION//////////////////////////////////////

        public ActionResult SendToUIApi()
        {
            return View();
        }
        

        public ActionResult Email()
        {
            return View();
        }

        /*NO ADULT CHILD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FindBooking(string email, string bookingCode)
        {
            var query_book = from b in db.gaefa_book_info where b.email == email && b.bookCode == bookingCode select b;

            if(query_book.FirstOrDefault() == null)
            {
                return Json(new { status = "failed", detail = "null"});
            }
            else
            {
                GaefaSignature sign = new GaefaSignature();
                JSONParser json = new JSONParser();
                //json.Url = json.UrlPackage + id;
                json.Url = json.BaseUrlGetDetail + "?agency_uid=" + GlobalVar.AGENCY_UID + "&package_id=" + query_book.FirstOrDefault().tourID + "&signature=" + sign.GetSignature();
                GaefaPackageJSON packageJSON = json.GetGaefaPackage(json.Url);

                DateTime tempDT_start = DateTime.ParseExact(packageJSON.startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                packageJSON.startDate = tempDT_start.ToString("dd/MM/yyyy");
                

                if (packageJSON.endDate != null)
                {
                    DateTime tempDT_end = DateTime.ParseExact(packageJSON.endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    packageJSON.endDate = tempDT_end.ToString("dd/MM/yyyy");
                }

                return Json(new { status = "success", detail = packageJSON, bookCode = query_book.FirstOrDefault().bookCode, bookStatus = query_book.FirstOrDefault().status, method = query_book.FirstOrDefault().paymentMethod, date = query_book.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy") });
            }
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FindBooking(string email, string bookingCode)
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

                DateTime tempDT_start = DateTime.ParseExact(packageJSON.startDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                packageJSON.startDate = tempDT_start.ToString("dd/MM/yyyy");


                if (packageJSON.endDate != null)
                {
                    DateTime tempDT_end = DateTime.ParseExact(packageJSON.endDate.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    packageJSON.endDate = tempDT_end.ToString("dd/MM/yyyy");
                }

                return Json(new { status = "success", detail = packageJSON, bookCode = query_book.FirstOrDefault().bookCode, bookStatus = query_book.FirstOrDefault().status, method = query_book.FirstOrDefault().paymentMethod, date = query_book.FirstOrDefault().dateToGo.ToString("dd/MM/yyyy") });
            }
        }

        

        public ActionResult AfterTransferConfirmation()
        {
            return View();
        }

        /*NO ADULT CHILD
        [SessionExpire]
        public ActionResult Sold()
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                var queryBook = from b in db.gaefa_book_info where b.status == "Paid" select b;
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
                

                packageJSONList.ForEach(x =>
                {
                    packageList.Add(new GaefaPackage
                    {
                        id = x.id,
                        data = JsonConvert.DeserializeObject<Data>(x.data),
                        duration = x.duration,
                        endDate = x.endDate,
                        includeFlight = x.includeFlight,
                        includeHotel = x.includeHotel,
                        location = x.location,
                        minimumPack = x.minimumPack,
                        name = x.name,
                        note = x.note,
                        pricePerPack = x.pricePerPack,
                        startDate = x.startDate,
                    });
                });


                return View(new GaefaMultipleModel() { booking = queryBook.ToList(), gaefaPackage = packageList });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
        */

        [SessionExpire]
        public ActionResult Sold()
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
                    return RedirectToAction("Error","Gaefa");
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
                            endDate = x.endDate,
                            includeFlight = x.includeFlight,
                            includeHotel = x.includeHotel,
                            location = x.location,
                            minimumPack = x.minimumPack,
                            name = x.name,
                            note = x.note,
                            pricePerPack = x.pricePerPack,
                            startDate = x.startDate,
                        });
                    });

                    return View(new GaefaMultipleModelNew() { booking = queryBook.ToList(), gaefaPackage = packageList });
                }
                
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SyncWithGaefa()
        {
            var query_un_posted = from p in db.gaefa_paypal_info where p.postedToGaefa == false select p;
            
            int package_id;
            string order_reference;
            int total_pack;
            DateTime date;

            int countFailed = 0;
            int UnpostedCount = query_un_posted.Count();

            PostToGaefaDB PostToGaefa = new PostToGaefaDB();

            if(UnpostedCount == 0 || query_un_posted == null)
            {
                return Json(new { status = "failed", message = "none" });
            }
            else
            {
                IQueryable<gaefa_book_info> query_book;
                foreach (var item in query_un_posted.ToList())
                {
                    query_book = from b in db.gaefa_book_info where b.bookCode == item.bookCode select b;

                    package_id = query_book.FirstOrDefault().tourID;
                    order_reference = query_book.FirstOrDefault().orderReference;
                    total_pack = query_book.FirstOrDefault().passenger.Split(';').Length;
                    date = query_book.FirstOrDefault().dateToGo;

                    PostToGaefa.Post(package_id, order_reference, total_pack, date);

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
                    endDate = packageJSON.endDate,
                    includeFlight = packageJSON.includeFlight,
                    includeHotel = packageJSON.includeHotel,
                    location = packageJSON.location,
                    minimumPack = packageJSON.minimumPack,
                    name = packageJSON.name,
                    note = packageJSON.note,
                    pricePerPack = packageJSON.pricePerPack,
                    startDate = packageJSON.startDate,
                };

                return Json(new { detail = package });
            }
            
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckCoupon(string couponCode)
        {
            var query_coupon = from c in db.gaefa_coupon where c.couponCode == couponCode select c;

            if(query_coupon.FirstOrDefault() == null)
            {
                return Json(new { status = "not found", coupon = "null" });
            }
            else
            {
                if (query_coupon.FirstOrDefault().status)
                {
                    if(DateTime.Now.Date >= query_coupon.FirstOrDefault().availableDate && DateTime.Now.Date <= query_coupon.FirstOrDefault().expiryDate)
                    {
                        return Json(new { status = "success", coupon = query_coupon.FirstOrDefault() });
                    }
                    else
                    {
                        return Json(new { status = "date error", coupon = "null" });
                    }
                }
                else
                {
                    return Json(new { status = "used", coupon = "null" });
                }
                
            }
        }
    }
}