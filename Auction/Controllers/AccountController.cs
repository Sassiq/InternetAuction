using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using OnlineAuctionProject.Models;
using System.Web.Security;
using Microsoft.AspNet.Identity.Owin;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using OnlineAuctionProject.ManageWebsiteLanguage;
using OnlineAuctionProject.Resources;
using OnlineAuctionProject.Repository;


namespace OnlineAuctionProject.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login //Modal Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Finding a user with the provided username and password
                var user = await UserManager.FindAsync(model.UserName, model.Password);

                //Checking if the user was found and it's active
                if (user != null && user.Active)
                {
                    //Signing in
                    await SignInAsync(user, isPersistent: false);

                    //Calculating logins count and saving last login date and time
                    ApplicationUser CurrentUser = db.Users.Find(user.Id);
                    CurrentUser.LoginCount++;
                    Session.Add("LastLoginDateTime", CurrentUser.LastLoginDateTime);
                    CurrentUser.LastLoginDateTime = DateTime.Now;
                    db.SaveChanges();


                    return Json(true);

                }
            }

            return Json(false);
        }

        //Normal page Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login2(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //Finding a user with the provided username and password
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                //Checking if the user was found and it's active
                if (user != null && user.Active)
                {
                    //Singing in
                    await SignInAsync(user, isPersistent: false);

                    //Calculating logins count and saving last login date and time
                    ApplicationUser CurrentUser = db.Users.Find(user.Id);
                    Session.Add("LastLoginDateTime", CurrentUser.LastLoginDateTime);
                    CurrentUser.LoginCount++;
                    CurrentUser.LastLoginDateTime = DateTime.Now;
                    db.SaveChanges();

                    
                    return Redirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View("Login", model); 
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string returnUrl)
        {
            try
            {
                //Getting list of countries
                List<Country> countries = db.Countries.Where(x => x.Visible).ToList();

                //Displaying countries in current website language
                switch(SiteLanguages.GetCurrentLanguageCulture())
                {
                    case "en-US":
                        ViewBag.Country = new SelectList(countries, "Name", "Name");
                        break;
                    case "ar-SA":
                        ViewBag.Country = new SelectList(countries, "Name", "Name_Ar");
                        break;
                    default:
                        ViewBag.Country = new SelectList(countries, "Name", "Name");
                        break;
                }
                ViewBag.ReturnUrl = returnUrl;
            }
            catch { }
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

            //Getting list of countries to display if registration failed
            List<Country> countries = db.Countries.ToList();
           
            if (ModelState.IsValid)
            {
                //Checking if the email is already used
                var user = db.Users.Where(x => x.Email == model.Email).SingleOrDefault();
                if(user != null)
                {
                    ViewBag.Country = new SelectList(countries, "Name", "Name");
                    ModelState.AddModelError("EmailUsed", Resource.EmailUsed);
                    return View(model);
                }

                //Checking if the username is already used
                user = db.Users.Where(x => x.UserName == model.UserName).SingleOrDefault();
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

                var roleStore = new RoleStore<IdentityRole>(db);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);

                switch(SiteLanguages.GetCurrentLanguageCulture())
                {
                    case "en-US":
                        user.PreferredInterfaceLanguage = "English";
                        break;
                    case "ar-SA":
                        user.PreferredInterfaceLanguage = "Arabic";
                        break;
                    default:
                        user.PreferredInterfaceLanguage = "English";
                        break;
                }
                db.SaveChanges();
                /*
                if (!roleManager.RoleExists("User"))
                {
                    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                    role.Name = "User";
                    roleManager.Create(role);
                }
                */
                
                //assigning user to role [User]
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

            //Displaying countries in current website language
            switch (SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.Country = new SelectList(countries, "Name", "Name");
                    break;
                case "ar-SA":
                    ViewBag.Country = new SelectList(countries, "Name", "Name_Ar");
                    break;
                default:
                    ViewBag.Country = new SelectList(countries, "Name", "Name");
                    break;
            }
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

        //GET //Forgot password
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //GET // Forgot password
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            //Finding user by their email
            ApplicationUser user = db.Users.SingleOrDefault(x => x.Email == model.Email);
            try
            {
                if (user != null)
                {
                    //Creating a new password for user
                    string newPassword = "";
                    int passwordSize = 10;
                    char[] chars = new char[62];
                    chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
                    byte[] data = new byte[1];

                    using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
                    {
                        crypto.GetNonZeroBytes(data);
                        data = new byte[passwordSize];
                        crypto.GetNonZeroBytes(data);
                    }
                    StringBuilder result = new StringBuilder(passwordSize);
                    foreach (byte b in data)
                    {
                        result.Append(chars[b % (chars.Length)]);
                    }
                    newPassword = result.ToString();

                    //Hashing the new password
                    string newPasswordHashed = UserManager.PasswordHasher.HashPassword(newPassword);
                    user.PasswordHash = newPasswordHashed;
                    db.SaveChanges();

                    //Sending the new password to user by email
                    string subject = "", body = "";
                    switch(user.PreferredInterfaceLanguage)
                    {
                        case "English":
                            subject = "Online Auctions - Password Changed!";
                            body = "Dear User: " + user.UserName + ". \n" +
                                   "Your new password is:\n [ " + newPassword + " ] \n" +
                                   "for a safer account, please change it when you login to your account.\n" +
                                   "\n\n\n Online auctions team.";
                            break;
                        case "Arabic":
                            subject = "المزاد الالكتروني - تم تغيير كلمة المرور";
                            body = "المستخدم العزيز: " + user.UserName + ". \n" +
                                   "كلمة المرور الجديدة الخاصة بك هي: \n[ " + newPassword + " ] \n" +
                                   "الرجاء تغييرها عند تسجيل دخولك القادم من أجل حساب اكثر امانا.\n" +
                                   "\n\n\n فريق ادارة موقع المزاد العلني الالكتروني.";
                            break;
                        default:
                            subject = "Online Auctions: Password Changed!";
                            body = "Dear User: " + user.UserName + ". \n" +
                                   "Your new password is:\n [ " + newPassword + " ] \n" +
                                   "for a safer account, please change it when you login to your account.\n" +
                                   "\n\n\n Online auctions team.";
                            break;
                    }

                    Email email = new Email(model.Email, subject, body);
                    email.Send();

                    ViewBag.PasswordChangedMsg = Resource.PasswordChangedMsg;
                    return View();

                }
                ViewBag.PasswordChangedMsg = Resource.PasswordChangedMsgEmailNotFound;
                return View(model);
            }
            catch
            {

            }
            //If something went wrong, stay on page and display error message
            ViewBag.PasswordChangedMsg = Resource.ErrorSendingEmail;
            return View(model);
        }

        //GET // Editing user profile
        [HttpGet]
        [Authorize(Roles = "Admin, User, Support, Supervisor")]
        public ActionResult EditProfile()
        {
            //Getting list of countries
            List<Country> countries = db.Countries.Where(x => x.Visible).ToList();
            //Finding user
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

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
            Country country = db.Countries.SingleOrDefault(x => x.Name == user.Country);

            switch (SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.Country = new SelectList(countries, "Id", "Name", country.Id);
                    break;
                case "ar-SA":
                    ViewBag.Country = new SelectList(countries, "Id", "Name_Ar", country.Id);
                    break;
                default:
                    ViewBag.Country = new SelectList(countries, "Id", "Name", country.Id);
                    break;
            }
            return View(editProfileModel);
        }


        // POST // Editing user profile
        [HttpPost]
        [Authorize(Roles = "Admin, User, Support, Supervisor")]
        public ActionResult EditProfile(EditProfileModel model)
        {
            List<Country> countries = db.Countries.Where(x => x.Visible).ToList();
            ApplicationUser Me = db.Users.Find(User.Identity.GetUserId());
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser user = db.Users.SingleOrDefault(x => x.Email == model.Email && x.Id != Me.Id);
                    if(user != null)
                    {
                        ViewBag.EmailUsed = Resource.EmailUsed;
                        switch (SiteLanguages.GetCurrentLanguageCulture())
                        {
                            case "en-US":
                                ViewBag.Country = new SelectList(countries, "Id", "Name");
                                break;
                            case "ar-SA":
                                ViewBag.Country = new SelectList(countries, "Id", "Name_Ar");
                                break;
                            default:
                                ViewBag.Country = new SelectList(countries, "Id", "Name");
                                break;
                        }

                        return View(model);
                    }

                    //Changing user information
                    if (Me != null)
                    {
                        Me.FirstName = model.FirstName;
                        Me.LastName = model.LastName;
                        Me.Gender = model.Gender;
                        Me.Email = model.Email;
                        Me.PhoneNumber = model.PhoneNumber;
                        Me.Country = db.Countries.Find(int.Parse(Request["Country"].ToString())).Name;
                        Me.City = model.City;
                        Me.Street = model.Street;
                        Me.ZipCode = model.ZipCode;
                    }

                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                
            }
            switch (SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.Country = new SelectList(countries, "Id", "Name");
                    break;
                case "ar-SA":
                    ViewBag.Country = new SelectList(countries, "Id", "Name_Ar");
                    break;
                default:
                    ViewBag.Country = new SelectList(countries, "Id", "Name");
                    break;
            }

            return View(model);
        }



        //
        // POST: /Account/LogOff
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        ///////////////////////////////////////////////////////////////////

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