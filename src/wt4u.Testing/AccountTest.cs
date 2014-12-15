using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using wt4u.Controllers;
using wt4u.Models;

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using wt4u.BusinessLogic;

namespace wt4u.Testing
{
    [TestClass]
    public class AccountTest
    {
        private wt4uDBContext context;
        private TestHelper helper;
        private AccountAccess access;

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

        private AccountAccess Access
        {
            get
            {
                if (access == null)
                {
                    access = new AccountAccess("TestUser1");
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
        public void TestCreateUser()
        {
            var access = new AccountAccess(helper.TestUser1.UserName);
            int count = context.Users.Count();
            ApplicationUser u = new ApplicationUser();
            u.UserName = "test";
            u.Name = "Name";
            u.FirstName = "Firstname";
            var result = access.createUserNotAsync(u, "123456");

            int countNew = context.Users.Count();
            Console.WriteLine(count.ToString() + " " + countNew.ToString());
            Assert.AreEqual(count, countNew - 1);

            result = access.deleteUser(u);
        }

        [TestMethod]
        public void TestEditUser()
        {
            ApplicationUser u = helper.TestUser2;
            u.City = "blabla";
            new AccountAccess(helper.TestUser1.UserName).editUser(u);
            var city = context.Users.Find(u.Id).City;
            Assert.AreEqual("blabla", city);
        }

        [TestMethod]
        public void TestGetAllUsers()
        {
            int c1 = context.Users.Count();
            int c2 = new AccountAccess(helper.TestUser1.UserName).getAllUsers().Count;
            Assert.AreEqual(c1, c2);
        }

        [TestMethod]
        public void TestSetRole()
        {
            var access = new AccountAccess(helper.TestUser1.UserName);
            ApplicationUser u = helper.TestUser2; //ist Employee (id = 2)
            Assert.AreEqual(u.Roles.First().RoleId, 2);
            access.setRole(u, "Employer");
            Assert.AreEqual(access.getUserRole(u), "Employer");
            access.setRole(u, "Employee");
        }

        [TestMethod]
        public void TestGetUserRole()
        {
            ApplicationUser u = helper.TestUser2;
            Assert.AreEqual(new AccountAccess(helper.TestUser1.UserName).getUserRole(u), "Employee");
        }
    }
}
