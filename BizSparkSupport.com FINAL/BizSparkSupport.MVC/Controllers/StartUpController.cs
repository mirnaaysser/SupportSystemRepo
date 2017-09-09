using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BizSparkSupport.MVC.ViewModels;
using BizSparkSupport.Security;
using BizSparkSupport.MVC.Repositories;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Models;
using BizSparkSupport.Mailing;
using BizSparkSupport.MVC.Filters;

namespace BizSparkSupport.MVC.Controllers
{
    public class StartUpController : Controller
    {
        // GET: StartUp
        // SupportSystemEntities1 Db = new SupportSystemEntities1();
        GenericUnitOfWork unitOfWork;
        public ActionResult Index()
        {
            return View();
        }
        //[LoggedOutFilter]
        //public ActionResult Code()
        //{

        //    ViewBag.Message = null;
        //    return View("Code");

        //}

        [LoggedOutFilter]
        [HttpPost]
        public ActionResult CodeChecker(Code code)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Code> codeRepo = unitOfWork.GetRepoInstance<Code>();

            Code co = codeRepo.GetBy(c => c.CodeValue == code.CodeValue).FirstOrDefault();
            if (co == null)
            {
                ViewBag.Message = "Wrong Code";
                return View("~/Views/StartUp/Code.cshtml");
            }
            else
            {
                DateTime date = DateTime.Now;
                if (date > co.ExpireAt)
                {
                    ViewBag.Message = "This code had expired.";
                    return View("~/Views/StartUp/Code.cshtml");
                }
                else if (co.UsedBefore == true)
                {
                    ViewBag.Message = "This code has already been used";
                    return View("~/Views/StartUp/Code.cshtml");

                }
                else
                {
                    co.UsedBefore = true;
                    codeRepo.Edit(co);
                    unitOfWork.SaveChanges();
                    //Db.Codes.Attach(co);
                    //Db.Entry(co).State = EntityState.Modified;
                    //Db.SaveChanges();
                    return RedirectToAction("Register");
                }

            }

        }
        [LoggedOutFilter]
        [HttpGet]
        public ActionResult Register()
        {
            return View("Register");
        }
        [LoggedOutFilter]
        [HttpPost]
        public ActionResult Registerion(RegisterModel user)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", user);
            }
            else

            {
                //working
                unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
                GenericRepository<Startup> startupRepo = unitOfWork.GetRepoInstance<Startup>();
                string passHash = Cryptography.GetRandomKey(64);
                Guid g = Guid.NewGuid();
                string GuidString = Convert.ToBase64String(g.ToByteArray());
                GuidString = GuidString.Replace("=", "");
                GuidString = GuidString.Replace("+", "");

                User us = new User()
                {
                    VerificationToken = GuidString,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsVerified = false,
                    Mobile = user.Mobile,
                    Active = true,
                    CreatedAt = DateTime.Now,
                    Hash = passHash,
                    Password = Cryptography.Encrypt(user.Password, passHash)


                };

                //Db.Users.Add(us);
                userRepo.Add(us);

                Startup s = new Startup()
                {
                    CompanyID = us.UserID,
                    CompanyName = user.CompanyName,
                    CompanyNumber = user.CompanyNumber

                };
                startupRepo.Add(s);
                unitOfWork.SaveChanges();
                SendEmail.send(us.Email, us.FirstName, this.Request, us.VerificationToken, null, "~/Validate.html");
                //Db.Startups.Add(s);
                //Db.SaveChanges();

                return RedirectToAction("Login", "User");
            }
        }

        //[HttpPost]
        //public JsonResult EmailExists(string email)
        //{

        //    return Json(!String.Equals(email, "ehsansajjad@yahoo.com", StringComparison.OrdinalIgnoreCase));
        //}

        //[LoggedOutFilter]
        //public ActionResult RequestCode()
        //{
        //    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
        //    GenericRepository<RequestCode> reqRepo = unitOfWork.GetRepoInstance<RequestCode>();
        //    String Email = this.Request.Form["Email"];
        //    RequestCode Req = new RequestCode();
        //    Req.Email = Email;

        //    reqRepo.Add(Req);
        //    unitOfWork.SaveChanges();
        //    return RedirectToAction("Code");

        //}

        public ActionResult CheckVerification()
        {
            string param1 = this.Request.QueryString["email"];
            string param2 = this.Request.QueryString["Verifivation"];
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();

            User Verified = userRepo.GetBy(r => r.Email == param1).FirstOrDefault();
            if ((Verified != null) && (Verified.VerificationToken == param2))
            {
                Verified.IsVerified = true;
                userRepo.Edit(Verified);
                unitOfWork.SaveChanges();
                return RedirectToAction("Dashboard", "Case", new { area = "" });
            }
            else
            {
                return HttpNotFound();
            }

        }

        [LoggedOutFilter]
        public ActionResult RequestCode()
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<RequestCode> reqRepo = unitOfWork.GetRepoInstance<RequestCode>();
            String Email = this.Request.Form["Email"];
            RequestCode Req = new RequestCode();
            Req.Email = Email;

            reqRepo.Add(Req);
            string message = null;
            try
            {
                unitOfWork.SaveChanges();
                message = "Your request has been submitted successfully";
            }
            catch (Exception)
            {
                message = "Sorry, an error occured during processing your request";
            }
            return RedirectToAction("Code", new { message });
        }

        [LoggedOutFilter]
        public ActionResult Code(string message)
        {
            ViewBag.Message = message;
            return View("Code");

        }
        //[LoggedInStartupFilter]
        //[HttpGet]
        //public ActionResult EditProfile()
        //{

        //    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
        //    GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
        //    GenericRepository<Startup> startRepoo = unitOfWork.GetRepoInstance<Startup>();
        //    User test = Session["USER"] as User;
        //    User user = userRepo.GetBy(u => u.UserID == test.UserID).FirstOrDefault();
        //    DAL.Startup start = startRepoo.GetBy(s => s.CompanyID == test.UserID).FirstOrDefault();
        //    ViewModels.RegisterModel prof = new RegisterModel()
        //    {
        //        UserID = user.UserID,
        //        CompanyID = start.CompanyID,
        //        FirstName = user.FirstName,
        //        LastName = user.LastName,
        //        //Email = user.Email,
        //        Mobile = user.Mobile,
        //        CompanyName = start.CompanyName,
        //        CompanyNumber = start.CompanyNumber
        //    };

        //    return View("Profile", prof);
        //}

        //[LoggedInStartupFilter]
        //[HttpPost]
        //public ActionResult EditProfile(RegisterModel UpdatedUser)
        //{
        //    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
        //    GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
        //    GenericRepository<Startup> startRepoo = unitOfWork.GetRepoInstance<Startup>();
        //    User user = userRepo.GetBy(u => u.UserID == UpdatedUser.UserID).FirstOrDefault();
        //    DAL.Startup start = startRepoo.GetBy(s => s.CompanyID == UpdatedUser.UserID).FirstOrDefault();
        //    user.FirstName = UpdatedUser.FirstName;
        //    user.LastName = UpdatedUser.LastName;
        //    //user.Email = UpdatedUser.Email;
        //    user.Mobile = UpdatedUser.Mobile;
        //    start.CompanyName = UpdatedUser.CompanyName;
        //    start.CompanyNumber = UpdatedUser.CompanyNumber;
        //    userRepo.Edit(user);
        //    startRepoo.Edit(start);
        //    unitOfWork.SaveChanges();
        //    return RedirectToRoute("Case/Dashboard");

        //}

        //[LoggedInStartupFilter]
        //public ActionResult Changepassword()
        //{
        //    User test = Session["User"] as User;
        //    Session["PasswordError"] = "";
        //    if (test == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    else
        //    {
        //        return View("ChangePassword", test);
        //    }
        //}

        //[LoggedInStartupFilter]
        //[HttpPost]
        //public ActionResult ChangePassword(User user)
        //{
        //    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
        //    GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
        //    User Changer = userRepo.GetBy(u => u.UserID == user.UserID).FirstOrDefault();
        //    string oldPassword = Request.Form["OldPassword"];
        //    string decryptedPassword = Cryptography.Decrypt(Changer.Password, Changer.Hash);
        //    if (oldPassword != decryptedPassword)
        //    {
        //        Session["PasswordError"] = "Wrong Password";
        //        return View("ChangePassword");
        //    }
        //    else
        //    {
        //        Session["PasswordError"] = "";
        //        string newPassWord = Request.Form["NewPassword"];
        //        Changer.Password = Cryptography.Encrypt(newPassWord, Changer.Hash);
        //        userRepo.Edit(Changer);
        //        unitOfWork.SaveChanges();
        //        return RedirectToAction("EditProfile", "StartUp");
        //    }

        //}
    }
}