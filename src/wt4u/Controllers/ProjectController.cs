using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web.Mvc;
using wt4u.BusinessLogic;
using wt4u.Models;

namespace wt4u.Controllers
{
    [SecurityAuthorize(Roles = "Employer,Employee")]
    public class ProjectController : Controller
    {
        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Index()
        {
            ViewBag.isEmployer = User.IsInRole("Employer");
            ViewBag.LeadingProjects = new ProjectAccess(User.Identity.Name).GetUserLeadingProjectList();
            ViewBag.Projects = new ProjectAccess(User.Identity.Name).GetAllProjects();
            ViewBag.Table = "All";
            return View();
        }

        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult MyProjects()
        {
            ViewBag.isEmployer = User.IsInRole("Employer");
            ViewBag.LeadingProjects = new ProjectAccess(User.Identity.Name).GetUserLeadingProjectList();
            ViewBag.Projects = new ProjectAccess(User.Identity.Name).GetUserProjectList();
            ViewBag.Table = "My";
            return View("Index");
        }


        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectAccess pa = new ProjectAccess(User.Identity.Name);

            if (!pa.ProofAuthorizationProject(id)) return RedirectToAction("Error", "Home");

            ViewBag.ProjectBookingDetails = pa.GetProjectBookingDetails(id);
            ViewBag.ProjectDetails = pa.GetProjectDetails(id);
            Project project = pa.GetProject(id);

            if (project == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectName = project.Name;
            return View();
        }

        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Create()
        {
            ViewBag.Users = new AccountAccess(User.Identity.Name).getUserSelectList();
            return View();
        }


        [HttpPost]
        [SecurityAuthorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProjectId,Name,StartDate,EndDate")] Project project, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                ProjectAccess access = new ProjectAccess(User.Identity.Name);
                access.CreateProject(project);

                var leader = collection["ProjectLeader"];

                access.UpdateProjectLeader(project, int.Parse(leader));

                List<int> list = new List<int>();
                list.Add(int.Parse(leader));
                var team = list.ToArray();

                access.UpdateProjectAllocation(project, team);

                return RedirectToAction("Index");
            }

            ViewBag.Users = new AccountAccess(User.Identity.Name).getUserSelectList();
            return View();
        }

        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectAccess pa = new ProjectAccess(User.Identity.Name);

            if (!pa.ProofAuthorizationEditProject(id)) return RedirectToAction("Error", "Home");

            Project project = pa.GetProject(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            ViewBag.Project = pa.GetProjectToEdit(project);
            ViewBag.ProjectDetails = pa.GetProjectDetails(id);
            ViewBag.Error = "";
            ViewBag.PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "DirectLink";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult Edit([Bind(Include = "ProjectId,Name,StartDate,EndDate,isClosed")] Project project, FormCollection collection)
        {

            if (ModelState.IsValid)
            {
                ProjectAccess access = new ProjectAccess(User.Identity.Name);
                var leader = collection["ProjectLeader"];

                var list = collection["ProjectTeamHidden"].Split(',').Select(n => int.Parse(n)).ToList();
                list.Add(int.Parse(leader));
                var team = list.ToArray();

                ViewBag.Error = "";

                string users = access.UpdateProjectAllocation(project, team);
                if (users.Length > 0)
                {
                    ViewBag.Project = access.GetProjectToEdit(project);
                    ViewBag.PreviousPage = collection["PreviousPage"];
                    ViewBag.Error = users;
                    return View();
                };

                access.UpdateProjectLeader(project, int.Parse(leader));
                access.EditProject(project);

                String PreviousPage = collection["PreviousPage"];

                if (PreviousPage.EndsWith("/Project") || PreviousPage.EndsWith("/Project/Index"))
                {
                    return RedirectToAction("Index");
                }
                return RedirectToAction("MyProjects");
            }
            ProjectAccess pa = new ProjectAccess(User.Identity.Name);
            ViewBag.Project = pa.GetProjectToEdit(project);
            ViewBag.PreviousPage = collection["PreviousPage"];
            return View();

        }

        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = new ProjectAccess(User.Identity.Name).GetProject(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            ViewBag.Bookings = new ProjectAccess(User.Identity.Name).GetProjectBookings(project);
            return View(project);
        }

        [SecurityAuthorize(Roles = "Employer,Employee")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            new ProjectAccess(User.Identity.Name).DeleteProject(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult DeleteBooking(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            int? pId = new ProjectAccess(User.Identity.Name).GetBooking(id).ProjectId;
            return new wt4uTransaction().transact(delegate() { return new ProjectAccess(User.Identity.Name).DeleteBooking(id, pId); });
        }


        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult EditBooking(int? id, String error = "")
        {
            if (error != "") ViewBag.Error = "New Date interferes with " + error;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectBookingTime booking = new ProjectAccess(User.Identity.Name).GetBooking(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult EditBooking([Bind(Include = "BookingId,Start,End,Description,WorkingSessionID,ProjectID")] ProjectBookingTime booking)
        {
            return new wt4uTransaction().transact(delegate() { return new ProjectAccess(User.Identity.Name).EditBooking(booking, booking.ProjectId); });
        }

        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult ExportProjectsToCSV(String table)
        {
            List<ExpandoObject> csv_List = new List<ExpandoObject>();
            if (table == "All" && User.IsInRole("Employer")) csv_List = new ProjectAccess(User.Identity.Name).GetAllProjects();
            else if (table == "My") csv_List = new ProjectAccess(User.Identity.Name).GetUserProjectList();
            else return RedirectToAction("Error", "Home");

            StringWriter sw = new StringWriter();
            sw.WriteLine("\"ID\";\"Project Name\";\"Start\";\"End\";\"spent Time\";\"IsClosed\"");
            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment;filename=wt4u_export_Projects.csv");
            Response.ContentType = "text/csv";

            foreach (dynamic line in csv_List)
            {
                sw.Write(line.ProjectId + ";");
                sw.Write(line.Name + ";");
                sw.Write(line.StartDate + ";");
                sw.Write(line.EndDate + ";");
                sw.Write(line.Duration + ";");
                if (line.IsClosed) sw.WriteLine("yes");
                else sw.WriteLine("no");
            }
            Response.Write(sw.ToString());
            Response.End();

            if (table == "My") return RedirectToAction("MyProjects");
            return RedirectToAction("Index");
        }

        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult Report(int? id)
        {
            ProjectAccess pa = new ProjectAccess(User.Identity.Name);

            List<KeyValuePair<int, String>> projects;
            Project project;

            if (id == null)
            {
                projects = pa.GetProjectsForReport();
                if (projects.Count > 0)
                {
                    id = projects.First().Key;
                }
                else
                {
                    ViewBag.ProjectId = null;
                    return View();
                }
            }
            if (!pa.ProofAuthorizationEditProject(id)) return RedirectToAction("Error", "Home");

            ViewBag.ProjectDetails = pa.GetProjectDetails(id);
            project = pa.GetProject(id);

            if (project == null)
            {
                return HttpNotFound();
            }
            ViewBag.EmployeeTimes = pa.GetEmployeeTimes(project);
            ViewBag.MonthTimes = pa.GetMonthTimes(project);
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = project.ProjectId;
            projects = pa.GetProjectsForReport();
            ViewBag.ProjectList = projects;

            return View();
        }
    }
}
