
using BizSparkSupport.DAL;
using BizSparkSupport.MVC.Hubs;
using BizSparkSupport.MVC.ViewModels;
using Microsoft.AspNet.SignalR;
using ServiceBrokerListener.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace BizSparkSupport.MVC.Notification
{
    public class NotificationComponent

    {
        static string conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
        static int notiBadbe = 0;
        static SqlDependency sql;
        static SqlDependency sql1;

        private static NotificationComponent instance;

        public static NotificationComponent SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NotificationComponent();
                }
                return instance;
            }
        }

        public long clientID { set; get; }
        public int CatID { set; get; }

        public void RegisterEmp_Notification(DateTime currentTime, long uID)
        {
            NotificationComponent.SharedInstance.clientID = uID;
            using (BizSparkSupportEntities dc = new BizSparkSupportEntities())
            {
                var row = dc.Employees.Where(a => a.EmployeeID == uID).FirstOrDefault();
                int catid = (int)row.CategoryID;
                NotificationComponent.SharedInstance.CatID = catid;
                conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
                SqlDependency.Start(conStr);

                string sqlCommand = @"SELECT [CaseID],[Subject],[StartupID] from [dbo].[Case] where [SubmissionDate] > @AddedOn AND [CategoryID] = @catID";
                string sqlCommand1 = @"SELECT [CaseID],[EmployeeID],[AssignDate] from [dbo].[Employee_Case] where [AssignDate] > @AddedOn AND [EmployeeID] = @empID";
                string sqlCommand2 = @"SELECT [MessageID],[SenderID],[ReceiverID] from [dbo].[Messages] where [SubmissionDate] > @AddedOn AND [ReceiverID] = @empID";

                //you can notice here I have added table name like this [dbo].[Contacts] with [dbo], its mendatory when you use Sql Dependency
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    SqlCommand cmd = new SqlCommand(sqlCommand, con);
                    SqlCommand cmd1 = new SqlCommand(sqlCommand1, con);
                    SqlCommand cmd2 = new SqlCommand(sqlCommand2, con);

                    cmd.Parameters.AddWithValue("@AddedOn", currentTime);
                    cmd1.Parameters.AddWithValue("@AddedOn", currentTime);
                    cmd2.Parameters.AddWithValue("@AddedOn", currentTime);

                    cmd.Parameters.AddWithValue("@catID", catid);
                    cmd1.Parameters.AddWithValue("@empID", uID);
                    cmd2.Parameters.AddWithValue("@empID", uID);


                    if (con.State != System.Data.ConnectionState.Open)
                    {
                        con.Open();
                    }
                    cmd.Notification = null;
                    cmd1.Notification = null;
                    cmd2.Notification = null;

                    sql = new SqlDependency(cmd);
                    sql.OnChange += sqlDep_OnChange;
                    /////////////////////////////////////////////
                    sql1 = new SqlDependency(cmd1);
                    sql1.OnChange += sqlDep_OnChange1;
                    //////////////////////////////////////////
                    SqlDependency sqlDep2 = new SqlDependency(cmd2);
                    sqlDep2.OnChange += sqlDep_OnChange2;
                   // we must have to execute the command here
                    using (SqlDataReader reader = cmd1.ExecuteReader())
                    {
                        // nothing need to add here now
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // nothing need to add here now
                    }
                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        // nothing need to add here now
                    }
                }
            }
        }
        public void RegisterUser_Notification(DateTime currentTime, long uID)
        {
            NotificationComponent.SharedInstance.clientID = uID;
            using (BizSparkSupportEntities dc = new BizSparkSupportEntities())
            {
                conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
                SqlDependency.Start(conStr);

                string sqlCommand2 = @"SELECT [MessageID],[SenderID],[ReceiverID] from [dbo].[Messages] where [SubmissionDate] > @AddedOn AND [ReceiverID] = @empID";

                //you can notice here I have added table name like this [dbo].[Contacts] with [dbo], its mendatory when you use Sql Dependency
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    SqlCommand cmd2 = new SqlCommand(sqlCommand2, con);

                    cmd2.Parameters.AddWithValue("@AddedOn", currentTime);

                    cmd2.Parameters.AddWithValue("@empID", uID);


                    if (con.State != System.Data.ConnectionState.Open)
                    {
                        con.Open();
                    }
                    cmd2.Notification = null;

                    SqlDependency sqlDep2 = new SqlDependency(cmd2);
                    sqlDep2.OnChange += sqlDep_OnChangeNew;
                    //we must have to execute the command here

                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        // nothing need to add here now
                    }
                }
            }
        }

        private void sqlDep_OnChangeNew(object sender, SqlNotificationEventArgs e)
        {

            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChangeNew;

                //from here we will send notification message to client
                Debug.WriteLine("Source: " + e.Source);

                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<MessagesHub>();

                notificationHub.Clients.All.notify("added2");
            }
            RegisterUser_Notification(DateTime.Now, NotificationComponent.SharedInstance.clientID);
        }

        public void reregisterCase(DateTime currentTime, long uID)
        {
            NotificationComponent.SharedInstance.clientID = uID;
            using (BizSparkSupportEntities dc = new BizSparkSupportEntities())
            {
                var row = dc.Employees.Where(a => a.EmployeeID == uID).FirstOrDefault();
                int catid = (int)row.CategoryID;
                NotificationComponent.SharedInstance.CatID = catid;
                conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
                SqlDependency.Start(conStr);

                string sqlCommand = @"SELECT [CaseID],[Subject],[StartupID] from [dbo].[Case] where [SubmissionDate] > @AddedOn AND [CategoryID] = @catID";

                //you can notice here I have added table name like this [dbo].[Contacts] with [dbo], its mendatory when you use Sql Dependency
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    SqlCommand cmd = new SqlCommand(sqlCommand, con);

                    cmd.Parameters.AddWithValue("@AddedOn", currentTime);

                    cmd.Parameters.AddWithValue("@catID", catid);

                    if (con.State != System.Data.ConnectionState.Open)
                    {
                        con.Open();
                    }
                    cmd.Notification = null;
                    //  cmd2.Notification = null;

                    sql = new SqlDependency(cmd);
                    sql.OnChange += sqlDep_OnChange;
                    //we must have to execute the command here

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // nothing need to add here now
                    }

                }
            }

        }
        public void reregisterEmpCase(DateTime currentTime, long uID)
        {


            NotificationComponent.SharedInstance.clientID = uID;
            using (BizSparkSupportEntities dc = new BizSparkSupportEntities())
            {
                var row = dc.Employees.Where(a => a.EmployeeID == uID).FirstOrDefault();
                int catid = (int)row.CategoryID;
                NotificationComponent.SharedInstance.CatID = catid;
                conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
                SqlDependency.Start(conStr);

                string sqlCommand1 = @"SELECT [CaseID],[EmployeeID],[AssignDate] from [dbo].[Employee_Case] where [AssignDate] > @AddedOn AND [EmployeeID] = @empID";

                //you can notice here I have added table name like this [dbo].[Contacts] with [dbo], its mendatory when you use Sql Dependency
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    SqlCommand cmd1 = new SqlCommand(sqlCommand1, con);

                    cmd1.Parameters.AddWithValue("@AddedOn", currentTime);

                    cmd1.Parameters.AddWithValue("@empID", uID);


                    if (con.State != System.Data.ConnectionState.Open)
                    {
                        con.Open();
                    }
                    cmd1.Notification = null;

                    sql1 = new SqlDependency(cmd1);
                    sql1.OnChange += sqlDep_OnChange1;
                    //we must have to execute the command here
                    using (SqlDataReader reader = cmd1.ExecuteReader())
                    {
                        // nothing need to add here now
                    }


                }
            }
        }
        public void reregistermsgs(DateTime currentTime, long uID)
        {

            NotificationComponent.SharedInstance.clientID = uID;
            using (BizSparkSupportEntities dc = new BizSparkSupportEntities())
            {
                var row = dc.Employees.Where(a => a.EmployeeID == uID).FirstOrDefault();
                int catid = (int)row.CategoryID;
                NotificationComponent.SharedInstance.CatID = catid;
                conStr = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
                SqlDependency.Start(conStr);

                string sqlCommand2 = @"SELECT [MessageID],[SenderID],[ReceiverID] from [dbo].[Messages] where [SubmissionDate] > @AddedOn AND [ReceiverID] = @empID";

                //you can notice here I have added table name like this [dbo].[Contacts] with [dbo], its mendatory when you use Sql Dependency
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    SqlCommand cmd2 = new SqlCommand(sqlCommand2, con);

                    cmd2.Parameters.AddWithValue("@AddedOn", currentTime);

                    cmd2.Parameters.AddWithValue("@empID", uID);


                    if (con.State != System.Data.ConnectionState.Open)
                    {
                        con.Open();
                    }
                    cmd2.Notification = null;

                    //////////////////////////////////////////
                    SqlDependency sqlDep2 = new SqlDependency(cmd2);
                    sqlDep2.OnChange += sqlDep_OnChange2;
                    //we must have to execute the command here

                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        // nothing need to add here now
                    }
                }
            }

        }
        private void sqlDep_OnChange2(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChange2;

                //from here we will send notification message to client
                Debug.WriteLine("Source: " + e.Source);

                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();

                notificationHub.Clients.All.notify("added2");
            }
            reregistermsgs(DateTime.Now, NotificationComponent.SharedInstance.clientID);
        }

        private void sqlDep_OnChange1(object sender, SqlNotificationEventArgs e)
        {
            Debug.WriteLine("employee Case change");
            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChange1;


                //from here we will send notification message to client
                Debug.WriteLine("Source: " + e.Source);

                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();

                notificationHub.Clients.All.notify("added1");

            }
            reregisterEmpCase(DateTime.Now, NotificationComponent.SharedInstance.clientID);

        }

        void sqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            Debug.WriteLine(" Case change");

            //or you can also check => if (e.Info == SqlNotificationInfo.Insert) , if you want notification only for inserted record
            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChange;


                //from here we will send notification message to client
                Debug.WriteLine("Source: " + notiBadbe++);

                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();

                notificationHub.Clients.All.notify("added");

            }
            reregisterCase(DateTime.Now, NotificationComponent.SharedInstance.clientID);

        }

        public List<CaseArrival> GetNotifications(DateTime afterDate, long uID)
        {
            List<CaseArrival> news = new List<CaseArrival>();
            using (BizSparkSupportEntities dc = new BizSparkSupportEntities())
            {

                var row = dc.Employees.Where(a => a.EmployeeID == uID).FirstOrDefault();
                int catid = (int)row.CategoryID;
                var result = dc.Cases.Where(a => a.CategoryID == catid && a.SubmissionDate > afterDate).OrderByDescending(a => a.SubmissionDate).ToList();
                foreach (Case c in result)
                {
                    CaseArrival ar = new CaseArrival();
                    ar.CaseID = c.CaseID;
                    ar.Subject = c.Subject;
                    ar.StartUpName = dc.Startups.Where(a => a.CompanyID == c.StartupID).FirstOrDefault().CompanyName;
                    ar.PriorityColor = dc.Priorities.Where(a => a.PriorityID == c.PriorityID).FirstOrDefault().PriorityColor;
                    ar.listText = "New Case Added From ( " + ar.StartUpName + " )";
                    news.Add(ar);
                }

                var result1 = dc.Employee_Case.Where(a => a.EmployeeID == uID && a.AssignDate > afterDate).OrderByDescending(a => a.AssignDate).ToList();
                foreach (Employee_Case c in result1)
                {
                    CaseArrival ar = new CaseArrival();
                    ar.CaseID = c.CaseID;

                    var cases = dc.Cases.Where(a => a.CaseID == c.CaseID).FirstOrDefault();
                    ar.Subject = cases.Subject;
                    ar.StartUpName = dc.Startups.Where(a => a.CompanyID == cases.StartupID).FirstOrDefault().CompanyName;
                    ar.PriorityColor = dc.Priorities.Where(a => a.PriorityID == cases.PriorityID).FirstOrDefault().PriorityColor;
                    ar.listText = "You Are Assigned To New Case From ( " + ar.StartUpName + " )";
                    news.Add(ar);
                }

                var result2 = dc.Messages.Where(a => a.ReceiverID == uID && a.SubmissionDate > afterDate).OrderByDescending(a => a.SubmissionDate).ToList();
                foreach (Message c in result2)
                {
                    CaseArrival ar = new CaseArrival();
                    ar.CaseID = c.CaseID;

                    var cases = dc.Cases.Where(a => a.CaseID == c.CaseID).FirstOrDefault();
                    ar.Subject = cases.Subject;
                    ar.caseMsg = c.MessageContent;
                    ar.StartUpName = dc.Startups.Where(a => a.CompanyID == cases.StartupID).FirstOrDefault().CompanyName;
                    ar.PriorityColor = dc.Priorities.Where(a => a.PriorityID == cases.PriorityID).FirstOrDefault().PriorityColor;
                    ar.listText = "New Message From ( " + ar.StartUpName + " ) About ( " + ar.Subject + " ) : <br>" + ar.caseMsg;
                    news.Add(ar);
                }


            }
            return news;
        }


        public List<CaseArrival> GetMessages(DateTime afterDate, long uID)
        {
            List<CaseArrival> news = new List<CaseArrival>();
            using (BizSparkSupportEntities dc = new BizSparkSupportEntities())
            {

                var result2 = dc.Messages.Where(a => a.ReceiverID == uID && a.SubmissionDate > afterDate).OrderByDescending(a => a.SubmissionDate).ToList();
                foreach (Message c in result2)
                {
                    CaseArrival ar = new CaseArrival();
                    ar.CaseID = c.CaseID;

                    var cases = dc.Cases.Where(a => a.CaseID == c.CaseID).FirstOrDefault();
                    ar.Subject = cases.Subject;

                    ar.StartUpName = dc.Startups.Where(a => a.CompanyID == c.ReceiverID).FirstOrDefault().CompanyName;
                    ar.PriorityColor = dc.Priorities.Where(a => a.PriorityID == cases.PriorityID).FirstOrDefault().PriorityColor;
                    ar.caseMsg = c.MessageContent;
                    ar.listText = "New Message From ( Support Team ) : <br> " + ar.caseMsg;
                    news.Add(ar);
                }


            }
            return news;
        }
    }
}