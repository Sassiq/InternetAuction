using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OnlineAuctionProject.Models;
using OnlineAuctionProject.Resources;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineAuctionProject.Controllers
{
    public class AdminController : Controller
    {
        private AuctionContext _dbContext;

        public AdminController()
        {
            _dbContext = new AuctionContext();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Support,Supervisor")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Categories(int? page, string Search)
        {
            var categories = _dbContext.Categories.AsNoTracking()
                .Where(x => x.Category_Name.Contains(Search.Trim()) || string.IsNullOrEmpty(Search));

            var ordered = await categories.OrderBy(x => x.Category_Name).ToListAsync();

            return View(ordered.ToPagedList(page ?? 1, 6));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddCategory(Category model, HttpPostedFileBase uploadFile)
        {
            try
            {
                var category = await _dbContext.Categories.SingleOrDefaultAsync(x => x.Category_Name == model.Category_Name);

                if (category != null)
                {
                    ViewBag.CategoryAlreadyExists = Resource.CategoryAlreadyExists;
                    return View(model);
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        string path = Resource.ThumbnailPath;

                        //Saving uploaded image
                        if (uploadFile != null && uploadFile.ContentLength > 0)
                        {
                            if (checkFileType(uploadFile.FileName))
                            {
                                var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + Path.GetFileName(uploadFile.FileName);
                                path = "~/Images/Categories/" + fileName;
                                uploadFile.SaveAs(Server.MapPath(path));
                            }
                            else
                            {
                                path = Resource.ThumbnailPath;
                            }
                        }

                        category = new Category()
                        {
                            Category_Name = model.Category_Name,
                            Image = path,
                            Visible = true
                        };

                        _dbContext.Categories.Add(category);
                        await _dbContext.SaveChangesAsync();

                        return RedirectToAction("Categories", "Admin");
                    }
                }
            }
            catch
            {

            }
            return View(model);   
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditCategory(int Id)
        {
            var category = await _dbContext.Categories.FindAsync(Id);

            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditCategory(Category model, HttpPostedFileBase uploadFile)
        {
            try
            {
                var category = _dbContext.Categories.Find(model.Id);
                var c = await _dbContext.Categories.SingleOrDefaultAsync(x => (x.Category_Name == model.Category_Name) && x.Id != category.Id);
                if (c != null)
                {
                    ViewBag.CategoryAlreadyExists = Resource.CategoryAlreadyExists;
                    return View(model);
                }
                else
                {
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
                    category.Image = path;

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("Categories", "Admin");
                }
            }
            catch
            {

            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int Id)
        {
            try
            {
                Category category = await _dbContext.Categories.FindAsync(Id);
                if (category.Products.Count == 0)
                {
                    var CategoryImagePath = Server.MapPath(category.Image);

                    if (System.IO.File.Exists(CategoryImagePath) && CategoryImagePath != "~/Images/Items/no-thumbnail.png")
                        System.IO.File.Delete(CategoryImagePath);
                    _dbContext.Categories.Remove(category);
                    await _dbContext.SaveChangesAsync();

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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<PartialViewResult> ChangeCategoryVisibility(int Id)
        {
            var category = await _dbContext.Categories.FindAsync(Id);
            category.Visible = !category.Visible;
            await _dbContext.SaveChangesAsync();

            return PartialView("_ChangeCategoryVisibilityPartial", category);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Countries(int? page, string Search)
        {
            var countries = await _dbContext.Countries
                .AsNoTracking()
                .Where(x => x.Name.Contains(Search.Trim())
                       || x.Name_Ar.Contains(Search.Trim())
                       || string.IsNullOrEmpty(Search))
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(countries.ToPagedList(page ?? 1, 6));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCountry()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddCountry(Country model)
        {
            var country = await _dbContext.Countries.SingleOrDefaultAsync(x => x.Name == model.Name || x.Name_Ar == model.Name_Ar);
            if (country != null)
            {
                ViewBag.ErrorAddingCountry = Resource.CountryAlreadyExists;
                return View(model);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    country = new Country()
                    {
                        Name = model.Name,
                        Name_Ar = model.Name_Ar,
                        Visible = true
                    };

                    _dbContext.Countries.Add(country);
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction("Countries", "Admin");
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditCountry(int Id)
        {
            var country = await _dbContext.Countries.FindAsync(Id);

            return View(country);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditCountry(Country model)
        {
            var country = await _dbContext.Countries.FindAsync(model.Id);
            var c = await _dbContext.Countries.SingleOrDefaultAsync(x => (x.Name == model.Name
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
                country.Name = model.Name;
                country.Name_Ar = model.Name_Ar;
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Countries", "Admin");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCountry(int Id)
        {
            try
            {
                Country country = await _dbContext.Countries.FindAsync(Id);
                _dbContext.Countries.Remove(country);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Countries", "Admin");
            }
            catch { }

            return RedirectToAction("Countries", "Admin");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<PartialViewResult> ChangeCountryVisibility(int Id)
        {
            var country = await _dbContext.Countries.FindAsync(Id);
            country.Visible = !country.Visible;
            await _dbContext.SaveChangesAsync();

            return PartialView("_ChangeCountryVisibilityPartial", country);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Currencies(int? page, string Search)
        {
            var currencies = await _dbContext.Currencies
                .Where(x => x.Name.Contains(Search.Trim())
                       || string.IsNullOrEmpty(Search))
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(currencies.ToPagedList(page ?? 1, 6));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCurrency()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddCurrency(Currency model)
        {
            var currency = await _dbContext.Currencies.SingleOrDefaultAsync(x => x.Name == model.Name);

            if (currency != null)
            {
                ViewBag.ErrorAddingCurrency = Resource.CurrencyAlreadyExists;
                return View(currency);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    currency = new Currency()
                    {
                        Name = model.Name,
                        Visible = true
                    };
                    _dbContext.Currencies.Add(currency);
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction("Currencies", "Admin");
                }

                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditCurrency(int Id)
        {
            var currency = await _dbContext.Currencies.FindAsync(Id);

            return View(currency);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditCurrency(Currency model)
        {
            var currency = await _dbContext.Currencies.FindAsync(model.Id);
            var c = await _dbContext.Currencies
                .SingleOrDefaultAsync(x => x.Name == model.Name && x.Id != currency.Id);
            if (c != null)
            {
                ViewBag.ErrorAddingCurrency = Resource.CurrencyAlreadyExists;
                return View(model);
            }
            else
            {
                currency.Name = model.Name;
                await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction("Currencies", "Admin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCurrency(int Id)
        {
            try
            {
                Currency currency = _dbContext.Currencies.Find(Id);
                _dbContext.Currencies.Remove(currency);
                _dbContext.SaveChanges();

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
            Currency currency = _dbContext.Currencies.Find(Id);

            if (currency.Visible)
            {
                currency.Visible = false;
            }
            else
            {
                currency.Visible = true;
            }
            _dbContext.SaveChanges();

            return PartialView("_ChangeCurrencyVisibilityPartial", currency);
        }

        //GET //Messages list from users and visitors
        [HttpGet]
        [Authorize(Roles = "Admin,Support")]
        public ActionResult Messages(int? page, string Sender)
        {
            //Get list of messages
            IEnumerable<Message> messagesFromDB = _dbContext.Messages.ToList();
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
            Message message = _dbContext.Messages.Include("RepliedBy").SingleOrDefault(x => x.Id == Id);
            //Marking the message as seen
            message.IsSeen = true;
           
            _dbContext.SaveChanges();
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
                if (Id != null)
                {
                    //Saving the admin who replied the message
                    Message msg = _dbContext.Messages.Find(Id);
                    msg.RepliedBy = _dbContext.Users.Find(User.Identity.GetUserId());
                    _dbContext.SaveChanges();
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
                IEnumerable<Message> messagesToDelete = _dbContext.Messages.Where(x => MessageIdsToDelete.Contains(x.Id)).ToList();
                foreach (Message msg in messagesToDelete)
                {
                    _dbContext.Messages.Remove(msg);
                }
                _dbContext.SaveChanges();

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
                Message message = _dbContext.Messages.Find(Id);
                _dbContext.Messages.Remove(message);
                _dbContext.SaveChanges();

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
            ApplicationUser user = _dbContext.Users.Find( User.Identity.GetUserId());
         
            //Getting list of users (all users or by searching)
            IPagedList<ApplicationUser> users = _dbContext.Users.Where(x => x.Id != user.Id 
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
                ApplicationUser user = _dbContext.Users.Find(User.Identity.GetUserId());

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

                    IPagedList<ApplicationUser> users = _dbContext.Users.Where(x => x.Active.Equals(status)
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
            ApplicationUser user = _dbContext.Users.Find(Id);
            if (user.Active)
            {
                user.Active = false;
            }
            else
            {
                user.Active = true;
            }
            _dbContext.SaveChanges();


            return PartialView("_UserAccountStatusPartial", user);
        }
            
        

        //GET //Managing user roles (User, Admin, Support, Supervisor)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ManageUserRoles(string Id)
        {
            var userStore = new UserStore<ApplicationUser>(_dbContext);
            var userManager = new UserManager<ApplicationUser>(userStore);

            //Getting list of user's roles
            var userRoles = userManager.GetRoles(Id).ToList();

            ViewBag.User = _dbContext.Users.Find(Id);
            //Getting list of all available roles
            ViewBag.Roles = new SelectList(_dbContext.Roles.OrderBy(x => x.Name).ToList(), "Name", "Name");

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

                var roleStore = new RoleStore<IdentityRole>(_dbContext);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(_dbContext);
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
                var roleStore = new RoleStore<IdentityRole>(_dbContext);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(_dbContext);
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
            ApplicationUser user = _dbContext.Users.Find(Id);

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
            foreach (ApplicationUser user in _dbContext.Users.ToList())
            {
                statistic = _dbContext.Statistics.SingleOrDefault(x => x.User.Id == user.Id);

                if (statistic == null)
                {
                    statistic = new Statistic();

                    statistic.User = user;
                    statistic.UserAuctionsCount = _dbContext.Auctions.Where(x => x.Seller.Id == user.Id).Count();
                    statistic.UserWinningAuctionsCount = _dbContext.Auctions.Where(x => x.Buyer.Id == user.Id && x.Finish_Date <= DateTime.Now).Count();
                    statistic.UserBiddingsCount = _dbContext.Bids.Where(x => x.Bidder.Id == user.Id).Select(x => x.Auction).Distinct().Count();
                    statistic.PaidAuctions = _dbContext.Auctions.Where(x => x.Buyer.Id == user.Id && x.Finish_Date <= DateTime.Now && x.IsPaid).Count();
                    statistic.LoginsCount = statistic.User.LoginCount;

                    _dbContext.Statistics.Add(statistic);
                }
                else
                {
                    statistic.UserAuctionsCount = _dbContext.Auctions.Where(x => x.Seller.Id == user.Id).Count();
                    statistic.UserWinningAuctionsCount = _dbContext.Auctions.Where(x => x.Buyer.Id == user.Id && x.Finish_Date <= DateTime.Now).Count();
                    statistic.UserBiddingsCount = _dbContext.Bids.Where(x => x.Bidder.Id == user.Id).Select(x => x.Auction).Distinct().Count();
                    statistic.PaidAuctions = _dbContext.Auctions.Where(x => x.Buyer.Id == user.Id && x.Finish_Date <= DateTime.Now && x.IsPaid).Count();
                    statistic.LoginsCount = statistic.User.LoginCount;
                }
            }

            _dbContext.SaveChanges();

            //Ordering the results
            IPagedList<Statistic> statistics = _dbContext.Statistics.OrderByDescending(x => x.UserAuctionsCount)
                                                            .ThenByDescending(x => x.UserBiddingsCount)
                                                            .ThenByDescending(x => x.UserWinningAuctionsCount)
                                                            .Where(x => x.User.UserName.Contains(Search.Trim()) 
                                                                    || String.IsNullOrEmpty(Search))
                                                            .ToList()
                                                            .ToPagedList(page ?? 1, 10);
            //Getting all users count
            ViewBag.UsersCount = Resource.UsersCount + ": " + _dbContext.Users.Count().ToString();
            return View(statistics);
        }

        //GET //Auctions reports
        [HttpGet]
        [Authorize(Roles = "Admin,Supervisor")]
        public ActionResult Reports(int? page, string Search)
        {
            //Getting list of reports
            IPagedList<Auction> reports = _dbContext.Reports.Include("Auction")
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
            IPagedList<Report> reports = _dbContext.Reports.Include("Auction")
                                                   .Include("Auction.Product")
                                                  .Where(x => x.auction.Id == Id)
                                                  .ToList()
                                                  .ToPagedList(page ?? 1, 10);
            foreach (var report in reports)
            {
                //Marking the report as seen
                report.Seen = true;
            }

            _dbContext.SaveChanges();

            ViewBag.ReturnUrl = returnUrl;

            return View(reports);
        }

        
        //GET //Auction and bids details
        [HttpGet]
        [Authorize(Roles = "Admin, Supervisor")]
        public ActionResult BidsDetails(int Id)
        {
            //Getting auction
            Auction auction = _dbContext.Auctions.Include("Product")
                                         .Include("Product.Currency")
                                         .Include("Product.Category")
                                         .Include("Seller")
                                         .Include("Buyer")
                                         .Include("Bids")
                                         .Include("Bids.Bidder")
                                         .SingleOrDefault(x => x.Id == Id);

            ViewBag.Category = new SelectList(_dbContext.Categories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name", auction.Product.Category.Id);

            return View(auction);
        }

        //GET //Auctions management
        [HttpGet]
        [Authorize(Roles = "Admin, Supervisor")]
        public ActionResult AuctionsManagement(int? page, int? Category, string Search)
        {
            IEnumerable<Auction> auctions = _dbContext.Auctions.Include("Product")
                                                       .Where(x => (x.Product.Category.Id == Category
                                                                || Category == null)
                                                                && (x.Product.Name.Contains(Search)
                                                                    || String.IsNullOrEmpty(Search))
                                                                )
                                                       .OrderBy(x => (x.Finish_Date > DateTime.Now ? "A" : "B"))
                                                       .ToList();

            ViewBag.Category = new SelectList(_dbContext.Categories.Where(x => x.Visible).OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");

            ViewBag.AuctionsCount = auctions.Count();

            return View(auctions.ToPagedList(page ?? 1, 12));
        }
      
        //POST //Changing auction category
        [HttpPost]
        [Authorize(Roles = "Admin, Supervisor")]
        public ActionResult ChangeProductCategory(int Id, int Category)
        {
            Auction auction = _dbContext.Auctions.Include("Product").Include("Product.Category").SingleOrDefault(x => x.Id == Id);
            Category category = _dbContext.Categories.Find(Category);
            auction.Product.Category = category;
            _dbContext.SaveChanges();

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

            var userStore = new UserStore<ApplicationUser>(_dbContext);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if(roles == null)
            {
                ViewBag.ErrorSendingEmail = Resource.ErrorSelectSendTo;
                return View(model);
            }
           
                foreach (var user in _dbContext.Users)
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
            Auction auction = _dbContext.Auctions.Include("Product")
                                           .Include("Product.Category")
                                           .Include("Seller")
                                           .SingleOrDefault(x => x.Id == Id);
            //Getting the product
            Product prodcut = _dbContext.Products.Find(auction.Product.Id);
            //Getting list of bids on the auction
            List<Bid> bids = _dbContext.Bids.Where(x => x.Auction.Id == auction.Id).ToList();
            //Get the list of reports on the auctions
            List<Report> reports = _dbContext.Reports.Where(x => x.auction.Id == auction.Id).ToList();
            //Get the list of product images
            List<ProductPhoto> images = _dbContext.Images.Where(x => x.Product.Id == prodcut.Id).ToList();

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
                    _dbContext.Bids.Remove(bid);
                }
                //Deleting images from DB
                foreach (var p in images)
                {
                    _dbContext.Images.Remove(p);
                }
                //Deleteing Reports from DB
                foreach (var r in reports)
                {
                    _dbContext.Reports.Remove(r);
                }
                //Deleting the auction from DB
                _dbContext.Auctions.Remove(auction);
                //Deleting the product from DB
                _dbContext.Products.Remove(prodcut);
                _dbContext.SaveChanges();

                //Sending email to seller
                try
                {
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
            Auction auction = _dbContext.Auctions.Include("Seller")
                                         .Include("Product")
                                         .Include("Product.Category")
                                         .SingleOrDefault(x => x.Id == Id);
            //Getting the product
            Product prodcut = _dbContext.Products.Find(auction.Product.Id);
            //Getting list of bids on the auction
            List<Bid> bids = _dbContext.Bids.Where(x => x.Auction.Id == auction.Id).ToList();
            //Getting list of reports on the auction
            List<Report> reports = _dbContext.Reports.Where(x => x.auction.Id == auction.Id).ToList();
            //Getting product images
            List<ProductPhoto> images = _dbContext.Images.Where(x => x.Product.Id == prodcut.Id).ToList();

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

            //Sending a message to the administation
            Message msg = new Message();
            msg.Email = "Automatic@Email.website";
            msg.MessageDateAndTime = DateTime.Now;
            msg.SenderType = "Website";
            msg.IsSeen = false;
            msg.RepliedBy = null;

            switch (_dbContext.Users.Find(User.Identity.GetUserId()).PreferredInterfaceLanguage)
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
                        _dbContext.Bids.Remove(bid);
                    }
                    //Deleting images from DB
                    foreach (var p in images)
                    {
                        _dbContext.Images.Remove(p);
                    }
                    //Deleting reports from DB
                    foreach (var r in reports)
                    {
                        _dbContext.Reports.Remove(r);
                    }
                    //Deleting the auction from DB
                    _dbContext.Auctions.Remove(auction);
                    //Deleting the product from DB
                    _dbContext.Products.Remove(prodcut);

                    _dbContext.Messages.Add(msg);
                    _dbContext.SaveChanges();

                    //Sending an email to the user telling them that their auction was removed

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

        //Check if the uploaded file i an image file
        private bool checkFileType(string fileName)
        {
            string fileExtention = Path.GetExtension(fileName);

            switch (fileExtention.ToLower())
            {
                case ".png":
                    return true;
                case ".jpg":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }
    }
}