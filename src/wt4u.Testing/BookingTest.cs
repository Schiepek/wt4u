using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using wt4u.Controllers;
using wt4u.Models;

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;

namespace wt4u.Testing
{
    [TestClass]
    public class BookingTest
    {
        private wt4uDBContext context;
        private TestHelper helper;

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

        [TestInitialize]
        public void InitializeTestData()
        {
           Helper.InitializeTestData(Context);
        }

        [TestCleanup]
        public void CleanUpTestData()
        {
            Helper.CleanUpTestData();
        }

        [TestMethod]
        public void TestStartWorkingSession()
        {
            ApplicationUser Employee = Context.Users.Find(Helper.TestUser1.Id);

            WorkingSession TestWorkingSession = new WorkingSession();
            TestWorkingSession.Start = DateTime.Now;
            TestWorkingSession.End = null;
            TestWorkingSession.EmployeeId = Employee.Id;

            Context.WorkingSessions.Add(TestWorkingSession);
            Context.SaveChanges();
            Assert.AreEqual(TestWorkingSession.WorkingSessionId, context.WorkingSessions.Find(TestWorkingSession.WorkingSessionId).WorkingSessionId);
            Assert.IsNull(context.WorkingSessions.Find(TestWorkingSession.WorkingSessionId).End);

            Context.WorkingSessions.Remove(TestWorkingSession);
            Context.SaveChanges();
        }

        // Tests to end the WorkingSession2 which was created in the TestHelper
        [TestMethod]
        public void TestEndWorkingSession()
        {
            WorkingSession TestWorkingSession = Context.WorkingSessions.Find(Helper.TestWorkingSession2.WorkingSessionId);

            TestWorkingSession.End = DateTime.Now;

            Context.Entry(TestWorkingSession).State = EntityState.Modified;
            Context.SaveChanges();

            Assert.AreEqual(TestWorkingSession.WorkingSessionId, context.WorkingSessions.Find(TestWorkingSession.WorkingSessionId).WorkingSessionId);
            Assert.IsNotNull(context.WorkingSessions.Find(TestWorkingSession.WorkingSessionId).End);
        }

        [TestMethod]
        public void TestStartAndEndOnProjectBookingTime()
        {
            Project TestProject = Context.Projects.Find(Helper.TestProject1.ProjectId);
            WorkingSession TestWorkingSession = Context.WorkingSessions.Find(Helper.TestWorkingSession2.WorkingSessionId);

            ProjectBookingTime TestProjectBookingTime = new ProjectBookingTime();
            TestProjectBookingTime.Description = "TestProjectBookingTime";
            TestProjectBookingTime.Start = DateTime.Now;
            TestProjectBookingTime.WorkingSessionId = TestWorkingSession.WorkingSessionId;
            TestProjectBookingTime.ProjectId = TestProject.ProjectId;

            Context.ProjectBookingTimes.Add(TestProjectBookingTime);
            Context.SaveChanges();

            Assert.IsNotNull(Context.ProjectBookingTimes.Find(TestProjectBookingTime.BookingId).Start);
            Assert.IsNull(Context.ProjectBookingTimes.Find(TestProjectBookingTime.BookingId).End);

            TestProjectBookingTime.End = DateTime.Now;

            Context.Entry(TestProjectBookingTime).State = EntityState.Modified;
            Context.SaveChanges();

            Assert.IsNotNull(Context.ProjectBookingTimes.Find(TestProjectBookingTime.BookingId).End);

            Context.ProjectBookingTimes.Remove(TestProjectBookingTime);
            Context.SaveChanges();
        }

        [TestMethod]
        public void TestStartAndEndBreak()
        {
            WorkingSession TestWorkingSession = Context.WorkingSessions.Find(Helper.TestWorkingSession2.WorkingSessionId);

            Break TestBreak = new Break();

            TestBreak.Start = DateTime.Now;
            TestBreak.WorkingSessionId = TestWorkingSession.WorkingSessionId;

            Context.Breaks.Add(TestBreak);
            Context.SaveChanges();

            Assert.IsNotNull(Context.Breaks.Find(TestBreak.BreakId).Start);
            Assert.IsNull(Context.Breaks.Find(TestBreak.BreakId).End);

            TestBreak.End = DateTime.Now;

            Context.Entry(TestBreak).State = EntityState.Modified;
            Context.SaveChanges();

            Assert.IsNotNull(Context.Breaks.Find(TestBreak.BreakId).End);
            Assert.AreEqual(TestWorkingSession.WorkingSessionId, Context.Breaks.Find(TestBreak.BreakId).WorkingSessionId);

            Context.Breaks.Remove(TestBreak);
            Context.SaveChanges();
        }
    }
}
