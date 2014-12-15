using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using wt4u.Models;
using wt4u.BusinessLogic;
using System.Transactions;
using System.IO;
using System.Dynamic;

namespace wt4u.Controllers
{
    [SecurityAuthorize(Roles = "Employer, Employee")]
    public class WorkingSessionController : Controller
    {
        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Index()
        {
            ViewBag.WorkingSessions = new WorkingSessionAccess(User.Identity.Name).GetAllWorkingSessionsForView();
            ViewBag.Table = "All";
            return View();
        }

        public ActionResult MyWorkingSessions()
        {
            ViewBag.WorkingSessions = new WorkingSessionAccess(User.Identity.Name).GetUserWorkingSessionsForView();
            ViewBag.Table = ("My");
            return View("Index");
        }

        public ActionResult Details(int? id)
        {
            WorkingSessionAccess access = new WorkingSessionAccess(User.Identity.Name);
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            WorkingSession workingsession = access.GetWorkingSession(id);

            if (!access.ProofAuthorizationWorkingSession(id)) return RedirectToAction("Error", "Home");

            ViewBag.WorkingSession = workingsession;
            ViewBag.WorkingTime = access.GetWorkingTime(workingsession);

            ViewBag.BreakTime = access.GetBreakTimeDuringWorkingSession(workingsession);
            ViewBag.ProjectBookingTime = access.GetProjectBookingTimeTotalDuringWorkingSession(workingsession.WorkingSessionId);

            ViewBag.Projects = access.GetWorkingSessionProjectBookingTimesForView(id);
            ViewBag.Breaks = access.GetWorkingSessionBreaksForView(id);

            if (workingsession == null) return HttpNotFound();
            return View();
        }

        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Edit(int? id, string error = "")
        {
            if (error != "") ViewBag.Error = "New Date interferes with " + error;
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            WorkingSession workingsession = new WorkingSessionAccess(User.Identity.Name).GetWorkingSession(id);
            if (workingsession == null) return HttpNotFound();
            return View(workingsession);
        }

        [SecurityAuthorize(Roles = "Employer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "WorkingSessionId,Start,End,EmployeeId")] WorkingSession workingsession)
        {
            if (!ModelState.IsValid) return View(workingsession);
            return new wt4uTransaction().transact(delegate() { return new WorkingSessionAccess(User.Identity.Name).Edit(workingsession); });
        }

        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult EditBreak(int? id, string error = "")
        {
            if (error != "") ViewBag.Error = "New Date interferes with " + error;
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Break b = new WorkingSessionAccess(User.Identity.Name).GetBreak(id);
            if (b == null) return HttpNotFound();
            return View(b);
        }

        [SecurityAuthorize(Roles = "Employer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBreak([Bind(Include = "BreakId,StartTime,endTime,WorkingSessionId")] Break b)
        {
            if (!ModelState.IsValid) return View(b);
            return new wt4uTransaction().transact(delegate() { return new WorkingSessionAccess(User.Identity.Name).EditBreak(b); });
        }

        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult EditBooking(int? id, String error = "")
        {
            if (error != "") ViewBag.Error = "New Date interferes with " + error;
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ProjectBookingTime booking = new ProjectAccess(User.Identity.Name).GetBooking(id);
            if (booking == null) return HttpNotFound();
            return View(booking);
        }

        [SecurityAuthorize(Roles = "Employer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBooking([Bind(Include = "BookingId,Start,End,Description,WorkingSessionID,ProjectID")] ProjectBookingTime booking)
        {
            if (!ModelState.IsValid) return View(booking);
            return new wt4uTransaction().transact(delegate() { return new ProjectAccess(User.Identity.Name).EditBooking(booking, booking.WorkingSessionId); });
        }

        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            WorkingSession workingsession = new WorkingSessionAccess(User.Identity.Name).GetWorkingSession(id);
            if (workingsession == null) return HttpNotFound();
            setDeleteMessage(id);
            return View(workingsession);
        }

        [SecurityAuthorize(Roles = "Employer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            return new wt4uTransaction().transact(delegate() { return new WorkingSessionAccess(User.Identity.Name).Delete(id); });
        }

        [SecurityAuthorize(Roles = "Employer")]
        [HttpPost]
        public ActionResult DeleteBreak(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            int? wId = new WorkingSessionAccess(User.Identity.Name).GetBreak(id).WorkingSessionId;
            return new wt4uTransaction().transact(delegate() { return new WorkingSessionAccess(User.Identity.Name).DeleteBreak(id); });
        }

        [SecurityAuthorize(Roles = "Employer")]
        [HttpPost]
        public ActionResult DeleteBooking(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            int? wId = new ProjectAccess(User.Identity.Name).GetBooking(id).WorkingSessionId;
            return new wt4uTransaction().transact(delegate() { return new ProjectAccess(User.Identity.Name).DeleteBooking(id, wId); });
        }

        public ActionResult ExportWorkingSessionsToCSV(String table)
        {
            List<ExpandoObject> CSV_List = new List<ExpandoObject>();
            if (table == "All" && User.IsInRole("Employer")) CSV_List = new WorkingSessionAccess(User.Identity.Name).GetAllWorkingSessionsForView();
            else if (table == "My") CSV_List = new WorkingSessionAccess(User.Identity.Name).GetUserWorkingSessionsForView();
            else return RedirectToAction("Error", "Home");

            StringWriter sw = new StringWriter();
            sw.WriteLine("\"ID\";\"Username\";\"Surname\";\"Firstname\";\"Start\";\"End\";\"Workingtime\"");
            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment;filename=wt4u_export_WorkingSessions.csv");
            Response.ContentType = "text/csv";

            foreach (dynamic line in CSV_List)
            {
                sw.Write(line.WorkingSessionId + ";");
                sw.Write(line.UserName + ";");
                sw.Write(line.Name + ";");
                sw.Write(line.FirstName + ";");
                sw.Write(line.Start + ";");
                sw.Write(line.End + ";");
                sw.WriteLine(String.Format("{0:00}:{1:00}:{2:00}", (int)line.WorkingTime.TotalHours, line.WorkingTime.Minutes, line.WorkingTime.Seconds));
            }
            Response.Write(sw.ToString());
            Response.End();

            if (table == "My") return RedirectToAction("MyWorkingSessions");
            return RedirectToAction("Index");
        }

        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Report()
        {
            ViewBag.Projects = new ProjectAccess(User.Identity.Name).GetAllProjects();
            ViewBag.DayTime = new WorkingSessionAccess(User.Identity.Name).GetTotalDayTimesForView();
            return View();
        }

        private void setDeleteMessage(int? id)
        {
            List<String> delList = new List<String>();
            foreach (Break b in new WorkingSessionAccess(User.Identity.Name).GetWorkingSessionBreaks(id))
            {
                delList.Add("Break " + b.BreakId);
            }
            foreach (ProjectBookingTime b in new WorkingSessionAccess(User.Identity.Name).GetWorkingSessionProjectBookingTimes(id))
            {
                delList.Add("Booking " + b.BookingId);
            }
            ViewBag.DeleteMessage = delList;
        }
    }
}
