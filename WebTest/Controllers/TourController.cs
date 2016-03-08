using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using WebTest;
using System.Web.Script.Serialization;
using WebTest.Infrastructure;
using WebTest.Security;
using System.Runtime.Serialization.Json;
using WebTest.Models;
using PagedList;

namespace WebTest.Controllers
{

    
    public class TourController : Controller
    {
        private testEntities3 db = new testEntities3();

        public class filterObject
        {
            public string destination;
            public DateTime? dateFrom;
            public DateTime? dateUntil;
            public string flight;
            public string inn;
            public string custom;
        }

        // GET: FindTravel
        public ActionResult Index()
        {
            return View();
        }

        [SessionExpire]
        public ActionResult Create()
        {
            return View();
        }

        /*public ActionResult ListAll()
        {
            DateTime DateNow = DateTime.Now.Date;
            var query = from t in db.tour_info where t.dateFrom >= DateNow select t;
            return View("List", query.OrderBy(t => t.dateFrom).ToList());
        }*/

        /*public ActionResult ListAll(int? page)
        {
            DateTime DateNow = DateTime.Now.Date;

            var query = from t in db.tour_info where t.dateFrom >= DateNow select t;
            int amountList = 4;
            //return View("List", query.OrderBy(t => t.dateFrom).ToList());
            int pageNumber = (page ?? 1);
            return View("List", query.OrderBy(t => t.dateFrom).ToPagedList(pageNumber,amountList));
        }*/

        public JsonResult ListAllInJson()
        {
            DateTime DateNow = DateTime.Now.Date;
            return Json(db.tour_info.Where(t => t.dateFrom >= DateNow).OrderBy(t => t.dateFrom).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult List(string destination, string flight, string inn, string custom, DateTime? dateFrom, DateTime? dateUntil, int? page)
        {
            int amountList = 3;
            dateFrom = (dateFrom ?? DateTime.Now.Date);
            int pageNumber = (page ?? 1);
            IQueryable<tour_info> filter;
            if (!dateUntil.HasValue)
            {
                if (destination == "all")
                {
                    if (flight == "Both")
                    {
                        if (inn == "Both")
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.dateFrom >= dateFrom select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.custom == custom && t.dateFrom >= dateFrom select t;
                            }
                        }
                        else
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.inn == inn && t.dateFrom >= dateFrom select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.inn == inn && t.custom == custom && t.dateFrom >= dateFrom select t;
                            }
                        }
                    }
                    else
                    {
                        if (inn == "Both")
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.flight == flight && t.dateFrom >= dateFrom select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.flight == flight && t.custom == custom && t.dateFrom >= dateFrom select t;
                            }
                        }
                        else
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.flight == flight && t.inn == inn && t.dateFrom >= dateFrom select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.flight == flight && t.inn == inn && t.custom == custom && t.dateFrom >= dateFrom select t;
                            }
                        }
                    }
                }
                else
                {
                    if (flight == "Both")
                    {
                        if (inn == "Both")
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.dateFrom >= dateFrom select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.custom == custom && t.dateFrom >= dateFrom select t;
                            }
                        }
                        else
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.inn == inn && t.dateFrom >= dateFrom select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.inn == inn && t.custom == custom && t.dateFrom >= dateFrom select t;
                            }
                        }
                    }
                    else
                    {
                        if (inn == "Both")
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.flight == flight && t.dateFrom >= dateFrom select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.flight == flight && t.custom == custom && t.dateFrom >= dateFrom select t;
                            }
                        }
                        else
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.flight == flight && t.inn == inn && t.dateFrom >= dateFrom select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.flight == flight && t.inn == inn && t.custom == custom && t.dateFrom >= dateFrom select t;
                            }
                        }
                    }
                }
            }
            else
            {
                if (destination == "all")
                {
                    if (flight == "Both")
                    {
                        if (inn == "Both")
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.custom == custom && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                        }
                        else
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.inn == inn && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.inn == inn && t.custom == custom && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                        }
                    }
                    else
                    {
                        if (inn == "Both")
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.flight == flight && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.flight == flight && t.custom == custom && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                        }
                        else
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.flight == flight && t.inn == inn && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.flight == flight && t.inn == inn && t.custom == custom && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                        }
                    }
                }
                else
                {
                    if (flight == "Both")
                    {
                        if (inn == "Both")
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.custom == custom && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                        }
                        else
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.inn == inn && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.inn == inn && t.custom == custom && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                        }
                    }
                    else
                    {
                        if (inn == "Both")
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.flight == flight && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.flight == flight && t.custom == custom && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                        }
                        else
                        {
                            if (custom == "Both")
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.flight == flight && t.inn == inn && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                            else
                            {
                                filter = from t in db.tour_info where t.destination == destination && t.flight == flight && t.inn == inn && t.custom == custom && t.dateFrom >= dateFrom && t.dateFrom <= t.dateUntil && t.dateUntil >= t.dateFrom && t.dateUntil <= dateUntil select t;
                            }
                        }
                    }
                }
            }
            
            ViewBag.filter = new filterObject { destination = destination, dateFrom = dateFrom, dateUntil = dateUntil, flight = flight, inn = inn, custom = custom };
            return View("List", filter.OrderBy(t => t.dateFrom).ThenBy(t => t.destination).ToPagedList(pageNumber, amountList));
            //return View(filter.OrderBy(t => t.dateFrom).ToList());
            //return (Json(filter.ToList(), JsonRequestBehavior.AllowGet));
        }

        public string FilterJson(string destination, DateTime dateFrom, DateTime dateUntil, string flight, string inn, string custom)
        {

            filterObject filter = new filterObject
            {
                destination = destination,
                dateFrom = dateFrom,
                dateUntil = dateUntil,
                flight = flight,
                inn = inn,
                custom = custom,
            };

            var filterJson = new JavaScriptSerializer().Serialize(filter);
            //ViewBag.test = filterJson;

            return filterJson;
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public string CreateJson(string destination, DateTime dateFrom, DateTime dateUntil, string flight, string inn, string custom, decimal price)
        {
            if (flight == null)
            {
                flight = "False";
            }

            if (inn == null)
            {
                inn = "False";
            }

            if (custom == null)
            {
                custom = "False";
            }

            tour_info a = new tour_info
            {
                destination = destination,
                dateFrom = dateFrom,
                dateUntil = dateUntil,
                flight = flight,
                inn = inn,
                custom = custom,
                price = price,
            };

            if (ModelState.IsValid)
            {
                var createJson = new JavaScriptSerializer().Serialize(a);
                //ViewBag.test = createJson;
                //return View("Json");
                return createJson;
            }
            else
            {
                ViewBag.test = "error";
                //return View();
                return null;
            }
            
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string destination, DateTime dateFrom, DateTime dateUntil, string flight, string inn, string custom, decimal price)
        {
            if (flight == null)
            {
                flight = "False";
            }

            if (inn == null)
            {
                inn = "False";
            }

            if (custom == null)
            {
                custom = "False";
            }

            tour_info a = new tour_info
            {
                destination = destination,
                dateFrom = dateFrom,
                dateUntil = dateUntil,
                flight = flight,
                inn = inn,
                custom = custom,
                price = price,
            };

            if (ModelState.IsValid)
            {
                db.tour_info.Add(a);
                db.SaveChanges();
                TempData["successCreate"] = "<script>alert('Tour Created Successfully');</script>";
            }
            return View();
        }

        
        public ActionResult Checkout(int? id)
        {
            if (Session[GlobalVar.SESSION_NAME] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tour_info t = db.tour_info.Find(id);
                if (t == null)
                {
                    return HttpNotFound();
                }
                return View(t);
            }
            else
            {
                //TempData["checkoutLogin"] = "<script>alert('You have to be logged in to be able to checkout');</script>";
                return RedirectToAction("ListAll", "Tour");
            }
            
        }
        
    }


}