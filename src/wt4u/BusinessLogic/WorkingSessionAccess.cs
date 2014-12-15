using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using wt4u.Models;

namespace wt4u.BusinessLogic
{
    public class WorkingSessionAccess
    {
        private wt4uDBContext Db;
        private ApplicationUser User;

        public WorkingSessionAccess(String Username)
        {
            Db = new wt4uDBContext();
            User = Db.Users.Where(e => e.UserName.Equals(Username)).First();
        }

        public List<ExpandoObject> GetAllWorkingSessionsForView()
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();

            foreach (WorkingSession w in GetAllWorkingSessions())
            {
                dynamic expando = new ExpandoObject();
                expando.WorkingSessionId = w.WorkingSessionId;
                expando.UserName = w.Employee.UserName;
                expando.Name = w.Employee.Name;
                expando.FirstName = w.Employee.FirstName;
                expando.Start = w.Start;
                expando.End = w.End;
                expando.WorkingTime = GetWorkingTime(w);

                dynamicList.Add(expando);
            }
            return dynamicList;
        }

        public List<WorkingSession> GetUserWorkingSessions()
        {
            var userworkingsessions = Db.WorkingSessions.Where(w => w.EmployeeId.Equals(User.Id));
            if (userworkingsessions == null) return new List<WorkingSession>();
            return userworkingsessions.ToList();
        }

        public List<ExpandoObject> GetUserWorkingSessionsForView()
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();

            foreach (WorkingSession w in GetUserWorkingSessions())
            {
                dynamic expando = new ExpandoObject();
                expando.WorkingSessionId = w.WorkingSessionId;
                expando.UserName = w.Employee.UserName;
                expando.Name = w.Employee.Name;
                expando.FirstName = w.Employee.FirstName;
                expando.Start = w.Start;
                expando.End = w.End;
                expando.WorkingTime = GetWorkingTime(w);

                dynamicList.Add(expando);
            }
            return dynamicList;
        }

        public List<WorkingSession> GetAllWorkingSessions()
        {
            var allworkingsessions = Db.WorkingSessions;
            if (allworkingsessions == null) return new List<WorkingSession>();
            return allworkingsessions.ToList<WorkingSession>();
        }

        public TimeSpan GetWorkingTime(WorkingSession session)
        {
            DateTime? sessionEnd = session.End;
            if (sessionEnd == null) sessionEnd = DateTime.Now;
            return sessionEnd.Value - session.Start - GetBreakTimeDuringWorkingSession(session);
        }

        public TimeSpan GetBreakTimeDuringWorkingSession(WorkingSession session)
        {
            TimeSpan totalBreakTime = TimeSpan.Zero;
            var breaks = Db.Breaks.Where(b => b.WorkingSessionId.Equals(session.WorkingSessionId));
            foreach (Break b in breaks)
            {
                if (b.End == null) continue;
                totalBreakTime = totalBreakTime.Add(b.End.Value - b.Start);
            }
            return totalBreakTime;
        }

        public TimeSpan GetProjectBookingTimeTotalDuringWorkingSession(int? WorkingSessionId)
        {
            TimeSpan totalTime = TimeSpan.Zero;
            foreach (ProjectBookingTime booking in GetWorkingSessionProjectBookingTimes(WorkingSessionId))
            {
                if (booking.End == null) continue;
                totalTime = totalTime.Add(booking.End.Value - booking.Start);
            }
            return totalTime;
        }

        public WorkingSession GetWorkingSession(int? WorkingSessionId)
        {
            return Db.WorkingSessions.Find(WorkingSessionId);
        }

        public Break GetBreak(int? BreakId)
        {
            return Db.Breaks.Find(BreakId);
        }

        public List<ProjectBookingTime> GetWorkingSessionProjectBookingTimes(int? id)
        {
            List<ProjectBookingTime> bookings = new List<ProjectBookingTime>();
            foreach (ProjectBookingTime b in Db.ProjectBookingTimes.Where(b => b.WorkingSessionId == id)) bookings.Add(b);
            return bookings;
        }

        public List<ExpandoObject> GetWorkingSessionProjectBookingTimesForView(int? id)
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();

            foreach (ProjectBookingTime b in GetWorkingSessionProjectBookingTimes(id))
            {
                dynamic expando = new ExpandoObject();
                expando.BookingId = b.BookingId;
                expando.ProjectName = b.Project.Name;
                expando.ProjectId = b.ProjectId;
                expando.Start = b.Start;
                expando.End = b.End;
                expando.Time = GetProjectBookingTimePerBooking(b);
                expando.Description = b.Description;
                dynamicList.Add(expando);
            }
            return dynamicList;
        }

        public List<ExpandoObject> GetTotalDayTimesForView()
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();
            List<TimeSpan> dayTimeList = GetTotalTimesPerDay();

            for (int i = 0; i < Enum.GetValues(typeof(DayOfWeek)).Length; i++)
            {
                dynamic expando = new ExpandoObject();
                expando.Day = Enum.GetValues(typeof(DayOfWeek)).GetValue(i);
                expando.DayTime = dayTimeList[i];
                dynamicList.Add(expando);
            }
            return dynamicList;
        }

        private List<TimeSpan> GetTotalTimesPerDay()
        {
            List<TimeSpan> dayTimeList = new List<TimeSpan>();
            foreach(DayOfWeek w in Enum.GetValues(typeof(DayOfWeek))) dayTimeList.Add(new TimeSpan(0));
            foreach (WorkingSession s in GetAllWorkingSessions())
            {
                switch (s.Start.DayOfWeek)
                {
                    case DayOfWeek.Monday: dayTimeList[1] += GetWorkingTime(s); break;
                    case DayOfWeek.Tuesday: dayTimeList[2] += GetWorkingTime(s); break;
                    case DayOfWeek.Wednesday: dayTimeList[3] += GetWorkingTime(s); break;
                    case DayOfWeek.Thursday: dayTimeList[4] += GetWorkingTime(s); break;
                    case DayOfWeek.Friday: dayTimeList[5] += GetWorkingTime(s); break;
                    case DayOfWeek.Saturday: dayTimeList[6] += GetWorkingTime(s); break;
                    case DayOfWeek.Sunday: dayTimeList[0] += GetWorkingTime(s); break;
                }
            }
            return dayTimeList;
        }

        private TimeSpan GetProjectBookingTimePerBooking(ProjectBookingTime booking)
        {
            if (booking.End == null) return TimeSpan.Zero;
            return booking.End.Value - booking.Start;
        }

        public TimeSpan GetTotalUserWorkingTime()
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (WorkingSession session in GetUserWorkingSessions()) total = total + GetWorkingTime(session);
            return total;
        }

        public List<ExpandoObject> GetWorkingSessionBreaksForView(int? id)
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();

            foreach (Break b in GetWorkingSessionBreaks(id))
            {
                dynamic expando = new ExpandoObject();
                expando.BreakId = b.BreakId;
                expando.Start = b.Start;
                expando.End = b.End;
                expando.Time = GetTimePerBreak(b);
                dynamicList.Add(expando);
            }
            return dynamicList;
        }

        public List<Break> GetWorkingSessionBreaks(int? id)
        {
            return Db.Breaks.Where(b => b.WorkingSessionId == id).ToList();
        }

        private TimeSpan GetTimePerBreak(Break b)
        {
            if (b.End == null) return TimeSpan.Zero;
            return b.End.Value - b.Start;
        }

        public bool ProofAuthorizationWorkingSession(int? id)
        {
            return GetWorkingSession(id).EmployeeId == User.Id || User.Roles.First<CustomUserRole>().RoleId == 1;
        }

        public ActionResult Delete(int id)
        {
            WorkingSession workingsession = GetWorkingSession(id);
            foreach (ProjectBookingTime b in GetWorkingSessionProjectBookingTimes(id)) new ProjectAccess(User.UserName).DeleteCurrentProjectAllocation(b);
            Db.WorkingSessions.Remove(workingsession);
            Db.SaveChanges();
            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));
        }

        public ActionResult DeleteBreak(int? id)
        {
            Break b = GetBreak(id);
            int WorkingSessionId = b.WorkingSessionId;
            Db.Breaks.Remove(b);
            Db.SaveChanges();
            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Details", id = WorkingSessionId }));
        }
        public ActionResult Edit(WorkingSession workingsession)
        {
            Db.Entry(workingsession).State = EntityState.Modified;
            String proof = ProofWorkingSessionDate(workingsession);
            if (proof != "false")
            {
                Db.Entry(workingsession).State = EntityState.Unchanged;
                return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Edit", Error = proof }));
            }
            Db.SaveChanges();
            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index" }));
        }

        private String ProofWorkingSessionDate(WorkingSession session)
        {
            DateTime? sessionEnd = session.End;
            if (sessionEnd == null) sessionEnd = DateTime.MaxValue;
            if (session.Start > sessionEnd) return "itself";
            foreach (Break b in GetWorkingSessionBreaks(session.WorkingSessionId))
            {
                DateTime? breakEnd = b.End;
                if (b.End == null) breakEnd = DateTime.MaxValue;
                if (session.Start > b.Start || sessionEnd <= breakEnd) return "Break ID = " + b.BreakId;
            }
            foreach (ProjectBookingTime b in GetWorkingSessionProjectBookingTimes(session.WorkingSessionId))
            {
                DateTime? bookingEnd = b.End;
                if (b.End == null) bookingEnd = DateTime.MaxValue;
                if (session.Start > b.Start || sessionEnd <= bookingEnd) return "ProjectBookingTime ID = " + b.BookingId;
            }
            return "false";
        }

        public ActionResult EditBreak(Break b)
        {
            Db.Entry(b).State = EntityState.Modified;
            String proof = ProofBreakDate(b);
            if (proof != "false")
            {
                Db.Entry(b).State = EntityState.Unchanged;
                return new RedirectToRouteResult(new RouteValueDictionary(new { action = "EditBreak", Error = proof }));
            }
            Db.SaveChanges();
            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Details", id = b.WorkingSessionId }));
        }

        private string ProofBreakDate(Break b)
        {
            if (b.End < b.Start) return "itself";
            WorkingSession session = GetWorkingSession(b.WorkingSessionId);
            if (b.End == null) return "WorkingSession (Break has to be closed for modifying)";
            DateTime? sessionEnd = session.End;
            if (sessionEnd == null) sessionEnd = DateTime.Now;
            if (b.Start < session.Start || b.End > sessionEnd) return "WorkingSession ID = " + session.WorkingSessionId;
            foreach (Break br in GetWorkingSessionBreaks(session.WorkingSessionId))
            {
                if (b.BreakId == br.BreakId) continue;
                if (b.End > br.Start && b.End < br.End) return "Break ID = " + br.BreakId;
                if (b.Start > br.Start && b.Start < br.End) return "Break ID = " + br.BreakId;
            }
            foreach (ProjectBookingTime bo in GetWorkingSessionProjectBookingTimes(session.WorkingSessionId))
            {
                if (b.End > bo.Start && b.End < bo.End) return "Booking ID = " + bo.BookingId;
                if (b.Start > bo.Start && b.Start < bo.End) return "Booking ID = " + bo.BookingId;
            }
            return "false";
        }
    }
}