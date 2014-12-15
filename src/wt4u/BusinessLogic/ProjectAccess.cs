using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
    public class ProjectAccess
    {
        private wt4uDBContext Db;
        ApplicationUser User;

        public ProjectAccess(String username)
        {
            Db = new wt4uDBContext();
            User = Db.Users.Where(e => e.UserName.Equals(username)).First();
        }

        public List<Project> GetAllUserProjects()
        {
            var allocations = Db.ProjectAllocations.Where(b => b.EmployeeId.Equals(User.Id) && b.EndDate == null);
            var projects = new List<Project>();
            if (allocations.Count() > 0)
            {
                var allocationList = allocations.ToList<ProjectAllocation>();

                foreach (ProjectAllocation a in allocationList)
                {
                    projects.Add(a.Project);
                }
            }
            var leadings = Db.ProjectLeadings.Where(l => l.EmployeeId.Equals(User.Id) && l.EndDate == null);
            if (leadings.Count() > 0)
            {
                var leadingList = leadings.ToList();
                foreach (ProjectLeading pl in leadingList)
                {
                    if (!projects.Contains(pl.Project))
                    {
                        projects.Add(pl.Project);
                    }
                }
            }
            return projects;
        }

        public List<Project> GetAllUserProjectsToBook()
        {
            var allocations = Db.ProjectAllocations.Where(b => b.EmployeeId.Equals(User.Id) && b.EndDate == null && !b.Project.isClosed).ToList();
            var projects = new List<Project>();
            foreach (ProjectAllocation a in allocations)
            {
                projects.Add(a.Project);
            }

            var leadings = Db.ProjectLeadings.Where(l => l.EmployeeId.Equals(User.Id) && l.EndDate == null && !l.Project.isClosed);
            if (leadings.Count() > 0)
            {
                var leadingList = leadings.ToList();
                foreach (ProjectLeading pl in leadingList)
                {
                    if (!projects.Contains(pl.Project))
                    {
                        projects.Add(pl.Project);
                    }
                }
            }
            if (projects.Count == 0) return projects;
            projects = projects.OrderBy(p => p.Name).ToList();
            return projects;
        }

        public IEnumerable<SelectListItem> GetUserProjects()
        {

            var projects = GetAllUserProjects();

            IEnumerable<SelectListItem> selectList =
                from p in projects
                select new SelectListItem { Text = p.Name, Value = p.ProjectId.ToString() };

            return selectList;
        }

        public IEnumerable<SelectListItem> GetUserProjectsToBook()
        {

            var projects = GetAllUserProjectsToBook();

            IEnumerable<SelectListItem> selectList =
                from p in projects
                select new SelectListItem { Text = p.Name, Value = p.ProjectId.ToString() };

            return selectList;
        }

        public List<ExpandoObject> GetUserProjectList()
        {
            TimeSpan duration;
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();
            TimeSpan userTime;

            var projects = GetAllUserProjects();

            foreach (Project p in projects)
            {
                duration = new TimeSpan(0);
                userTime = new TimeSpan(0);
                var bookings = Db.ProjectBookingTimes.Where(pb => pb.ProjectId == p.ProjectId);
                List<ProjectBookingTime> times = new List<ProjectBookingTime>();
                if (bookings.Count() > 0) times = bookings.ToList();
                foreach (ProjectBookingTime pb in times)
                {
                    if (pb.End != null)
                    {
                        if (pb.WorkingSession.EmployeeId.Equals(User.Id))
                        {
                            TimeSpan uspan = pb.End.Value.Subtract(pb.Start);
                            userTime += uspan;
                        }
                        TimeSpan span = pb.End.Value.Subtract(pb.Start);
                        duration += span;
                    }
                }
                dynamic expando = new ExpandoObject();
                expando.Leader = GetProjectLeader(p).Name;
                expando.ProjectId = p.ProjectId;
                expando.Name = p.Name;
                expando.StartDate = p.StartDate;
                expando.EndDate = p.EndDate;
                expando.Duration = duration;
                expando.UserTime = userTime;
                expando.IsClosed = p.isClosed;
                dynamicList.Add(expando);
            }
            return dynamicList;
        }

        public List<int> GetUserLeadingProjectList()
        {
            var leadings = Db.ProjectLeadings.Where(l => l.EmployeeId.Equals(User.Id) && l.EndDate == null);
            var projects = new List<int>();
            if (leadings.Count() > 0)
            {
                var leadingList = leadings.ToList();

                foreach (ProjectLeading l in leadingList)
                {
                    projects.Add(l.ProjectID);
                }
            }

            return projects;
        }

        public void SetCurrentProjectAllocation(int projectId, bool isCurrentProject)
        {
            var allocations = Db.ProjectAllocations.Where(a => a.ProjectId.Equals(projectId) && a.EmployeeId.Equals(User.Id) && a.EndDate == null);
            if (allocations.Count() > 0)
            {
                ProjectAllocation currentAllocation = allocations.First();
                currentAllocation.IsCurrentProject = isCurrentProject;
                Db.Entry(currentAllocation).State = EntityState.Modified;
                Db.SaveChanges();
            }
            
        }

        public void SetCurrentProjectAllocation(bool isCurrentProject)
        {
            ProjectAllocation currentAllocation = GetCurrentProjectAllocation();
            currentAllocation.IsCurrentProject = isCurrentProject;
            Db.Entry(currentAllocation).State = EntityState.Modified;
            Db.SaveChanges();
        }

        public ProjectAllocation GetCurrentProjectAllocation()
        {
            var projects = Db.ProjectAllocations.Where(b => b.EmployeeId.Equals(User.Id) && b.IsCurrentProject == true);
            if (projects.Count() == 0) return null;
            var projectList = projects.ToList();

            return projectList.First();
        }

        public void DeleteCurrentProjectAllocation(ProjectBookingTime booking)
        {
            ProjectAllocation allocation = GetCurrentProjectAllocation();
            if (allocation == null) return;
            if (booking.End == null && booking.ProjectId == allocation.ProjectId) SetCurrentProjectAllocation(false);
        }

        public bool HasCurrentProject()
        {
            if (GetCurrentProjectAllocation() != null) return true;
            return false;
        }

        public List<ExpandoObject> GetAllProjects()
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();
            var projects = Db.Projects;
            if (projects.Count() == 0) return dynamicList;

            foreach (Project p in projects.ToList())
            {
                dynamic expando = new ExpandoObject();
                expando.ProjectId = p.ProjectId;
                expando.Name = p.Name;
                expando.StartDate = p.StartDate;
                expando.EndDate = p.EndDate;
                expando.Duration = GetProjectTotalTime(p.ProjectId);
                expando.IsClosed = p.isClosed;
                dynamicList.Add(expando);
            }
            return dynamicList;
        }

        private TimeSpan GetProjectTotalTime(int? id)
        {
            TimeSpan duration = new TimeSpan(0);
            var times = Db.ProjectBookingTimes.Where(pb => pb.ProjectId == id);
            if (times.Count() > 0)
            {
                var timeList = times.ToList();
                foreach (ProjectBookingTime pb in timeList)
                {
                    if (pb.End != null)
                    {
                        TimeSpan span = pb.End.Value.Subtract(pb.Start);
                        duration += span;
                    }
                }
            }
            return duration;
        }

        public List<ExpandoObject> GetAllProjectsToLead()
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();

            var leadings = Db.ProjectLeadings.Where(p => p.EndDate == null && p.EmployeeId == User.Id).ToList();

            foreach (ProjectLeading l in leadings)
            {
                dynamic expando = new ExpandoObject();
                expando.ProjectId = l.ProjectID;
                expando.Name = l.Project.Name;
                expando.StartDate = l.Project.StartDate;
                expando.EndDate = l.Project.EndDate;
                expando.IsClosed = l.Project.isClosed;
                dynamicList.Add(expando);
            }

            return dynamicList;
        }

        public ExpandoObject GetProjectDetails(int? id)
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();
            var projects = Db.Projects.Where(p => p.ProjectId == id).ToList<Project>();
            if (projects.Count() == 0) return null;
            var project = projects.First();

            TimeSpan span = new TimeSpan(0);
            var bookings = Db.ProjectBookingTimes.Where(pb => pb.ProjectId == id && pb.End != null).Include(pb => pb.WorkingSession.Employee).ToList<ProjectBookingTime>();
            foreach (ProjectBookingTime b in bookings)
            {
                span += b.End.Value.Subtract(b.Start);
            }
            ProjectBookingTime first = GetFirstProjectBookingId(bookings);
            bool isClosed = project.isClosed;
            ProjectBookingTime last = GetLastProjectBookingId(bookings);
            ApplicationUser leader = GetProjectLeader(project);
            dynamic expando = new ExpandoObject();
            expando.ProjectLeader = leader != null ? leader.Name + " " + leader.FirstName + " (" + leader.UserName + ")" : "No Project Leader defined";
            expando.ProjectTeam = GetProjectTeamNames(project);
            expando.ProjectId = project.ProjectId;
            expando.ProjectName = project.Name;
            expando.StartDate = String.Format("{0:dd.MM.yyyy}", project.StartDate);
            expando.EndDate = String.Format("{0:dd.MM.yyyy}", project.EndDate);
            expando.StartIst = first != null ? string.Format("{0:dd.MM.yyyy}", Db.ProjectBookingTimes.Where(p => p.BookingId == first.BookingId).First().Start) : "Project isn't started";
            expando.EndIst = isClosed && last != null ? string.Format("{0:dd.MM.yyyy}", Db.ProjectBookingTimes.Where(p => p.BookingId == last.BookingId).First().End) : "Project isn't closed";
            expando.Duration = String.Format("{0:00}:{1:00}:{2:00} Hours", (int)span.TotalHours, span.Minutes, span.Seconds);

            return expando;
        }


        public ExpandoObject GetProjectToEdit(Project project)
        {
            dynamic expando = new ExpandoObject();
            expando.ProjectId = project.ProjectId;
            expando.Name = project.Name;
            expando.StartDate = project.StartDate;
            expando.EndDate = project.EndDate;
            expando.isClosed = project.isClosed;

            var leader = GetProjectLeader(project);

            AccountAccess access = new AccountAccess(User.UserName);

            var selectUserList = access.getUserSelectList(leader);

            expando.ProjectLeader = access.getUserSelectList(leader);

            expando.AllUsers = access.getUserSelectList();

            var teamUsers = GetProjectTeam(project);

            expando.ProjectTeam = access.getTeamSelectList(teamUsers);
            return expando;
        }

        public ProjectBookingTime GetFirstProjectBookingId(List<ProjectBookingTime> list)
        {
            ProjectBookingTime res = null;
            foreach (ProjectBookingTime pb in list)
            {
                if (res == null) res = pb;
                if (pb.Start < res.Start) res = pb;
            }
            return res;
        }

        public List<String> GetProjectTeamNames(Project project)
        {
            List<String> res = new List<string>();
            var allocations = GetProjectTeam(project);
            foreach (ApplicationUser p in allocations)
            {
                if (p == null)
                {
                    res.Add("No Employee allocated");
                    return res;
                }
                res.Add(p.Name + " " + p.FirstName + " (" + p.UserName + ")");
            }
            return res;
        }

        public List<ApplicationUser> GetProjectTeam(Project project)
        {
            List<ApplicationUser> res = new List<ApplicationUser>();
            var allocations = Db.ProjectAllocations.Where(p => p.ProjectId == project.ProjectId && p.EndDate == null).ToList();
            if (allocations.Count() == 0) res.Add(null);
            foreach (ProjectAllocation p in allocations)
            {
                res.Add(p.Employee);
            }
            return res;
        }

        public ApplicationUser GetProjectLeader(Project project)
        {
            var leadings = Db.ProjectLeadings.Where(p => p.ProjectID == project.ProjectId && p.EndDate == null);
            if (leadings.Count() == 0) return null;
            ApplicationUser leader = leadings.First().Employee;
            return leader;
        }

        public ProjectBookingTime GetLastProjectBookingId(List<ProjectBookingTime> list)
        {
            ProjectBookingTime res = null;
            foreach (ProjectBookingTime pb in list)
            {
                if (res == null) res = pb;
                if (pb.End > res.End) res = pb;
            }
            return res;
        }

        public List<ExpandoObject> GetProjectBookingDetails(int? id)
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();

            var projects = Db.Projects.Where(p => p.ProjectId == id).ToList<Project>();
            if (projects.Count() == 0) return null;

            List<ProjectBookingTime> bookings;
            if (User.Roles.First<CustomUserRole>().RoleId == 1)
            {
                bookings = Db.ProjectBookingTimes.Where(pb => pb.ProjectId == id && pb.End != null).Include(pb => pb.WorkingSession.Employee).ToList<ProjectBookingTime>();
            }
            else
            {
                bookings = Db.ProjectBookingTimes.Where(pb => pb.ProjectId == id && pb.End != null && pb.WorkingSession.EmployeeId == User.Id).Include(pb => pb.WorkingSession.Employee).ToList<ProjectBookingTime>();
            }

            foreach (ProjectBookingTime b in bookings)
            {
                dynamic expando = new ExpandoObject();
                expando.BookingId = b.BookingId;
                expando.Start = b.Start;
                expando.End = b.End;
                expando.Duration = b.End.Value.Subtract(b.Start);
                expando.Description = b.Description;
                expando.Name = b.WorkingSession.Employee.Name + " " + b.WorkingSession.Employee.FirstName;
                dynamicList.Add(expando);
            }

            return dynamicList;
        }

        public List<ProjectBookingTime> GetProjectBookings(Project project)
        {
            var bookings = Db.ProjectBookingTimes.Where(pb => pb.ProjectId == project.ProjectId).ToList<ProjectBookingTime>();
            return bookings;
        }

        public Project GetProject(int? id)
        {
            var project = Db.Projects.Find(id);
            return project;
        }

        public void EditProject(Project project)
        {
            Db.Entry(project).State = EntityState.Modified;
            Db.SaveChanges();
        }

        private bool IsValidProjectDate(Project project)
        {
            DateTime? endDate = project.EndDate;
            if (endDate == null) endDate = DateTime.MaxValue;
            if (project.StartDate > endDate) return false;
            return true;
        }

        public void DeleteProject(int id)
        {
            Project project = Db.Projects.Find(id);
            Db.Projects.Remove(project);
            Db.SaveChanges();
        }

        public void CreateProject(Project project)
        {
            Db.Projects.Add(project);
            Db.SaveChanges();
        }

        public void CreateProjectLeading(ProjectLeading leading)
        {
            Db.ProjectLeadings.Add(leading);
            Db.SaveChanges();
        }

        public void CreateProjectAllocation(ProjectAllocation allocation)
        {
            Db.ProjectAllocations.Add(allocation);
            Db.SaveChanges();
        }

        public ProjectBookingTime GetBooking(int? id)
        {
            return Db.ProjectBookingTimes.Find(id);
        }
        public ActionResult DeleteBooking(int? id, int? controllerId)
        {
            ProjectBookingTime booking = GetBooking(id);
            DeleteCurrentProjectAllocation(booking);
            Db.ProjectBookingTimes.Remove(booking);
            Db.SaveChanges();
            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Details", id = controllerId }));
        }

        public ActionResult EditBooking(ProjectBookingTime booking, int? id)
        {
            Db.Entry(booking).State = EntityState.Modified;
            String proof = ProofBookingDate(booking);
            if (proof != "false")
            {
                Db.Entry(booking).State = EntityState.Unchanged;
                return new RedirectToRouteResult(new RouteValueDictionary(new { action = "EditBooking", Error = proof }));
            }
            Db.SaveChanges();
            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Details", id = id }));
        }

        private string ProofBookingDate(ProjectBookingTime booking)
        {
            WorkingSessionAccess wAccess = new WorkingSessionAccess(User.UserName);
            if (booking.End < booking.Start) return "itself";
            WorkingSession session = wAccess.GetWorkingSession(booking.WorkingSessionId);
            if (booking.End == null) return "WorkingSession (Projectbookingtime has to be closed for modifying)";
            DateTime? sessionEnd = session.End;
            if (sessionEnd == null) sessionEnd = DateTime.Now;
            if (booking.Start < session.Start || booking.End > sessionEnd) return "WorkingSession ID = " + session.WorkingSessionId;
            foreach (Break br in wAccess.GetWorkingSessionBreaks(session.WorkingSessionId))
            {
                if (booking.End > br.Start && booking.End < br.End) return "Break ID = " + br.BreakId;
                if (booking.Start > br.Start && booking.Start < br.End) return "Break ID = " + br.BreakId;
            }
            foreach (ProjectBookingTime bo in wAccess.GetWorkingSessionProjectBookingTimes(session.WorkingSessionId))
            {
                if (booking.BookingId == bo.BookingId) continue;
                if (booking.End > bo.Start && booking.End < bo.End) return "Booking ID = " + bo.BookingId;
                if (booking.Start > bo.Start && booking.Start < bo.End) return "Booking ID = " + bo.BookingId;
            }
            return "false";
        }

        public void UpdateProjectLeader(Project project, int leader)
        {
            var manager = new UserManager<ApplicationUser, int>(new UserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(new wt4uDBContext()));
            ApplicationUser employee = manager.FindById(leader);

            if (employee != null)
            {
                var leadings = Db.ProjectLeadings.Where(p => p.ProjectID == project.ProjectId && p.EndDate == null).ToList();
                if (leadings.Count() > 0)
                {
                    foreach (ProjectLeading pl in leadings)
                    {
                        pl.EndDate = DateTime.Now;
                    }
                    Db.SaveChanges();
                }


                ProjectLeading leading = new ProjectLeading();
                leading.ProjectID = project.ProjectId;
                leading.EmployeeId = employee.Id;
                leading.StartDate = DateTime.Now;
                leading.EndDate = null;

                CreateProjectLeading(leading);
            }
        }

        public String UpdateProjectAllocation(Project project, int[] team)
        {
            var allocations = Db.ProjectAllocations.Where(p => p.ProjectId == project.ProjectId && p.EndDate == null);
            if (allocations.Count() > 0)
            {
                if (project.isClosed)
                {
                    String users = CheckRunningProjectBookingTimes(project, allocations.ToList<ProjectAllocation>());
                    if (users.Length > 0)
                    {
                        return "Following Users have to stop working on the project before you can close it: " + users.Substring(0, users.Length - 2);
                    }
                    var currents = GetIsCurrentProjectAllocations(project);
                    foreach (ProjectAllocation a in currents)
                    {
                        a.IsCurrentProject = false;
                    }
                }
                List<ProjectAllocation> deleted = new List<ProjectAllocation>();
                foreach (ProjectAllocation pa in allocations)
                {
                    if (Array.Find(team, i => i == pa.EmployeeId) > 0)
                    {
                        team = team.Where(j => j != pa.EmployeeId).ToArray();
                    }
                    else
                    {
                        deleted.Add(pa);
                    }
                }
                String deletedUsers = CheckRunningProjectBookingTimes(project, deleted);
                if (deletedUsers.Length > 0)
                {
                    return "Following Users you want to delete are working on this project: " + deletedUsers.Substring(0, deletedUsers.Length - 2);
                }
                foreach (ProjectAllocation a in deleted)
                {
                    a.EndDate = DateTime.Now;
                    CleanCurrentProject(project, a.EmployeeId);
                }

                Db.SaveChanges();
            }
            var manager = new UserManager<ApplicationUser, int>(new UserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(new wt4uDBContext()));
            foreach (int i in team)
            {
                ApplicationUser employee = manager.FindById(i);
                if (employee != null)
                {
                    ProjectAllocation allocation = new ProjectAllocation();
                    allocation.ProjectId = project.ProjectId;
                    allocation.EmployeeId = employee.Id;
                    allocation.IsCurrentProject = false;
                    allocation.StartDate = DateTime.Now;
                    allocation.EndDate = null;

                    CreateProjectAllocation(allocation);
                }

            }
            return "";
        }

        public String CheckRunningProjectBookingTimes(Project project, List<ProjectAllocation> allocations)
        {
            String res = "";
            foreach (ProjectAllocation pa in allocations)
            {
                if (HasRunningProjectBookingTimes(project, pa.EmployeeId)) res += pa.Employee.UserName + ", ";
            }
            return res;
        }

        public bool HasRunningProjectBookingTimes(Project project, int id)
        {
            var bookings = Db.ProjectBookingTimes.Where(p => p.End == null && p.ProjectId == project.ProjectId && p.WorkingSession.EmployeeId == id);
            if (bookings.Count() > 0) return true;
            return false;
        }

        public void CleanCurrentProject(Project project, int id)
        {
            var allocations = Db.ProjectAllocations.Where(p => p.EmployeeId == id && p.ProjectId == project.ProjectId && p.IsCurrentProject);
            if (allocations.Count() > 0)
            {
                ProjectAllocation pa = allocations.First();
                pa.IsCurrentProject = false;
                Db.SaveChanges();
            }
        }

        public List<ProjectAllocation> GetIsCurrentProjectAllocations(Project project)
        {
            var allocations = Db.ProjectAllocations.Where(p => p.ProjectId == project.ProjectId && p.IsCurrentProject);
            return allocations.ToList<ProjectAllocation>();
        }

        public bool ProofAuthorizationProject(int? id)
        {
            return (Db.ProjectLeadings.Where(l => l.ProjectID == id && l.EmployeeId == User.Id && l.EndDate == null).Count() > 0) ||
                (Db.ProjectAllocations.Where(p => p.ProjectId == id && p.EmployeeId == User.Id && p.EndDate == null).Count() > 0) ||
                User.Roles.First<CustomUserRole>().RoleId == 1;
        }

        public bool ProofAuthorizationEditProject(int? id)
        {
            return (Db.ProjectLeadings.Where(l => l.ProjectID == id && l.EmployeeId == User.Id && l.EndDate == null).Count() > 0) ||
                User.Roles.First<CustomUserRole>().RoleId == 1;
        }

        private string AppendZero(int num)
        {
            if (num < 10)
            {
                return "0" + num;
            }
            return "" + num;
        }

        public dynamic GetEmployeeTimes(Project project)
        {
            var allocations = Db.ProjectAllocations.Where(p => p.ProjectId == project.ProjectId).OrderBy(n => n.Employee.Name).ThenBy(f => f.Employee.FirstName).ToList();

            List<ApplicationUser> team = new List<ApplicationUser>();
            foreach (ProjectAllocation pa in allocations)
            {
                var user = team.Where(u => u.Id == pa.EmployeeId).ToList();
                if (user.Count == 0)
                {
                    team.Add(pa.Employee);
                }
            }
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();

            TimeSpan userTime;

            foreach (ApplicationUser a in team)
            {
                var times = Db.ProjectBookingTimes.Where(p => p.ProjectId == project.ProjectId && p.WorkingSession.EmployeeId == a.Id).ToList();
                if (times.Count > 0)
                {
                    dynamic expando = new ExpandoObject();
                    expando.Name = a.Name + " " + a.FirstName;

                    userTime = new TimeSpan(0);
                    foreach (ProjectBookingTime pb in times)
                    {
                        if (pb.End != null)
                        {
                            TimeSpan span = pb.End.Value.Subtract(pb.Start);
                            userTime += span;
                        }
                    }
                    expando.Time = AppendZero(userTime.Hours) + ":" + AppendZero(userTime.Minutes) + ":" + AppendZero(userTime.Seconds) + " Hours";
                    dynamicList.Add(expando);
                }

            }
            return dynamicList;
        }

        public dynamic GetMonthTimes(Project project)
        {
            List<ExpandoObject> dynamicList = new List<ExpandoObject>();
            var bookings = Db.ProjectBookingTimes.Where(p => p.ProjectId == project.ProjectId && p.End != null).OrderBy(n => n.Start).ToList();

            if (bookings.Count == 0)
            {
                return new List<ExpandoObject>();
            }
            DateTime min = bookings.First().Start;
            DateTime current = DateTime.Now;

            foreach (ProjectBookingTime pb in bookings)
            {
                if (pb.End.Value < min)
                    min = pb.End.Value;
            }

            min = min.AddDays(-min.Day + 1);
            min = new DateTime(min.Year, min.Month, min.Day, 0, 0, 0);

            while (min <= current)
            {
                TimeSpan duration = new TimeSpan(0);
                foreach (ProjectBookingTime b in bookings)
                {
                    if (b.End.Value.Year == current.Year && b.End.Value.Month == current.Month)
                    {
                        duration += b.End.Value.Subtract(b.Start);
                    }
                }
                dynamic expando = new ExpandoObject();
                expando.Month = current.Year.ToString() + "-" + current.Month.ToString();
                expando.Time = AppendZero(duration.Hours) + ":" + AppendZero(duration.Minutes) + ":" + AppendZero(duration.Seconds) + " Hours";
                dynamicList.Add(expando);

                current = current.AddMonths(-1);
            }

            return dynamicList;
        }

        public List<KeyValuePair<int, String>> GetProjectsForReport()
        {
            var allProjects = GetAllProjectsToLead();
            if (User.Roles.First<CustomUserRole>().RoleId == 1)
            {
                allProjects = GetAllProjects();
            }

            var projects = new List<KeyValuePair<int, String>>();

            foreach (dynamic d in allProjects)
            {
                projects.Add(new KeyValuePair<int, string>(d.ProjectId, d.Name));
            }

            projects = projects.OrderBy(p => p.Value).ToList();

            return projects;
        }

    }
}