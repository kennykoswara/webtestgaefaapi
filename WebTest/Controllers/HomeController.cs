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
        

        public ActionResult Index() //Dashboard front page
        {
            var query_wait_approval = from b in db.gaefa_book_new where b.status == "Waiting" select b;
            var query_sold = from b in db.gaefa_book_new where b.status == "Paid" select b;
            var query_wait_confirmation = from b in db.gaefa_book_new where b.status == "Unpaid" select b;
            var query_disapproved = from b in db.gaefa_book_new where b.status == "Disapproved" select b;
            var query_un_posted = from p in db.gaefa_book_new where p.postedToGaefa == false && p.status == "Paid" select p;

            ViewBag.WaitingApproval = query_wait_approval.Count();
            ViewBag.Sold = query_sold.Count();
            ViewBag.WaitingConfirmation = query_wait_confirmation.Count();
            ViewBag.Disapproved = query_disapproved.Count();
            ViewBag.Unposted = query_un_posted.Count();

            return View();
        }

        public ActionResult Login() //Login Page, Not Shown in navbar because only for admin. Must navigate to Home/Login
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

        public ActionResult LoginSuccessful(string urlReferrer) //Decided not to use this for now
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
        public ActionResult Login(string loginEmail, string loginPassword, bool rememberMe) //Process login details
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
                    string token = BasicHelper.getRandomString(50);
                    query.FirstOrDefault().token = token;

                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();
                    }

                    Session[GlobalVar.SESSION_NAME] = query.FirstOrDefault().fullname;
                    Session[GlobalVar.SESSION_ID] = query.FirstOrDefault().id;
                    Session[GlobalVar.SESSION_EMAIL] = query.FirstOrDefault().email;
                    //TempData["msg"] = "<script>alert('Login Successful');</script>";
                    if (rememberMe)
                    {
                        //create a cookie
                        HttpCookie myCookie = new HttpCookie("WebTestCookie");
                        if (!string.IsNullOrEmpty(myCookie.Values["token"]))
                        {
                            myCookie.Values.Set("token", token);
                        }
                        else
                        {
                            myCookie.Values.Add("token", token);
                        }
                        
                        //set cookie expiry date-time. Made it to last for next 12 hours.
                        myCookie.Expires = DateTime.Now.AddDays(7d);

                        //Most important, write the cookie to client.
                        Response.Cookies.Add(myCookie);
                    }

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
        public ActionResult Logout() //Logout function
        {
            if (Session[GlobalVar.SESSION_NAME] == null || Session[GlobalVar.SESSION_ID] == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                string email = Session[GlobalVar.SESSION_EMAIL].ToString();
                var query = from e in db.user_info where e.email == email select e;

                query.FirstOrDefault().token = null;

                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                }

                HttpCookie myCookie = new HttpCookie("WebTestCookie");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);

                Session.Abandon();
                Session.RemoveAll();
            }
            return View("Index");
        }
        
        public ActionResult Register() //Not used anymore
        {
            return View();
        }
        
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string fullname, string email, string password) //Not used anymore
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
                    }
                }
                else
                {
                    status = "error";
                }

                //return RedirectToAction("Index");
            }
            return Json(new { status = status });
        }
        */

            /*
        public void SendEmailConfirmation(user_info user, email_confirmation email) //Not used anymore
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

        public ActionResult ConfirmSuccess() //Not used anymore
        {
            return View();
        }

        public ActionResult ConfirmFailed() //Not used anymore
        {
            return View();
        }

        public ActionResult ConfirmAlready() //Not used anymore
        {
            return View();
        }

        [SessionExpire]
        public ActionResult ProfileTab() //Not used anymore
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
        public ActionResult ChangeFullName() //Not Used Anymore
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

        */

        [SessionExpire]
        public ActionResult ChangePassword() //Page to Change Password
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

        /*
        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitChangeFullName(string newFullName) //Not Used Anymore
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
        */

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitChangePassword(string currPass, string newPass) //Change password for admin
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

        public ActionResult ForgotPassword() //If admin forgot password
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
        public ActionResult SendForgotPasswordEmail(string email) //email about forgot password
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


        public ActionResult CreateNewPassword(string email, string token) //Page after click link confirmation of forgot password in email
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
        public ActionResult ConfirmNewPassword(string email, string token, string password) //Function to create new password after forgot
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

        [SessionExpire]
        public ActionResult DiscountGenerator() //Page to make coupon or promo code
        {
            return View();
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateCoupon(string discType, int? minPack, decimal? minPrice, int ticketAmount, int? discPercentage, decimal? discPrice, DateTime expiryDate, DateTime availableDate) //Function to generate coupon code
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

            if(minPrice != null)
            {
                if(discPrice > minPrice)
                {
                    return Json(new { status = "error", couponCount = -1 });
                }
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
        public ActionResult CouponsList() //Function to show list of generated coupon
        {
            var query_coupon = from c in db.gaefa_coupon select c;
            
            return View(model: query_coupon.ToList().OrderByDescending(x => x.status));
        }

        [SessionExpire]
        public ActionResult RemoveCoupon(string couponCode) //Function to remove coupon
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
        public ActionResult GeneratePromo(string discType, int? minPack, decimal? minPrice, int promoAmount, int? discPercentage, decimal? discPrice, DateTime expiryDate, DateTime availableDate, string promoCode) //Function to generate promo code
        {
            var promo_query = from p in db.gaefa_promo where p.promoCode == promoCode select p;

            if(promo_query.FirstOrDefault() != null)
            {
                return Json(new { status = "existed promo code"});
            }

            if (promoAmount == 0)
            {
                return Json(new { status = "error"});
            }

            if (discPercentage == null && discPrice == null)
            {
                return Json(new { status = "error"});
            }

            if (minPrice == null && minPack == null)
            {
                return Json(new { status = "error"});
            }

            if (minPrice != null)
            {
                if (discPrice > minPrice)
                {
                    return Json(new { status = "error"});
                }
            }

            if(promoCode == "" || promoCode == null)
            {
                return Json(new { status = "error"});
            }

            gaefa_promo x = new gaefa_promo
            {
                promoCode = promoCode,
                availableDate = availableDate,
                amount = promoAmount,
                discPercentage = discPercentage,
                discPrice = discPrice,
                expiryDate = expiryDate,
                packMin = minPack,
                priceMin = minPrice,
                used = 0,
            };

            if (ModelState.IsValid)
            {
                db.gaefa_promo.Add(x);
                db.SaveChanges();
            }

            return Json(new { status = "success", promoCode = promoCode});
        }

        [SessionExpire]
        public ActionResult PromoList() //Function to show list of generated promo code
        {
            var query_promo = from x in db.gaefa_promo select x;

            return View(model: query_promo.ToList().OrderByDescending(x => x.availableDate));
        }

        [SessionExpire]
        public ActionResult RemovePromo(string promoCode)//Function to delete promoCode
        {
            var promo_query = from c in db.gaefa_promo where c.promoCode == promoCode select c;

            if (promo_query.FirstOrDefault() == null)
            {
                return RedirectToAction("Error", "Gaefa");
            }
            else
            {
                db.gaefa_promo.Remove(promo_query.FirstOrDefault());
                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                }

                return RedirectToAction("PromoList", "Home");
            }
        }
        
        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReportIssueToGaefaAdmin(string emailSubject, string emailBody) //Function for admin to send email to Gaefa in case error occured -> Change gaefa admin email ad GlobalVar.cs
        {
            BasicHelper.sendEmail(GlobalVar.EMAIL_GAEFA, "Report Issue from WebTest - " + emailSubject, emailBody);
            return Json(new { status = "success" });
        }

        public ActionResult SetSession(string token)
        {
            var user = from u in db.user_info where u.token == token select u;

            if (user.Any())
            {
                Session[GlobalVar.SESSION_EMAIL] = user.FirstOrDefault().email;
                Session[GlobalVar.SESSION_NAME] = user.FirstOrDefault().fullname;
                Session[GlobalVar.SESSION_ID] = user.FirstOrDefault().id;
            }

            return RedirectToAction("", "");
        }
    }
}