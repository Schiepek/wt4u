using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Web.Mvc;

namespace wt4u.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string Address { get; set; }
        public int? ZipCode { get; set; }
        public string City { get; set; }
        public bool isActive { get; set; }
    }

    public class CustomRole : IdentityRole<int, CustomUserRole>
    {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
    }

    public class CustomUserRole : IdentityUserRole<int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }

    public class SecurityAuthorizeAttribute : AuthorizeAttribute
    {

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {            
            if (filterContext.HttpContext.User.Identity.Name.Equals(""))
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Home/Error");
            }
        }
    }

    public class wt4uDBContext : IdentityDbContext<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public wt4uDBContext()
            : base("wt4uDBContext")
        {
        }

        //public DbSet<ApplicationUser> Employees { get; set; }
        public DbSet<WorkingSession> WorkingSessions { get; set; }
        public DbSet<Break> Breaks { get; set; }
        public DbSet<ProjectBookingTime> ProjectBookingTimes { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectLeading> ProjectLeadings { get; set; }
        public DbSet<ProjectAllocation> ProjectAllocations { get; set; }
    }

}