using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using OnlineAuctionProject.Models;
using OnlineAuctionProject.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace OnlineAuctionProject.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private AuctionContext _db;

        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuctionContext())))
        {
            _db = new AuctionContext();
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
            _db = new AuctionContext();
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null && user.Active)
                {
                    await SignInAsync(user, isPersistent: false);

                    var currentUser = _db.Users.Find(user.Id);
                    currentUser.LoginCount++;
                    currentUser.LastLoginDateTime = DateTime.Now;
                    await _db.SaveChangesAsync();

                    Session.Add("LastLoginDateTime", currentUser.LastLoginDateTime);

                    return Json(true);
                }
            }

            return Json(false);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login2(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null && user.Active)
                {
                    await SignInAsync(user, isPersistent: false);

                    var CurrentUser = _db.Users.Find(user.Id);
                    Session.Add("LastLoginDateTime", CurrentUser.LastLoginDateTime);
                    CurrentUser.LoginCount++;
                    CurrentUser.LastLoginDateTime = DateTime.Now;
                    _db.SaveChanges();

                    
                    return Redirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            return View("Login", model); 
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string returnUrl)
        {
            if (Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
            {
                var countries = _db.Countries.Where(x => x.Visible).ToList();

                ViewBag.Country = new SelectList(countries, "Name", "Name");
                ViewBag.ReturnUrl = returnUrl;
            }

            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            returnUrl = ViewBag.ReturnUrl;

            var countries = _db.Countries.ToList();
           
            if (ModelState.IsValid)
            {
                //Checking if the email is already used
                var user = _db.Users.Where(x => x.Email == model.Email).SingleOrDefault();
                if(user != null)
                {
                    ViewBag.Country = new SelectList(countries, "Name", "Name");
                    ModelState.AddModelError("EmailUsed", Resource.EmailUsed);
                    return View(model);
                }

                //Checking if the username is already used
                user = _db.Users.Where(x => x.UserName == model.UserName).SingleOrDefault();
                if(user != null)
                {
                    ViewBag.Country = new SelectList(countries, "Name", "Name");
                    ModelState.AddModelError("UsernameUsed", Resource.UsernameUsed);
                    return View(model);
                }

                //Creating a new user
                user = new ApplicationUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Country = model.Country,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    Street = model.Street,
                    UserName = model.UserName,
                    Active = true,
                    LoginCount = 0,
                    RegistrationDateTime = DateTime.Now,
                    AccountNumber = model.AccountNumber
                };
                var result = await UserManager.CreateAsync(user, model.Password);

                var roleStore = new RoleStore<IdentityRole>(_db);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(_db);
                var userManager = new UserManager<ApplicationUser>(userStore);

                user.PreferredInterfaceLanguage = "English";
                await _db.SaveChangesAsync();

                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, "User");
                    Session.Add("newUser", true);
                    return Redirect(returnUrl);
                }
                else
                {
                    AddErrors(result);
                }
            }

            ViewBag.Country = new SelectList(countries, "Name", "Name");

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Manage/Change password
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? Resource.PasswordChanged
                : message == ManageMessageId.Error ? Resource.ErrorOccured
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [Authorize(Roles = "Admin, User, Support, Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    //Changing user password
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
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
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
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

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == model.Email);
            if (user != null)
            {
                var password = model.Email.GetHashCode().ToString();
                var hashPassword =  UserManager.PasswordHasher.HashPassword(password);
                user.PasswordHash = hashPassword;
                await _db.SaveChangesAsync();
                var subject = "Online Auctions: Password Changed!";
                var body = "Dear User: " + user.UserName + ". \n" +
                       "Your new password is:\n [ " + password + " ] \n" +
                       "for a safer account, please change it when you login to your account.\n" +
                       "\n\n\n Online auctions team.";

                ViewBag.PasswordChangedMsg = Resource.ErrorSendingEmail;
                return View(model);
            }

            ViewBag.PasswordChangedMsg = Resource.PasswordChangedMsgEmailNotFound;
            return View(model);
        }

        //GET // Editing user profile
        [HttpGet]
        [Authorize(Roles = "Admin, User, Support, Supervisor")]
        public ActionResult EditProfile()
        {
            //Getting list of countries
            List<Country> countries = _db.Countries.Where(x => x.Visible).ToList();
            //Finding user
            ApplicationUser user = _db.Users.Find(User.Identity.GetUserId());

            EditProfileModel editProfileModel = new EditProfileModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Country = user.Country,
                City = user.City,
                Street = user.Street,
                ZipCode = user.ZipCode,
                AccountNumber = user.AccountNumber
            };
            Country country = _db.Countries.SingleOrDefault(x => x.Name == user.Country);
            ViewBag.Country = new SelectList(countries, "Id", "Name", country.Id);

            return View(editProfileModel);
        }


        // POST // Editing user profile
        [HttpPost]
        [Authorize(Roles = "Admin, User, Support, Supervisor")]
        public async Task<ActionResult> EditProfile(EditProfileModel model)
        {
            var countries = await _db.Countries.Where(x => x.Visible).ToListAsync();
            var me = _db.Users.Find(User.Identity.GetUserId());
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == model.Email && x.Id != me.Id);
                    if(user != null)
                    {
                        ViewBag.EmailUsed = Resource.EmailUsed;
                        ViewBag.Country = new SelectList(countries, "Id", "Name");

                        return View(model);
                    }

                    //Changing user information
                    if (me != null)
                    {
                        me.FirstName = model.FirstName;
                        me.LastName = model.LastName;
                        me.Gender = model.Gender;
                        me.Email = model.Email;
                        me.PhoneNumber = model.PhoneNumber;
                        me.Country = _db.Countries.Find(int.Parse(Request["Country"].ToString())).Name;
                        me.City = model.City;
                        me.Street = model.Street;
                        me.ZipCode = model.ZipCode;
                    }

                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                
            }
            ViewBag.Country = new SelectList(countries, "Id", "Name");

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        #region Helpers

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
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
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
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
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

        #endregion
    }
}