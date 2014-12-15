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
    public class WorkingSessionTest
    {
        private WorkingSessionAccess access;
        private TestHelper helper;
        wt4uDBContext context;

        private WorkingSessionAccess Access
        {
            get
            {
                if (access == null)
                {
                    access = new WorkingSessionAccess("TestUser1");
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
        public void TestGetAllWorkingSessions()
        {
            bool TestSession1 = false;
            bool TestSession2 = false;
            Assert.IsTrue(Access.GetAllWorkingSessionsForView().Count() > 1);
            foreach (WorkingSession w in Access.GetAllWorkingSessions())
            {
                if (Helper.TestWorkingSession1.Start.ToString() == w.Start.ToString()) TestSession1 = true;
                if (Helper.TestWorkingSession2.Start.ToString() == w.Start.ToString()) TestSession2 = true;
            }
            Assert.IsTrue(TestSession1);
            Assert.IsTrue(TestSession2);
        }

        [TestMethod]
        public void TestGetWorkingSession()
        {
            WorkingSession session = Access.GetWorkingSession(Helper.TestWorkingSession1.WorkingSessionId);
            Assert.AreEqual(session.WorkingSessionId, Helper.TestWorkingSession1.WorkingSessionId);
        }

        [TestMethod]
        public void TestDeleteWorkingSession()
        {
            ApplicationUser Employee = Context.Users.Find(Helper.TestUser1.Id);

            WorkingSession TestWorkingSession = new WorkingSession();
            TestWorkingSession.Start = DateTime.Now;
            TestWorkingSession.End = null;
            TestWorkingSession.EmployeeId = Employee.Id;

            Context.WorkingSessions.Add(TestWorkingSession);
            Context.SaveChanges();

            int WorkingSessionId = TestWorkingSession.WorkingSessionId;

            Access.Delete(WorkingSessionId);
            Assert.IsNull(Access.GetWorkingSession(WorkingSessionId));
        }

        [TestMethod]
        public void TestGetWorkingSessionProjectBookingTimes()
        {
            bool Booking1 = false;
            bool Booking2 = false;

            List<ProjectBookingTime> bookings = Access.GetWorkingSessionProjectBookingTimes(Helper.TestWorkingSession1.WorkingSessionId);
            foreach (ProjectBookingTime booking in bookings)
            {
                if (booking.BookingId == Helper.TestBooking1.BookingId) Booking1 = true;
                if (booking.BookingId == Helper.TestBooking2.BookingId) Booking2 = true;
            }
            Assert.IsTrue(Booking1);
            Assert.IsTrue(Booking2);
        }

        [TestMethod]
        public void TestGetWorkingSessionBreaks()
        {
            bool Break1 = false;
            bool Break2 = false;

            List<Break> breaks = Access.GetWorkingSessionBreaks(Helper.TestWorkingSession1.WorkingSessionId);
            foreach (Break b in breaks)
            {
                if (Helper.TestBreak1.BreakId.ToString() == b.BreakId.ToString()) Break1 = true;
                if (Helper.TestBreak2.BreakId.ToString() == b.BreakId.ToString()) Break2 = true;
            }
            Assert.IsTrue(Break1);
            Assert.IsTrue(Break2);
        }

        [TestMethod]
        public void TestEditWorkingSession_CorrectFlow()
        {            
            WorkingSession session = Access.GetWorkingSession(Helper.TestWorkingSession1.WorkingSessionId);
            DateTime newDate = new DateTime(2016, 5, 20);
            session.End = newDate;
            Access.Edit(session);
            Assert.AreEqual(newDate.ToString(), Access.GetWorkingSession(Helper.TestWorkingSession1.WorkingSessionId).End.ToString());
        }

        [TestMethod]
        public void TestEditWorkingSession_StartAfterEnd()
        {
            WorkingSession session = Access.GetWorkingSession(Helper.TestWorkingSession1.WorkingSessionId);
            DateTime oldDate = session.Start;
            session.Start = session.End.Value + TimeSpan.FromDays(1);
            Access.Edit(session);
            Assert.AreEqual(oldDate.ToString(), Access.GetWorkingSession(session.WorkingSessionId).Start.ToString());
        }

        [TestMethod]
        public void TestEditWorkingSession_DuringBreak()
        {
            WorkingSession session = Access.GetWorkingSession(Helper.TestWorkingSession1.WorkingSessionId);
            DateTime oldDate = session.Start;
            session.Start = new DateTime(2014, 03, 03, 10, 30, 0);
            Access.Edit(session);
            Assert.AreEqual(oldDate.ToString(), Access.GetWorkingSession(session.WorkingSessionId).Start.ToString());
        }

        [TestMethod]
        public void TestEditBreak_CorrectFlow()
        {
            Break b = Access.GetBreak(Helper.TestBreak1.BreakId);
            DateTime? newDate = Helper.TestWorkingSession1.End - TimeSpan.FromMinutes(1);
            b.End = newDate;
            Access.EditBreak(b);
            Assert.AreEqual(newDate.ToString(), Access.GetBreak(Helper.TestBreak1.BreakId).End.ToString());
        }

        [TestMethod]
        public void TestEditBreak_StartAfterEnd()
        {
            Break b = Access.GetBreak(Helper.TestBreak1.BreakId);
            DateTime oldDate = b.Start;
            b.Start = b.End.Value + TimeSpan.FromSeconds(1);
            Access.EditBreak(b);
            Assert.AreEqual(oldDate.ToString(), Access.GetBreak(Helper.TestBreak1.BreakId).Start.ToString());
        }

        [TestMethod]
        public void TestEditBreak_EndAfterWorkingSession()
        {
            Break b = Access.GetBreak(Helper.TestBreak1.BreakId);
            DateTime? oldDate = b.End;
            b.End = Access.GetWorkingSession(b.WorkingSessionId).End + TimeSpan.FromSeconds(1);
            Access.EditBreak(b);
            Assert.AreEqual(oldDate.ToString(), Access.GetBreak(Helper.TestBreak1.BreakId).End.ToString());
        }

        [TestMethod]
        public void TestDeleteBreak()
        {
            WorkingSession session = Access.GetWorkingSession(Helper.TestWorkingSession1.WorkingSessionId);

            Break b = new Break(DateTime.Now, null, session.WorkingSessionId);

            Context.Breaks.Add(b);
            Context.SaveChanges();

            int BreakId = b.BreakId;

            Access.DeleteBreak(BreakId);
            Assert.IsNull(Access.GetBreak(BreakId));
        }
    }
}
