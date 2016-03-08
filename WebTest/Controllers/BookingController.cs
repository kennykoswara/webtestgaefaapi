using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebTest.Infrastructure;
using System.Data.Entity.Infrastructure;
using WebTest.Security;
using System.IO;

namespace WebTest.Controllers
{
    
    public class BookingController : Controller
    {
        public class MultipleModel
        {
            public List<book_info> booking;
            public List<user_info> user;
            public List<tour_info> tour;
        }

        public class BookingInfo
        {
            public int id;
            public string bookCode;
            public string[] passengerList;
            public string paymentMethod;
        }

        public class BookingDetailPage
        {
            public string bookCode;
            public DateTime dateFrom;
            public DateTime dateUntil;
            public string paymentMethod;
            public string destination;
            public string[] passengerList;
            public string flight;
            public string inn;
            public string custom;
            public decimal totalPrice;
            public string status;
        }

        public class BookingAndTransfer
        {
            public BookingDetailPage bookDetail;
            public transfer_info transferDetail;
        }
        
        private testEntities3 db = new testEntities3();

        public ActionResult Payment()
        {
            return View();
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(int tourID, string payopt)
        {
            var bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            string[] passenger = Request.Form.GetValues("passenger");
            var passengerList = string.Join(";", passenger);
            //var passengerList = passenger.Split(',').ToArray();
            var query = from b in db.book_info where b.bookCode == bookCode select b.bookCode;

            while (query.FirstOrDefault() != null)
            {
                bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            }

            var buyerID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());
            var priceQuery = from t in db.tour_info where t.id == tourID select t.price;
            var destinationQuery = from t in db.tour_info where t.id == tourID select t.destination;

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
                sb.AppendFormat("<form name='form' action='{0}' method='post'>", Url.Action("PayPalPayment", "PayPal", new { destination = destinationQuery.FirstOrDefault(), price = priceQuery.FirstOrDefault(), packageQuantity = passenger.Length, bookingCode = bookCode, passengers = passengerList, tourID = tourID, buyerID = buyerID}));

                // Other params go here
                sb.Append("</form>");
                sb.Append("</body>");
                sb.Append("</html>");

                Response.Write(sb.ToString());

                Response.End();

                return View();

                //return RedirectToAction("PayPalPayment", "PayPal", new { destination = destinationQuery.FirstOrDefault(), price = priceQuery.FirstOrDefault(), packageQuantity = passenger.Length });
            }
            else
            {
                book_info a = new book_info
                {
                    bookCode = bookCode,
                    tourID = tourID,
                    buyerID = buyerID,
                    paymentMethod = payopt,
                    passenger = passengerList,
                    totalPrice = passenger.Length * (decimal)(priceQuery.FirstOrDefault()),
                };

                if (ModelState.IsValid)
                {
                    db.book_info.Add(a);
                    db.SaveChanges();
                };

                return RedirectToAction("AfterCheckout", "Transfer", new {bookingCode = bookCode });
            }
        }

        [SessionExpire]
        [HttpPost]
        public ActionResult AjaxPayment(int tourID, string payopt)
        {
            var bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            string[] passenger = Request.Form.GetValues("passenger");
            var passengerList = string.Join(";", passenger);
            //var passengerList = passenger.Split(',').ToArray();
            var query = from b in db.book_info where b.bookCode == bookCode select b.bookCode;

            while (query.FirstOrDefault() != null)
            {
                bookCode = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
            }

            var buyerID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());
            var priceQuery = from t in db.tour_info where t.id == tourID select t.price;

            book_info a = new book_info
            {
                bookCode = bookCode,
                tourID = tourID,
                buyerID = buyerID,
                paymentMethod = payopt,
                passenger = passengerList,
                status = "Unpaid",
                totalPrice = passenger.Length * (decimal)(priceQuery.FirstOrDefault()),
            };

            if (ModelState.IsValid)
            {
                db.book_info.Add(a);
                db.SaveChanges();
            };

            return Json(new BookingInfo { id = buyerID, bookCode = bookCode, passengerList = passenger });
            //return RedirectToAction("AfterPayment", "Booking", new { bookCode = a.bookCode });
            //return RedirectToAction("Index", "TourInfo");
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detail(string fullName, string bookingCode)
        {
            if (fullName == "" || bookingCode == "")
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return RedirectToAction("Index", "Tour");
            }

            var query = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where b.bookCode == bookingCode && u.fullname == fullName select new { b, u, t };

            if (query.FirstOrDefault() == null)
            {
                TempData["book"] = "<script>alert('Booking not found');</script>";
                return RedirectToAction("Index", "Tour");
            }

            var tuple = new Tuple<book_info, user_info, tour_info>(query.FirstOrDefault().b, query.FirstOrDefault().u, query.FirstOrDefault().t);
            //book_info user = query.FirstOrDefault();

            BookingDetailPage x = new BookingDetailPage
            {
                bookCode = bookingCode,
                dateFrom = tuple.Item3.dateFrom,
                dateUntil = tuple.Item3.dateUntil,
                paymentMethod = tuple.Item1.paymentMethod,
                destination = tuple.Item3.destination,
                passengerList = tuple.Item1.passenger.Split(';'),
                flight = tuple.Item3.flight,
                inn = tuple.Item3.inn,
                custom = tuple.Item3.custom,
                totalPrice = tuple.Item1.totalPrice,
            };

            return View(x);
        }*/
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailWithoutPayment(string bookingCode)
        {
            if (bookingCode == "")
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return RedirectToAction("Index", "Tour");
            }

            var query = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where b.bookCode == bookingCode select new { b, u, t };

            if (query.FirstOrDefault() == null)
            {
                TempData["book"] = "<script>alert('Booking not found');</script>";
                return RedirectToAction("Index", "Tour");
            }

            var tuple = new Tuple<book_info, user_info, tour_info>(query.FirstOrDefault().b, query.FirstOrDefault().u, query.FirstOrDefault().t);
            //book_info user = query.FirstOrDefault();

            BookingDetailPage x = new BookingDetailPage
            {
                bookCode = bookingCode,
                dateFrom = tuple.Item3.dateFrom,
                dateUntil = tuple.Item3.dateUntil,
                paymentMethod = tuple.Item1.paymentMethod,
                destination = tuple.Item3.destination,
                passengerList = tuple.Item1.passenger.Split(';'),
                flight = tuple.Item3.flight,
                inn = tuple.Item3.inn,
                custom = tuple.Item3.custom,
                totalPrice = tuple.Item1.totalPrice,
                status = tuple.Item1.status,
            };

            return View("Detail",x);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailWithPayment(string bookingCode)
        {
            System.Diagnostics.Debug.WriteLine(bookingCode);
            var query = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id join tr in db.transfer_info on b.bookCode equals tr.bookCode where b.bookCode == bookingCode select new { b, u, t, tr };
            if(query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            var tuple = new Tuple<book_info, user_info, tour_info, transfer_info>(query.FirstOrDefault().b, query.FirstOrDefault().u, query.FirstOrDefault().t, query.FirstOrDefault().tr);


            BookingAndTransfer x = new BookingAndTransfer
            {
                bookDetail = new BookingDetailPage
                {
                    bookCode = bookingCode,
                    dateFrom = tuple.Item3.dateFrom,
                    dateUntil = tuple.Item3.dateUntil,
                    paymentMethod = tuple.Item1.paymentMethod,
                    destination = tuple.Item3.destination,
                    passengerList = tuple.Item1.passenger.Split(';'),
                    flight = tuple.Item3.flight,
                    inn = tuple.Item3.inn,
                    custom = tuple.Item3.custom,
                    status = tuple.Item1.status,
                    totalPrice = tuple.Item1.totalPrice,
                },
                transferDetail = query.FirstOrDefault().tr,
            };

            return View(x);
        }




        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detail(string fullName, string bookingCode)
        {
            if (fullName == "" || bookingCode == "")
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return RedirectToAction("Index", "Tour");
            }

            var query = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where b.bookCode == bookingCode && u.fullname == fullName select new { b, u, t };

            if (query.FirstOrDefault() == null)
            {
                TempData["book"] = "<script>alert('Booking not found');</script>";
                return RedirectToAction("Index", "Tour");
            }

            var tuple = new Tuple<book_info, user_info, tour_info>(query.FirstOrDefault().b,query.FirstOrDefault().u,query.FirstOrDefault().t);
            //book_info user = query.FirstOrDefault();

            return View(tuple);
        }*/


        [SessionExpire]
        public ActionResult Pending()
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                DateTime DateNow = DateTime.Now.Date;
                var userID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());

                var queryBook = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status != "Paid" select b;
                var queryUser = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status != "Paid" select u;
                var queryTour = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status != "Paid" select t;

                ViewBag.NeedConfirm = queryBook.Where(b => b.status == "Unpaid").Count();
                ViewBag.WaitApproval = queryBook.Where(b => b.status == "Waiting").Count();
                ViewBag.Disapproved = queryBook.Where(b => b.status == "Disapproved").Count();

                return View(new MultipleModel() { booking = queryBook.ToList(), user = queryUser.ToList(), tour = queryTour.ToList() });
            }
            else
            {
                return RedirectToAction("Index","Home");
            }
                
        }

        [SessionExpire]
        public ActionResult Purchased()
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                DateTime DateNow = DateTime.Now.Date;
                var userID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());

                var queryBook = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status == "Paid" select b;
                var queryUser = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status == "Paid" select u;
                var queryTour = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status == "Paid" select t;

                return View(new MultipleModel() { booking = queryBook.ToList(), user = queryUser.ToList(), tour = queryTour.ToList() });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [SessionExpire]
        public ActionResult ApproveList()
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                DateTime DateNow = DateTime.Now.Date;
                var userID = int.Parse(Session[GlobalVar.SESSION_ID].ToString());

                var queryBook = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status == "Waiting" select b;
                var queryUser = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status == "Waiting" select u;
                var queryTour = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where t.dateUntil >= DateNow && u.id == userID && b.status == "Waiting" select t;

                return View(new MultipleModel() { booking = queryBook.ToList(), user = queryUser.ToList(), tour = queryTour.ToList() });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelBooking(string bookingCode)
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                var query = from b in db.book_info where b.bookCode == bookingCode select b;
                //db.book_info.Attach(query.FirstOrDefault());
                db.book_info.Remove(query.FirstOrDefault());
                db.SaveChanges();
                return RedirectToAction("Pending", "Booking");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        
    }
}