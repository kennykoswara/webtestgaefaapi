using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTest.Infrastructure;
using WebTest.Models;
using WebTest.Security;

namespace WebTest.Controllers
{
    [SessionExpire]
    public class TransferController : Controller
    {
        testEntities3 db = new testEntities3();
        // GET: Transfer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AfterCheckout(string bookingCode)
        {
            return View("", model: bookingCode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer(string bookingCode)
        {
            var query = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where b.bookCode == bookingCode select new { b, u, t };

            if (query.FirstOrDefault() == null)
            {
                return RedirectToAction("Index", "Tour");
            }

            var tuple = new Tuple<book_info, user_info, tour_info>(query.FirstOrDefault().b, query.FirstOrDefault().u, query.FirstOrDefault().t);

            BookingController.BookingDetailPage x = new BookingController.BookingDetailPage
            {
                bookCode = tuple.Item1.bookCode,
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

            return View(x);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPaymentTransfer(string bookCode, DateTime date, string sender, decimal price, string bank, string bankFrom, string account, string email, HttpPostedFileBase receipt)
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                return RedirectToAction("Index","Home");
            }
            else
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

                transfer_info x = new transfer_info
                {
                    bookCode = bookCode,
                    paymentDate = date,
                    senderName = sender,
                    amountTransferred = price,
                    bankName = bank,
                    fromBank = bankFrom,
                    accountNumber = account,
                    email = email,
                    receipt = path,
                };

                if (ModelState.IsValid)
                {
                    db.transfer_info.Add(x);
                    var query = from b in db.book_info where b.bookCode == bookCode select b;
                    query.FirstOrDefault().status = "Waiting";
                    db.SaveChanges();
                }

                return RedirectToAction("Pending", "Booking");
            }
            
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovePayment(string bookCode, string approveChoice)
        {
            //if(Request.Form["approveButton"] != null)
            if(approveChoice == "Approve")
            {
                //TempData["Approved"] = "<script>alert('Payment Approved.');</script>";
                var query = from b in db.book_info where b.bookCode == bookCode select b;
                query.FirstOrDefault().status = "Paid";
                db.SaveChanges();
                //return RedirectToAction("ApproveList", "Booking");
                return Json(new { status = "approve" });
            }
            else
            {
                //TempData["Approved"] = "<script>alert('Payment Disapproved.');</script>";
                var query = from b in db.book_info where b.bookCode == bookCode select b;
                var transfer = from t in db.transfer_info where t.bookCode == bookCode select t;
                query.FirstOrDefault().status = "Disapproved";
                db.transfer_info.Remove(transfer.FirstOrDefault());
                db.SaveChanges();
                //return RedirectToAction("ApproveList", "Booking");
                return Json(new { status = "disapprove" });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DisapprovedTransfer(string bookingCode)
        {
            var query = from b in db.book_info join u in db.user_info on b.buyerID equals u.id join t in db.tour_info on b.tourID equals t.id where b.bookCode == bookingCode select new { b, u, t };

            if (query.FirstOrDefault() == null)
            {
                return RedirectToAction("Index", "Tour");
            }

            var tuple = new Tuple<book_info, user_info, tour_info>(query.FirstOrDefault().b, query.FirstOrDefault().u, query.FirstOrDefault().t);

            BookingController.BookingDetailPage x = new BookingController.BookingDetailPage
            {
                bookCode = tuple.Item1.bookCode,
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

            return View(x);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmAfterDisapproved(string bookCode, DateTime date, string sender, decimal price, string bank, string bankFrom, string account, string email, HttpPostedFileBase receipt)
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
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

                transfer_info x = new transfer_info
                {
                    bookCode = bookCode,
                    paymentDate = date,
                    senderName = sender,
                    amountTransferred = price,
                    bankName = bank,
                    fromBank = bankFrom,
                    accountNumber = account,
                    email = email,
                    receipt = path,
                };

                if (ModelState.IsValid)
                {
                    db.transfer_info.Add(x);
                    var query = from b in db.book_info where b.bookCode == bookCode select b;
                    query.FirstOrDefault().status = "Waiting";
                    db.SaveChanges();
                }

                return RedirectToAction("Pending", "Booking");
            }

        }
    }
}