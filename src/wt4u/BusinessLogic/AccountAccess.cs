using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using wt4u.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Net;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Web.Security;

namespace wt4u.BusinessLogic
{
    public class AccountAccess
    {
        private wt4uDBContext db;
        UserManager<ApplicationUser, int> manager;
        UserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim> store;
        ApplicationUser User;

        public AccountAccess(String Username)
        {
            db = new wt4uDBContext();
            store = new UserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(new wt4uDBContext());
            manager = new UserManager<ApplicationUser, int>(store);
            if (Username.Length > 0)
            {
                User = db.Users.Where(e => e.UserName.Equals(Username)).First();
                User = manager.FindById(User.Id);
            }
        }

        public List<ApplicationUser> getAllUsers()
        {
            return manager.Users.ToList<ApplicationUser>();
        }

        public SelectList getUserSelectList(ApplicationUser selection = null)
        {
            var Users = getAllUsers();
            var selectUserOptions = Users.OrderBy(u => u == null ? "" : u.Name).ThenBy(u => u == null ? "" : u.FirstName).ThenBy(u => u == null ? "" : u.UserName).Select(u => new
            {
                Id = u == null ? 0 : u.Id,
                FullName = u == null ? "" : u.Name + " " + u.FirstName + " (" + u.UserName + ")"
            }).ToList();

            return new SelectList(selectUserOptions, "Id", "FullName", selection != null ? selection.Id : 0);
        }

        public SelectList getTeamSelectList(List<ApplicationUser> list)
        {
            var selectTeamOptions = list.OrderBy(u => u == null ? "" : u.Name).ThenBy(u => u == null ? "" : u.FirstName).ThenBy(u => u == null ? "" : u.UserName).Select(u => new
            {
                Id = u == null ? 0 : u.Id,
                FullName = u == null ? "" : u.Name + " " + u.FirstName + " (" + u.UserName + ")"
            }).ToList();

            return new SelectList(selectTeamOptions, "Id", "FullName");
        }

        public ApplicationUser getUser(int? id)
        {
            return db.Users.Find(id);
        }
        public ActionResult editUser()
        {
            db.Entry(User).State = EntityState.Modified;
            db.SaveChanges();

            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Users" }));
        }

        public ActionResult editUser(ApplicationUser user)
        {
            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Users" }));
            }

            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Users" }));
        }

        public ActionResult editUser(EditViewModel model)
        {
            ApplicationUser u = manager.FindById(model.Id);
            u.UserName = model.UserName;
            u.Name = model.Name;
            u.FirstName = model.FirstName;
            u.Address = model.Address;
            u.City = model.City;
            u.ZipCode = model.ZipCode;
            u.isActive = model.isActive;
            if(model.Password != null)
            {
                ChangePassword(model.Id, model.Password);
            }
            setRole(u, model.Role);
            manager.Update(u);
            db.SaveChanges();

            return new RedirectToRouteResult(new RouteValueDictionary(new { action = "Users" }));
        }

        public Task<IdentityResult> editownUser(EditViewModel model)
        {
            User.UserName = model.UserName;
            User.Name = model.Name;
            User.FirstName = model.FirstName;
            User.Address = model.Address;
            User.City = model.City;
            User.ZipCode = model.ZipCode;
            return manager.UpdateAsync(User);
        }

        public Task<ApplicationUser> FindUser(string username, string password)
        {
            return manager.FindAsync(username, password);
        }

        public Task<IdentityResult> createUser(ApplicationUser user, string Password)
        {
            return manager.CreateAsync(user, Password);
        }

        public IdentityResult createUserNotAsync(ApplicationUser user, string Password)
        {
            return manager.Create<ApplicationUser, int>(user, Password);
        }

        public IdentityResult deleteUser(ApplicationUser u)
        {
            return manager.Delete<ApplicationUser, int>(u);
        }

        public Task<IdentityResult> ChangePassword(int id, string oldPassword, string newPassword)
        {
            return manager.ChangePasswordAsync(id, oldPassword, newPassword);
        }

        public void ChangePassword(int id, string newPassword)
        {
            manager.RemovePassword(id);
            manager.AddPassword(id, newPassword);
        }

        public Task<IdentityResult> RemovePassword(int id)
        {
            return manager.RemovePasswordAsync(id);
        }

        public Task<IdentityResult> AddPassword(int id, string newPassword)
        {
            return manager.AddPasswordAsync(id, newPassword);
        }

        public void Dispose(bool disposing)
        {
            if (disposing && manager != null)
            {
                manager.Dispose();
                manager = null;
            }
        }

        public Task<System.Security.Claims.ClaimsIdentity> createIdentity(ApplicationUser user)
        {
            return manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
        }

        public IList<UserLoginInfo> getLogins(int id)
        {
            return manager.GetLogins(id);
        }

        public bool HasPassword(int userId)
        {
            var user = manager.FindById(userId);
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public void createRole(CustomRole role)
        {
            db.Roles.Add(role);
            db.SaveChanges();
        }

        public List<CustomRole> getRoles()
        {
            return db.Roles.ToList();
        }

        public CustomRole getRoleById(int id)
        {
            return db.Roles.Where(r => r.Id.Equals(id)).FirstOrDefault();
        }

        public string getUserRole(ApplicationUser user)
        {
            var roles = manager.GetRoles(user.Id);
            if (roles.Count == 0)
            {
                return "";
            }
            var result = roles.First();
            return result;
        }
        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        public List<ExpandoObject> getWorkingTime(int? id)
        {
            List<ExpandoObject> wtlist = new List<ExpandoObject>();
            var workingSessions = db.WorkingSessions.Where(w => w.EmployeeId.Equals(User.Id) && w.End != null).ToList<WorkingSession>();
            if (workingSessions == null || workingSessions.Count() == 0)
            {
                return new List<ExpandoObject>();
            }
            DateTime min = workingSessions[0].End.Value;
            DateTime current = DateTime.Now;
            foreach (WorkingSession ws in workingSessions)
            {
                if (ws.End.Value < min)
                    min = ws.End.Value;
            }

            min = min.AddDays(-min.Day + 1);
            min = new DateTime(min.Year, min.Month, min.Day, 0, 0, 0);

            while (min <= current)
            {
                TimeSpan duration = new TimeSpan(0);
                foreach (WorkingSession w in workingSessions)
                {
                    if (w.End.Value.Year == current.Year && w.End.Value.Month == current.Month)
                    {
                        duration += w.End.Value.Subtract(w.Start);
                    }
                }
                dynamic expando = new ExpandoObject();
                expando.month = current.Year.ToString() + "-" + current.Month.ToString();
                expando.workingTime = duration;
                wtlist.Add(expando);

                current = current.AddMonths(-1);
            }
            return wtlist;
        }

        public void setRole(string username, string rolename)
        {
            if (User.Roles != null && User.Roles.Count > 0)
            {
                int oldRoleId = User.Roles.FirstOrDefault().RoleId;
                string oldRoleName = db.Roles.Where(m => m.Id == oldRoleId).First().Name;
                manager.RemoveFromRole(User.Id, oldRoleName);
            }
            manager.AddToRole(User.Id, rolename);
        }

        public void setRole(ApplicationUser u, string rolename)
        {
            if (u.Roles != null && u.Roles.Count > 0)
            {
                int oldRoleId = u.Roles.FirstOrDefault().RoleId;
                string oldRoleName = db.Roles.Where(m => m.Id == oldRoleId).First().Name;
                manager.RemoveFromRole(u.Id, oldRoleName);
            }
            manager.AddToRole(u.Id, rolename);
        }

        internal dynamic GetEmployeeForReport()
        {
            var allUsers = getAllUsers();

            var users = new List<KeyValuePair<int, String>>();

            foreach (ApplicationUser u in allUsers)
            {
                users.Add(new KeyValuePair<int, string>(u.Id, u.Name + " " + u.FirstName + " (" + u.UserName + ")"));
            }

            users = users.OrderBy(p => p.Value).ToList();

            return users;
        }
    }
}