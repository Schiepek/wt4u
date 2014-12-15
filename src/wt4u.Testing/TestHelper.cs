using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wt4u.Models;
using System.Data.SqlClient;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace wt4u.Testing
{
    public class TestHelper
    {
        wt4uDBContext context;
        UserManager<ApplicationUser, int> userManager;

        public ApplicationUser TestUser1 { get; set; }
        public ApplicationUser TestUser2 { get; set; }
        public WorkingSession TestWorkingSession1 { get; set; }
        //TestWorkingSession2 has no END-Time
        public WorkingSession TestWorkingSession2 { get; set; }
        public Project TestProject1 { get; set; }
        public Project TestProject2 { get; set; }
        public ProjectBookingTime TestBooking1 { get; set; }
        public ProjectBookingTime TestBooking2 { get; set; }
        public ProjectAllocation TestAllocation1 { get; set; }
        public ProjectAllocation TestAllocation2 { get; set; }
        public ProjectAllocation TestAllocation3 { get; set; }
        public ProjectLeading TestLeading1 { get; set; }
        public ProjectLeading TestLeading2 { get; set; }

        public Break TestBreak1 { get; set; }
        public Break TestBreak2 { get; set; }
        
        public void InitializeTestData(wt4uDBContext context)
        {
            this.context = context;
            this.userManager = new UserManager<ApplicationUser, int>(new UserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(context));

            createTestUser();
            createTestProject();
            createProjectAllocation();
            createProjectLeading();
            createTestWorkingSession();
            createProjectBookings();
            createTestBreaks();
        }

        public void CleanUpTestData()
        {
            context.Breaks.Remove(TestBreak1);
            context.Breaks.Remove(TestBreak2);

            context.ProjectBookingTimes.Remove(TestBooking1);
            context.ProjectBookingTimes.Remove(TestBooking2);

            context.ProjectAllocations.Remove(TestAllocation1);
            context.ProjectAllocations.Remove(TestAllocation2);
            context.ProjectAllocations.Remove(TestAllocation3);

            context.ProjectLeadings.Remove(TestLeading1);
            context.ProjectLeadings.Remove(TestLeading2);

            userManager.Delete<ApplicationUser, int>(TestUser1);
            userManager.Delete<ApplicationUser, int>(TestUser2);
            context.Projects.Remove(TestProject1);
            context.Projects.Remove(TestProject2);

            context.SaveChanges();
        }

        private void createTestUser()
        {
            TestUser1 = new ApplicationUser();
            TestUser2 = new ApplicationUser();

            TestUser1.UserName = "TestUser1";
            TestUser2.UserName = "TestUser2";

            TestUser1.Name = "TestUser1";
            TestUser2.Name = "TestUser2";

            TestUser1.isActive = true;
            TestUser2.isActive = true;

            var result1 = userManager.Create<ApplicationUser,int>(TestUser1, "123456");
            var result2 = userManager.Create<ApplicationUser,int>(TestUser2, "123456");
            
            userManager.AddToRole<ApplicationUser, int>(userManager.FindByName(TestUser1.Name).Id, "Employer");
            userManager.AddToRole<ApplicationUser, int>(userManager.FindByName(TestUser2.Name).Id, "Employee");
        }

        private void createTestWorkingSession()
        {
            TestWorkingSession1 = new WorkingSession();
            TestWorkingSession2 = new WorkingSession();

            TestWorkingSession1.EmployeeId = TestUser1.Id;
            TestWorkingSession2.EmployeeId = TestUser2.Id;

            TestWorkingSession1.Start = new DateTime(2014, 1 , 1);
            TestWorkingSession2.Start = new DateTime(2014, 1 , 1);
            TestWorkingSession1.End = new DateTime(2015, 11 , 11);

            context.WorkingSessions.Add(TestWorkingSession1);
            context.WorkingSessions.Add(TestWorkingSession2);
            context.SaveChanges();
        }

        private void createTestProject()
        {
            TestProject1 = new Project();
            TestProject2 = new Project();

            TestProject1.Name = "TestProject1";
            TestProject2.Name = "TestProject2";

            TestProject1.StartDate = new DateTime(2014, 04, 10);
            TestProject1.EndDate = new DateTime(2015, 05, 4);
            TestProject2.StartDate = new DateTime(2014, 05, 20);
            TestProject2.EndDate = new DateTime(2016, 05, 20);

            TestProject1.isClosed = false;
            TestProject2.isClosed = false;

            context.Projects.Add(TestProject1);
            context.Projects.Add(TestProject2);
            context.SaveChanges();
        }

        private void createProjectAllocation()
        {
            TestAllocation1 = new ProjectAllocation();
            TestAllocation2 = new ProjectAllocation();
            TestAllocation3 = new ProjectAllocation();

            TestAllocation1.StartDate = new DateTime(2014, 03, 04);
            TestAllocation1.EmployeeId = TestUser1.Id;
            TestAllocation1.ProjectId = TestProject1.ProjectId;
            TestAllocation1.IsCurrentProject = false;

            TestAllocation2.StartDate = new DateTime(2014, 04, 04);
            TestAllocation2.EndDate = new DateTime(2015, 04, 05);
            TestAllocation2.EmployeeId = TestUser1.Id;
            TestAllocation2.ProjectId = TestProject2.ProjectId;
            TestAllocation2.IsCurrentProject = false;

            TestAllocation3.StartDate = new DateTime(2011, 04, 04);
            TestAllocation3.EndDate = null;
            TestAllocation3.EmployeeId = TestUser2.Id;
            TestAllocation3.ProjectId = TestProject1.ProjectId;
            TestAllocation3.IsCurrentProject = false;

            context.ProjectAllocations.Add(TestAllocation1);
            context.ProjectAllocations.Add(TestAllocation2);
            context.ProjectAllocations.Add(TestAllocation3);
            context.SaveChanges();
        }

        private void createProjectLeading()
        {
            TestLeading1 = new ProjectLeading();
            TestLeading2 = new ProjectLeading();

            TestLeading1.StartDate = new DateTime(2014, 03, 04);
            TestLeading1.EmployeeId = TestUser1.Id;
            TestLeading1.ProjectID = TestProject1.ProjectId;
            
            TestLeading2.StartDate = new DateTime(2011, 04, 04);
            TestLeading2.EmployeeId = TestUser2.Id;
            TestLeading2.ProjectID = TestProject2.ProjectId;

            context.ProjectLeadings.Add(TestLeading1);
            context.ProjectLeadings.Add(TestLeading2);
            context.SaveChanges();
        }

        private void createProjectBookings()
        {
            TestBooking1 = new ProjectBookingTime();
            TestBooking2 = new ProjectBookingTime();

            TestBooking1.Description = "TestDescription1";
            TestBooking1.ProjectId = TestProject1.ProjectId;
            TestBooking1.WorkingSessionId = TestWorkingSession1.WorkingSessionId;
            TestBooking1.Start = new DateTime(2014, 05, 03, 10, 0, 0);
            TestBooking1.End = new DateTime(2014, 05, 03, 11, 5, 0);

            TestBooking2.Description = "TestDescription2";
            TestBooking2.ProjectId = TestProject1.ProjectId;
            TestBooking2.WorkingSessionId = TestWorkingSession1.WorkingSessionId;
            TestBooking2.Start = new DateTime(2014, 05, 03, 10, 0, 0);
            TestBooking2.End = new DateTime(2014, 05, 03, 11, 0, 0);

            context.ProjectBookingTimes.Add(TestBooking1);
            context.ProjectBookingTimes.Add(TestBooking2);
            context.SaveChanges();
        }

        private void createTestBreaks()
        {
            TestBreak1 = new Break(new DateTime(2014, 03, 03, 10, 0, 0),new DateTime(2014, 03, 03, 11, 5, 0),TestWorkingSession1.WorkingSessionId);
            TestBreak2 = new Break(new DateTime(2014, 04, 03, 10, 0, 0),new DateTime(2014, 04, 03, 11, 5, 0),TestWorkingSession1.WorkingSessionId);

            context.Breaks.Add(TestBreak1);
            context.Breaks.Add(TestBreak2);
            context.SaveChanges();
        }
        
    }
}
