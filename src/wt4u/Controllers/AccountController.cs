using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using wt4u.Models;
using wt4u.BusinessLogic;
using System.Net;
using System.Transactions;
using System;
using System.Collections.Generic;
using System.Web.Security;

namespace wt4u.Controllers
{
    [SecurityAuthorize]
    public class AccountController : Controller
    {
        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Users()
        {
            return View(new AccountAccess(User.Identity.Name).getAllUsers());
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await new AccountAccess(User.Identity.Name).FindUser(model.UserName, model.Password);
                if (user != null && user.isActive)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
            return View(model);
        }

        //
        // GET: /Account/Register
        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Register()
        {
            ViewBag.RoleList = getAllRoles();
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [SecurityAuthorize(Roles = "Employer")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            ViewBag.RoleList = getAllRoles();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() { UserName = model.UserName, FirstName = model.FirstName, Name = model.Name, Address = model.Address, ZipCode = model.ZipCode, City = model.City, isActive = true };
                var result = await new AccountAccess(User.Identity.Name).createUser(user, model.Password);
                if (result.Succeeded)
                {
                    setRole(user.UserName, Request.Form["Role"]);
                    TempData["userCreatedMessage"] = "<div class=\"alert alert-success\">User " + user.UserName + " created!</div>";
                    return RedirectToAction("Register", "Account");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
            return View(model);
        }

        // GET: /Project/Edit/5
        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser u = new AccountAccess(User.Identity.Name).getUser(id);
            if (u == null)
            {
                return HttpNotFound();
            }
            ViewBag.RoleList = getAllRoles(u);
            ViewBag.userName = u.UserName;
            EditViewModel model = new EditViewModel();
            model.UserName = u.UserName;
            model.FirstName = u.FirstName;
            model.Name = u.Name;
            model.Address = u.Address;
            model.ZipCode = u.ZipCode;
            model.City = u.City;
            model.isActive = u.isActive;
            return View(model);
        }


        // POST: /Project/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SecurityAuthorize(Roles = "Employer")]
        public async Task<ActionResult> Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!User.Identity.Name.Equals(model.UserName))
                {
                    return new wt4uTransaction().transact(delegate() { return new AccountAccess(User.Identity.Name).editUser(model); });
                }
                else
                {
                    IdentityResult result = await new AccountAccess(User.Identity.Name).editownUser(model);
                    if (model.Password != null)
                    {
                        IdentityResult rem = await new AccountAccess(User.Identity.Name).RemovePassword(model.Id);
                        IdentityResult addpw = await new AccountAccess(User.Identity.Name).AddPassword(model.Id, model.Password);
                    }
                    return RedirectToAction("Users");
                }
            }
            else
            {
                return RedirectToAction("Users");
            }
        }

        // GET: /Project/Details/5
        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Details(int? id)
        {
            ApplicationUser u = new AccountAccess(User.Identity.Name).getUser(id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountAccess aa = new AccountAccess(u.UserName);
            ProjectAccess pa = new ProjectAccess(u.UserName);
            ViewBag.User = u;
            ViewBag.ProjectAllocations = pa.GetUserProjectList();
            ViewBag.WorkingTime = aa.getWorkingTime(id);

            return View();
        }

        //
        // GET: /Account/Manage
        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ihr Kennwort wurde geändert."
                : message == ManageMessageId.SetPasswordSuccess ? "Ihr Kennwort wurde festgelegt."
                : message == ManageMessageId.RemoveLoginSuccess ? "Die externe Anmeldung wurde entfernt."
                : message == ManageMessageId.Error ? "Fehler"
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SecurityAuthorize(Roles = "Employer,Employee")]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await new AccountAccess(User.Identity.Name).ChangePassword(int.Parse(User.Identity.GetUserId()), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await new AccountAccess(User.Identity.Name).AddPassword(int.Parse(User.Identity.GetUserId()), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten; Formular erneut anzeigen.
            return View(model);
        }


        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SecurityAuthorize(Roles = "Employer,Employee")]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            new AccountAccess(User.Identity.Name).Dispose(disposing);
            base.Dispose(disposing);
        }

        #region Hilfsprogramme

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await new AccountAccess(User.Identity.Name).createIdentity(user);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            return new AccountAccess(User.Identity.Name).HasPassword(int.Parse(User.Identity.GetUserId()));
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }
        }
        #endregion

        public ActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleCreate(string RoleName)
        {
            CustomRole role = new CustomRole(RoleName);

            if (ModelState.IsValid)
            {
                new AccountAccess(User.Identity.Name).createRole(role);
            }
            return RedirectToAction("RoleIndex");
        }

        public ActionResult RoleIndex()
        {
            var roles = new AccountAccess(User.Identity.Name).getRoles();
            return View(roles);
        }

        public string getUserRole(ApplicationUser user)
        {
            string result = new AccountAccess(User.Identity.Name).getUserRole(user);
            return result;
        }

        public List<SelectListItem> getAllRoles(ApplicationUser editUser)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            List<CustomRole> allRoles = new AccountAccess(User.Identity.Name).getRoles();
            if (editUser.Roles.Count > 0)
            {
                foreach (CustomRole c in allRoles)
                {
                    if (editUser.Roles.First().RoleId == c.Id)
                    {
                        result.Add(new SelectListItem
                        {
                            Text = c.Name,
                            Value = c.Name,
                        });
                    }
                }
                foreach (CustomRole c in allRoles)
                {
                    if (editUser.Roles.First().RoleId != c.Id)
                    {
                        result.Add(new SelectListItem
                        {
                            Text = c.Name,
                            Value = c.Name,
                        });
                    }
                }
            }
            else
            {
                foreach (CustomRole c in allRoles)
                {
                    result.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Name,
                    });
                }
            }
            return result;
        }

        public List<SelectListItem> getAllRoles()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            List<CustomRole> allRoles = new AccountAccess(User.Identity.Name).getRoles();
            foreach (CustomRole c in allRoles)
            {

                result.Add(new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Name,
                });
            }
            return result;
        }

        public void setRole(string username, string rolename)
        {
            new AccountAccess(username).setRole(username, rolename);
        }

        public void setPassword(ApplicationUser user, string newPassword)
        {
            if (newPassword.Length > 0)
                new AccountAccess(user.UserName).ChangePassword(user.Id, newPassword);
        }

        [SecurityAuthorize(Roles = "Employer")]
        public ActionResult Report(int? id)
        {
            AccountAccess a = new AccountAccess(User.Identity.Name);
            ApplicationUser u = a.getUser(id);
            if (id == null)
            {
                u = a.getUser(int.Parse(User.Identity.GetUserId()));
            }
            AccountAccess aa = new AccountAccess(u.UserName);
            ProjectAccess pa = new ProjectAccess(u.UserName);
            ViewBag.User = u;
            ViewBag.ProjectAllocations = pa.GetUserProjectList();
            ViewBag.WorkingTime = aa.getWorkingTime(id);
            ViewBag.EmployeeList = a.GetEmployeeForReport();

            return View();
        }

    }
}