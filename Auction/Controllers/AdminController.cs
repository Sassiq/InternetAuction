using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineAuctionProject.Models;
using System.IO;
using PagedList;
using PagedList.Mvc;
using System.Net.Mail;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OnlineAuctionProject.ManageWebsiteLanguage;
using OnlineAuctionProject.Resources;
using OnlineAuctionProject.Repository;

namespace OnlineAuctionProject.Controllers
{
    public class AdminController : BaseController
    {
        ApplicationDbContext db = new ApplicationDbContext();


        //GET // Administraion page
        [HttpGet]
        [Authorize(Roles = "Admin,Support,Supervisor")]
        public ActionResult Index()
        {
            return View();
        }

        //GET // Auctions Categories
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Categories(int? page, string Search)
        {
            //Getting list of categories
            IEnumerable<Category> categories = db.Categories.Where(x => x.Category_Name.Contains(Search.Trim())
                                                                    || x.Category_Name_Ar.Contains(Search.Trim())
                                                                    || String.IsNullOrEmpty(Search));


            IPagedList<Category> ordered = null;
            //Ordering the categories
            switch(SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ordered = categories.OrderBy(x => x.Category_Name).ToList().ToPagedList(page ?? 1, 6);
                    break;
                case "ar-SA":
                    ordered = categories.OrderBy(x => x.Category_Name_Ar).ToList().ToPagedList(page ?? 1, 6);
                    break;
                default:
                    ordered = categories.OrderBy(x => x.Category_Name).ToList().ToPagedList(page ?? 1, 6);
                    break;
            }

            return View(ordered);
        }

        //GET // Add new category
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCategory()
        {
            return View();
        }

        //POST // Add new category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCategory(Category model, HttpPostedFileBase uploadFile)
        {
            try
            {
                //Checking if the category already exists
                Category category = db.Categories.SingleOrDefault(x => x.Category_Name == model.Category_Name || x.Category_Name_Ar == model.Category_Name_Ar);

                if (category != null)
                {
                    ViewBag.CategoryAlreadyExists = Resource.CategoryAlreadyExists;
                    return View(model);
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        //If the admin did not upload an image, set a default image
                        string path = "~/Images/Items/no-thumbnail.png";
                        string fileName = "";

                        //Saving uploaded image
                        if (uploadFile != null && uploadFile.ContentLength > 0)
                        {
                            if (checkFileType(uploadFile.FileName))
                            {
                                fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + Path.GetFileName(uploadFile.FileName);
                                path = "~/Images/Categories/" + fileName;
                                uploadFile.SaveAs(Server.MapPath(path));
                            }
                            else
                            {
                                path = "~/Images/Items/no-thumbnail.png";
                            }
                        }
                        //Creating a new category
                        category = new Category()
                        {
                            Category_Name = model.Category_Name,
                            Category_Name_Ar = model.Category_Name_Ar,
                            Image = path,
                            Visible = true
                        };
                        db.Categories.Add(category);
                        db.SaveChanges();

                        return RedirectToAction("Categories", "Admin");
                    }
                }
            }
            catch
            {

            }
            return View(model);   
        }

        //GET // Edit category
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditCategory(int Id)
        {
            Category category = db.Categories.Find(Id);

            return View(category);
        }

        //POST // Edit category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditCategory(Category model, HttpPostedFileBase uploadFile)
        {
            try
            {
                //Getting  category to edit
                Category category = db.Categories.Find(model.Id);
                
                //Checking if a category with the same name already exists
                Category c = db.Categories.SingleOrDefault(x => (x.Category_Name == model.Category_Name
                                                             || x.Category_Name_Ar == model.Category_Name_Ar
                                                             || x.Category_Name == model.Category_Name_Ar
                                                             || x.Category_Name_Ar == model.Category_Name)
                                                             && x.Id != category.Id);
                if (c != null)
                {
                    ViewBag.CategoryAlreadyExists = Resource.CategoryAlreadyExists;
                    return View(model);
                }
                else
                {
                    //Editing the cateogry
                    string path = "";
                    string fileName = "";

                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        var CategoryImagePath = Server.MapPath(category.Image);

                        if (System.IO.File.Exists(CategoryImagePath))
                            System.IO.File.Delete(CategoryImagePath);

                        if (checkFileType(uploadFile.FileName))
                        {
                            fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + Path.GetFileName(uploadFile.FileName);
                            path = "~/Images/Categories/" + fileName;
                            uploadFile.SaveAs(Server.MapPath(path));
                        }
                    }
                    else
                    {
                        path = category.Image;
                    }
                    category.Category_Name = model.Category_Name;
                    category.Category_Name_Ar = model.Category_Name_Ar;
                    category.Image = path;

                    db.SaveChanges();

                    return RedirectToAction("Categories", "Admin");
                }
            }
            catch
            {

            }
            return View(model);
        }

        //POST //Delete category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCategory(int Id)
        {
            try
            {
                //Getting category to delete
                Category category = db.Categories.Find(Id);
                //Checking if the category contains product, if so, cannot delete.
                if (category.Products.Count == 0)
                {
                    var CategoryImagePath = Server.MapPath(category.Image);

                    //Delete category image from the server
                    if (System.IO.File.Exists(CategoryImagePath) && CategoryImagePath != "~/Images/Items/no-thumbnail.png")
                        System.IO.File.Delete(CategoryImagePath);
                    //Deleting the category from DB
                    db.Categories.Remove(category);
                    db.SaveChanges();

                    return RedirectToAction("Categories", "Admin");
                }
                else
                {
                    ViewBag.CannotDelete = Resource.CannotDeleteCategory.ToString();
                    return RedirectToAction("Categories", "Admin");
                }
            }
            catch
            {

            }

            return RedirectToAction("Categories", "Admin");
        }

        //GET // Change category visibilty on website (Hide, Show)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public PartialViewResult ChangeCategoryVisibility(int Id)
        {
            Category category = db.Categories.Find(Id);

            if(category.Visible)
            {
                category.Visible = false;
            }
            else
            {
                category.Visible = true;
            }
            db.SaveChanges();

            return PartialView("_ChangeCategoryVisibilityPartial", category);
        }

        //GET // Countries
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Countries(int? page, string Search)
        {
            //Getting list of categories
            IPagedList<Country> countries = db.Countries.Where(x => x.Name.Contains(Search.Trim())
                                                                 || x.Name_Ar.Contains(Search.Trim())
                                                                 || String.IsNullOrEmpty(Search))
                                                        .OrderBy(x => x.Name)
                                                        .ToList()
                                                        .ToPagedList(page ?? 1, 6);

            return View(countries);
        }

        //GET // Add new country
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCountry()
        {
            return View();
        }

        //POST // Add new country
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCountry(Country model)
        {
            //Checking if country already exists
            Country country = db.Countries.SingleOrDefault(x => x.Name == model.Name || x.Name_Ar == model.Name_Ar);
            if (country != null)
            {
                ViewBag.ErrorAddingCountry = Resource.CountryAlreadyExists;
                return View(model);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    //Creating a new country
                    country = new Country()
                    {
                        Name = model.Name,
                        Name_Ar = model.Name_Ar,
                        Visible = true
                    };
                    //Adding country to DB
                    db.Countries.Add(country);
                    db.SaveChanges();
                    return RedirectToAction("Countries", "Admin");
                }
            }
            return View(model);
        }

        //GET // Edit country
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditCountry(int Id)
        {

            return View(db.Countries.Find(Id));
        }

        //POST // Edit country
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditCountry(Country model)
        {
            //Getting country to edit
            Country country = db.Countries.Find(model.Id);

            //Checking if a country with the same name exists
            Country c = db.Countries.SingleOrDefault(x => (x.Name == model.Name
                                                       || x.Name_Ar == model.Name_Ar
                                                       || x.Name == model.Name_Ar
                                                       || x.Name_Ar == model.Name)
                                                       && x.Id != country.Id);
            if (c != null)
            {
                ViewBag.ErrorAddingCountry = Resource.CountryAlreadyExists;
                return View(model);
            }
            else
            {
                //Editing country
                country.Name = model.Name;
                country.Name_Ar = model.Name_Ar;
                db.SaveChanges();

                return RedirectToAction("Countries", "Admin");
            }
        }

        //POST // Delete country
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCountry(int Id)
        {
            try
            {
                //Getting country to delete
                Country country = db.Countries.Find(Id);
                //Delete country from DB
                db.Countries.Remove(country);
                db.SaveChanges();

                return RedirectToAction("Countries", "Admin");
            }
            catch
            {

            }

            return RedirectToAction("Countries", "Admin");
        }

        //GET // Change country visibility in website (Hide, Show)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public PartialViewResult ChangeCountryVisibility(int Id)
        {
            //Getting the country
            Country country = db.Countries.Find(Id);

            if (country.Visible)
            {
                country.Visible = false;
            }
            else
            {
                country.Visible = true;
            }
            db.SaveChanges();

            return PartialView("_ChangeCountryVisibilityPartial", country);
        }


        //GET // Currencies used in website
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Currencies(int? page, string Search)
        {
            //Getting list of currencies
            IPagedList<Currency> currencies = db.Currencies.Where(x => x.Name.Contains(Search.Trim())
                                                                    || String.IsNullOrEmpty(Search))
                                                           .OrderBy(x => x.Name)
                                                           .ToList()
                                                           .ToPagedList(page ?? 1, 6);

            return View(currencies);
        }

        //GET // Add a new currency
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCurrency()
        {
            return View();
        }

        //POST // Add a new currency
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCurrency(Currency model)
        {
            //Checking if the currency already exists
            Currency currency = db.Currencies.SingleOrDefault(x => x.Name == model.Name);

            if (currency != null)
            {
                ViewBag.ErrorAddingCurrency = Resource.CurrencyAlreadyExists;
                return View(currency);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    //Creating a new currency
                    currency = new Currency()
                    {
                        Name = model.Name,
                        Visible = true
                    };
                    //Adding currency to DB
                    db.Currencies.Add(currency);
                    db.SaveChanges();
                    return RedirectToAction("Currencies", "Admin");
                }

                return View(model);
            }
        }

        //GET //Edit currency
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditCurrency(int Id)
        {
            return View(db.Currencies.Find(Id));
        }

        //POST //Edit Currency
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditCurrency(Currency model)
        {
            //Getting currency to edit.
            Currency currency = db.Currencies.Find(model.Id);
            //Checking if a currency with same name already exists
            Currency c = db.Currencies.SingleOrDefault(x => x.Name == model.Name
                                                         && x.Id != currency.Id);
            if (c != null)
            {
                ViewBag.ErrorAddingCurrency = Resource.CurrencyAlreadyExists;
                return View(model);
            }
            else
            {
                //Editing currency
                currency.Name = model.Name;
                db.SaveChanges();
            }
            return RedirectToAction("Currencies", "Admin");
        }

        //GET //Delete currency
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCurrency(int Id)
        {
            try
            {
                //Getting currency to delete
                Currency currency = db.Currencies.Find(Id);
                //Delete currency from DB
                db.Currencies.Remove(currency);
                db.SaveChanges();

                return RedirectToAction("Currencies", "Admin");
            }
            catch
            {

            }
            ViewBag.CannotDeleteCurrency = Resource.CannotDeleteCurrency;
            return RedirectToAction("Currencies", "Admin");
        }

        //GET //Change currency visibility in website (Hide, Show)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public PartialViewResult ChangeCurrencyVisibility(int Id)
        {
            //Getting the currency
            Currency currency = db.Currencies.Find(Id);

            if (currency.Visible)
            {
                currency.Visible = false;
            }
            else
            {
                currency.Visible = true;
            }
            db.SaveChanges();

            return PartialView("_ChangeCurrencyVisibilityPartial", currency);
        }

        //GET //Messages list from users and visitors
        [HttpGet]
        [Authorize(Roles = "Admin,Support")]
        public ActionResult Messages(int? page, string Sender)
        {
            //Get list of messages
            IEnumerable<Message> messagesFromDB = db.Messages.ToList();
            IPagedList messages = null;

            //Filtering messages by sender type (User, Visitor,Website)
            switch(Sender)
            {
                case "User":
                    messages = messagesFromDB.Where(x => x.SenderType == "User").OrderByDescending(x => x.IsSeen)
                                                                                .OrderByDescending(x => x.MessageDateAndTime)
                                                                                .ToList()
                                                                                .ToPagedList(page ?? 1, 6);
                    break;
                case "Visitor":
                    messages = messagesFromDB.Where(x => x.SenderType == "Visitor").OrderByDescending(x => x.IsSeen)
                                                                               .OrderByDescending(x => x.MessageDateAndTime)
                                                                               .ToList()
                                                                               .ToPagedList(page ?? 1, 6);
                    break;
                case "Website":
                    messages = messagesFromDB.Where(x => x.SenderType == "Website").OrderByDescending(x => x.IsSeen)
                                                                               .OrderByDescending(x => x.MessageDateAndTime)
                                                                               .ToList()
                                                                               .ToPagedList(page ?? 1, 6);
                    break;

                default:
                    messages = messagesFromDB.OrderByDescending(x => x.IsSeen)
                                             .OrderByDescending(x => x.MessageDateAndTime)
                                             .ToList()
                                             .ToPagedList(page ?? 1, 6);
                    break;
            }

            return View(messages);
        }

        //GET //View a message
        [HttpGet]
        [Authorize(Roles = "Admin,Support")]
        public ActionResult MessageView(int Id)
        {
            //Get the message
            Message message = db.Messages.Include("RepliedBy").SingleOrDefault(x => x.Id == Id);
            //Marking the message as seen
            message.IsSeen = true;
           
            db.SaveChanges();
            return View(message);
        }

        //GET //Reply to message
        [HttpGet]
        [Authorize(Roles = "Admin,Support")]
        public ActionResult Reply(string ToEmail, int? Id)
        {
            ReplyMessageViewModel replyMessageViewModel = new ReplyMessageViewModel();
            replyMessageViewModel.Email = ToEmail.ToString();
            replyMessageViewModel.MessageText = "\n\n\n\n\n\n\n\n\n\n" +
                                                "-------------------------------------------\n" +
                                                "Online auctions website\n" +
                                                "Admin: " + User.Identity.GetUserName().ToString();
            if (Id != null)
                ViewBag.MessageID = Id;
            return View(replyMessageViewModel);
        }

        //POST //Reply to message
        [HttpPost]
        [Authorize(Roles = "Admin,Support")]
        public ActionResult Reply(ReplyMessageViewModel model, int? Id)
        {
            try
            {
                Email email = new Email(model.Email, "Reply from Online Auctions", model.MessageText);
                email.Send();

                if (Id != null)
                {
                    //Saving the admin who replied the message
                    Message msg = db.Messages.Find(Id);
                    msg.RepliedBy = db.Users.Find(User.Identity.GetUserId());
                    db.SaveChanges();
                }
                return RedirectToAction("Messages", "Admin");
            }
            catch
            {

            }
            //If something went wrong stay on page and display error message.
            ViewBag.ErrorSendingEmail = Resource.ErrorSendingEmail;
            return View(model);

        }

        //POST //Delete selected messages
        [HttpPost]
        [Authorize(Roles = "Admin,Support")]
        public ActionResult DeleteMessage(IEnumerable<int> MessageIdsToDelete)
        {
            try
            {
                //Getting list of messages to delete
                IEnumerable<Message> messagesToDelete = db.Messages.Where(x => MessageIdsToDelete.Contains(x.Id)).ToList();
                foreach (Message msg in messagesToDelete)
                {
                    db.Messages.Remove(msg);
                }
                db.SaveChanges();

                return RedirectToAction("Messages", "Admin");
            }
            catch
            {

            }
            return RedirectToAction("Messages", "Admin");
        }

        //POST //Delete single message
        [HttpPost]
        [Authorize(Roles = "Admin,Support")]
        public ActionResult DeleteSingleMessage(int Id)
        {
            try
            {
                Message message = db.Messages.Find(Id);
                db.Messages.Remove(message);
                db.SaveChanges();

                return RedirectToAction("Messages", "Admin");
            }
            catch
            {

            }
            return View("MessageView", new { Id = Id });
        }

        //GET //Get website users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ManageUsers(int? page, string Search)
        {
            ApplicationUser user = db.Users.Find( User.Identity.GetUserId());
         
            //Getting list of users (all users or by searching)
            IPagedList<ApplicationUser> users = db.Users.Where(x => x.Id != user.Id 
                                                                && (x.UserName.Contains(Search.Trim())
                                                                || x.FirstName.Contains(Search.Trim())
                                                                || x.LastName.Contains(Search.Trim())
                                                                || x.Email.Contains(Search.Trim())
                                                                || x.PhoneNumber.Contains(Search.Trim())
                                                                || String.IsNullOrEmpty(Search)))
                                                                .OrderBy(x => x.UserName)
                                                                .ToList()
                                                                .ToPagedList(page ?? 1, 10);

            return View(users);
        }

       
        //GET //Filter Users by Active or blocked accounts
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult SearchUser(int? page, string Status)
        {
            try
            {
                ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

                var status = false;
                if (Status == "All")
                {
                    return RedirectToAction("ManageUsers");
                }
                else
                {
                    switch(Status)
                    {
                        case "Active":
                            status = true;
                            break;
                        case "Blocked":
                            status = false;
                            break;
                    }

                    IPagedList<ApplicationUser> users = db.Users.Where(x => x.Active.Equals(status)
                                                                             && x.Id != user.Id)
                                                          .ToList()
                                                          .ToPagedList(page ?? 1, 10);
                    return View("ManageUsers", users);
                }
            }
            catch { }
            return RedirectToAction("ManageUsers");
        }

        //GET //Block or unblock a user account
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public PartialViewResult BlockUnblockUser(string Id)
        {
            ApplicationUser user = db.Users.Find(Id);
            if (user.Active)
            {
                user.Active = false;
            }
            else
            {
                user.Active = true;
            }
            db.SaveChanges();


            return PartialView("_UserAccountStatusPartial", user);
        }
            
        

        //GET //Managing user roles (User, Admin, Support, Supervisor)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ManageUserRoles(string Id)
        {
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            //Getting list of user's roles
            var userRoles = userManager.GetRoles(Id).ToList();

            ViewBag.User = db.Users.Find(Id);
            //Getting list of all available roles
            ViewBag.Roles = new SelectList(db.Roles.OrderBy(x => x.Name).ToList(), "Name", "Name");

            return View(userRoles);
        }

        //POST //Add user to a role
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ManageUserRoles_AddToRole(string Id)
        {
            try
            {
                //Getting the requested role
                string role = Request["Roles"].ToString();

                var roleStore = new RoleStore<IdentityRole>(db);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);

                //Adding the user to the role
                userManager.AddToRole(Id, role);

                return RedirectToAction("ManageUserRoles", new { Id = Id });
            }
            catch
            {

            }
            return RedirectToAction("ManageUserRoles", new { Id = Id });
        }

        //GET //Remove user from role
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ManageUserRoles_RemoveFromRole(string Id, string role)
        {
            try
            {
                var roleStore = new RoleStore<IdentityRole>(db);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);

                userManager.RemoveFromRole(Id, role);

                return RedirectToAction("ManageUserRoles", new { Id = Id });
            }
            catch
            {

            }
            return RedirectToAction("ManageUserRoles", new { Id = Id });
        }

        //GET //User information details
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult UserDetails(string Id)
        {
            //Getting the user
            ApplicationUser user = db.Users.Find(Id);

            return View(user);
        }

        //GET //Get a statisitcs page
        [HttpGet]
        [Authorize(Roles = "Admin,Supervisor")]
        public ActionResult Statistics(int? page, string Search)
        {
            //For each user in the website 
            //Get:
            //User's auctions count
            //User's winning auctions count
            //User's biddings count
            //User's paid auctions count
            //User's login to website count
            Statistic statistic;
            foreach (ApplicationUser user in db.Users.ToList())
            {
                statistic = db.Statistics.SingleOrDefault(x => x.User.Id == user.Id);

                if (statistic == null)
                {
                    statistic = new Statistic();

                    statistic.User = user;
                    statistic.UserAuctionsCount = db.Auctions.Where(x => x.Seller.Id == user.Id).Count();
                    statistic.UserWinningAuctionsCount = db.Auctions.Where(x => x.Buyer.Id == user.Id && x.Finish_Date <= DateTime.Now).Count();
                    statistic.UserBiddingsCount = db.Bids.Where(x => x.Bidder.Id == user.Id).Select(x => x.Auction).Distinct().Count();
                    statistic.PaidAuctions = db.Auctions.Where(x => x.Buyer.Id == user.Id && x.Finish_Date <= DateTime.Now && x.IsPaid).Count();
                    statistic.LoginsCount = statistic.User.LoginCount;

                    db.Statistics.Add(statistic);
                }
                else
                {
                    statistic.UserAuctionsCount = db.Auctions.Where(x => x.Seller.Id == user.Id).Count();
                    statistic.UserWinningAuctionsCount = db.Auctions.Where(x => x.Buyer.Id == user.Id && x.Finish_Date <= DateTime.Now).Count();
                    statistic.UserBiddingsCount = db.Bids.Where(x => x.Bidder.Id == user.Id).Select(x => x.Auction).Distinct().Count();
                    statistic.PaidAuctions = db.Auctions.Where(x => x.Buyer.Id == user.Id && x.Finish_Date <= DateTime.Now && x.IsPaid).Count();
                    statistic.LoginsCount = statistic.User.LoginCount;
                }
            }

            db.SaveChanges();

            //Ordering the results
            IPagedList<Statistic> statistics = db.Statistics.OrderByDescending(x => x.UserAuctionsCount)
                                                            .ThenByDescending(x => x.UserBiddingsCount)
                                                            .ThenByDescending(x => x.UserWinningAuctionsCount)
                                                            .Where(x => x.User.UserName.Contains(Search.Trim()) 
                                                                    || String.IsNullOrEmpty(Search))
                                                            .ToList()
                                                            .ToPagedList(page ?? 1, 10);
            //Getting all users count
            ViewBag.UsersCount = Resource.UsersCount + ": " + db.Users.Count().ToString();
            return View(statistics);
        }

        //GET //Auctions reports
        [HttpGet]
        [Authorize(Roles = "Admin,Supervisor")]
        public ActionResult Reports(int? page, string Search)
        {
            //Getting list of reports
            IPagedList<Auction> reports = db.Reports.Include("Auction")
                                                     .Include("Auction.Product")
                                                     .Select(x => x.auction)
                                                     .Where(x => x.Product.Name.Contains(Search.Trim())
                                                              || String.IsNullOrEmpty(Search))
                                                     .Distinct()
                                                     .OrderBy(x => x.Product.Name)
                                                     .ToList()
                                                     .ToPagedList(page ?? 1, 10);


            return View(reports);
        }

        //GET //Report details (reportes names)
        [HttpGet]
        [Authorize(Roles = "Admin,Supervisor")]
        public ActionResult ReportDetails(int? page, int Id, string returnUrl)
        {
            IPagedList<Report> reports = db.Reports.Include("Auction")
                                                   .Include("Auction.Product")
                                                  .Where(x => x.auction.Id == Id)
                                                  .ToList()
                                                  .ToPagedList(page ?? 1, 10);
            foreach (var report in reports)
            {
                //Marking the report as seen
                report.Seen = true;
            }

            db.SaveChanges();

            ViewBag.ReturnUrl = returnUrl;

            return View(reports);
        }

        
        //GET //Auction and bids details
        [HttpGet]
        [Authorize(Roles = "Admin, Supervisor")]
        public ActionResult BidsDetails(int Id)
        {
            //Getting auction
            Auction auction = db.Auctions.Include("Product")
                                         .Include("Product.Currency")
                                         .Include("Product.Category")
                                         .Include("Seller")
                                         .Include("Buyer")
                                         .Include("Bids")
                                         .Include("Bids.Bidder")
                                         .SingleOrDefault(x => x.Id == Id);

            switch(SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.Category = new SelectList(db.Categories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name", auction.Product.Category.Id);
                    break;
                case "ar-SA":
                    ViewBag.Category = new SelectList(db.Categories.OrderBy(x => x.Category_Name_Ar).ToList(), "Id", "Category_Name_Ar", auction.Product.Category.Id);
                    break;
                default:
                    ViewBag.Category = new SelectList(db.Categories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name", auction.Product.Category.Id);
                    break;
            }

            return View(auction);
        }

        //GET //Auctions management
        [HttpGet]
        [Authorize(Roles = "Admin, Supervisor")]
        public ActionResult AuctionsManagement(int? page, int? Category, string Search)
        {
            IEnumerable<Auction> auctions = db.Auctions.Include("Product")
                                                       .Where(x => (x.Product.Category.Id == Category
                                                                || Category == null)
                                                                && (x.Product.Name.Contains(Search)
                                                                    || String.IsNullOrEmpty(Search))
                                                                )
                                                       .OrderBy(x => (x.Finish_Date > DateTime.Now ? "A" : "B"))
                                                       .ToList();

            switch (SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.Category = new SelectList(db.Categories.Where(x => x.Visible).OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");
                    break;
                case "ar-SA":
                    ViewBag.Category = new SelectList(db.Categories.Where(x => x.Visible).OrderBy(x => x.Category_Name_Ar).ToList(), "Id", "Category_Name_Ar");
                    break;
                default:
                    ViewBag.Category = new SelectList(db.Categories.Where(x => x.Visible).OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");
                    break;
            }

            ViewBag.AuctionsCount = auctions.Count();

            return View(auctions.ToPagedList(page ?? 1, 12));
        }
      
        //POST //Changing auction category
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        public ActionResult ChangeProductCategory(int Id, int Category)
        {
            Auction auction = db.Auctions.Include("Product").Include("Product.Category").SingleOrDefault(x => x.Id == Id);
            Category category = db.Categories.Find(Category);
            auction.Product.Category = category;
            db.SaveChanges();

            return RedirectToAction("BidsDetails", "Admin", new { Id = Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Supervisor, Support")]
        public ActionResult SendEmailToAllUsers()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor, Support")]
        public ActionResult SendEmailToAllUsers(EmailToAllUsers model, IEnumerable<string> roles)
        {
            List<ApplicationUser> users = new List<ApplicationUser>();

            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if(roles == null)
            {
                ViewBag.ErrorSendingEmail = Resource.ErrorSelectSendTo;
                return View(model);
            }
           
                foreach (var user in db.Users)
                {
                    foreach (var role in roles)
                    {
                        if (userManager.IsInRole(user.Id, role))
                        {
                            if (!users.Contains(user))
                                users.Add(user);
                        }
                    }
                }
            
            try
            {
                foreach (var user in users)
                {
                    Email email = new Email(user.Email, model.Subject, model.EmailText);
                    email.Send();
                }
                return RedirectToAction("Messages", "Admin");
            }
            catch
            { }
            ViewBag.ErrorSendingEmail = Resource.ErrorSendingEmail;

            return View(model);
        }

        //GET //Remove auction by admin or supervisor
        [HttpGet]
        [Authorize(Roles = "Admin, Supervisor")]
        public ActionResult Remove(int Id, string returnUrl)
        {
            //Getting the auction
            Auction auction = db.Auctions.Include("Product")
                                           .Include("Product.Category")
                                           .Include("Seller")
                                           .SingleOrDefault(x => x.Id == Id);
            //Getting the product
            Product prodcut = db.Products.Find(auction.Product.Id);
            //Getting list of bids on the auction
            List<Bid> bids = db.Bids.Where(x => x.Auction.Id == auction.Id).ToList();
            //Get the list of reports on the auctions
            List<Report> reports = db.Reports.Where(x => x.auction.Id == auction.Id).ToList();
            //Get the list of product images
            List<ProductPhoto> images = db.Images.Where(x => x.Product.Id == prodcut.Id).ToList();

            //Creating an email to send to auction seller
            string subject = "", body = "";
            switch (auction.Seller.PreferredInterfaceLanguage)
            {
                case "English":
                    subject = "Auction removed";
                    body = "Dear Mr/Mrs: " + auction.Seller.FirstName + " " + auction.Seller.LastName + "\n" +
                          "We are sorry to inform you that your auction " + auction.Product.Name +
                          " was removed from our website\n" +
                          "\n\n Online auctions team";
                    break;
                case "Arabic":
                    subject = "حذف مزاد";
                    body = "السيد/السيدة: " + auction.Seller.FirstName + " " + auction.Seller.LastName + "\n" +
                          "نأسف لاعلامكم بأن المزاد الخاص بكم " + auction.Product.Name +
                          " قد تم حذفه من الموقع الالكتروني\n" +
                          "\n\n فريق ادارة موقع المزاد العلني الالكتروني";
                    break;
                default:
                    subject = "Auction removed";
                    body = "Dear Mr/Mrs: " + auction.Seller.FirstName + " " + auction.Seller.LastName + "\n" +
                          "We are sorry to inform you that your auction " + auction.Product.Name +
                          " was removed from our website\n" +
                          "\n\n Online auctions team";
                    break;
            }



            Email email = new Email(auction.Seller.Email, subject, body);

            try
            {
                var categoryId = auction.Product.Category.Id;
                //Deleteing product images from server
                string[] productImagesPaths = prodcut.Images.Select(i => i.Path).ToArray<string>();

                for (int i = 0; i < productImagesPaths.Count(); i++)
                {
                    var ProductImagePath = Server.MapPath(productImagesPaths[i]);

                    if (System.IO.File.Exists(ProductImagePath) && productImagesPaths[i] != "~/Images/Items/no-thumbnail.png")
                        System.IO.File.Delete(ProductImagePath);
                }

                //Deleting bids operations from DB
                foreach (var bid in bids)
                {
                    db.Bids.Remove(bid);
                }
                //Deleting images from DB
                foreach (var p in images)
                {
                    db.Images.Remove(p);
                }
                //Deleteing Reports from DB
                foreach (var r in reports)
                {
                    db.Reports.Remove(r);
                }
                //Deleting the auction from DB
                db.Auctions.Remove(auction);
                //Deleting the product from DB
                db.Products.Remove(prodcut);
                db.SaveChanges();

                //Sending email to seller
                try
                {
                    email.Send();
                }
                catch
                {
                    
                }
                return RedirectToAction("AuctionsManagement", "Admin");
            }
            catch { }
            //If something went wrong, stay on page
            return View("BidsDetails", auction);
        }

        //GET //Automatic remove auction by the website after getting reports on the auction
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AutomaticRemove(int Id)
        {
            //Getting the auction
            Auction auction = db.Auctions.Include("Seller")
                                         .Include("Product")
                                         .Include("Product.Category")
                                         .SingleOrDefault(x => x.Id == Id);
            //Getting the product
            Product prodcut = db.Products.Find(auction.Product.Id);
            //Getting list of bids on the auction
            List<Bid> bids = db.Bids.Where(x => x.Auction.Id == auction.Id).ToList();
            //Getting list of reports on the auction
            List<Report> reports = db.Reports.Where(x => x.auction.Id == auction.Id).ToList();
            //Getting product images
            List<ProductPhoto> images = db.Images.Where(x => x.Product.Id == prodcut.Id).ToList();

            //Creating an email to send to seller
            //Creating an email to send to auction seller
            string subject = "", body = "";
            switch (auction.Seller.PreferredInterfaceLanguage)
            {
                case "English":
                    subject = "Auction removed";
                    body = "Dear Mr/Mrs: " + auction.Seller.FirstName + " " + auction.Seller.LastName + "\n" +
                          "We are sorry to inform you that your auction " + auction.Product.Name +
                          " was removed from our website after getting reports from other users\n" +
                          "\n\n Online auctions team";
                    break;
                case "Arabic":
                    subject = "حذف مزاد";
                    body = "السيد/السيدة: " + auction.Seller.FirstName + " " + auction.Seller.LastName + "\n" +
                          "نأسف لاعلامكم بأن المزاد الخاص بكم " + auction.Product.Name +
                          " قد تم حذفه من الموقع الالكتروني بسبب التبليغ عنه من مستخدمين اخرين\n" +
                          "\n\n فريق ادارة موقع المزاد العلني الالكتروني";
                    break;
                default:
                    subject = "Auction removed";
                    body = "Dear Mr/Mrs: " + auction.Seller.FirstName + " " + auction.Seller.LastName + "\n" +
                          "We are sorry to inform you that your auction " + auction.Product.Name +
                          " was removed from our website after getting reports from other users\n" +
                          "\n\n Online auctions team";
                    break;
            }

            Email email = new Email(auction.Seller.Email, subject, body);

            //Sending a message to the administation
            Message msg = new Message();
            msg.Email = "Automatic@Email.website";
            msg.MessageDateAndTime = DateTime.Now;
            msg.SenderType = "Website";
            msg.IsSeen = false;
            msg.RepliedBy = null;

            switch (db.Users.Find(User.Identity.GetUserId()).PreferredInterfaceLanguage)
            {
                case "English":
                    msg.MessageText = "The auction: [ " + auction.Product.Name + " ] was removed from website after getting 50 reports from other users. ";
                    break;
                case "Arabic":
                    msg.MessageText = "المزاد: [ " + auction.Product.Name + " ] قد تم حذفه من الموقع الالكتروني بسبب تلقيه 50 تبليغ من المستخدمين. ";
                    break;
                default:
                    msg.MessageText = "The auction: [ " + auction.Product.Name + " ] was removed from website after getting 50 reports from other users. ";
                    break;
            }

            //Check if it is allowed to remove auction
            if (Session["AllowAuctionRemove"] != null)
            {
                try
                {
                    var categoryId = auction.Product.Category.Id;
                    //Deleteing the product images from server
                    string[] productImagesPaths = prodcut.Images.Select(i => i.Path).ToArray<string>();

                    for (int i = 0; i < productImagesPaths.Count(); i++)
                    {
                        var ProductImagePath = Server.MapPath(productImagesPaths[i]);

                        if (System.IO.File.Exists(ProductImagePath) && productImagesPaths[i] != "~/Images/Items/no-thumbnail.png")
                            System.IO.File.Delete(ProductImagePath);
                    }

                    //Deleting bids operations from DB
                    foreach (var bid in bids)
                    {
                        db.Bids.Remove(bid);
                    }
                    //Deleting images from DB
                    foreach (var p in images)
                    {
                        db.Images.Remove(p);
                    }
                    //Deleting reports from DB
                    foreach (var r in reports)
                    {
                        db.Reports.Remove(r);
                    }
                    //Deleting the auction from DB
                    db.Auctions.Remove(auction);
                    //Deleting the product from DB
                    db.Products.Remove(prodcut);

                    db.Messages.Add(msg);
                    db.SaveChanges();

                    //Sending an email to the user telling them that their auction was removed

                    email.Send();

                    return RedirectToAction("Index", "Auction", new { Id = categoryId });
                }
                catch
                {
                }
                Session.Remove("AllowAuctionRemove");
            }
            //If something went wrong, stay on page
            return View("Error");
        }
	}
}