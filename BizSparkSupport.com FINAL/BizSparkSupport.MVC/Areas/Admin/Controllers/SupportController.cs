using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Repositories;
using BizSparkSupport.MVC.Areas.Admin.ViewModels;
using PagedList;
using BizSparkSupport.MVC.Filters;
using System.Diagnostics;
using BizSparkSupport.MVC.Notification;

namespace BizSparkSupport.MVC.Areas.Admin.Controllers
{
    [LoggedInAdminFilter]
    public class SupportController : Controller
    {
        GenericUnitOfWork unitOfWork;

        [LoginFilter]
        // Return New Cases in a list
        public ActionResult Dashboard() {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 2; }
            ///////////////////////////////////////Register Notification For Support Team
            var currentTime = DateTime.Now;
            Session["LastUpdated"] = currentTime;
            NotificationComponent.SharedInstance.RegisterEmp_Notification(currentTime, userId);
            //////////////////////////////////////////////////

            int newCnt = 0, openCnt = 0, assignedCnt = 0;
            Dashboard model = new Dashboard();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            Employee currentEmp = empRepository.GetBy(emp => emp.EmployeeID == userId).FirstOrDefault();

            //Cases
            model.New = new List<Case>();
            model.Open = new List<Case>();
            model.Assigned = new List<Case>();
            //History
            model.Solved = new List<Case>();

            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            var result = empCaseRepository.GetBy(ec => ec.EmployeeID == userId);
            List<Case> empCases = new List<Case>();

            List<Case> myList = new List<Case>();
            int pageSize = 2;
            int pageNumber = 1;
            int pageCount = 1;

            if (result != null)
            {
                foreach (Employee_Case ec in result)
                {
                    if (ec.IsDeleted != false)
                    {
                        Case temp = caseRepository.GetBy(c => c.CaseID == ec.CaseID).FirstOrDefault();
                        if (temp.StatusID == 1)
                        {
                            // Solved
                            myList.Add(temp);
                        }
                        else if (temp.StatusID == 2 || temp.StatusID == 3)
                        {
                            if (ec.OpenedAt == null)
                            {
                                //Not Opened yet
                                //   if (assignedCnt < 2)
                                {
                                    model.Assigned.Add(temp);
                                }
                            }
                            else
                            {
                                //Opened (Current Or Escalated)
                                //   if (openCnt < 2)
                                {
                                    model.Open.Add(temp);
                                }
                            }
                        }
                    }
                }

                pageCount = (int)Math.Ceiling((double)myList.Count() / pageSize);
                model.Solved = (myList.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToList();
            }
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageCount = pageCount;
            // Get New Cases Not Assigned To Anyone
            // Get Cases with the same category Id as the current emp and 
            var newCases = caseRepository.GetBy(c => c.CategoryID == currentEmp.CategoryID && c.StatusID == 4);
            if (newCases != null)
            {
                foreach (Case c in newCases)
                {
                    // if (newCnt < 2)
                    {
                        model.New.Add(c);
                    }
                    //  else
                    {
                        //   break;
                    }
                }
            }
            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            User user = userRepo.GetBy(u => u.UserID == userId).FirstOrDefault();
            ViewBag.User = user;
            return View(model);
        }

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
            return RedirectToAction("Dashboard");
        }


        public ActionResult HistoryPartial(int? page)
        {
            long userId = 0;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }

            List<Case> myList = new List<Case>();
            List<Case> myList2 = new List<Case>();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            var result = empCaseRepository.GetBy(ec => ec.EmployeeID == userId);

            int pageSize = 2;
            int pageNumber = (page ?? 1);
            int pageCount = 1;

            if (result != null)
            {
                foreach (Employee_Case ec in result)
                {
                    Case temp = caseRepository.GetBy(c => c.CaseID == ec.CaseID).FirstOrDefault();
                    if (temp.StatusID == 1)
                    {
                        // Solved
                        myList.Add(temp);
                    }
                }

                pageCount = (int)Math.Ceiling((double)myList.Count() / pageSize);
                myList2 = (myList.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToList();

            }
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageCount = pageCount;

            return PartialView("HistoryPartial", myList2);
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

            var list = NotificationComponent.SharedInstance.GetNotifications(notificationRegisterTime, userId);
            //update session here for get only new added contacts (notification)
            Session["LastUpdate"] = DateTime.Now;
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // Return New Cases in a list
        public ActionResult GetNewList() {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];

            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 2; }
            List<Case> myList = new List<Case>();
            int pageSize = 2;
            int pageNumber = 1;
            int pageCount = 1;
            ViewBag.TableName = "Open Cases";
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();

            Dashboard model = new Dashboard();
            Employee currentEmp = empRepository.GetBy(emp => emp.EmployeeID == userId).FirstOrDefault();

            model.New = caseRepository.GetBy(c => c.CategoryID == currentEmp.CategoryID && c.StatusID == 4).ToList();

            ////////////////////////////////////////////////////////////

            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            var result = empCaseRepository.GetBy(ec => ec.EmployeeID == userId);

            if (result != null)
            {
                foreach (Employee_Case ec in result)
                {
                    if (ec.IsDeleted != false)
                    {
                        Case temp = caseRepository.GetBy(c => c.CaseID == ec.CaseID).FirstOrDefault();
                        if (temp.StatusID == 1)
                        {
                            // Solved
                            myList.Add(temp);
                        }
                    }
                }

                pageCount = (int)Math.Ceiling((double)myList.Count() / pageSize);
                model.Solved = (myList.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToList();
            }

            model.Solved = (myList.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToList();
            /////////////////////////////////////////////////
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageCount = pageCount;

            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            User user = userRepo.GetBy(u => u.UserID == userId).FirstOrDefault();
            ViewBag.User = user;
            return View("NewCasesList", model);

        }

        // Return Cases assigned to that employee but not open yet
        public ActionResult GetAssignedList()
        {
            ViewBag.TableName = "Assigned Cases";
            return View("CasesList", GetEmpCaseList(false));
        }

        // Return Opened Cases (Escalated or Current)
        public ActionResult GetOpenList() {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 2; }
            List<Case> myList = new List<Case>();
            int pageSize = 2;
            int pageNumber = 1;
            int pageCount = 1;
            ViewBag.TableName = "Open Cases";
            Dashboard model = new Dashboard();
            model.Open = GetEmpCaseList(true);
            ////////////////////////////////////////////////////////////

            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            var result = empCaseRepository.GetBy(ec => ec.EmployeeID == userId);

            if (result != null)
            {
                foreach (Employee_Case ec in result)
                {
                    if (ec.IsDeleted != false)
                    {
                        Case temp = caseRepository.GetBy(c => c.CaseID == ec.CaseID).FirstOrDefault();
                        if (temp.StatusID == 1)
                        {
                            // Solved
                            myList.Add(temp);
                        }
                    }
                }

                pageCount = (int)Math.Ceiling((double)myList.Count() / pageSize);
                model.Solved = (myList.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToList();
            }

            model.Solved = (myList.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToList();
            /////////////////////////////////////////////////
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageCount = pageCount;

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];

            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            User user = userRepo.GetBy(u => u.UserID == userId).FirstOrDefault();
            ViewBag.User = user;
            return View("OpenCasesList", model);
        }

        // Given The status of the cases , return List of cases assigned to current employee with the same status
        public List<Case> GetEmpCaseList(bool flag)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 2; }
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            var result = empCaseRepository.GetBy(ec => ec.EmployeeID == userId);
            List<Case> empCases = new List<Case>();

            if (result != null)
            {
                foreach (Employee_Case ec in result)
                {
                    Case temp = caseRepository.GetBy(c => c.CaseID == ec.CaseID).FirstOrDefault();

                    if (flag == true) // Get open Cases
                    {
                        if ((temp.StatusID == 2 && ec.OpenedAt != null) || temp.StatusID == 3)
                        {
                            empCases.Add(temp);
                        }
                    }
                    else
                    {
                        if (temp.StatusID == 2 && ec.OpenedAt == null)
                        {
                            empCases.Add(temp);
                        }
                    }
                }
            }
            return empCases;
        }

        // Return Details of a Certain Case
        public ActionResult CaseDetails(long caseId)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 2; }
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Status> statusRepository = unitOfWork.GetRepoInstance<Status>();
            Case userCase = caseRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();
            userCase.Status = statusRepository.GetBy(st => st.StatusID == userCase.StatusID).FirstOrDefault();
            GenericRepository<File> fileRepository = unitOfWork.GetRepoInstance<File>();
            File userFile = fileRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();
            DateTime ServerTime = DateTime.Now;
            ViewBag.ServerTime = ServerTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            DateTime End = userCase.SubmissionDate.AddDays(3);

            ViewBag.Timer = End.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

            CaseDetailsVM caseDetailsVM = new CaseDetailsVM();
            caseDetailsVM.Case = userCase;

            if (userFile != null)
            {
                caseDetailsVM.FileID = userFile.FileID;
                caseDetailsVM.FileName = userFile.FileName;
                caseDetailsVM.Content = userFile.Content;
                caseDetailsVM.SavedFileName = userFile.SavedFileName;
            }
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            if (userCase.StatusID == 2)
            {
                Employee_Case empCase = empCaseRepository.GetBy(ec => ec.CaseID == caseId && ec.EmployeeID == userId).FirstOrDefault();
                if (empCase.OpenedAt == null)
                {
                    empCase.OpenedAt = DateTime.Now;
                    empCaseRepository.Edit(empCase);
                    unitOfWork.SaveChanges();
                }
            }
            if (userCase.StatusID == 2 || userCase.StatusID == 3)
            {
                Employee_Case empCase = empCaseRepository.GetBy(ec => ec.CaseID == caseId && ec.EmployeeID == userId).FirstOrDefault();
                if (empCase.OpenedAt != null)
                {
                    DateTime timer2 = (DateTime)empCase.OpenedAt;
                    ViewBag.Timer2 = timer2.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                }
            }
            caseDetailsVM.MessagesModel = new MessagesVM();
            caseDetailsVM.MessagesModel.Messages = new List<Message>();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            var result = messageRepository.GetBy(m => m.CaseID == caseId && (m.ReceiverID == userId || m.SenderID == userId)).OrderByDescending(m => m.SubmissionDate);
            if (result != null)
            {
                foreach (Message message in result)
                {
                    caseDetailsVM.MessagesModel.Messages.Add(message);
                }
            }
            int page = 1;
            int pageSize = 3;
            int pageNumber = 1;
            caseDetailsVM.MessagesModel.Messages.ToPagedList(pageNumber, pageSize);
            return View(caseDetailsVM);
        }

        public FileResult DownloadFile(long id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<File> fileRepository = unitOfWork.GetRepoInstance<File>();
            File userFile = fileRepository.GetBy(c => c.FileID == id).FirstOrDefault();

            return File(userFile.Content, userFile.FileType, userFile.FileName);
        }

        // Save Message
        public ActionResult SendMessage(Message message)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee_Case> emp_caseRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            Employee_Case result = emp_caseRepository.GetBy(ec => ec.CaseID == message.CaseID).LastOrDefault();

            if (result != null)
            {
                message.SenderID = result.EmployeeID;
                message.SubmissionDate = DateTime.Now;
                messageRepository.Add(message);
            }
            unitOfWork.SaveChanges();
            return RedirectToAction("CaseDetails", new { caseId = message.CaseID });
        }

        // Escalate Case
        public ActionResult EscalateCase(long caseId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();

            Case updatedCase = caseRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();
            updatedCase.StatusID = 3;
            caseRepository.Edit(updatedCase);
            unitOfWork.SaveChanges();

            return RedirectToAction("CaseDetails", new { caseId = caseId });
        }

        // Close Case
        public ActionResult CloseCase(long caseId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Message> msgRepo = unitOfWork.GetRepoInstance<Message>();
            List<Message> caseMessges = msgRepo.GetBy(cm => cm.CaseID == caseId).ToList();

            if (caseMessges.Count != 0)
            {
                Case updatedCase = caseRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();
                updatedCase.StatusID = 1;
                updatedCase.ClosedAt = DateTime.Now;
                caseRepository.Edit(updatedCase);
                unitOfWork.SaveChanges();
            }
            else
            {
                TempData["CloseCaseError"] = "This Case is has no messages and must be Solved before you can close it";
            }

            return RedirectToAction("CaseDetails", new { caseId = caseId });
        }

        // Take The new Case
        public ActionResult TakeNewCase(long caseId)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 2; }
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            Employee_Case newRecord = new Employee_Case()
            {
                EmployeeID = userId,
                CaseID = caseId
            };
            empCaseRepository.Add(newRecord);

            Case updatedCase = caseRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();
            updatedCase.StatusID = 2;
            caseRepository.Edit(updatedCase);
            unitOfWork.SaveChanges();

            return RedirectToAction("CaseDetails", new { caseId = caseId });
        }

        //Messages
        public ActionResult Messages(long caseId, int? page)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 1; }
            MessagesVM model = new MessagesVM();
            model.Messages = new List<Message>();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            var result = messageRepository.GetBy(m => m.CaseID == caseId && (m.ReceiverID == userId || m.SenderID == userId)).OrderByDescending(m => m.SubmissionDate);
            if (result != null)
            {
                foreach (Message message in result)
                {
                    model.Messages.Add(message);
                }
            }
            page = 1;
            int pageSize = 3;
            int pageNumber = (page ?? 1);
            model.Messages.ToPagedList(pageNumber, pageSize);
            return View(model);
        }

        public ActionResult ShowMessage(long messageId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            Message message = messageRepository.GetBy(m => m.MessageID == messageId).FirstOrDefault();
            return PartialView("ShowMessage", message);
        }
    }
}