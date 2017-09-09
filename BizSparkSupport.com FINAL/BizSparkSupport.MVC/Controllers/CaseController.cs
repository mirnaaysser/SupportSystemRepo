using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BizSparkSupport.MVC.Repositories;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.ViewModels;
using System.IO;
using PagedList;
using BizSparkSupport.MVC.Filters;
using BizSparkSupport.Security;
using BizSparkSupport.MVC.Notification;
using System.Diagnostics;

namespace BizSparkSupport.MVC.Controllers
{
    [LoggedInStartupFilter]
    public class CaseController : Controller
    {
        GenericUnitOfWork unitOfWork;

        // GET: Portal/Case
        public ActionResult Dashboard()
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 1; }
            //////////////////////////////Register Notification
            var currentTime = DateTime.Now;
            Session["LastUpdated"] = currentTime;
            NotificationComponent.SharedInstance.RegisterUser_Notification(currentTime, userId);
            
            /////////////////////////////////
            
            int solvedCnt = 0, solvingCnt = 0, EscCnt = 0;
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();

            Dashboard model = new Dashboard()
            {
                Closed = new List<Case>(),
                Current = new List<Case>(),
                Escalated = new List<Case>()
            };

            GenericRepository<Priority> priority_caseRepository = unitOfWork.GetRepoInstance<Priority>();
            //GenericRepository<Status> status_caseRepository = unitOfWork.GetRepoInstance<Status>();
            GenericRepository<Category> category_caseRepository = unitOfWork.GetRepoInstance<Category>();

            List<Priority> priorities = priority_caseRepository.GetAll().ToList();
            List<Category> categories = category_caseRepository.GetAll().ToList();

            ViewBag.Priorities = priorities;
            ViewBag.Categories = categories;

            NewCaseVM newcasevm = new NewCaseVM();
            newcasevm.Case = new Case();
            newcasevm.CaseID = newcasevm.Case.CaseID;
            model.newCaseVM = newcasevm;

            var result = caseRepository.GetBy(c => c.StartupID == userId).OrderBy(c => c.CategoryID);
            if (result.Count() != 0)
            {
                foreach (Case c in result)
                {
                    if (c.StatusID == 1)
                    {
                        if (solvedCnt < 2)
                        {
                            model.Closed.Add(c);
                            solvedCnt++;
                        }
                    }
                    else if (c.StatusID == 2 || c.StatusID == 4)
                    {
                        if (solvingCnt < 2)
                        {
                            model.Current.Add(c);
                            solvingCnt++;
                        }
                    }
                    else
                    {
                        if (EscCnt < 2)
                        {
                            model.Escalated.Add(c);
                            EscCnt++;
                        }
                    }
                }
            }
            
            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            GenericRepository<Startup> startRepoo = unitOfWork.GetRepoInstance<Startup>();
            User test = Session["USER"] as User;
            User user = userRepo.GetBy(u => u.UserID == test.UserID).FirstOrDefault();
            Startup start = startRepoo.GetBy(s => s.CompanyID == test.UserID).FirstOrDefault();
            RegisterModel prof = new RegisterModel();
            prof.UserID = user.UserID;
            prof.CompanyID = start.CompanyID;
            prof.FirstName = user.FirstName;
            prof.LastName = user.LastName;
            prof.Mobile = user.Mobile;
            prof.CompanyName = start.CompanyName;
            prof.CompanyNumber = start.CompanyNumber;
            //RegisterModel prof = new RegisterModel()
            //{
            //    UserID = user.UserID,
            //    CompanyID = start.CompanyID,
            //    FirstName = user.FirstName,
            //    LastName = user.LastName,
            //    //Email = user.Email,
            //    Mobile = user.Mobile,
            //    CompanyName = start.CompanyName,
            //    CompanyNumber = start.CompanyNumber
            //};
            ViewBag.Profile = prof;
            return View(model);
        }

        public JsonResult GetNotifications()
        {
            long userId = 0;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            Debug.WriteLine("Test");
            var notificationRegisterTime = Session["LastUpdated"] != null ? Convert.ToDateTime(Session["LastUpdated"]) : DateTime.Now;

            var list = NotificationComponent.SharedInstance.GetMessages(notificationRegisterTime, userId);
            //update session here for get only new added contacts (notification)
            Session["LastUpdate"] = DateTime.Now;
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        [HttpPost]
        public ActionResult EditProfile(RegisterModel UpdatedUser)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            GenericRepository<Startup> startRepoo = unitOfWork.GetRepoInstance<Startup>();
            User user = userRepo.GetBy(u => u.UserID == UpdatedUser.UserID).FirstOrDefault();
            DAL.Startup start = startRepoo.GetBy(s => s.CompanyID == UpdatedUser.UserID).FirstOrDefault();
            user.FirstName = UpdatedUser.FirstName;
            user.LastName = UpdatedUser.LastName;
            //user.Email = UpdatedUser.Email;
            user.Mobile = UpdatedUser.Mobile;
            start.CompanyName = UpdatedUser.CompanyName;
            start.CompanyNumber = UpdatedUser.CompanyNumber;
            userRepo.Edit(user);
            startRepoo.Edit(start);
            unitOfWork.SaveChanges();
            return RedirectToAction("Dashboard");

        }
        
        public ActionResult Changepassword()
        {
            User test = Session["User"] as User;
            Session["PasswordError"] = "";
            if (test == null)
            {
                return HttpNotFound();
            }
            else
            {
                return View("ChangePassword", test);
            }
        }
        
        [HttpPost]
        public ActionResult ChangePassword(User user)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            User Changer = userRepo.GetBy(u => u.UserID == user.UserID).FirstOrDefault();
            string oldPassword = Request.Form["OldPassword"];
            string decryptedPassword = Cryptography.Decrypt(Changer.Password, Changer.Hash);
            if (oldPassword != decryptedPassword)
            {
                Session["PasswordError"] = "Wrong Password";
                return View("ChangePassword");
            }
            else
            {
                Session["PasswordError"] = "";
                string newPassWord = Request.Form["NewPassword"];
                Changer.Password = Cryptography.Encrypt(newPassWord, Changer.Hash);
                userRepo.Edit(Changer);
                unitOfWork.SaveChanges();
                return RedirectToAction("Dashboard");
            }

        }


        // Return List of Solved Cases
        public ActionResult GetSolvedList()
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 1; }
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();

            var result = caseRepository.GetBy(c => c.StartupID == userId).OrderBy(c => c.CategoryID);
            List<Case> SolvedList = new List<Case>();
            if (result.Count() != 0)
            {
                foreach (Case c in result)
                {
                    if (c.StatusID == 1)
                    {
                        SolvedList.Add(c);
                    }
                }
            }
            ViewBag.TableName = "Solved Cases";
            return View("CasesList", SolvedList);
        }

        // Return List of Current Cases
        public ActionResult GetSolvingList() {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 1; }
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();

            var res = caseRepository.GetBy(c => c.StartupID == userId).OrderBy(c => c.CategoryID);
            List<Case> SolvingList = new List<Case>();
            if (res.Count() != 0)
            {
                foreach (Case c in res)
                {
                    if (c.StatusID == 2 || c.StatusID == 4)
                    {
                        SolvingList.Add(c);
                    }
                }
            }
            ///////////////////////////////////////
            GenericRepository<Priority> priority_caseRepository = unitOfWork.GetRepoInstance<Priority>();
            //GenericRepository<Status> status_caseRepository = unitOfWork.GetRepoInstance<Status>();
            GenericRepository<Category> category_caseRepository = unitOfWork.GetRepoInstance<Category>();

            List<Priority> priorities = priority_caseRepository.GetAll().ToList();
            List<Category> categories = category_caseRepository.GetAll().ToList();

            ViewBag.Priorities = priorities;
            ViewBag.Categories = categories;
            Dashboard model = new Dashboard() { Current = SolvingList };

            NewCaseVM newcasevm = new NewCaseVM();
            newcasevm.Case = new Case();
            newcasevm.CaseID = newcasevm.Case.CaseID;
            model.newCaseVM = newcasevm;

            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            GenericRepository<Startup> startRepoo = unitOfWork.GetRepoInstance<Startup>();
            User test = Session["USER"] as User;
            User user = userRepo.GetBy(u => u.UserID == test.UserID).FirstOrDefault();
            Startup start = startRepoo.GetBy(s => s.CompanyID == test.UserID).FirstOrDefault();
            RegisterModel prof = new RegisterModel();
            prof.UserID = user.UserID;
            prof.CompanyID = start.CompanyID;
            prof.FirstName = user.FirstName;
            prof.LastName = user.LastName;
            prof.Mobile = user.Mobile;
            prof.CompanyName = start.CompanyName;
            prof.CompanyNumber = start.CompanyNumber;

            ViewBag.Profile = prof;
            //////////////////////////////////////
            ViewBag.TableName = "Solving Cases";
            return View("CurrentCasesList", model);
        }

        // Return List of Escalated Cases
        public ActionResult GetEscalatedList() {

            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 1; }
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();

            var result = caseRepository.GetBy(c => c.StartupID == userId).OrderBy(c => c.CategoryID);
            List<Case> EscalatedList = new List<Case>();
            if (result.Count() != 0)
            {
                foreach (Case c in result)
                {
                    if (c.StatusID == 3)
                    {
                        EscalatedList.Add(c);
                    }
                }
            }
            ///////////////////////////////////////////////////////////////////
            GenericRepository<Priority> priority_caseRepository = unitOfWork.GetRepoInstance<Priority>();
            //GenericRepository<Status> status_caseRepository = unitOfWork.GetRepoInstance<Status>();
            GenericRepository<Category> category_caseRepository = unitOfWork.GetRepoInstance<Category>();

            List<Priority> priorities = priority_caseRepository.GetAll().ToList();
            List<Category> categories = category_caseRepository.GetAll().ToList();

            ViewBag.Priorities = priorities;
            ViewBag.Categories = categories;
            Dashboard model = new Dashboard() { Escalated = EscalatedList };

            NewCaseVM newcasevm = new NewCaseVM();
            newcasevm.Case = new Case();
            newcasevm.CaseID = newcasevm.Case.CaseID;
            model.newCaseVM = newcasevm;

            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            GenericRepository<Startup> startRepoo = unitOfWork.GetRepoInstance<Startup>();
            User test = Session["USER"] as User;
            User user = userRepo.GetBy(u => u.UserID == test.UserID).FirstOrDefault();
            Startup start = startRepoo.GetBy(s => s.CompanyID == test.UserID).FirstOrDefault();
            RegisterModel prof = new RegisterModel();
            prof.UserID = user.UserID;
            prof.CompanyID = start.CompanyID;
            prof.FirstName = user.FirstName;
            prof.LastName = user.LastName;
            prof.Mobile = user.Mobile;
            prof.CompanyName = start.CompanyName;
            prof.CompanyNumber = start.CompanyNumber;

            ViewBag.Profile = prof;
            //////////////////////////////////////////
            ViewBag.TableName = "Escalated Cases";
            return View("EscalatedCasesList", model);
        }


        // Return Details of a Certain Case
        [EncryptedActionParameter]
        public ActionResult CaseDetails(long caseId)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 1; }

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Status> statusRepository = unitOfWork.GetRepoInstance<Status>();
            Case userCase = caseRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();
            userCase.Status = statusRepository.GetBy(st => st.StatusID == userCase.StatusID).FirstOrDefault();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            if (empCaseRepository.GetBy(c => c.CaseID == caseId).Count() != 0)
            {
                ViewBag.LastRating = empCaseRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault().Rating;
            }
            if (userCase.StartupID == userId)
            {
                CaseDetailsVM caseDetailsVM = new CaseDetailsVM();
                caseDetailsVM.Case = userCase;
                caseDetailsVM.MessagesModel = new MessagesVM();
                caseDetailsVM.MessagesModel.Messages = new List<Message>();

                unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

                int page = 1;
                int pageSize = 2;
                int pageNumber = 1;
                int pageCount = 1;

                var result = messageRepository.GetBy(m => m.CaseID == caseId && (m.ReceiverID == userId || m.SenderID == userId)).OrderByDescending(m => m.SubmissionDate);
                if (result != null)
                {
                    pageCount = (int)Math.Ceiling((double)result.Count() / pageSize);
                    var result2 = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                    foreach (Message message in result2)
                    {
                        caseDetailsVM.MessagesModel.Messages.Add(message);
                    }
                }

                ViewBag.CurrentPage = pageNumber;
                ViewBag.PageCount = pageCount;
                ViewBag.Case = caseId;

                return View(caseDetailsVM);
            }
            else
            {
                return HttpNotFound();
            }
        }

        //Save Message
        public ActionResult SendMessage(Message message)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee_Case> emp_caseRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            Employee_Case result = emp_caseRepository.GetBy(ec => ec.CaseID == message.CaseID).LastOrDefault();

            if (result != null)
            {
                message.ReceiverID = result.EmployeeID;
                message.SubmissionDate = DateTime.Now;
                messageRepository.Add(message);
            }
            unitOfWork.SaveChanges();
            return RedirectToAction("CaseDetails", new { caseId = message.CaseID });
        }

        //public ActionResult NewCase()
        //{
        //    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
        //    GenericRepository<Priority> priority_caseRepository = unitOfWork.GetRepoInstance<Priority>();
        //    //GenericRepository<Status> status_caseRepository = unitOfWork.GetRepoInstance<Status>();
        //    GenericRepository<Category> category_caseRepository = unitOfWork.GetRepoInstance<Category>();

        //    List<Priority> priorities = priority_caseRepository.GetAll().ToList();
        //    List<Category> categories = category_caseRepository.GetAll().ToList();

        //    ViewBag.Priorities = priorities;
        //    ViewBag.Categories = categories;

        //    NewCaseVM newcasevm = new NewCaseVM();
        //    newcasevm.Case = new Case();
        //    newcasevm.CaseID = newcasevm.Case.CaseID;
        //    return View(newcasevm);
        //}



        //Submit New Case
        public ActionResult SendCase(NewCaseVM newCase)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 1; }
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caserepo = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<DAL.File> filerepo = unitOfWork.GetRepoInstance<DAL.File>();

            Case startupCase = new Case();
            startupCase = newCase.Case;
            startupCase.SubmissionDate = DateTime.Now;
            startupCase.StatusID = 4;
            startupCase.StartupID = userId;
            caserepo.Add(startupCase);

            //try
            //{
            if (newCase.File != null)
            {
                Guid g = Guid.NewGuid();
                string GuidString = Convert.ToBase64String(g.ToByteArray());
                GuidString = GuidString.Replace("=", "");
                GuidString = GuidString.Replace("+", "");

                string _FileName = Path.GetFileName(newCase.File.FileName);
                string _UniqueFileName = GuidString + "+" + _FileName;
                //string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _UniqueFileName);


                byte[] uploadedFile = new byte[newCase.File.InputStream.Length];
                //var x = newCase.File.InputStream.Read(uploadedFile, 0, uploadedFile.Length);
                DAL.File NewFile = new DAL.File();
                NewFile.FileName = _FileName;
                NewFile.SavedFileName = _UniqueFileName;
                NewFile.CaseID = startupCase.CaseID;
                NewFile.Content = uploadedFile;
                NewFile.FileType = newCase.File.ContentType;
                filerepo.Add(NewFile);
            }
            //}
            //catch
            //{
            //    ViewBag.Message = "File upload failed!!";
            //    return View("NewCase", newCase);
            //}

            unitOfWork.SaveChanges();

            return RedirectToAction("Dashboard");
        }
        //Messages
        public ActionResult Messages(long caseId, int? page)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { return RedirectToAction("Login", "User", new { area = "" }); }

            MessagesVM model = new MessagesVM();

            model.Messages = new List<Message>();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            int pageSize = 2;
            int pageNumber = (page ?? 1);
            int pageCount = 1;

            var result = messageRepository.GetBy(m => m.CaseID == caseId && (m.ReceiverID == userId || m.SenderID == userId)).OrderByDescending(m => m.SubmissionDate);
            if (result != null)
            {
                pageCount = (int)Math.Ceiling((double)result.Count() / pageSize);
                var result2 = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                foreach (Message message in result2)
                {
                    model.Messages.Add(message);
                }
            }

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageCount = pageCount;
            ViewBag.Case = caseId;

            return View(model);
        }

        public ActionResult MessagesPartial(long caseId, int? page)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { return RedirectToAction("Login", "User", new { area = "" }); }

            List<Message> myList = new List<DAL.Message>();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            int pageSize = 2;
            int pageNumber = (page ?? 1);
            int pageCount = 1;

            var result = messageRepository.GetBy(m => m.CaseID == caseId && (m.ReceiverID == userId || m.SenderID == userId)).OrderByDescending(m => m.SubmissionDate);
            if (result != null)
            {
                pageCount = (int)Math.Ceiling((double)result.Count() / pageSize);
                var result2 = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                foreach (Message message in result2)
                {
                    myList.Add(message);
                }
            }
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageCount = pageCount;
            ViewBag.Case = caseId;
            return PartialView("MessageList", myList);
        }

        public ActionResult ShowMessage(long messageId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            Message message = messageRepository.GetBy(m => m.MessageID == messageId).FirstOrDefault();
            return PartialView("ShowMessage", message);
        }

        public ActionResult SubmitRating(Employee_Case empCase)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            //double? x = empCase.Rating;
            Employee_Case updatedRecord = empCaseRepository.GetBy(ec => ec.CaseID == empCase.CaseID).FirstOrDefault();
            if (updatedRecord != null)
            {

                updatedRecord.Rating = empCase.Rating;
                empCaseRepository.Edit(updatedRecord);

                unitOfWork.SaveChanges();
            }
            return RedirectToAction("CaseDetails", new { caseId = empCase.CaseID });
        }

    }
}