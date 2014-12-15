using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wt4u.Models;

using System.Data.Entity;
using System.Net;
using System.Web.Routing;
using System.Dynamic;

namespace wt4u.BusinessLogic
{
    public class HomeAccess
    {
        private wt4uDBContext Db;
        ApplicationUser User;

        public HomeAccess(String username)
        {
            Db = new wt4uDBContext();
            User = Db.Users.Where(e => e.UserName.Equals(username)).First();
        }


        public ActionResult Start()
        {
            if (User == null) return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Error" }));
            if (IsWorking()) return new RedirectToRouteResult(
                new RouteValueDictionary(new { action = "Index", Error = "checkInError" }));

            WorkingSession workingsession = new WorkingSession(DateTime.Now, null, User.Id);

            Db.WorkingSessions.Add(workingsession);
            Db.SaveChanges();

            ProjectAllocation currentAllocation = new ProjectAccess(User.UserName).GetCurrentProjectAllocation();
            if (currentAllocation != null) StartProjectBookingTime(currentAllocation.ProjectId, true);

            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));
        }

        public ActionResult End()
        {
            if (User == null) return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Error" }));
            if (!IsWorking()) return new RedirectToRouteResult(
                new RouteValueDictionary(new { action = "Index", Error = "checkOutError" }));

            if (IsInBreak()) EndBreak(true);
            else if (IsInProjectBookingTime()) EndProjectBookingTime("", true);

            WorkingSession workingsession = CurrentWorkingSession();
            workingsession.End = DateTime.Now;

            Db.Entry(workingsession).State = EntityState.Modified;
            Db.SaveChanges();

            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));
        }

        public ActionResult StartBreak()
        {
            if (User == null) return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Error" }));
            if (!IsWorking()) return new RedirectToRouteResult(
                new RouteValueDictionary(new { action = "Index", Error = "BreakCheckInError" }));

            if (IsInProjectBookingTime()) EndProjectBookingTime(" ", true);

            Break b = new Break(DateTime.Now, null, CurrentWorkingSession().WorkingSessionId);

            Db.Breaks.Add(b);
            Db.SaveChanges();

            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));
        }

        public ActionResult EndBreak(bool stopWorkingSession = false)
        {
            if (!IsWorking() || !IsInBreak()) return new RedirectToRouteResult(
               new RouteValueDictionary(new { action = "Index", Error = "BreakCheckOutError" }));

            Break b = CurrentBreak();
            b.End = DateTime.Now;

            Db.Entry(b).State = EntityState.Modified;
            Db.SaveChanges();

            if (!stopWorkingSession)
            {
                ProjectAllocation currentAllocation = new ProjectAccess(User.UserName).GetCurrentProjectAllocation();
                if (currentAllocation != null) StartProjectBookingTime(currentAllocation.ProjectId);
            }
            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));
        }

        public ActionResult StartProjectBookingTime(int projectId, bool continues = false)
        {
            if (!IsWorking() || IsInBreak()) return new RedirectToRouteResult(
                    new RouteValueDictionary(new { action = "Index", Error = "ProjectCheckInError" }));

            if (!continues) new ProjectAccess(User.UserName).SetCurrentProjectAllocation(projectId, true);

            ProjectBookingTime pb = new ProjectBookingTime(projectId, "", DateTime.Now, null, CurrentWorkingSession().WorkingSessionId);

            Db.ProjectBookingTimes.Add(pb);
            Db.SaveChanges();

            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));
        }

        public ActionResult EndProjectBookingTime(String description, bool continues = false)
        {
            if (!IsWorking() || IsInBreak() || !IsInProjectBookingTime()) return new RedirectToRouteResult(
                    new RouteValueDictionary(new { action = "Index", Error = "ProjectCheckOutError" }));

            if (description.Equals("") && !continues)
            {
                return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", Error = "EmptyComment" }));
            }

            if (!continues) new ProjectAccess(User.UserName).SetCurrentProjectAllocation(false);

            ProjectBookingTime ProjectBooking = CurrentProjectBookingTime();

            var bookings = Db.ProjectBookingTimes.Where(pb => pb.ProjectId.Equals(ProjectBooking.ProjectId) && pb.WorkingSession.EmployeeId.Equals(User.Id) && (pb.Description.Equals("") || pb.Description.Equals(" "))).ToList();

            ProjectBooking.End = DateTime.Now;

            foreach (ProjectBookingTime b in bookings)
            {
                b.Description = description;
            }

            Db.Entry(ProjectBooking).State = EntityState.Modified;
            Db.SaveChanges();

            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));
        }

        public ExpandoObject GetStatusForView()
        {
            WorkingSession currentSession = CurrentWorkingSession();
            dynamic expando = new ExpandoObject();
            expando.Session = currentSession;
            expando.Break = CurrentBreak();
            expando.Booking = CurrentProjectBookingTime();
            expando.CurrentWorkingTime = new WorkingSessionAccess(User.UserName).GetWorkingTime(currentSession);
            expando.TotalWorkingTime = new WorkingSessionAccess(User.UserName).GetTotalUserWorkingTime();

            return expando;
        }
        public WorkingSession CurrentWorkingSession()
        {
            var sessions = Db.WorkingSessions.Where(w => w.EmployeeId == User.Id && w.End == null);
            if (sessions.Count() == 0) return null;
            return sessions.First();
        }

        public Break CurrentBreak()
        {
            WorkingSession currentsessions = CurrentWorkingSession();
            if (currentsessions == null) return null;
            var Breaks = Db.Breaks.Where(b => b.WorkingSessionId == currentsessions.WorkingSessionId && b.End == null);
            if (Breaks.Count() == 0) return null;
            return Breaks.First();
        }

        public ProjectBookingTime CurrentProjectBookingTime()
        {
            WorkingSession currentsession = CurrentWorkingSession();
            if (currentsession == null) return null;
            var ProjectBookingTimes = Db.ProjectBookingTimes.Where(b => b.WorkingSessionId == currentsession.WorkingSessionId && b.End == null);
            if (ProjectBookingTimes.Count() == 0) return null;
            return ProjectBookingTimes.First();
        }



        public bool IsWorking()
        {
            if (CurrentWorkingSession() != null) return true;
            return false;
        }

        public bool IsInBreak()
        {
            if (CurrentBreak() != null) return true;
            return false;
        }

        public bool IsInProjectBookingTime()
        {
            if (CurrentProjectBookingTime() != null) return true;
            return false;
        }

    }
}