using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using System.Configuration;
using Daishi.PaySharp;
using System.Text;
using PayPal.PayPalAPIInterfaceService.Model;
using PayPal.PayPalAPIInterfaceService;
using WebTest.Models;
using WebTest.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using WebTest.Security;

namespace WebTest.Controllers
{
    [SessionExpire]
    public class PayPalController : Controller
    {
        public class BookingAndPayPal
        {
            public BookingController.BookingDetailPage bookDetail;
            public paypal_info paypalDetail;
        }

        testEntities3 db = new testEntities3();
        // GET: PayPal
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Success(string bookCode, string passengers, int tourID, int buyerID, string token, string PayerID)
        {
            PayPalExpressCheckout paypal = new PayPalExpressCheckout();
            GetExpressCheckoutDetailsResponseType p = paypal.GetExpressCheckout(token);
            DoExpressCheckoutPaymentResponseType d = paypal.DoExpressCheckout();
            

            if (d.Ack.ToString() == "SUCCESS")
            {
                var priceQuery = from t in db.tour_info where t.id == tourID select t.price;

                book_info a = new book_info
                {
                    bookCode = bookCode,
                    tourID = tourID,
                    buyerID = buyerID,
                    paymentMethod = "PayPal",
                    passenger = passengers,
                    status = "Paid",
                    totalPrice = passengers.Split(';').Length * (decimal)(priceQuery.FirstOrDefault()),
                };

                paypal_info pp = new paypal_info
                {
                    bookCode = bookCode,
                    paypalAddress = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.Street1 + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.CityName + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.StateOrProvince + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.CountryName + ", " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.Address.PostalCode,
                    paypalAmount = decimal.Parse(p.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].OrderTotal.value),
                    paypalDateAndTime = d.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].PaymentDate,
                    paypalPayerID = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID,
                    paypalTransactionID = d.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID,
                    paypalName = p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerName.FirstName + " " + p.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerName.LastName,
                    paypalNote = p.GetExpressCheckoutDetailsResponseDetails.PaymentDetails[0].NoteText,
                };

                if (ModelState.IsValid)
                {
                    db.book_info.Add(a);
                    db.paypal_info.Add(pp);
                    
                    db.SaveChanges();
                };
            }
            
            return View();
        }

        public ActionResult Failed()
        {
            return View();
        }

        public ActionResult Cancel()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PayPalPayment(string destination, decimal price, int packageQuantity, string bookingCode, string passengers, int tourID, int buyerID, decimal totalPrice)
        {

            PayPalExpressCheckout paypal = new PayPalExpressCheckout();
            paypal.RETURN_URL = "http://localhost:55929/PayPal/Success?bookCode=" + bookingCode + "&passengers=" + passengers + "&tourID=" + tourID + "&buyerID=" + buyerID;
            paypal.CANCEL_URL = "http://localhost:55929/PayPal/Cancel/";
            if(paypal.SetExpressCheckout(destination, price, packageQuantity, "aaaa", 0))
            {
                return Redirect(PayPalExpressCheckout.PayPalExpressCheckoutURL + paypal.TOKEN);
            }
            else
            {
                return RedirectToAction("Failed","PayPal");
            }


            //return RedirectToAction("Index","Tour");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayPalDetail(string bookingCode)
        {
            var query = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id join pp in db.paypal_info on b.bookCode equals pp.bookCode where b.bookCode == bookingCode select new { b, u, t, pp };
            var tuple = new Tuple<book_info, user_info, tour_info, paypal_info>(query.FirstOrDefault().b, query.FirstOrDefault().u, query.FirstOrDefault().t, query.FirstOrDefault().pp);

            var paypalDateCultureInvariant = (DateTime.ParseExact(tuple.Item4.paypalDateAndTime.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture)).ToString();
            var paypalDateFormat = paypalDateCultureInvariant.Substring(2, 2) + "/" + paypalDateCultureInvariant.Substring(0, 1) + "/" + paypalDateCultureInvariant.Substring(5, 4) + " - " + tuple.Item4.paypalDateAndTime.Substring(11, 8) + " (GMT)";

            BookingAndPayPal x = new BookingAndPayPal
            {
                bookDetail = new BookingController.BookingDetailPage
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
                paypalDetail = new paypal_info
                {
                    bookCode = bookingCode,
                    paypalAddress = tuple.Item4.paypalAddress,
                    paypalAmount = tuple.Item4.paypalAmount,
                    paypalDateAndTime = paypalDateFormat,
                    paypalTransactionID = tuple.Item4.paypalTransactionID,
                    paypalName = tuple.Item4.paypalName,
                    paypalPayerID = tuple.Item4.paypalPayerID,
                    paypalNote = tuple.Item4.paypalNote,
                }
            };

            return View(x);
        }
    }
}