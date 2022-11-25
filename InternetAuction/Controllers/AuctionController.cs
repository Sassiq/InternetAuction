using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineAuctionProject.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using PagedList;
using PagedList.Mvc;
using OnlineAuctionProject.ManageWebsiteLanguage;
using System.Data.Entity.Validation;
using OnlineAuctionProject.Resources;
using System.Net.Mail;
using OnlineAuctionProject.Repository;

namespace OnlineAuctionProject.Controllers
{
    [Authorize(Roles = "User")]
    public class AuctionController : BaseController
    {
        ApplicationDbContext db = new ApplicationDbContext();

        //GET //All auctions in a category, can be filterd by product status and currency
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index(int? page, int Id, string ProductStatus, int? Currency)
        {
            //Getting current user
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

            //getting list of ongoing auctions
            IEnumerable<Auction> auctions = db.Auctions.Include("Seller")
                                                    .Include("Buyer")
                                                    .Include("Product")
                                                    .Include("Product.Currency")
                                                    .Include("Product.Images")
                                                    .Include("Product.Images.Product")
                                                    .Where(x => x.Product.Category.Id == Id
                                                                && x.Finish_Date > DateTime.Now
                                                                && !x.IsPaid
                                                                && (x.Product.Status == ProductStatus
                                                                    || ProductStatus == "All"
                                                                    || String.IsNullOrEmpty(ProductStatus)))
                                                    .ToList();

            if (User.Identity.IsAuthenticated)
                auctions = auctions.Where(x => x.Seller.Id != user.Id).ToList();

            ViewBag.AuctionsCount = auctions.Count();

            auctions = auctions.ToPagedList(page ?? 1, 12);
            //Filtering by currency
            if(Currency != null)
            {
                auctions = auctions.Where(x => x.Product.Currency.Id == Currency).ToList().ToPagedList(page ?? 1, 12);
            }
            //Displaying category 
            switch (SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.CategoryName = db.Categories.Find(Id).Category_Name.ToString();
                    break;
                case "ar-SA":
                    ViewBag.CategoryName = db.Categories.Find(Id).Category_Name_Ar.ToString();
                    break;
                default:
                    ViewBag.CategoryName = db.Categories.Find(Id).Category_Name.ToString();
                    break;
            }

            //Getting list of currencies
            ViewBag.Currency = new SelectList(db.Currencies.Where(x => x.Visible).ToList(), "Id", "Name");
            

            return View(auctions);
        }

        //GET //View an auction
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ViewAuction(int Id)
        {
            //Getting the auction
            Auction auction = db.Auctions.Include("Seller")
                                                .Include("Buyer")
                                                .Include("Product")
                                                .Include("Product.Currency")
                                                .Include("Product.Category")
                                                .Include("Bids")
                                                .SingleOrDefault(x => x.Id == Id);

            return View(auction);
        }

        //POST //Bidding on auction
        [HttpPost]
        public ActionResult Bid(int Id, double value,string returnUrl)
        {
            //Getting the auction
            Auction auction = db.Auctions.Include("Buyer")
                                         .Include("Seller")
                                         .Include("Product.Category")
                                         .SingleOrDefault(x => x.Id == Id);
            //Getting the current user
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            //Getting the new bid value
            value = double.Parse(Request["Current_Bid"]);

            //Checking if the new value is greater than the current value
            if (value > auction.Current_Bid && auction.Seller.Id != User.Identity.GetUserId())
            {
                //Setting the new buyer and the new bid
                auction.Buyer = user;
                auction.Current_Bid = value;

                //Saving bidding operation to DB
                Bid bid = new Bid()
                    {
                        Auction = auction,
                        Bidder = user,
                        BidDateTime = DateTime.Now,
                        BidValue = value
                    };
                db.Bids.Add(bid);

                db.SaveChanges();

                return Redirect(returnUrl);
            }
            //if the new value is less or equal than the current value, display an error message.
            ViewBag.ErrorBidding = Resource.Error + ": "+ Resource.BidErrorMsg.ToString();

            //Bidding from the my ongoing auctions page
            if (returnUrl == "/Auction/MyOngoingBiddings")
            {
                IEnumerable<Bid> bids = db.Bids.Include("Auction")
                                          .Include("Auction.Product")
                                          .Include("Auction.Product.Category")
                                          .Include("Auction.Product.Currency")
                                          .Include("Auction.Buyer")
                                          .Where(x => x.Bidder.Id == user.Id
                                                   && x.Auction.Finish_Date > DateTime.Now)
                                          .ToList();

                IPagedList<Auction> auctions = bids.Select(x => x.Auction).Distinct().ToPagedList(1, 12);

                return View("MyOngoingBiddings", auctions);
            }

            return Redirect(returnUrl);
        }


        //GET //Add a new auction
        [HttpGet]
        public ActionResult AddNewAuction()
        {
            //Getting list of categories
            List<Category> categories = db.Categories.Where(x => x.Visible).ToList();
            //Getting list of currencies
            List<Currency> currencies = db.Currencies.Where(x => x.Visible).ToList();
            ViewBag.Currency = new SelectList(currencies, "Id", "Name");

            //Displaying categories according to current website language
            switch(SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.Category = new SelectList(categories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");
                    break;
                case "ar-SA":
                    ViewBag.Category = new SelectList(categories.OrderBy(x => x.Category_Name_Ar).ToList(), "Id", "Category_Name_Ar");
                    break;
                default:
                    ViewBag.Category = new SelectList(categories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");
                    break;
            }

            AddAuctionModel model = new AddAuctionModel()
            {
                AccountNumber = db.Users.Find(User.Identity.GetUserId()).AccountNumber
            };
            

            return View(model);
        }

        //POST //Add a new auction
        [HttpPost]
        public ActionResult AddNewAuction(AddAuctionModel model, HttpPostedFileBase[] uploadFile)
        {
            try
            {
                //If no image uploaded, then save a default image
                string path = "~/Images/Items/no-thumbnail.png";
                string fileName = "";

                //Creating a list of images
                List<ProductPhoto> Images = new List<ProductPhoto>();

                //Uploading multiple images
                if (uploadFile[0] != null)
                {
                    try
                    {
                        foreach (HttpPostedFileBase file in uploadFile)
                        {
                            if (file != null && checkFileType(file.FileName))
                            {
                                fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + file.FileName;
                                path = "~/Images/Items/" + fileName;
                                file.SaveAs(Server.MapPath(path));
                                Images.Add(new ProductPhoto() { Path = path });
                            }
                        }
                    }
                    catch
                    {
                        ViewBag.ImageSizeError = Resource.ImageUploadError;
                        return View(model);
                    }
                }
                else
                {
                    Images.Add(new ProductPhoto() { Path = path });
                }
                //Creating a new product
                Product product = new Product()
                {
                    Name = model.Product.Name,
                    Description = model.Product.Description,
                    Currency = db.Currencies.Find(Convert.ToInt32(Request["Currency"].ToString())),
                    Price = model.Product.Price,
                    Category = db.Categories.Find(Convert.ToInt32(Request["Category"].ToString())),
                    Status = Request["StatusRadio"].ToString()
                };
                //Setting the product images
                foreach (var p in Images)
                {
                    p.Product = product;
                }

                product.Images = Images;

                //Adding the new product to DB
                db.Products.Add(product);


                ApplicationUser seller = db.Users.Find(User.Identity.GetUserId());
                //Creating a new auction
                Auction auction = new Auction()
                {
                    Seller = seller,
                    Buyer = null,
                    Product = product,
                    Current_Bid = model.Product.Price,
                    Start_Date = DateTime.Now,
                    Finish_Date = DateTime.Now.AddDays(model.DurationDays).AddHours(model.DurationHrs),
                    IsPaid = false
                };
                seller.AccountNumber = model.AccountNumber;

                //Adding the auction to DB
                db.Auctions.Add(auction);

                db.SaveChanges();

                return RedirectToAction("MyAuctions", "Auction");
            }
            catch { }

            List<Category> categories = db.Categories.ToList();
            List<Currency> currencies = db.Currencies.ToList();

            //Displaying categories according to current website language
            switch(SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.Category = new SelectList(categories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");
                    break;
                case "ar-SA":
                    ViewBag.Category = new SelectList(categories.OrderBy(x => x.Category_Name_Ar).ToList(), "Id", "Category_Name_Ar");
                    break;
                default:
                    ViewBag.Category = new SelectList(categories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");
                    break;
            }
            ViewBag.Currency = new SelectList(currencies, "Id", "Name");

            return View(model);
        }

        //GET //My auctions
        [HttpGet]
        public ActionResult MyAuctions(int? page, string AuctionStatus, int? Category)
        {
            //Get the current user
            ApplicationUser me = db.Users.Find(User.Identity.GetUserId());

            //Get list of user's auctions
            IEnumerable<Auction> myAuctions = db.Auctions.Include("Product")
                                                        .Include("Product.Category")
                                                        .Include("Buyer")
                                                        .Include("Product.Currency")
                                                        .Where(x => x.Seller.Id == me.Id
                                                                 && (x.Product.Category.Id == Category
                                                                 || Category == null))
                                                        .OrderBy(x => (x.Finish_Date > DateTime.Now ? "A" : "B"))
                                                        .ThenBy(x => x.IsPaid)
                                                        .ToList();

            ViewBag.AuctionsCount = myAuctions.Count();

            //Filtering auctions by: Ongoing, Finished, Paid, Not paid
            IPagedList<Auction> filterd = null;

            switch (AuctionStatus)
            {
                case "Ongoing":
                    filterd = myAuctions.Where(x => x.Finish_Date.Value > DateTime.Now).ToList().ToPagedList(page ?? 1, 5);
                    break;
                case "Finished":
                    filterd = myAuctions.Where(x => x.Finish_Date.Value <= DateTime.Now).ToList().ToPagedList(page ?? 1, 5);
                    break;
                case "Paid":
                    filterd = myAuctions.Where(x => x.Finish_Date.Value <= DateTime.Now && x.IsPaid).ToList().ToPagedList(page ?? 1, 5);
                    break;
                case "NotPaid":
                    filterd = myAuctions.Where(x => x.Finish_Date.Value <= DateTime.Now && !x.IsPaid).ToList().ToPagedList(page ?? 1, 5);
                    break;
                default:
                    filterd = myAuctions.ToPagedList(page ?? 1, 5);
                    break;
            }

            ViewBag.MyAuctionsCount = myAuctions.Count();
            ViewBag.MyOngoingAuctionsCount = myAuctions.Where(x => x.Finish_Date > DateTime.Now).Count();
            ViewBag.FinishedAuctionsCount = myAuctions.Where(x => x.Finish_Date <= DateTime.Now).Count();
            ViewBag.MyPaidAuctionsCount = myAuctions.Where(x => x.IsPaid).Count();
            ViewBag.MyNotPaidAuctionsCount = myAuctions.Where(x => x.Finish_Date <= DateTime.Now && !x.IsPaid).Count();

            List<Category> myAuctionsCategories = db.Auctions.Where(x => x.Seller.Id == me.Id)
                                                             .Select(x => x.Product.Category)
                                                             .Distinct()
                                                             .ToList();

            switch (SiteLanguages.GetCurrentLanguageCulture())
            {
                case "en-US":
                    ViewBag.Category = new SelectList(myAuctionsCategories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");
                    break;
                case "ar-SA":
                    ViewBag.Category = new SelectList(myAuctionsCategories.OrderBy(x => x.Category_Name_Ar).ToList(), "Id", "Category_Name_Ar");
                    break;
                default:
                    ViewBag.Category = new SelectList(myAuctionsCategories.OrderBy(x => x.Category_Name).ToList(), "Id", "Category_Name");
                    break;
            }

            return View(filterd);
        }

       

        //GET //User's ongoing biddings
        [HttpGet]
        public ActionResult MyOngoingBiddings(int? page, string Search)
        {
            //Getting the current user
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

            //Getting the auctions that the logged in user are currently bidding in.

            IEnumerable<Bid> bids = db.Bids.Include("Auction")
                                          .Include("Auction.Product")
                                          .Include("Auction.Product.Category")
                                          .Include("Auction.Product.Currency")
                                          .Include("Auction.Buyer")
                                          .Where(x => x.Bidder.Id == user.Id 
                                                   && x.Auction.Finish_Date > DateTime.Now
                                                   &&(x.Auction.Product.Name.Contains(Search)
                                                   || String.IsNullOrEmpty(Search)))
                                          .ToList();

            IEnumerable<Auction> auctions = bids.Select(x => x.Auction).Distinct();
            ViewBag.AuctionsCount = auctions.Count();

            return View(auctions.ToPagedList(page ?? 1, 12));
        }

        //GET //User's winning auctions
        [HttpGet]
        public ActionResult MyWinningAuctions()
        {
            Session["AuctionsToPay"] = null;
            //Getting the current user
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

            //Getting user's winning auctions by currency
            List<Currency> currencies = db.Auctions.Include("Product")
                                                             .Include("Product.Category")
                                                             .Include("Product.Currency")
                                                             .Where(x => x.Buyer.Id == user.Id
                                                                      && x.Finish_Date <= DateTime.Now
                                                                      && x.IsPaid == false)
                                                             .Select(x => x.Product.Currency)
                                                             .Distinct()
                                                             .ToList();

            ViewBag.Currencies = currencies;
            int firstCurrency = (currencies.Count > 0 ? currencies[0].Id : -1);

            IEnumerable<Auction> winningAuctions = db.Auctions.Include("Product")
                                                            .Include("Product.Category")
                                                            .Include("Product.Currency")
                                                            .Where(x => x.Buyer.Id == user.Id
                                                                     && x.Finish_Date <= DateTime.Now
                                                                     && x.IsPaid == false
                                                                     && (x.Product.Currency.Id == firstCurrency
                                                                     || firstCurrency == -1))
                                                            .ToList();
            //Getting the overall value 
            if (winningAuctions.Count() > 0)
                ViewBag.Payment = winningAuctions.Sum(x => x.Current_Bid).ToString() +
                                                " " + winningAuctions.FirstOrDefault().Product.Currency.Name;
            //Saving the auction to pay in a session
            Session.Add("AuctionsToPay", winningAuctions);

            return View(winningAuctions);
        }

        //GET //User's winning auctions by currencu
        [HttpGet]
        public PartialViewResult WinningAuctions(int currency)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

            IEnumerable<Auction> winningAuctions = db.Auctions.Include("Product")
                                                             .Include("Product.Category")
                                                             .Include("Product.Currency")
                                                             .Where(x => x.Buyer.Id == user.Id
                                                                      && x.Finish_Date <= DateTime.Now
                                                                      && x.IsPaid == false
                                                                      && (x.Product.Currency.Id == currency
                                                                      || currency == null))
                                                             .ToList();

            ViewBag.Payment = winningAuctions.Sum(x => x.Current_Bid).ToString() +
                                           " " + winningAuctions.FirstOrDefault().Product.Currency.Name;

            Session.Add("AuctionsToPay", winningAuctions);

            return PartialView("_WinningAuctionsPartial", winningAuctions);
        }

        //GET //Payment Page
        [HttpGet]
        public ActionResult PayBill()
        {
            //Getting the auctions list that the user must pay for.
            List<Auction> auctions = null;
            if (Session["AuctionsToPay"] != null)
            {
                auctions = Session["AuctionsToPay"] as List<Auction>;
            }
            else
            {
                return RedirectToAction("MyWinningAuctions");
            }

            //List of available credit cards
            List<string> cardsList = new List<string>();
            cardsList.Add("Visa");
            cardsList.Add("Master Card");
            cardsList.Add("American Express");
            cardsList.Add("Discover Network");

            ViewBag.CardTypes = new SelectList(cardsList);

            //Setting the payment value
            Payment payment = new Payment();
            payment.PaymentValue = auctions.Sum(x => x.Current_Bid) + " " + auctions.FirstOrDefault().Product.Currency.Name;

            //Creating a list of months and years
            List<int> months = new List<int>();
            for (int i = 01; i <= 12; i++)
                months.Add(i);

            List<int> years = new List<int>();
            for (int i = DateTime.Now.Year; i <= DateTime.Now.AddYears(10).Year; i++)
                years.Add(i);

            ViewBag.Month = new SelectList(months);
            ViewBag.Year = new SelectList(years);

            return View(payment);
        }

        //POST //Payment
        [HttpPost]
        public ActionResult PayBill(Payment payment)
        {
            bool paymentSuccess = false;
            List<Auction> auctions = null;
            if (Session["AuctionsToPay"] != null)
            {
                auctions = Session["AuctionsToPay"] as List<Auction>;
            }
            else
            {
                return RedirectToAction("MyWinningAuctions");
            }

            //List of available credit cards
            List<string> cardsList = new List<string>();
            cardsList.Add("Visa");
            cardsList.Add("Master Card");
            cardsList.Add("American Express");
            cardsList.Add("Discover Network");

            List<int> months = new List<int>();
            for (int i = 01; i <= 12; i++)
                months.Add(i);

            List<int> years = new List<int>();
            for (int i = DateTime.Now.Year; i <= DateTime.Now.AddYears(10).Year; i++)
                years.Add(i);

            try
            {
                foreach (Auction auction in auctions)
                {
                    Auction auc = db.Auctions.Include("Seller").SingleOrDefault(x => x.Id == auction.Id);
                    payment.SellerAccountNumber = auc.Seller.AccountNumber;
                    /*
                        Payment Proccess goes here
                     */
                    paymentSuccess = true;
                    if (paymentSuccess)
                    {
                        auction.IsPaid = true;
                        auction.PaymentDateTime = DateTime.Now;
                        db.Auctions.Find(auction.Id).IsPaid = auction.IsPaid;
                        db.Auctions.Find(auction.Id).PaymentDateTime = auction.PaymentDateTime;
                        db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.CardTypes = new SelectList(cardsList);
                        ViewBag.Month = new SelectList(months);
                        ViewBag.Year = new SelectList(years);
                        ViewBag.PaymentFailed = Resource.PaymentFailed;
                        return View(payment);
                    }
                }
                return RedirectToAction("RedirectToPage", "Base", new { returnUrl = "/Auction/MyWinningAuctions" });

            }
            catch
            {

            }
            

            ViewBag.CardTypes = new SelectList(cardsList);
            ViewBag.Month = new SelectList(months);
            ViewBag.Year = new SelectList(years);
            return View(payment);
        }


        //Search for auctions
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Search(int? page, string Keyword, int? Category)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            IEnumerable<Auction> auctions = db.Auctions.Include("Product")
                                                       .Include("Seller")
                                                      .Include("Product.Category")
                                                      .Include("Product.Currency")
                                                      .Where(x => ((x.Product.Name.Contains(Keyword.Trim())
                                                               || x.Product.Description.Contains(Keyword.Trim())
                                                               || x.Product.Currency.Name.Contains(Keyword.Trim())
                                                               || x.Product.Status.Contains(Keyword.Trim())
                                                               || String.IsNullOrEmpty(Keyword))
                                                               && (x.Product.Category.Id == Category
                                                                     || Category == null))
                                                               && x.Finish_Date > DateTime.Now)
                                                               .ToList();
                                                               
            if (User.Identity.IsAuthenticated)
                auctions = auctions.Where(x => x.Seller.Id != user.Id).ToList();

            ViewBag.AuctionsCount = auctions.Count();

            ViewBag.SearchKeyword = "[ " + Keyword + " ]";
            if (Category != null)
            {
                switch(SiteLanguages.GetCurrentLanguageCulture())
                {
                    case"en-US":
                        ViewBag.SearchKeyword = "[ " + Keyword + " ] " + Resource.In +" : [ " + db.Categories.Find(Category).Category_Name + " ]";
                        break;
                    case "ar-SA":
                        ViewBag.SearchKeyword = "[ " + Keyword + " ] " + Resource.In +" : [ " + db.Categories.Find(Category).Category_Name_Ar + " ]";
                        break;
                    default:
                        ViewBag.SearchKeyword = "[ " + Keyword + " ] " + Resource.In +" : [ " + db.Categories.Find(Category).Category_Name + " ]";
                        break;
                }
            }
            return View(auctions.ToPagedList(page ?? 1, 12));
        }

        //GET //Manually close an auction
        [HttpGet]
        public ActionResult CloseAuction(int Id)
        {
            try
            {
                Auction auction = db.Auctions.Find(Id);
                auction.Finish_Date = DateTime.Now;
                db.SaveChanges();
            }
            catch
            {

            }

            return RedirectToAction("MyAuctions", "Auction");
        }

        //POST //Manually extend auction time
        [HttpPost]
        public ActionResult ExtendAuctionTime()
        {
            try
            {
                int auctionID = int.Parse(Request["FormAuctionId"].ToString());
                Auction auction = db.Auctions.Where(x => !x.IsPaid)
                                             .SingleOrDefault(x => x.Id == auctionID);

                DateTime dt = Convert.ToDateTime(auction.Finish_Date);

                //If auction is already finished, extend time by adding to current date and time
                if (auction.Finish_Date <= DateTime.Now)
                {
                    auction.Finish_Date = DateTime.Now.AddDays(Convert.ToInt32(Request["DurationDays"].ToString()))
                                           .AddHours(Convert.ToInt32(Request["DurationHrs"].ToString()));
                }
                else // else extend by adding to auction stored finish date and time
                {
                    auction.Finish_Date = dt.AddDays(Convert.ToInt32(Request["DurationDays"].ToString()))
                                            .AddHours(Convert.ToInt32(Request["DurationHrs"].ToString()));
                }
                db.SaveChanges();
            }
            catch
            {

            }
            return RedirectToAction("MyAuctions", "Auction");
        }


        //POST //Change auction openning bid
        [HttpPost]
        public ActionResult ChangeOpeningBid()
        {
            try
            {
                int id = Convert.ToInt32(Request["OpeningBidFormAuctionId"].ToString());

                Auction auction = db.Auctions.Include("Product")
                                             .Include("Product.Category")
                                             .Include("Product.Currency")
                                             .Where(x => x.Buyer == null)
                                             .SingleOrDefault(x => x.Id == id);
                if (auction.Buyer == null)
                {
                    //Changing the openning bid price
                    auction.Product.Price = Convert.ToDouble(Request["CurrentBid"].ToString());
                    auction.Current_Bid = auction.Product.Price;

                    db.SaveChanges();
                }
            }
            catch
            {

            }
            return RedirectToAction("MyAuctions", "Auction");
        }

        //GET //Reporting an auction to administration
        [HttpGet]
        public ActionResult ReportToAdmin(int Id)
        {
            Auction auction = db.Auctions.Find(Id);
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());


            Report report = new Report()
            {
                auction = auction,
                Seen = false,
                Reporter = user,
                ReportDateAndTime = DateTime.Now
            };
            db.Reports.Add(report);

            db.SaveChanges();


            //Automatically remove auction when it gets 50 reports
            int reportsCount = db.Reports.Include("Auction").Where(x => x.auction.Id == Id).Count();
            
            if(reportsCount >= 50)
            {
                Session.Add("AllowAuctionRemove", true);
                return RedirectToAction("AutomaticRemove", new { Id = Id });
            }

            return RedirectToAction("ViewAuction", "Auction", new { Id = Id });
        }

        //GET //User's paid auctions
        [HttpGet]
        public ActionResult PaidAuctions(int? page)
        {
            //Getting the current users
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());

            //Get the list of already paid auction
            IEnumerable<Auction> paidAuctions = db.Auctions.Include("Product")
                                                          .Include("Product.Currency")
                                                          .Include("Product.Category")
                                                          .Where(x => x.Buyer.Id == user.Id && x.IsPaid)
                                                          .ToList();

            return View(paidAuctions.ToPagedList(page ?? 1, 6));
        }

	}
}