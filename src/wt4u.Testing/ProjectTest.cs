using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using wt4u.BusinessLogic;
using wt4u.Models;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace wt4u.Testing
{
    [TestClass]
    public class ProjectTest
    {
        private ProjectAccess access;
        private TestHelper helper;
        wt4uDBContext context;

        private ProjectAccess Access
        {
            get
            {
                if (access == null)
                {
                    access = new ProjectAccess("TestUser1");
                }
                return access;
            }
            set { }
        }

        private TestHelper Helper
        {
            get
            {
                if (helper == null)
                {
                    helper = new TestHelper();
                }
                return helper;
            }
        }

        private wt4uDBContext Context
        {
            get
            {
                if (context == null)
                {
                    context = new wt4uDBContext();
                }
                return context;
            }
        }

        [TestInitialize]
        public void InitializeTestData()
        {
            Helper.InitializeTestData(new wt4uDBContext());
        }

        [TestCleanup]
        public void CleanUpTestData()
        {
            Helper.CleanUpTestData();
        }


        [TestMethod]
        public void TestGetProject()
        {
            Project p1 = helper.TestProject1;
            Assert.IsNotNull(p1.ProjectId);
            int id = p1.ProjectId;

            Project p2 = Access.GetProject(id);
            Assert.AreEqual(p1.Name, p2.Name);
            Assert.AreEqual(p1.StartDate, p2.StartDate);
            Assert.AreEqual(p1.EndDate, p2.EndDate);

            Project p3 = Access.GetProject(null);
            Assert.IsNull(p3);
        }

        [TestMethod]
        public void TestCreateAndDeleteProject()
        {
            Project p1 = new Project();
            p1.Name = "TestingProject";
            p1.StartDate = new DateTime(2014, 04, 10);
            p1.EndDate = new DateTime(2015, 05, 03);
            Access.CreateProject(p1);
            Project p2 = Access.GetProject(p1.ProjectId);
            Assert.AreEqual(p1.Name, p2.Name);
            Assert.AreEqual(p1.StartDate, p2.StartDate);
            Assert.AreEqual(p1.EndDate, p2.EndDate);
            int id = p2.ProjectId;
            Access.DeleteProject(id);
            Assert.IsNull(Access.GetProject(id));
        }

        [TestMethod]
        public void TestEditProject()
        {

            Project p1 = helper.TestProject1;
            Assert.IsNotNull(p1.ProjectId);
            int id = p1.ProjectId;

            Project p2 = Access.GetProject(id);

            p2.Name = "EditedName";

            Access.EditProject(p2);
            Project p3 = Access.GetProject(id);
            Assert.AreEqual(p2.Name, p3.Name);
        }

        [TestMethod]
        public void TestProjectDetails()
        {
            var list1 = Access.GetProjectBookingDetails(helper.TestProject1.ProjectId);
            var list2 = Context.ProjectBookingTimes.Where(p => p.ProjectId == helper.TestProject1.ProjectId).ToList();
            Assert.AreEqual(list2.Count, list1.Count);

            var list3 = Access.GetProjectBookingDetails(null);
            Assert.IsNull(list3);
        }

        [TestMethod]
        public void TestGetAllProjects()
        {
            var list1 = Access.GetAllProjects();
            var list2 = Context.Projects.ToList();
            Assert.AreEqual(list2.Count, list1.Count);
        }

        [TestMethod]
        public void TestProjectAllocation()
        {
            var list1 = Access.GetUserProjects();

            var allocations = Context.ProjectAllocations.Where(p => p.EmployeeId == helper.TestUser1.Id && p.EndDate == null).ToList<ProjectAllocation>(); ;
            var projects = new List<Project>();
            foreach (ProjectAllocation a in allocations)
            {
                projects.Add(a.Project);
            }
            foreach (ProjectLeading pl in Context.ProjectLeadings.Where(l => l.EmployeeId == helper.TestUser1.Id && l.EndDate == null).ToList<ProjectLeading>())
            {
                if (!projects.Contains(pl.Project))
                {
                    projects.Add(pl.Project);
                }
            }


            Assert.AreEqual(projects.Count(), list1.Count());

        }

        [TestMethod]
        public void TestCurrentProjectAllocation()
        {
            ProjectAllocation Allocation1 = Access.GetCurrentProjectAllocation();
            Assert.IsNull(Allocation1);
            Assert.IsFalse(Access.HasCurrentProject());

            Access.SetCurrentProjectAllocation(helper.TestProject1.ProjectId, true);
            ProjectAllocation Allocation2 = Access.GetCurrentProjectAllocation();
            Assert.IsNotNull(Allocation2);
            Assert.IsTrue(Access.HasCurrentProject());

            Access.SetCurrentProjectAllocation(false);
            Assert.IsFalse(Access.HasCurrentProject());
        }

        [TestMethod]
        public void TestEditBooking_CorrectFlow()
        {
            ProjectBookingTime booking = Access.GetBooking(Helper.TestBooking1.BookingId);
            DateTime? newDate = Helper.TestWorkingSession1.End - TimeSpan.FromSeconds(1);
            booking.End = newDate;
            Access.EditBooking(booking, booking.ProjectId);
            Assert.AreEqual(newDate.ToString(), Access.GetBooking(Helper.TestBooking1.BookingId).End.ToString());
        }

        [TestMethod]
        public void TestEditBooking_StartAfterEnd()
        {
            ProjectBookingTime booking = Access.GetBooking(Helper.TestBooking1.BookingId);
            DateTime oldDate = booking.Start;
            booking.Start = booking.End.Value + TimeSpan.FromSeconds(1);
            Access.EditBooking(booking, booking.ProjectId);
            Assert.AreEqual(oldDate.ToString(), Access.GetBooking(Helper.TestBooking1.BookingId).Start.ToString());
        }

        [TestMethod]
        public void TestEditBooking_EndAfterWorkingSession()
        {
            ProjectBookingTime booking = Access.GetBooking(Helper.TestBooking1.BookingId);
            DateTime? oldDate = booking.End;
            booking.End = new WorkingSessionAccess("TestUser1").GetWorkingSession(booking.WorkingSessionId).End + TimeSpan.FromSeconds(1);
            Access.EditBooking(booking, booking.ProjectId);
            Assert.AreEqual(oldDate.ToString(), Access.GetBooking(Helper.TestBooking1.BookingId).End.ToString());
        }

        [TestMethod]
        public void TestDeleteBooking()
        {
            ProjectBookingTime booking = new ProjectBookingTime(Helper.TestProject1.ProjectId, "Description", DateTime.Now, null, Helper.TestWorkingSession1.WorkingSessionId);

            Context.ProjectBookingTimes.Add(booking);
            Context.SaveChanges();

            int BookingId = booking.BookingId;

            Access.DeleteBooking(BookingId, Helper.TestProject1.ProjectId);
            Assert.IsNull(Access.GetBooking(BookingId));
        }

        [TestMethod]
        public void TestCreateProjectAllocation()
        {
            var list1 = Context.ProjectAllocations.ToList();

            ProjectAllocation allocation = new ProjectAllocation();
            allocation.EmployeeId = helper.TestUser2.Id;
            allocation.ProjectId = helper.TestProject2.ProjectId;
            allocation.StartDate = new DateTime(2014, 03, 04); ;
            allocation.IsCurrentProject = false;

            Access.CreateProjectAllocation(allocation);

            var list2 = Context.ProjectAllocations.ToList();

            Assert.AreEqual(list1.Count + 1, list2.Count);
        }

        [TestMethod]
        public void TestCreateProjectLeading()
        {
            var list1 = Context.ProjectLeadings.ToList();

            ProjectLeading leading = new ProjectLeading();
            leading.EmployeeId = helper.TestUser2.Id;
            leading.ProjectID = helper.TestProject2.ProjectId;
            leading.StartDate = new DateTime(2014, 03, 04); ;

            Access.CreateProjectLeading(leading);

            var list2 = Context.ProjectLeadings.ToList();

            Assert.AreEqual(list1.Count + 1, list2.Count);
        }

        [TestMethod]
        public void TestGetFirstLastProjectBookingId()
        {
            List<ProjectBookingTime> list = Access.GetProjectBookings(helper.TestProject1);

            ProjectBookingTime booking = Access.GetFirstProjectBookingId(list);

            ProjectBookingTime b = list.Aggregate((curmin, x) => (curmin == null || x.Start < curmin.Start ? x : curmin));

            Assert.AreEqual(booking.BookingId, b.BookingId);

            booking = Access.GetLastProjectBookingId(list);

            b = list.Aggregate((curmin, x) => (curmin == null || x.Start > curmin.Start ? x : curmin));

            Assert.AreEqual(booking.BookingId, b.BookingId);
        }

        [TestMethod]
        public void TestGetProjectDetails()
        {

            var details = Access.GetProjectDetails(0);

            Assert.IsNull(details);

            details = Access.GetProjectDetails(helper.TestProject1.ProjectId);

            Assert.IsNotNull(details);
        }

        [TestMethod]
        public void TestGetUserProjectList()
        {

            var list1 = Access.GetAllUserProjects();

            var list2 = Access.GetUserProjectList();

            Assert.AreEqual(list1.Count, list2.Count);
        }


    }
}
