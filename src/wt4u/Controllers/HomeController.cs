using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using wt4u.Models;
using wt4u.BusinessLogic;
using System.Dynamic;
using System.Web.Script.Serialization;

namespace wt4u.Controllers
{
    [SecurityAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Index(String error = "")
        {
            ViewBag.ProjectID = new ProjectAccess(User.Identity.Name).GetUserProjectsToBook();
            setStatus();
            setError(error);
            return View();
        }

        public ActionResult Error(String error = "")
        {
            ViewBag.ErrorMessage = error;
            return View();
        }

        [HttpPost, ActionName("CheckInOut")]
        [ValidateAntiForgeryToken]
        public ActionResult CheckInOut(string checkButton)
        {
            ActionResult result = RedirectToAction("Index");
            if (checkButton == "Start") result = new wt4uTransaction().transact(delegate() { return new HomeAccess(User.Identity.Name).Start(); });
            else if (checkButton == "End") result = new wt4uTransaction().transact(delegate() { return new HomeAccess(User.Identity.Name).End(); });
            return result;
        }

        [HttpPost, ActionName("BreakCheckInOut")]
        [ValidateAntiForgeryToken]
        public ActionResult BreakCheckInOut(string breakButton)
        {
            ActionResult result = RedirectToAction("Index");
            if (breakButton == "Start") result = new wt4uTransaction().transact(delegate() { return new HomeAccess(User.Identity.Name).StartBreak(); });
            if (breakButton == "End") result = new wt4uTransaction().transact(delegate() { return new HomeAccess(User.Identity.Name).EndBreak(); });
            return result;
        }

        [HttpPost, ActionName("ProjectCheckInOut")]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectCheckInOut(string projectButton, int projectId = 0, string description = "")
        {
            ActionResult result = RedirectToAction("Index");
            if (projectButton == "Start") result = new wt4uTransaction().transact(delegate()
                { return new HomeAccess(User.Identity.Name).StartProjectBookingTime(projectId); });
            if (projectButton == "End") result = new wt4uTransaction().transact(delegate()
                { return new HomeAccess(User.Identity.Name).EndProjectBookingTime(description); });

            return result;
        }

        public ActionResult Status()
        {
            setStatus();
            HomeAccess hAccess = new HomeAccess(User.Identity.Name);
            WorkingSessionAccess wAccess = new WorkingSessionAccess(User.Identity.Name);
            WorkingSession currentSession = hAccess.CurrentWorkingSession();
            Break currentBreak = hAccess.CurrentBreak();
            ProjectBookingTime currentBooking = hAccess.CurrentProjectBookingTime();
            ViewBag.CurrentWorkingTime = TimeSpan.Zero;

            if (currentSession == null) ViewBag.WorkStatus = "You are not working";
            else
            {
                ViewBag.WorkStatus = "You are working since " + currentSession.Start;
                ViewBag.CurrentWorkingTime = wAccess.GetWorkingTime(currentSession);
            }
            ViewBag.TotalWorkingTime = wAccess.GetTotalUserWorkingTime();

            if (currentBreak == null) ViewBag.BreakStatus = "You are not in a Break";
            else ViewBag.BreakStatus = "Your are in a Break since " + currentBreak.Start;

            if (currentBooking == null) ViewBag.ProjectStatus = "Your are currently not working on a Project";
            else ViewBag.ProjectStatus = "You are currently working on Project " + currentBooking.Project.Name + " since " + currentBooking.Start;
            return View();
        }

        private void setStatus()
        {
            HomeAccess homeAccess = new HomeAccess(User.Identity.Name);
            ViewBag.isWorking = homeAccess.IsWorking();
            ViewBag.isInBreak = homeAccess.IsInBreak();
            ViewBag.isInProjectBookingTime = homeAccess.IsInProjectBookingTime();
        }

        public String getStatus()
        {
            String status = "";
            HomeAccess homeAccess = new HomeAccess(User.Identity.Name);
            ProjectAccess projectAccess = new ProjectAccess(User.Identity.Name);

            if (!homeAccess.IsWorking()) { status = ("Not Working"); }
            else { status = "Working since " + homeAccess.CurrentWorkingSession().Start; }

            if (homeAccess.IsInBreak()) { status = "In Break since " + homeAccess.CurrentBreak().Start; }

            if (homeAccess.IsInProjectBookingTime()) { status += " (Project " + homeAccess.CurrentProjectBookingTime().Project.Name + ")"; }
            else if (projectAccess.HasCurrentProject()) { status += "(Recent Project: " + projectAccess.GetCurrentProjectAllocation().Project.Name + ")"; }

            return status;
        }

        private void setError(String error)
        {
            switch (error)
            {
                case "TransactionAborted": ViewBag.Error = "Transaction Aborted, Please try again"; return;
                case "checkInError": ViewBag.Error = "You are already checked in"; return;
                case "checkOutError": ViewBag.Error = "You are already checked out"; return;
                case "BreakCheckInError": ViewBag.Error = "You are already in break"; return;
                case "BreakCheckOutError": ViewBag.Error = "You are not in a break"; return;
                case "ProjectCheckInError": ViewBag.Error = "You are already working on a projekt"; return;
                case "ProjectCheckOutError": ViewBag.Error = "You are not working on a project"; return;
                case "EmptyComment": ViewBag.Error = "Please enter a Comment"; return;
                default: ViewBag.Error = ""; return;
            }
        }
    }
}