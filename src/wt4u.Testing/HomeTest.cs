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
    public class HomeTest
    {
        private HomeAccess access;
        private TestHelper helper;
        wt4uDBContext context;

        private HomeAccess Access
        {
            get
            {
                if (access == null)
                {
                    access = new HomeAccess("TestUser1");
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
        public void TestStartWorkingSession()
        {
            DateTime TestStart = DateTime.Now;
            Assert.IsFalse(Access.IsWorking());
            Access.Start();
            Assert.IsTrue(Access.IsWorking());
            Assert.IsTrue(Access.CurrentWorkingSession().Start > TestStart);
            Assert.IsNull(Access.CurrentWorkingSession().End);
        }

        [TestMethod]
        public void TestEndWorkingSession()
        {
            HomeAccess localAccess = new HomeAccess("TestUser2");
            Assert.IsTrue(localAccess.IsWorking());
            localAccess.End();
            Assert.IsFalse(localAccess.IsWorking());
        }

        [TestMethod]
        public void TestStartBreak()
        {
            DateTime TestTime = DateTime.Now;
            HomeAccess localAccess = new HomeAccess("TestUser2");
            Assert.IsTrue(localAccess.IsWorking());
            Assert.IsFalse(localAccess.IsInBreak());
            localAccess.StartBreak();
            Assert.IsTrue(localAccess.IsInBreak());
            Assert.IsTrue(localAccess.CurrentBreak().Start > TestTime);
            Assert.IsNull(localAccess.CurrentBreak().End);
        }

        [TestMethod]
        public void TestStartEndBreak()
        {
            DateTime TestTime = DateTime.Now;
            HomeAccess localAccess = new HomeAccess("TestUser2");
            Assert.IsTrue(localAccess.IsWorking());
            Assert.IsFalse(localAccess.IsInBreak());
            localAccess.StartBreak();
            localAccess.EndBreak();
            Assert.IsFalse(localAccess.IsInBreak());
        }

        [TestMethod]
        public void TestStartProjectBookingTime()
        {
            DateTime TestTime = DateTime.Now;
            HomeAccess localAccess = new HomeAccess("TestUser2");
            Assert.IsFalse(localAccess.IsInProjectBookingTime());
            localAccess.StartProjectBookingTime(Helper.TestProject1.ProjectId);
            Assert.IsTrue(localAccess.IsInProjectBookingTime());
            Assert.IsNull(localAccess.CurrentProjectBookingTime().End);
            Assert.IsTrue(localAccess.CurrentProjectBookingTime().Start > TestTime);
        }

        [TestMethod]
        public void TestStartProjectBookingTimeWithoutWorkingSession()
        {
            DateTime TestTime = DateTime.Now;
            HomeAccess localAccess = new HomeAccess("TestUser1");
            Assert.IsFalse(localAccess.IsInProjectBookingTime());
            localAccess.StartProjectBookingTime(Helper.TestProject1.ProjectId);
            Assert.IsFalse(localAccess.IsInProjectBookingTime());
        }

        [TestMethod]
        public void TestStartEndProjectBookingTime()
        {
            HomeAccess localAccess = new HomeAccess("TestUser1");
            Assert.IsFalse(localAccess.IsInProjectBookingTime());
            localAccess.StartProjectBookingTime(Helper.TestProject1.ProjectId, false);
            localAccess.EndProjectBookingTime("TestDescription", false);
            Assert.IsFalse(localAccess.IsInProjectBookingTime());
        }        
    }
}
