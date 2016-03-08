using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebTest.Infrastructure;
using WebTest.Models;
using WebTest.PortalList;
using WebTest.Security;

namespace WebTest.Controllers
{

    public class HomeController : Controller
    {
        testEntities3 db = new testEntities3();

        /*NO ADULT CHILD
        public ActionResult Index()
        {
            var query_wait_approval = from b in db.gaefa_book_info where b.status == "Waiting" select b;
            var query_sold = from b in db.gaefa_book_info where b.status == "Paid" select b;
            var query_wait_confirmation = from b in db.gaefa_book_info where b.status == "Unpaid" select b;
            var query_disapproved = from b in db.gaefa_book_info where b.status == "Disapproved" select b;
            var query_un_posted = from p in db.gaefa_paypal_info where p.postedToGaefa == false select p;

            ViewBag.WaitingApproval = query_wait_approval.Count();
            ViewBag.Sold = query_sold.Count();
            ViewBag.WaitingConfirmation = query_wait_confirmation.Count();
            ViewBag.Disapproved = query_disapproved.Count();
            ViewBag.Unposted = query_un_posted.Count();

            return View();
        }
        */

        public ActionResult Index()
        {
            var query_wait_approval = from b in db.gaefa_book_new where b.status == "Waiting" select b;
            var query_sold = from b in db.gaefa_book_new where b.status == "Paid" select b;
            var query_wait_confirmation = from b in db.gaefa_book_new where b.status == "Unpaid" select b;
            var query_disapproved = from b in db.gaefa_book_new where b.status == "Disapproved" select b;
            var query_un_posted = from p in db.gaefa_paypal_new where p.postedToGaefa == false select p;

            ViewBag.WaitingApproval = query_wait_approval.Count();
            ViewBag.Sold = query_sold.Count();
            ViewBag.WaitingConfirmation = query_wait_confirmation.Count();
            ViewBag.Disapproved = query_disapproved.Count();
            ViewBag.Unposted = query_un_posted.Count();

            return View();
        }

        public ActionResult Login()
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        public ActionResult LoginSuccessful(string urlReferrer)
        {
            if (Session[GlobalVar.SESSION_NAME] != null || Session[GlobalVar.SESSION_ID] != null)
            {
                string url = (urlReferrer ?? Url.Action("Index", "Home"));
                return View(model: url);
            }
            else
            {
                return RedirectToAction("Error", "Gaefa");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string loginEmail, string loginPassword)
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                loginPassword = BasicHelper.hashPassword(loginPassword);

                var query = from e in db.user_info where e.email == loginEmail && e.password == loginPassword select e;
                string status;

                if (query.FirstOrDefault() == null)
                {
                    //TempData["msg"] = "<script>alert('Please enter a valid email address and password');</script>";
                    status = "Incorrect";
                }
                else
                {
                    Session[GlobalVar.SESSION_NAME] = query.FirstOrDefault().fullname;
                    GlobalVar.ID_USER = query.FirstOrDefault().id;
                    Session[GlobalVar.SESSION_ID] = query.FirstOrDefault().id;
                    Session[GlobalVar.SESSION_EMAIL] = query.FirstOrDefault().email;
                    //TempData["msg"] = "<script>alert('Login Successful');</script>";
                    status = "Success";
                    //System.Diagnostics.Debug.WriteLine(Request.Url.GetLeftPart(UriPartial.Authority));
                    //System.Diagnostics.Debug.WriteLine(Url.Action("Detail", "Booking"));  
                }
                /*if(Request.UrlReferrer.ToString() == Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("Detail", "Booking"))
                {
                    return View("Index");
                }
                else
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }*/
                //return Redirect(Request.UrlReferrer.ToString());
                System.Diagnostics.Debug.WriteLine("----" + Request.UrlReferrer.ToString());
                return Json(new { status = status, url = Request.UrlReferrer.ToString().Replace("&", "__ampersand__") });
            }
            else
            {
                return View("Index");
            }
        }

        [SessionExpire]
        public ActionResult Logout()
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                Session.Abandon();
                Session.RemoveAll();
                FormsAuthentication.SignOut();
            }
            return View();
        }

        // GET: UserInfo/Create
        public ActionResult Register()
        {
            return View();
        }

        // POST: UserInfo/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string fullname, string email, string password)
        {
            password = BasicHelper.hashPassword(password);

            var checkEmail = from e in db.user_info where e.email == email select e.email;
            var query_email = from qe in db.email_confirmation where qe.email == email select qe;
            string status;

            if (checkEmail.FirstOrDefault() != null)
            {
                //TempData["msg"] = "<script>alert('Email has been used. Please use another email.');</script>";
                //return RedirectToAction("Index");
                status = "used";
            }
            else
            {
                user_info a = new user_info
                {
                    fullname = fullname,
                    email = email,
                    password = password,
                };

                if (query_email.FirstOrDefault() != null)
                {
                    db.email_confirmation.Remove(query_email.FirstOrDefault());
                }

                email_confirmation e = new email_confirmation
                {
                    email = email,
                    token = Guid.NewGuid().ToString().Replace("-", ""),
                };


                if (ModelState.IsValid)
                {
                    db.user_info.Add(a);
                    db.email_confirmation.Add(e);
                    db.SaveChanges();
                    SendEmailConfirmation(a, e);
                    status = "success";
                    //TempData["msg"] = "<script>alert('A link has been sent to your email. Please click that link to confirm your email address.');</script>";
                    /*try
                    {
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                    ve.PropertyName,
                                    eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                    ve.ErrorMessage);
                            }
                        }
                    }*/
                }
                else
                {
                    status = "error";
                }

                //return RedirectToAction("Index");
            }
            return Json(new { status = status });
        }


        public void SendEmailConfirmation(user_info user, email_confirmation email)
        {
            var body = "<p>Hi, " + user.fullname + ".</p><p>Welcome to WebTest.</p><p>Before you can login with your account, please confirm your account on this " + "<a href='" + GlobalVar.BASE_URL + "Home/ConfirmEmail?email=" + user.email + "&token=" + email.token + "'>link</a></p>";
            var message = new MailMessage();
            message.To.Add(new MailAddress(user.email));  // replace with valid value 
            message.From = new MailAddress("admin@WebTest.com");  // replace with valid value
            message.Subject = "Welcome";
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "webtestjegul@gmail.com",  // replace with valid value
                    Password = "konyakujelly"  // replace with valid value
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
        }

        /*
        public ActionResult ConfirmEmail(string email, string token)
        {
            var query_user = from u in db.user_info where u.email == email select u;
            var query_email = from e in db.email_confirmation where e.email == email && e.token == token select e;

            if (query_email.FirstOrDefault() != null)
            {
                if (query_user.FirstOrDefault().emailConfirmed)
                {
                    return RedirectToAction("ConfirmAlready", "Home");
                }
                else
                {
                    query_user.FirstOrDefault().emailConfirmed = true;
                    db.email_confirmation.Remove(query_email.FirstOrDefault());
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();
                    }
                    return RedirectToAction("ConfirmSuccess", "Home");
                }
            }
            else
            {
                return RedirectToAction("ConfirmFailed", "Home");
            }
        }
        */

        public ActionResult ConfirmSuccess()
        {
            return View();
        }

        public ActionResult ConfirmFailed()
        {
            return View();
        }

        public ActionResult ConfirmAlready()
        {
            return View();
        }

        [SessionExpire]
        public ActionResult ProfileTab()
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                string email = Session[GlobalVar.SESSION_EMAIL].ToString();
                var query = from u in db.user_info where u.email == email select u;
                user_info user = query.FirstOrDefault();
                return View("Profile", model: user);
            }
        }

        [SessionExpire]
        public ActionResult ChangeFullName()
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                string email = Session[GlobalVar.SESSION_EMAIL].ToString();
                var query = from u in db.user_info where u.email == email select u;
                return View(model: query.FirstOrDefault().fullname);
            }
        }

        [SessionExpire]
        public ActionResult ChangePassword()
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                return View("ChangePassword");
            }
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitChangeFullName(string newFullName)
        {
            //TempData["msg"] = "<script>alert('Full name changed successfully.');</script>";
            string email = Session[GlobalVar.SESSION_EMAIL].ToString();
            var query = from u in db.user_info where u.email == email select u;
            query.FirstOrDefault().fullname = newFullName;
            db.SaveChanges();
            Session[GlobalVar.SESSION_NAME] = newFullName;
            //return RedirectToAction("ProfileTab","Home");
            return Json(new { status = "success" });
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitChangePassword(string currPass, string newPass)
        {
            string email = Session[GlobalVar.SESSION_EMAIL].ToString();
            var query = from u in db.user_info where u.email == email select u;

            currPass = BasicHelper.hashPassword(currPass);
            newPass = BasicHelper.hashPassword(newPass);

            if (query.FirstOrDefault().password == currPass)
            {
                //TempData["changed"] = "<script>alert('Password changed successfully.');</script>";
                query.FirstOrDefault().password = newPass;
                db.SaveChanges();
                //return RedirectToAction("ProfileTab", "Home");
                return Json(new { currentPassword = "true" });
            }
            else
            {
                //TempData["pass"] = "<script>alert('The current password you input is wrong. Please try again.');</script>";
                //return RedirectToAction("ChangePassword", "Home");
                return Json(new { currentPassword = "false" });
            }

        }

        public ActionResult ForgotPassword()
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendForgotPasswordEmail(string email)
        {
            var query = from u in db.user_info where u.email == email select u;
            var query_passConfirm = from p in db.password_confirmation where p.email == email select p;
            var token = Guid.NewGuid().ToString().Replace("-", "");

            if (query.FirstOrDefault() != null)
            {
                if (query_passConfirm.FirstOrDefault() != null)
                {
                    db.password_confirmation.Remove(query_passConfirm.FirstOrDefault());
                }

                password_confirmation p = new password_confirmation
                {
                    email = email,
                    token = BasicHelper.hashPassword(token),
                };

                if (ModelState.IsValid)
                {
                    db.password_confirmation.Add(p);
                    db.SaveChanges();
                }

                var subject = "Forgot Password";
                var body = "<p>Hi, it seems that you have just pressed forgot password.</p>";
                body += "<p>If this is your request, please click this <a href='" + Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("CreateNewPassword", "Home", new { email = email, token = token }) + "'>link</a> to change your password.</p>";
                body += "<p>If you didn't request this, just ignore this e-mail.</p>";
                body += "<br/>";
                body += "<p>Thank you.</p>";
                BasicHelper.sendEmail(email, subject, body);
                return Json(new { status = "success" });
            }
            else
            {
                //TempData["UnregisteredEmail"] = "<script>alert('The email you submitted is no registered in our database. Please try again.');</script>";
                return Json(new { status = "failed" });
            }
        }


        public ActionResult CreateNewPassword(string email, string token)
        {
            if(email == null || token == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                var hashedToken = BasicHelper.hashPassword(token);
                var query_password = from p in db.password_confirmation where p.email == email && p.token == hashedToken select p;
                if (query_password.FirstOrDefault() != null)
                {
                    //System.Diagnostics.Debug.WriteLine("---" + query_password.FirstOrDefault().email);
                    return View(model: query_password.FirstOrDefault());
                }
                else
                {
                    return RedirectToAction("Error", "Gaefa");
                }
            }
            

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmNewPassword(string email, string token, string password)
        {
            var query_password = from p in db.password_confirmation where p.email == email && p.token == token select p;
            var query_user = from u in db.user_info where u.email == email select u;
            
            query_user.FirstOrDefault().password = BasicHelper.hashPassword(password);
            
            if(query_password.FirstOrDefault() != null)
            {
                db.password_confirmation.Remove(query_password.FirstOrDefault());
                db.SaveChanges();
                return Json(new { status = "success" });
            }
            else
            {
                return Json(new { status = "failed" });
            }
            //TempData["msg"] = "<script>alert('Password changed successfully.');</script>";
            //return RedirectToAction("Index","Home");
        }

        public ActionResult GenerateOrderReference()
        {
            string a = DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + BasicHelper.getRandomString(6);
            return View(model: a);
        }

        [SessionExpire]
        public ActionResult CouponGenerator()
        {
            /*
            GaefaSignature sign = new GaefaSignature();
            GaefaPagination pagination = new GaefaPagination()
            {
                limit = 10000,
                start = 0,
            };
            GaefaFilter filter = new GaefaFilter()
            {
                titleOrLocation = "",
                include_flight = null,
                include_inn = null,
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
            List<int> ticketIDList = new List<int>();

            if(ListOfGaefaPackageJSON == null)
            {
                return RedirectToAction("Error","Gaefa");
            }
            else
            {
                ListOfGaefaPackageJSON.ForEach(x =>
                {
                    ticketIDList.Add(x.id);
                });

                return View(model: ticketIDList);
            }*/
            return View();
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateCoupon(string discType, int? minPack, decimal? minPrice, int ticketAmount, int? discPercentage, decimal? discPrice, DateTime expiryDate, DateTime availableDate)
        {
            IQueryable<gaefa_coupon> coupon_query;
            int count = 0;

            if(ticketAmount == 0)
            {
                return Json(new { status = "error", couponCount = -1 });
            }

            if(discPercentage == null && discPrice == null)
            {
                return Json(new { status = "error", couponCount = -1 });
            }

            if(minPrice == null && minPack == null)
            {
                return Json(new { status = "error", couponCount = -1 });
            }

            for(int i = 0; i < ticketAmount; i++)
            {
                string couponCode = BasicHelper.getRandomString(10).ToUpper();
                coupon_query = from c in db.gaefa_coupon where c.couponCode == couponCode select c;

                if(coupon_query.FirstOrDefault() != null)
                {
                    couponCode = BasicHelper.getRandomString(10).ToUpper();
                }
                else
                {
                    if(discType == "percentage")
                    {
                        gaefa_coupon c = new gaefa_coupon
                        {
                            //ticketID = ticketID,
                            packMin = minPack,
                            priceMin = minPrice,
                            couponCode = couponCode,
                            expiryDate = expiryDate,
                            availableDate = availableDate,
                            discPercentage = discPercentage,
                            discPrice = null,
                            status = true, //true means available to be used, false mean the opposite
                        };

                        if (ModelState.IsValid)
                        {
                            db.gaefa_coupon.Add(c);
                            db.SaveChanges();
                        }
                    }
                    else if(discType == "price")
                    {
                        gaefa_coupon c = new gaefa_coupon
                        {
                            //ticketID = ticketID,
                            packMin = minPack,
                            priceMin = minPrice,
                            couponCode = couponCode,
                            expiryDate = expiryDate,
                            availableDate = availableDate,
                            discPercentage = null,
                            discPrice = discPrice,
                            status = true, //true means available to be used, false mean the opposite
                        };

                        if (ModelState.IsValid)
                        {
                            db.gaefa_coupon.Add(c);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        return Json(new { status = "error", couponCount = -1 });
                    }
                }
                count += 1;
            }
            
            return Json(new { status = "success", couponCount = count });
        }

        [SessionExpire]
        public ActionResult CouponsList()
        {
            var query_coupon = from c in db.gaefa_coupon select c;
            
            return View(model: query_coupon.ToList().OrderByDescending(x => x.status));
        }

        [SessionExpire]
        public ActionResult RemoveCoupon(string couponCode)
        {
            var coupon_query = from c in db.gaefa_coupon where c.couponCode == couponCode select c;

            if(coupon_query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                if (coupon_query.FirstOrDefault().status)
                {
                    db.gaefa_coupon.Remove(coupon_query.FirstOrDefault());
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();
                    }

                    return RedirectToAction("CouponsList", "Home");
                }
                else
                {
                    return RedirectToAction("Error", "Gaefa");
                }
                
            }
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReportIssueToGaefaAdmin(string emailSubject, string emailBody)
        {
            BasicHelper.sendEmail(GlobalVar.EMAIL_GAEFA, "Report Issue from WebTest - " + emailSubject, emailBody);
            return Json(new { status = "success" });
        }
    }
}