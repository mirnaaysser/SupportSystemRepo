using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BizSparkSupport.Security;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Repositories;
using BizSparkSupport.MVC.Filters;
using BizSparkSupport.Mailing;

namespace BizSparkSupport.MVC.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        GenericUnitOfWork unitOfWork;
        //Support Team Login
        [LoggedOutFilter]
        public ActionResult Login()
        {
            return View("TeamLogin");
        }

        [LoggedOutFilter]
        [HttpPost]
        public ActionResult Login(string email, string password, bool RememberMe)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<DAL.User> userRepository = unitOfWork.GetRepoInstance<DAL.User>();
            GenericRepository<Employee> employeeRepository = unitOfWork.GetRepoInstance<Employee>();

            var result = userRepository.GetBy(u => u.Email == email);
            var result3 = employeeRepository.GetBy(e => e.UserName == email);
            //1. Check for email.
            if (result.Count() != 0 || result3.Count() != 0)
            {
                User testedUser;
                if (result.Count() != 0)
                {
                    testedUser = result.FirstOrDefault();
                }
                else
                {
                    Employee emp = result3.FirstOrDefault();
                    testedUser = userRepository.GetBy(u => u.UserID == emp.EmployeeID).FirstOrDefault();
                }
                if (testedUser.Active != false)
                {
                    //2. Check if this user is Employee
                    GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
                    var result2 = empRepository.GetBy(e => e.EmployeeID == testedUser.UserID);

                    //3. Check for password.
                    string decryptedPassword = Cryptography.Decrypt(testedUser.Password, testedUser.Hash);
                    if (decryptedPassword == password && result2.Count() != 0)
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
                        if (result.Count() == 0)
                        {
                            return RedirectToAction("EditProfile", "Login", new { area = "Admin" });
                        }
                        return RedirectToAction("Dashboard", "Support", new { area = "Admin" });
                    }
                    else
                    {
                        ViewBag.Error = "Incorrect Email or Password";
                    }
                }
                else
                {
                    ViewBag.Error = "This Account Has Been Deleted , Please Try Again";
                }
            }
            else
            {
                ViewBag.Error = "Incorrect Email or Password";
            }

            return View("TeamLogin");
        }

        [LoggedInAdminFilter]
        //Support Team Edit Profile
        public ActionResult EditProfile()
        {

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            User test = Session["USER"] as User;
            User user = userRepo.GetBy(u => u.UserID == test.UserID).FirstOrDefault();

            return View("~/Areas/Admin/Views/Support/EditProfile.cshtml", user);
        }

        [LoggedInAdminFilter]
        [HttpPost]
        public ActionResult EditProfile(User UpdatedUser)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();

            User user = userRepo.GetBy(u => u.UserID == UpdatedUser.UserID).FirstOrDefault();
            user.FirstName = UpdatedUser.FirstName;
            user.LastName = UpdatedUser.LastName;
            user.Email = UpdatedUser.Email;
            user.Mobile = UpdatedUser.Mobile;
            userRepo.Edit(user);
            unitOfWork.SaveChanges();

            return RedirectToAction("Dashboard", "Support", new { area = "Admin" });

        }

        public ActionResult ForgetPassword()
        {
            return View();
        }

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
                catch (Exception e)
                {
                    string x = e.ToString();
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
    }
}