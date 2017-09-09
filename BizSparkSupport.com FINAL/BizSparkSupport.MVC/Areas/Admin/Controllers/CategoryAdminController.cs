using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Repositories;
using BizSparkSupport.MVC.Areas.Admin.ViewModels;
using BizSparkSupport.MVC.Filters;
using PagedList;

namespace BizSparkSupport.MVC.Areas.Admin.Controllers
{
    [LoggedInAdminFilter]
    public class CategoryAdminController : Controller
    {
        GenericUnitOfWork unitOfWork;
        // Return New Cases in a list
        public ActionResult CategoryAdminDashboard() {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }

            CatAdminDashboardVM model = new CatAdminDashboardVM();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            Employee currentEmp = empRepository.GetBy(emp => emp.EmployeeID == userId).FirstOrDefault();

            //Cases
            model.New = new List<Case>();
            model.AssignedOpen = new List<Case>();
            model.AssignedNotOpen = new List<Case>();
            model.Escalated = new List<Case>();
            //History
            model.Solved = new List<Case>();

            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            var result = caseRepository.GetBy(c => c.CategoryID == currentEmp.CategoryID);
            List<Case> empCases = new List<Case>();

            if (result != null)
            {
                foreach (Case ec in result)
                {
                    if (ec.StatusID == 1)
                    {
                        // Solved
                        model.Solved.Add(ec);
                    }
                    else if (ec.StatusID == 2)
                    {
                        var assignedCase = empCaseRepository.GetBy(e => e.CaseID == ec.CaseID).FirstOrDefault();
                        if (assignedCase != null)
                        {
                            if (assignedCase.OpenedAt == null)
                            {
                                //Not Opened yet
                                model.AssignedNotOpen.Add(ec);
                            }
                            else
                            {
                                //Opened (Current)
                                model.AssignedOpen.Add(ec);
                            }
                        }

                    }
                    else if (ec.StatusID == 3)
                    {
                        //Escalated
                        model.Escalated.Add(ec);
                    }
                    else if (ec.StatusID == 4)
                    {
                        //New Cases
                        model.New.Add(ec);
                    }
                }
            }
            return View(model);
        }

        public ActionResult GetNewList()
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            Employee currentEmp = empRepository.GetBy(emp => emp.EmployeeID == userId).FirstOrDefault();

            ViewBag.TableName = "New Cases";

            List<Case> newList = caseRepository.GetBy(c => c.StatusID == 4 && c.CategoryID == currentEmp.CategoryID).ToList();
            return View("CategoryAdminCasesList", newList);
        }

        public ActionResult GetAssignedList() {
            ViewBag.TableName = "Assigned Cases";
            ////////////////////////////////////////////
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }

            CatAdminDashboardVM model = new CatAdminDashboardVM();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            Employee currentEmp = empRepository.GetBy(emp => emp.EmployeeID == userId).FirstOrDefault();

            //Cases
            model.New = new List<Case>();
            model.AssignedOpen = new List<Case>();
            model.AssignedNotOpen = new List<Case>();
            model.Escalated = new List<Case>();
            //History
            model.Solved = new List<Case>();

            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            var result = caseRepository.GetBy(c => c.CategoryID == currentEmp.CategoryID);
            List<Case> empCases = new List<Case>();

            if (result != null)
            {
                foreach (Case ec in result)
                {
                    if (ec.StatusID == 1)
                    {
                        // Solved
                        model.Solved.Add(ec);
                    }
                    else if (ec.StatusID == 2)
                    {
                        var assignedCase = empCaseRepository.GetBy(e => e.CaseID == ec.CaseID).FirstOrDefault();
                        if (assignedCase != null)
                        {
                            if (assignedCase.OpenedAt == null)
                            {
                                //Not Opened yet
                                model.AssignedNotOpen.Add(ec);
                            }
                            else
                            {
                                //Opened (Current)
                                model.AssignedOpen.Add(ec);
                            }
                        }

                    }
                    else if (ec.StatusID == 3)
                    {
                        //Escalated
                        model.Escalated.Add(ec);
                    }
                    else if (ec.StatusID == 4)
                    {
                        //New Cases
                        model.New.Add(ec);
                    }
                }
            }

            //////////////////////////////////////////////
            return View("CategoryAdminAssignedCasesList", model);
        }

        // Return Opened Cases (Escalated or Current)
        public ActionResult GetOpenList() {
            ViewBag.TableName = "Open Cases";
            ////////////////////////////////////////////////
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }

            CatAdminDashboardVM model = new CatAdminDashboardVM();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            Employee currentEmp = empRepository.GetBy(emp => emp.EmployeeID == userId).FirstOrDefault();

            //Cases
            model.New = new List<Case>();
            model.AssignedOpen = new List<Case>();
            model.AssignedNotOpen = new List<Case>();
            model.Escalated = new List<Case>();
            //History
            model.Solved = new List<Case>();

            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            var result = caseRepository.GetBy(c => c.CategoryID == currentEmp.CategoryID);
            List<Case> empCases = new List<Case>();

            if (result != null)
            {
                foreach (Case ec in result)
                {
                    if (ec.StatusID == 1)
                    {
                        // Solved
                        model.Solved.Add(ec);
                    }
                    else if (ec.StatusID == 2)
                    {
                        var assignedCase = empCaseRepository.GetBy(e => e.CaseID == ec.CaseID).FirstOrDefault();
                        if (assignedCase != null)
                        {
                            if (assignedCase.OpenedAt == null)
                            {
                                //Not Opened yet
                                model.AssignedNotOpen.Add(ec);
                            }
                            else
                            {
                                //Opened (Current)
                                model.AssignedOpen.Add(ec);
                            }
                        }

                    }
                    else if (ec.StatusID == 3)
                    {
                        //Escalated
                        model.Escalated.Add(ec);
                    }
                    else if (ec.StatusID == 4)
                    {
                        //New Cases
                        model.New.Add(ec);
                    }
                }
            }

            /////////////////////////////////////////////////
            return View("CategoryAdminOpenCasesList", model);
        }

        // Given The status of the cases , return List of cases assigned to current employee with the same status
        public List<Case> GetEmpCaseList(bool flag)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else { userId = 10; }
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            Employee currentEmp = empRepository.GetBy(emp => emp.EmployeeID == userId).FirstOrDefault();

            List<Case> assignedCases = caseRepository.GetBy(ac => ac.StatusID == 2 && ac.CategoryID == currentEmp.CategoryID).ToList();
            List<Case> empCases = new List<Case>();
            foreach (Case assignedCase in assignedCases)
            {
                Employee_Case temp = empCaseRepository.GetBy(c => c.CaseID == assignedCase.CaseID).FirstOrDefault();
                if (flag == true)
                {
                    if (temp.OpenedAt != null)
                    {
                        empCases.Add(assignedCase);
                    }
                }
                else
                {
                    if (temp.OpenedAt == null)
                    {
                        empCases.Add(assignedCase);
                    }
                }
            }
            return empCases;
        }

        public ActionResult GetEscalatedList() {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }

            CatAdminDashboardVM model = new CatAdminDashboardVM();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            Employee currentEmp = empRepository.GetBy(emp => emp.EmployeeID == userId).FirstOrDefault();

            //Cases
            model.New = new List<Case>();
            model.AssignedOpen = new List<Case>();
            model.AssignedNotOpen = new List<Case>();
            model.Escalated = new List<Case>();
            //History
            model.Solved = new List<Case>();

            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Employee_Case> empCaseRepository = unitOfWork.GetRepoInstance<Employee_Case>();

            var result = caseRepository.GetBy(c => c.CategoryID == currentEmp.CategoryID);
            List<Case> empCases = new List<Case>();

            if (result != null)
            {
                foreach (Case ec in result)
                {
                    if (ec.StatusID == 1)
                    {
                        // Solved
                        model.Solved.Add(ec);
                    }
                    else if (ec.StatusID == 2)
                    {
                        var assignedCase = empCaseRepository.GetBy(e => e.CaseID == ec.CaseID).FirstOrDefault();
                        if (assignedCase != null)
                        {
                            if (assignedCase.OpenedAt == null)
                            {
                                //Not Opened yet
                                model.AssignedNotOpen.Add(ec);
                            }
                            else
                            {
                                //Opened (Current)
                                model.AssignedOpen.Add(ec);
                            }
                        }

                    }
                    else if (ec.StatusID == 3)
                    {
                        //Escalated
                        model.Escalated.Add(ec);
                    }
                    else if (ec.StatusID == 4)
                    {
                        //New Cases
                        model.New.Add(ec);
                    }
                }
            }

            ///////////////////////
            return View("CategoryAdminEscalatedCasesList", model);
        }

        // Return Details of a Certain Case
        public ActionResult CaseDetails(long caseId)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];

            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            Case userCase = caseRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();

            GenericRepository<File> fileRepository = unitOfWork.GetRepoInstance<File>();
            File userFile = fileRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();

            CaseDetailsVM caseDetailsVM = new CaseDetailsVM();
            caseDetailsVM.Case = userCase;

            if (userFile != null)
            {
                caseDetailsVM.FileID = userFile.FileID;
                caseDetailsVM.FileName = userFile.FileName;
                caseDetailsVM.Content = userFile.Content;
                caseDetailsVM.SavedFileName = userFile.SavedFileName;
            }

            //if (userCase.StatusID == 4)
            //{
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            List<Employee> allEmp = empRepository.GetBy(c => c.CategoryID == userCase.CategoryID && c.RoleID == 3).ToList();

            caseDetailsVM.AllEmployees = allEmp;
            //}

            if (userCase.StatusID != 4)
            {
                GenericRepository<Employee_Case> empCaseRepo = unitOfWork.GetRepoInstance<Employee_Case>();
                Employee_Case assignedEmp = empCaseRepo.GetBy(ec => ec.CaseID == userCase.CaseID).FirstOrDefault();
                //GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
                //Employee emp = empRepository.GetBy(e => e.EmployeeID == assignedEmp.EmployeeID).FirstOrDefault();
                GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();
                if(userCase.StatusID == 2)
                {
                    User assignedUser = userRepository.GetBy(u => u.UserID == assignedEmp.EmployeeID).FirstOrDefault();
                }
                

                List<Employee_Case> allEmpCases = empCaseRepo.GetBy(ec => ec.CaseID == userCase.CaseID).ToList();
                //List<Employee> allEmp = new List<Employee>();
                List<User> allAssignedEmps = new List<User>();
                foreach (Employee_Case empc in allEmpCases)
                {
                    //Employee temp = empRepository.GetBy(e => e.EmployeeID == empc.EmployeeID).FirstOrDefault();
                    User temp2 = userRepository.GetBy(u => u.UserID == empc.EmployeeID).FirstOrDefault();
                    //allEmp.Add(temp);
                    allAssignedEmps.Add(temp2);
                }
                caseDetailsVM.AllEmployeeCases = allEmpCases;
                caseDetailsVM.AllAssignedEmployees = allAssignedEmps;
                caseDetailsVM.MessagesModel = new MessagesVM();
                caseDetailsVM.MessagesModel.Messages = new List<Message>();

                unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

                var result = messageRepository.GetBy(m => m.CaseID == caseId).OrderByDescending(m => m.SubmissionDate);
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
            }
            
            return View("CategoryAdminCaseDetails", caseDetailsVM);
        }

        public ActionResult AssignEmployees(long empId, long caseId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee_Case> empRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            List<Employee_Case> allAssingedEmps = empRepository.GetBy(ec => ec.EmployeeID == empId && ec.CaseID == caseId).ToList();
            if(allAssingedEmps == null)
            {
                Employee_Case assignEmp = new Employee_Case();
                assignEmp.EmployeeID = empId;
                assignEmp.CaseID = caseId;
                assignEmp.AssignDate = DateTime.Now;
                empRepository.Add(assignEmp);

                GenericRepository<Case> empCase = unitOfWork.GetRepoInstance<Case>();
                Case currentCase = empCase.GetBy(c => c.CaseID == caseId).FirstOrDefault();
                currentCase.StatusID = 2;
                empCase.Edit(currentCase);
            }
            else
            {
                Employee_Case currentEmpCase = empRepository.GetBy(ec => ec.EmployeeID == empId && ec.CaseID == caseId).FirstOrDefault();
                currentEmpCase.IsDeleted = false;
                if(currentEmpCase.Case.StatusID == 3)
                {
                    currentEmpCase.Case.StatusID = 2;
                }
                empRepository.Edit(currentEmpCase);
            }

            unitOfWork.SaveChanges();
            return RedirectToAction("CaseDetails", new { caseId = caseId });
        }

        public ActionResult RemoveAssignedEmployee(long empId, long caseId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee_Case> empRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            Employee_Case empCase = empRepository.GetBy(ec => ec.CaseID == caseId && ec.EmployeeID == empId).FirstOrDefault();
            empCase.IsDeleted = true;
            empRepository.Edit(empCase);
            unitOfWork.SaveChanges();

            return RedirectToAction("CaseDetails", new { caseId = caseId });
        }

        public ActionResult CloseCase(long caseId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Message> msgRepo = unitOfWork.GetRepoInstance<Message>();

            List<Message> caseMessges = msgRepo.GetBy(cm => cm.CaseID == caseId).ToList();
            if(caseMessges.Count != 0)
            {
                Case updatedCase = caseRepository.GetBy(c => c.CaseID == caseId).FirstOrDefault();
                updatedCase.StatusID = 1;
                updatedCase.ClosedAt = DateTime.Now;
                caseRepository.Edit(updatedCase);
                unitOfWork.SaveChanges();
            }
            else
            {
                TempData["CloseCaseError"] = "This Case is New and must be Solved before you can close it";
                //ViewBag.CloseCaseError = "This Case is New and must be Solved before you can close it";
            }

            return RedirectToAction("CaseDetails", new { caseId = caseId });
        }

        public FileResult DownloadFile(long id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<File> fileRepository = unitOfWork.GetRepoInstance<File>();
            File userFile = fileRepository.GetBy(c => c.FileID == id).FirstOrDefault();

            return File(userFile.Content, userFile.FileType, userFile.FileName);
        }

        public ActionResult SendMessage(Message message)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }

            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            message.SenderID = userId;
            message.SubmissionDate = DateTime.Now;
            messageRepository.Add(message);
            unitOfWork.SaveChanges();
            return RedirectToAction("CaseDetails", new { caseId = message.CaseID });
        }

        public ActionResult AllEmployees()
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }

            GenericRepository<Employee> empRepo = unitOfWork.GetRepoInstance<Employee>();
            GenericRepository<Employee_Case> casesRepo = unitOfWork.GetRepoInstance<Employee_Case>();
            Employee currentEmp = empRepo.GetBy(ce => ce.EmployeeID == userId).FirstOrDefault();
            List<Employee> allEmployees = empRepo.GetBy(ae => ae.RoleID == 3 && ae.CategoryID == currentEmp.CategoryID).ToList();

            List<EmployeeCasesVM> empCasesVM = new List<EmployeeCasesVM>();
            if(allEmployees != null)
            {
                foreach (Employee emp in allEmployees)
                {
                    int totalCases = casesRepo.GetBy(cr => cr.EmployeeID == emp.EmployeeID).Count();
                    if (totalCases != 0)
                    {
                        empCasesVM.Add(new EmployeeCasesVM()
                        {
                            Employee = emp,
                            CasesNumber = totalCases
                        });
                    }
                    else
                    {
                        empCasesVM.Add(new EmployeeCasesVM()
                        {
                            Employee = emp,
                            CasesNumber = 0
                        });
                    }
                }
            }
            
            return View(empCasesVM);
        }

        public ActionResult EmployeeCases(long empId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee_Case> empCasesRepo = unitOfWork.GetRepoInstance<Employee_Case>();
            GenericRepository<Employee> empRepo = unitOfWork.GetRepoInstance<Employee>();
            GenericRepository<Case> casesRepo = unitOfWork.GetRepoInstance<Case>();
            Employee employee = empRepo.GetBy(emp => emp.EmployeeID == empId).FirstOrDefault();
            List<Employee_Case> empCases = empCasesRepo.GetBy(ec => ec.EmployeeID == empId).ToList();
            List<Case> cases = new List<Case>();
            foreach(Employee_Case empC in empCases)
            {
                Case temp = casesRepo.GetBy(c => c.CaseID == empC.CaseID).FirstOrDefault();
                cases.Add(temp);
            }
            ViewBag.EmpName = employee.UserName;
            return View(cases);
        }

        //Messages
        public ActionResult Messages(long caseId, int? page)
        {
            long userId;
            if (Session["USER"] != null)
            {
                userId = ((User)Session["USER"]).UserID;
            }
            else
            {
                return RedirectToAction("Login", "Login", new { area = "Admin" });
            }
            MessagesVM model = new MessagesVM();
            model.Messages = new List<Message>();

            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            var result = messageRepository.GetBy(m => m.CaseID == caseId).OrderByDescending(m => m.SubmissionDate);
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
            return View("CategoryAdminMessages", model);
        }

        public ActionResult ShowMessage(long messageId)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Message> messageRepository = unitOfWork.GetRepoInstance<Message>();

            Message message = messageRepository.GetBy(m => m.MessageID == messageId).FirstOrDefault();
            return PartialView("CategoryAdminShowMessage", message);
        }
    }
}