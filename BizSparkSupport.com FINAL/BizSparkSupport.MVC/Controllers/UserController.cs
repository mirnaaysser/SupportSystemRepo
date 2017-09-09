using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BizSparkSupport.Security;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Repositories;
using BizSparkSupport.Mailing;
using BizSparkSupport.MVC.Filters;

namespace BizSparkSupport.MVC.Controllers
{
    
    public class UserController : Controller
    {
        GenericUnitOfWork unitOfWork;

        [LoggedOutFilter]
        public ActionResult Login()
        {
            return View();
        }

        [LoggedOutFilter]
        [HttpPost]
        public ActionResult Login(string email, string password, bool RememberMe)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();

            var result = userRepository.GetBy(u => u.Email == email);

            //1. Check for email.
            if (result.Count() != 0)
            {
                User testedUser = result.FirstOrDefault();
                if (testedUser.Active != false)
                {


                    //2. Check for password.
                    string decryptedPassword = Cryptography.Decrypt(testedUser.Password, testedUser.Hash);
                    if (decryptedPassword == password)
                    {
                        // No error messages.
                        ViewBag.Error = null;

                        //Save user data in the current session.
                        Session["USER"] = testedUser;

                        //Check if user want a remember me cookie.
                        if (RememberMe)
                        {
                            testedUser.RememberToken = Cryptography.GetRandomKey(8);
                            userRepository.Edit(testedUser);
                            unitOfWork.SaveChanges();

                            HttpCookie LoginCookie = new HttpCookie("LoginCookie");
                            LoginCookie.Values.Add("RemeberMe", testedUser.RememberToken);
                            LoginCookie.Values.Add("UserID", testedUser.UserID.ToString());
                            LoginCookie.Expires = DateTime.Now.AddDays(30);
                            Response.Cookies.Set(LoginCookie);
                        }

                        return RedirectToAction("Dashboard", "Case");
                    }
                    else
                    {
                        ViewBag.Error = "Incorrect Email or Password";
                    }
                }
                else
                {
                    ViewBag.Error = "NoT Active User";
                }
            }
            else
            {
                ViewBag.Error = "Incorrect Email or Password";
            }

            return View();
        }

        [LoggedOutFilter]
        public ActionResult ForgetPassword()
        {
            return View();
        }

        [LoggedOutFilter]
        [HttpPost]
        public ActionResult ForgetPassword(string email)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();

            var users = userRepository.GetBy(u => u.Email == email);

            if (users.Count() != 0)
            {
                User user = users.FirstOrDefault();

                try
                {
                    if (ModelState.IsValid)
                    {
                        string body = string.Format("Dear {0} , <br/> Please Use the link below to Reset Your Password : \n" +
                            "<a href=\"{1}\"title=\"User Email Confirm\">{1}</a>",
                            user.FirstName + " " + user.LastName,
                            Url.Action("ResetPassword", "User", new { Token = user.VerificationToken, Email = user.Email }, Request.Url.Scheme));

                        Mailing.EmailConfig conf = new Mailing.EmailConfig("bizsparkstartupcare@gmail.com", "BizSpark Support", "P@ssw0rd8")
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true
                        };

                        Email uEmail = new Email(conf);
                        uEmail.Send(user.Email, "Reset Password", body);
                    }
                }
                catch (Exception)
                {

                }
                ViewBag.UserEmail = email;
                return View("ForgetPassMessage");
            }
            else
            {
                ViewBag.ErrorMessage = "The email you entered is not registered please try again.";
                return View();
            }
        }
       
        public ActionResult Logout()
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            long id = ((User)Session["USER"]).UserID;
            if (empRepository.GetBy(e => e.EmployeeID == id).Count() != 0)
            {
                Session["USER"] = null;
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }
            else
            {
                Session["USER"] = null;
                return RedirectToAction("Login", "User", new { area = "" });
            }
        }
    }
}