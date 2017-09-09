using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Repositories;
using BizSparkSupport.MVC.Models;
using System.Data.Entity;
using BizSparkSupport.Security;
using System.Web.Routing;
using BizSparkSupport.MVC.Areas.Admin.ViewModels;
using BizSparkSupport.MVC.Filters;

namespace BizSparkSupport.MVC.Areas.Admin.Controllers
{
    [LoggedInAdminFilter]
    public class AdminController : Controller
    {
        GenericUnitOfWork unitOfWork;

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Requests()
        {
            //filter
            User Admin = Session["USER"] as User;
            if (Admin == null)
            {
                return HttpNotFound();
            }
            else
            {
                unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                GenericRepository<Employee> empRepo = unitOfWork.GetRepoInstance<Employee>();
                Employee Requester = empRepo.GetBy(r => r.EmployeeID == Admin.UserID).FirstOrDefault();
                if (Requester.RoleID != 1)
                {
                    return HttpNotFound();
                }
                else
                {
                    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                    GenericRepository<RequestCode> reqRepo = unitOfWork.GetRepoInstance<RequestCode>();
                    ViewBag.error = null;
                    List<RequestCode> requsests = reqRepo.GetBy(r => r.IsDeleted == false).ToList();

                    return View("Requests", requsests);
                }
            }
        }

        public ActionResult SendCode(int id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<RequestCode> reqRepo = unitOfWork.GetRepoInstance<RequestCode>();
            GenericRepository<Code> codeRepo = unitOfWork.GetRepoInstance<Code>();
            RequestCode request = reqRepo.GetBy(r => r.RequestID == id).FirstOrDefault();



            Code sendingCode = codeRepo.GetBy(c => c.UsedBefore == false && c.Sent == false).FirstOrDefault();
            if (sendingCode == null)
            {
                ViewBag.error = "All codes are used or sent but not used";
                List<RequestCode> requsests = reqRepo.GetBy(r => r.IsDeleted == false).ToList();
                return View("~/Areas/Admin/Views/Admin/Requests.cshtml", requsests);
            }
            else
            {
                sendingCode.Sent = true;
                codeRepo.Edit(sendingCode);
                request.IsDeleted = true;
                reqRepo.Edit(request);

                unitOfWork.SaveChanges();


                SendEmail.send(request.Email, sendingCode.ExpireAt.ToString(), this.Request, sendingCode.CodeValue, null, "~/email.html");
                return RedirectToAction("Requests");
            }
        }

        public ActionResult Ignore(int id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<RequestCode> reqRepo = unitOfWork.GetRepoInstance<RequestCode>();

            RequestCode request = reqRepo.GetBy(r => r.RequestID == id).FirstOrDefault();
            request.IsDeleted = true;

            reqRepo.Edit(request);
            unitOfWork.SaveChanges();
            return RedirectToAction("Requests");
        }

        [HttpGet]
        public ActionResult Codes()
        {
            User Admin = Session["USER"] as User;
            if (Admin == null)
            {
                return HttpNotFound();
            }
            else
            {


                unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                GenericRepository<Employee> empRepo = unitOfWork.GetRepoInstance<Employee>();
                Employee Requester = empRepo.GetBy(r => r.EmployeeID == Admin.UserID).FirstOrDefault();
                if (Requester.RoleID != 1)
                {
                    return HttpNotFound();
                }

                else
                {
                    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                    GenericRepository<Code> codeRepo = unitOfWork.GetRepoInstance<Code>();
                    ViewBag.Total = codeRepo.GetAll().Count();
                    ViewBag.Used = codeRepo.GetBy(r => r.UsedBefore == true).Count();
                    List<Code> codes = codeRepo.GetAll().ToList();
                    return View("Codes", codes);
                }
            }
        }

        public ActionResult GenerateCode()
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Code> codeRepo = unitOfWork.GetRepoInstance<Code>();

            int number = Int32.Parse(Request.Form["Number"]);


            for (int i = 0; i < number; i++)
            {
                Code c = new Code()
                {
                    CodeValue = Guid.NewGuid().ToString("N").Substring(0, 12),
                    UsedBefore = false,
                    Sent = false,
                    ExpireAt = DateTime.Now.AddDays(60)
                };
                codeRepo.Add(c);
                unitOfWork.SaveChanges();
            }
            return RedirectToAction("Codes");

        }

        [HttpGet]
        public ActionResult Dashboard()
        {
            User Admin = Session["USER"] as User;
            if (Admin == null)
            {
                return HttpNotFound();
            }
            else
            {


                unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                GenericRepository<Employee> emp2Repo = unitOfWork.GetRepoInstance<Employee>();
                Employee Requester = emp2Repo.GetBy(r => r.EmployeeID == Admin.UserID).FirstOrDefault();
                if (Requester.RoleID != 1)
                {
                    return HttpNotFound();
                }

                else
                {
                    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                    GenericRepository<Category> cateRepo = unitOfWork.GetRepoInstance<Category>();
                    GenericRepository<Role> rolesRepo = unitOfWork.GetRepoInstance<Role>();
                    //GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();

                    GenericRepository<Employee> empRepo = unitOfWork.GetRepoInstance<Employee>();
                    List<Employee> empo = empRepo.GetBy(z => z.User.Active == true).ToList();
                    //  User[] userEmp = new User[empo.Count()];
                    ViewBag.categories = cateRepo.GetAll().ToList();
                    ViewBag.roles = rolesRepo.GetAll().ToList();
                    return View("Dashboard", empo);
                }

            }
        }

        public ActionResult NewCategory()
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Category> cateRepo = unitOfWork.GetRepoInstance<Category>();
            String Name = this.Request.Form["CategoryName"];
            String Description = this.Request.Form["Description"];

            Category c = new Category()
            {
                CategoryName = Name,
                CategoryDescription = Description
            };
            cateRepo.Add(c);
            unitOfWork.SaveChanges();
            return RedirectToAction("Dashboard");

        }

        public ActionResult NewEmployee()
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepo = unitOfWork.GetRepoInstance<Employee>();
            GenericRepository<User> userRepo = unitOfWork.GetRepoInstance<User>();
            string cate = this.Request.Form["Category"];
            string role = this.Request.Form["Role"];
            string name = this.Request.Form["Username"];
            string password = this.Request.Form["Password"];
            string passHash = Cryptography.GetRandomKey(64);
            User emp = new User()
            {
                IsVerified = true,
                Active = true,
                CreatedAt = DateTime.Now,
                Hash = passHash,
                Password = Cryptography.Encrypt(password, passHash)
            };
            userRepo.Add(emp);
            Employee empc = new Employee()
            {
                EmployeeID = emp.UserID,
                RoleID = Int32.Parse(role),
                UserName = name,
                CategoryID = Int32.Parse(cate)

            };
            empRepo.Add(empc);
            unitOfWork.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        public ActionResult ResetPasword(string email)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> userRepository = unitOfWork.GetRepoInstance<User>();

            User user = userRepository.GetBy(u => u.Email == email).FirstOrDefault();
            // string CodeValue = Guid.NewGuid().ToString("N").Substring(0, 8);
            Random rnd = new Random();
            int CodeValue = rnd.Next(1000, 100000000);
            user.Password = Cryptography.Encrypt(CodeValue.ToString(), user.Hash);
            userRepository.Edit(user);
            unitOfWork.SaveChanges();
            SendEmail.send(user.Email, user.FirstName, this.Request, user.VerificationToken, CodeValue, "~/new 7.html");
            return RedirectToAction("Dashboard");


        }

        public ActionResult ViewEmployee(int id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            GenericRepository<Category> catRepository = unitOfWork.GetRepoInstance<Category>();
            GenericRepository<Role> roleRepository = unitOfWork.GetRepoInstance<Role>();
            GenericRepository<Employee_Case> empcaRepository = unitOfWork.GetRepoInstance<Employee_Case>();
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            Employee emp = empRepository.GetBy(s => s.EmployeeID == id).FirstOrDefault();
            Category cat = catRepository.GetBy(c => c.CategoryID == emp.CategoryID).FirstOrDefault();
            Role rol = roleRepository.GetBy(r => r.RoleID == emp.RoleID).FirstOrDefault();
            List<Employee_Case> empc = empcaRepository.GetBy(c => c.EmployeeID == id).ToList();
            int esclated = 0;
            int solving = 0;
            int solved = 0;
            foreach (var item in empc)
            {
                Case c = caseRepository.GetBy(x => x.CaseID == item.CaseID).FirstOrDefault();
                if (c != null)
                {
                    if (c.StatusID == 1)
                    {
                        solved++;
                    }
                    else if (c.StatusID == 2)
                    {
                        solving++;
                    }
                    else if (c.StatusID == 3)
                    {
                        esclated++;
                    }
                }
            }
            int total = empcaRepository.GetBy(c => c.EmployeeID == id).Count();
            if (cat != null)
            {
                ViewEmployee empl = new ViewEmployee()
                {
                    UserID = emp.EmployeeID,
                    UserName = emp.UserName,
                    FirstName = emp.User.FirstName,
                    LastName = emp.User.LastName,
                    Mobile = emp.User.Mobile,
                    Email = emp.User.Email,
                    CategoryName = cat.CategoryName,
                    TotalCases = total,
                    pendingCases = solving,
                    SolvedCases = solved,
                    esclatedCases = esclated,
                    RoleName = rol.RoleName
                };
                ViewBag.categories = catRepository.GetAll().ToList();
                ViewBag.roles = roleRepository.GetAll().ToList();
                return View("EmployeeProfile", empl);
            }
            else
            {
                ViewEmployee empl = new ViewEmployee()
                {
                    UserID = emp.EmployeeID,
                    UserName = emp.UserName,
                    FirstName = emp.User.FirstName,
                    LastName = emp.User.LastName,
                    Mobile = emp.User.Mobile,
                    Email = emp.User.Email,
                    CategoryName = " ",
                    TotalCases = total,
                    pendingCases = solving,
                    SolvedCases = solved,
                    esclatedCases = esclated,
                    RoleName = rol.RoleName
                };
                ViewBag.categories = catRepository.GetAll().ToList();
                ViewBag.roles = roleRepository.GetAll().ToList();
                return View("EmployeeProfile", empl);

            }

        }

        public ActionResult StartUpCases(long sID)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> cRepo = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Startup> sRepo = unitOfWork.GetRepoInstance<Startup>();
            Startup startup = sRepo.GetBy(su => su.CompanyID == sID).FirstOrDefault();
            //  List<Case> l = new List<Case> ();
            List<Case> s = cRepo.GetBy(a => a.StartupID == sID).ToList();
            ViewBag.StartUpName = startup.CompanyName;
            return View(s);
        }

        public ActionResult ChangeCategory()
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            string id = this.Request.Form["employeeid"];
            int eid = Int32.Parse(id);
            string cate = this.Request.Form["Category"];
            Employee emp = empRepository.GetBy(u => u.EmployeeID == eid).FirstOrDefault();
            emp.CategoryID = Int32.Parse(cate);
            empRepository.Edit(emp);
            unitOfWork.SaveChanges();
            return RedirectToAction("ViewEmployee", new RouteValueDictionary(
                  new { controller = "Admin", action = "ViewEmployee", id = emp.EmployeeID }));
        }

        public ActionResult ChangeRole()
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            string id = this.Request.Form["employeeid"];
            int eid = Int32.Parse(id);
            string role = this.Request.Form["Role"];
            Employee emp = empRepository.GetBy(u => u.EmployeeID == eid).FirstOrDefault();
            emp.RoleID = Int32.Parse(role);
            empRepository.Edit(emp);
            unitOfWork.SaveChanges();
            return RedirectToAction("ViewEmployee", new RouteValueDictionary(
                  new { controller = "Admin", action = "ViewEmployee", id = emp.EmployeeID }));
        }

        public ActionResult DeleteUser(int id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            Employee emp = empRepository.GetBy(u => u.EmployeeID == id).FirstOrDefault();
            //empRepository.Delete(emp);
            emp.User.Active = false;
            empRepository.Edit(emp);
            unitOfWork.SaveChanges();
            return RedirectToAction("Dashboard");

        }

        [HttpGet]
        public ActionResult Categories()
        {
            User Admin = Session["USER"] as User;
            if (Admin == null)
            {
                return HttpNotFound();
            }
            else
            {


                unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                GenericRepository<Employee> empRepo = unitOfWork.GetRepoInstance<Employee>();
                Employee Requester = empRepo.GetBy(r => r.EmployeeID == Admin.UserID).FirstOrDefault();
                if (Requester.RoleID != 1)
                {
                    return HttpNotFound();
                }

                else
                {
                    unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
                    GenericRepository<Category> catRepository = unitOfWork.GetRepoInstance<Category>();
                    GenericRepository<Employee> adminRepository = unitOfWork.GetRepoInstance<Employee>();
                    GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();

                    List<Category> All = catRepository.GetAll().ToList();
                    List<CatgeroyView> Al = new List<CatgeroyView>();
                    foreach (var item in All)
                    {
                        Employee e = adminRepository.GetBy(c => c.CategoryID == item.CategoryID && c.RoleID == 2).FirstOrDefault();
                        int total = caseRepository.GetBy(c => c.CategoryID == item.CategoryID).Count();
                        if (e != null)
                        {
                            Al.Add(new CatgeroyView()
                            {
                                CategoryName = item.CategoryName,
                                CategoryID = item.CategoryID,
                                CategoryAdmin = e.UserName,
                                CasesNumber = total
                            });
                        }
                        else
                        {
                            Al.Add(new CatgeroyView()
                            {
                                CategoryName = item.CategoryName,
                                CategoryID = item.CategoryID,
                                CategoryAdmin = "No Admin",
                                CasesNumber = total
                            });

                        }
                    }
                    return View("Categories", Al);
                }
            }
        }

        public ActionResult CategoryDetail(int id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Category> catRepository = unitOfWork.GetRepoInstance<Category>();
            GenericRepository<Employee> adminRepository = unitOfWork.GetRepoInstance<Employee>();
            Category cd = catRepository.GetBy(c => c.CategoryID == id).FirstOrDefault();
            Employee e = adminRepository.GetBy(c => c.CategoryID == id && c.RoleID == 2).FirstOrDefault();
            if (e != null)
            {
                CatgeroyView catDetail = new CatgeroyView()
                {
                    CategoryID = cd.CategoryID,
                    CategoryAdmin = e.UserName,
                    CategoryDescription = cd.CategoryDescription,
                    CategoryName = cd.CategoryName

                };
                return View("CategoryDetail", catDetail);
            }
            else
            {
                CatgeroyView catDetail = new CatgeroyView()
                {
                    CategoryID = cd.CategoryID,
                    CategoryAdmin = "No Admin",
                    CategoryDescription = cd.CategoryDescription,
                    CategoryName = cd.CategoryName

                };
                return View("CategoryDetail", catDetail);
            }

        }

        public PartialViewResult CategoryEmployee(int id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            List<Employee> empcat = empRepository.GetBy(c => c.CategoryID == id).OrderBy(c => c.RoleID).ToList();
            return PartialView("_LoadView", empcat);
        }

        public ActionResult EditCategory(CatgeroyView cat)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Category> catRepository = unitOfWork.GetRepoInstance<Category>();
            Category cd = catRepository.GetBy(c => c.CategoryID == cat.CategoryID).FirstOrDefault();
            cd.CategoryName = cat.CategoryName;
            cd.CategoryDescription = cat.CategoryDescription;
            catRepository.Edit(cd);
            unitOfWork.SaveChanges();
            return RedirectToAction("CategoryDetail", new RouteValueDictionary(
                              new { controller = "Admin", action = "CategoryDetail", id = cat.CategoryID }));
        }

        public PartialViewResult CategoryCases(int id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Case> caseRepository = unitOfWork.GetRepoInstance<Case>();
            GenericRepository<Status> statRepository = unitOfWork.GetRepoInstance<Status>();
            GenericRepository<Startup> startRepository = unitOfWork.GetRepoInstance<Startup>();
            List<Case> catCases = caseRepository.GetBy(c => c.CategoryID == id).ToList();
            List<CategoryCases> casesReturned = new List<CategoryCases>();

            foreach (var item in catCases)
            {
                Status caseStatus = statRepository.GetBy(s => s.StatusID == item.StatusID).FirstOrDefault();
                String StatName = caseStatus.StatusName;
                Startup companyCase = startRepository.GetBy(s => s.CompanyID == item.StartupID).FirstOrDefault();
                String startUpName = companyCase.CompanyName;
                if (item.ClosedAt != null)
                {
                    casesReturned.Add(new CategoryCases()
                    {
                        CaseID = item.CaseID,
                        CompanyName = startUpName,
                        StatusName = StatName,
                        SubmissionDate = item.SubmissionDate,
                        Subject = item.Subject,
                        ClosedAt = item.ClosedAt.ToString()
                    });

                }
                else
                {
                    casesReturned.Add(new CategoryCases()
                    {
                        CaseID = item.CaseID,
                        CompanyName = startUpName,
                        StatusName = StatName,
                        SubmissionDate = item.SubmissionDate,
                        Subject = item.Subject,
                        ClosedAt = "In Process"
                    });

                }

            }
            return PartialView("_LoadCases", casesReturned);
        }

        [HttpPost]
        public int CheckEmail(String email)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Employee> empRepository = unitOfWork.GetRepoInstance<Employee>();
            var test = empRepository.GetBy(e => e.UserName == email);

            if (test.Count() > 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public ActionResult ChangeState(long id)
        {
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<User> uRepo = unitOfWork.GetRepoInstance<User>();
            var user = uRepo.GetBy(a => a.UserID == id).First();
            user.Active = !user.Active;
            uRepo.Edit(user);
            unitOfWork.SaveChanges();
            return RedirectToAction("Startups");
        }

        public ActionResult Startups()
        {
            List<StartUpDetails> list = new List<StartUpDetails>();
            unitOfWork = (GenericUnitOfWork)Session["UNITOFWORK"];
            GenericRepository<Startup> myRepo = unitOfWork.GetRepoInstance<Startup>();
            GenericRepository<User> uRepo = unitOfWork.GetRepoInstance<User>();
            GenericRepository<Case> cRepo = unitOfWork.GetRepoInstance<Case>();

            List<Startup> allstarts = myRepo.GetAll().ToList();
            foreach (Startup s in allstarts)
            {
                StartUpDetails st = new StartUpDetails();
                st.StartUpName = s.CompanyName;
                st.telephone = s.CompanyNumber;
                st.email = uRepo.GetBy(a => a.UserID == s.CompanyID).FirstOrDefault().Email;
                st.noCases = cRepo.GetBy(a => a.StartupID == s.CompanyID).Count().ToString();
                st.Active = uRepo.GetBy(a => a.UserID == s.CompanyID).FirstOrDefault().Active;


                st.Date = uRepo.GetBy(a => a.UserID == s.CompanyID).FirstOrDefault().CreatedAt.ToShortDateString();
                st.StartUpID = s.CompanyID;
                //  st.listCases = cRepo.GetBy(a => a.StartupID == s.CompanyID).ToList();
                list.Add(st);
            }

            return View(list);
        }
    }
}