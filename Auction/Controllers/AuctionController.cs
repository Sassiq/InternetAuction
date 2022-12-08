using Microsoft.AspNet.Identity;
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
    [Authorize(Roles = "User, Admin")]
    public class AuctionController : Controller
    {
        private AuctionContext _dbContext;

        public AuctionController()
        {
            _dbContext = new AuctionContext();
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Index(int? page, int Id, string ProductStatus, int? Currency)
        {

            var user = _dbContext.Users.Find(User.Identity.GetUserId());
            var auctions = await _dbContext.Auctions
                .AsNoTracking()
                .Include("Seller")
                .Include("Buyer")
                .Include("Product")
                .Include("Product.Currency")
                .Include("Product.Images")
                .Include("Product.Images.Product")
                .Where(x => x.Product.Category.Id == Id
                        && x.Finish_Date > DateTime.Now
                        && !x.IsPaid
                        && (x.Product.Status == ProductStatus || ProductStatus == "All" || string.IsNullOrEmpty(ProductStatus)))
                .ToListAsync();

            if (User.Identity.IsAuthenticated)
            { 
                auctions = auctions.Where(x => x.Seller.Id != user.Id).ToList(); 
            }

            ViewBag.AuctionsCount = auctions.Count();

            var pagedAuctions = auctions.ToPagedList(page ?? 1, 12);
            if(Currency != null)
            {
                pagedAuctions = auctions.Where(x => x.Product.Currency.Id == Currency).ToList().ToPagedList(page ?? 1, 12);
            }

            ViewBag.CategoryName = (await _dbContext.Categories.FindAsync(Id)).Category_Name.ToString();
            var currencies = await _dbContext.Currencies
                .AsNoTracking()
                .Where(x => x.Visible)
                .ToListAsync();
            ViewBag.Currency = new SelectList(currencies, "Id", "Name");

            return View(pagedAuctions);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ViewAuction(int Id)
        {
            var auction = await _dbContext.Auctions
                .AsNoTracking()
                .Include("Seller")
                .Include("Buyer")
                .Include("Product")
                .Include("Product.Currency")
                .Include("Product.Category")
                .Include("Bids")
                .SingleOrDefaultAsync(x => x.Id == Id);

            return View(auction);
        }

        [HttpPost]
        public async Task<ActionResult> Bid(int Id, double value, string returnUrl)
        {
            var user = _dbContext.Users.Find(User.Identity.GetUserId());
            var auction = await _dbContext.Auctions
                .Include("Buyer")
                .Include("Seller")
                .Include("Product.Category")
                .SingleOrDefaultAsync(x => x.Id == Id);
            value = (int)double.Parse(Request["Current_Bid"]);
            if (auction != null && value > auction.Current_Bid && auction.Seller.Id != User.Identity.GetUserId())
            {
                auction.Buyer = user;
                auction.Current_Bid = value;

                Bid bid = new Bid()
                {
                    Auction = auction,
                    Bidder = user,
                    BidDateTime = DateTime.Now,
                    BidValue = value
                };
                _dbContext.Bids.Add(bid);
                await _dbContext.SaveChangesAsync();

                return Redirect(returnUrl);
            }

            ViewBag.ErrorBidding = Resource.Error + ": "+ Resource.BidErrorMsg.ToString();
            if (returnUrl == "/Auction/MyOngoingBiddings")
            {
                var bids = await _dbContext.Bids
                    .AsNoTracking()
                    .Include("Auction")
                    .Include("Auction.Product")
                    .Include("Auction.Product.Category")
                    .Include("Auction.Product.Currency")
                    .Include("Auction.Buyer")
                    .Where(x => x.Bidder.Id == user.Id && x.Auction.Finish_Date > DateTime.Now)
                    .ToListAsync();

                var auctions = bids.Select(x => x.Auction).Distinct().ToPagedList(1, 12);

                return View("MyOngoingBiddings", auctions);
            }

            return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<ActionResult> AddNewAuction()
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Where(x => x.Visible)
                .OrderBy(x => x.Category_Name)
                .ToListAsync();
            var currencies = await _dbContext.Currencies
                .AsNoTracking()
                .Where(x => x.Visible)
                .ToListAsync();
            ViewBag.Currency = new SelectList(currencies, "Id", "Name");
            ViewBag.Category = new SelectList(categories, "Id", "Category_Name");
            var model = new AddAuctionModel()
            {
                AccountNumber = _dbContext.Users.Find(User.Identity.GetUserId()).AccountNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AddNewAuction(AddAuctionModel model, HttpPostedFileBase[] uploadFile)
        {
            try
            {
                string path = "~/Images/Items/no-thumbnail.png";
                string fileName = "";
                var images = new List<ProductPhoto>();
                if (uploadFile[0] != null)
                {
                    try
                    {
                        foreach (HttpPostedFileBase file in uploadFile)
                        {
                            if (file != null && CheckMimeType(file.FileName))
                            {
                                fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + file.FileName;
                                path = "~/Images/Items/" + fileName;
                                file.SaveAs(Server.MapPath(path));
                                images.Add(new ProductPhoto() { Path = path });
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
                    images.Add(new ProductPhoto() { Path = path });
                }

                var product = new Product()
                {
                    Name = model.Product.Name,
                    Description = model.Product.Description,
                    Currency = _dbContext.Currencies.Find(Convert.ToInt32(Request["Currency"].ToString())),
                    Price = model.Product.Price,
                    Category = _dbContext.Categories.Find(Convert.ToInt32(Request["Category"].ToString())),
                    Status = Request["StatusRadio"].ToString()
                };

                foreach (var p in images)
                {
                    p.Product = product;
                }

                product.Images = images;

                _dbContext.Products.Add(product);

                var seller = _dbContext.Users.Find(User.Identity.GetUserId());
                seller.AccountNumber = model.AccountNumber;
                var auction = new Auction()
                {
                    Seller = seller,
                    Buyer = null,
                    Product = product,
                    Current_Bid = model.Product.Price,
                    Start_Date = DateTime.Now,
                    Finish_Date = DateTime.Now.AddDays(model.DurationDays).AddHours(model.DurationHrs),
                    IsPaid = false
                };

                _dbContext.Auctions.Add(auction);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("MyAuctions", "Auction");
            }
            catch { }

            var categories = await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(x => x.Category_Name)
                .ToListAsync();
            var currencies = await _dbContext.Currencies.ToListAsync();
            ViewBag.Category = new SelectList(categories, "Id", "Category_Name");
            ViewBag.Currency = new SelectList(currencies, "Id", "Name");

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> MyAuctions(int? page, string AuctionStatus, int? Category)
        {
            var me = _dbContext.Users.Find(User.Identity.GetUserId());
            var myAuctions = await _dbContext.Auctions
                .AsNoTracking()
                .Include("Product")
                .Include("Product.Category")
                .Include("Buyer")
                .Include("Product.Currency")
                .Where(x => x.Seller.Id == me.Id && (x.Product.Category.Id == Category || Category == null))
                .OrderBy(x => (x.Finish_Date > DateTime.Now ? "A" : "B"))
                .ThenBy(x => x.IsPaid)
                .ToListAsync();

            ViewBag.AuctionsCount = myAuctions.Count();
            IPagedList<Auction> filterd;
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

            var myAuctionsCategories = _dbContext.Auctions
                .AsNoTracking()
                .Where(x => x.Seller.Id == me.Id)
                .Select(x => x.Product.Category)
                .Distinct();
            ViewBag.Category = new SelectList(await myAuctionsCategories.OrderBy(x => x.Category_Name).ToListAsync(), "Id", "Category_Name");

            return View(filterd);
        }

        [HttpGet]
        public async Task<ActionResult> MyOngoingBiddings(int? page, string Search)
        {
            var user = _dbContext.Users.Find(User.Identity.GetUserId());
            var bids = await _dbContext.Bids
                .AsNoTracking()
                .Include("Auction")
                .Include("Auction.Product")
                .Include("Auction.Product.Category")
                .Include("Auction.Product.Currency")
                .Include("Auction.Buyer")
                .Where(x => x.Bidder.Id == user.Id 
                        && x.Auction.Finish_Date > DateTime.Now
                        &&(x.Auction.Product.Name.Contains(Search)
                        || String.IsNullOrEmpty(Search)))
                .ToListAsync();

            var auctions = bids.Select(x => x.Auction).Distinct();
            ViewBag.AuctionsCount = auctions.Count();

            return View(auctions.ToPagedList(page ?? 1, 12));
        }

        [HttpGet]
        public async Task<ActionResult> MyWinningAuctions()
        {
            Session["AuctionsToPay"] = null;
            var user = _dbContext.Users.Find(User.Identity.GetUserId());
            var currencies = await _dbContext.Auctions
                .AsNoTracking()
                .Include("Product")
                .Include("Product.Category")
                .Include("Product.Currency")
                .Where(x => x.Buyer.Id == user.Id
                        && x.Finish_Date <= DateTime.Now
                        && x.IsPaid == false)
                .Select(x => x.Product.Currency)
                .Distinct()                   
                .ToListAsync();

            ViewBag.Currencies = currencies;
            int firstCurrency = (currencies.Count > 0 ? currencies[0].Id : -1);

            var winningAuctions = await _dbContext.Auctions
                .Include("Product")
                .Include("Product.Category")
                .Include("Product.Currency")
                .Where(x => x.Buyer.Id == user.Id
                        && x.Finish_Date <= DateTime.Now
                        && x.IsPaid == false
                        && (x.Product.Currency.Id == firstCurrency || firstCurrency == -1))
                .ToListAsync();
            if (winningAuctions.Count() > 0)
            {
                ViewBag.Payment = winningAuctions.Sum(x => x.Current_Bid).ToString() + " " + winningAuctions.FirstOrDefault().Product.Currency.Name;
            }
            Session.Add("AuctionsToPay", winningAuctions);

            return View(winningAuctions);
        }

        [HttpGet]
        public async Task<PartialViewResult> WinningAuctions(int currency)
        {
            var user = _dbContext.Users.Find(User.Identity.GetUserId());
            var winningAuctions = await _dbContext.Auctions
                .AsNoTracking()
                .Include("Product")
                .Include("Product.Category")
                .Include("Product.Currency")
                .Where(x => x.Buyer.Id == user.Id
                        && x.Finish_Date <= DateTime.Now
                        && x.IsPaid == false
                        && (x.Product.Currency.Id == currency))
                .ToListAsync();

            var winAuction = winningAuctions.FirstOrDefault();
            if (winAuction != null)
            {
                ViewBag.Payment = winningAuctions.Sum(x => x.Current_Bid).ToString() +
                                 " " + winningAuctions.FirstOrDefault().Product.Currency.Name;
            }

            Session.Add("AuctionsToPay", winningAuctions);

            return PartialView("_WinningAuctionsPartial", winningAuctions);
        }

        [HttpGet]
        public ActionResult PayBill()
        {
            List<Auction> auctions;
            if (Session["AuctionsToPay"] != null)
            {
                auctions = Session["AuctionsToPay"] as List<Auction>;
            }
            else
            {
                return RedirectToAction("MyWinningAuctions");
            }

            var cardsList = new List<string>();
            cardsList.Add("Visa");
            cardsList.Add("Master Card");
            cardsList.Add("American Express");
            cardsList.Add("Discover Network");

            ViewBag.CardTypes = new SelectList(cardsList);

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

        [HttpPost]
        public async Task<ActionResult> PayBill(Payment payment)
        {
            bool paymentSuccess = false;
            List<Auction> auctions;
            if (Session["AuctionsToPay"] != null)
            {
                auctions = Session["AuctionsToPay"] as List<Auction>;
            }
            else
            {
                return RedirectToAction("MyWinningAuctions");
            }

            var cardsList = new List<string>();
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
                    Auction auc = _dbContext.Auctions.Include("Seller").SingleOrDefault(x => x.Id == auction.Id);
                    payment.SellerAccountNumber = auc.Seller.AccountNumber;
                    /*
                        Payment Proccess goes here
                     */
                    paymentSuccess = true;
                    if (paymentSuccess)
                    {
                        auction.IsPaid = true;
                        auction.PaymentDateTime = DateTime.Now;
                        (await _dbContext.Auctions.FindAsync(auction.Id)).IsPaid = auction.IsPaid;
                        (await _dbContext.Auctions.FindAsync(auction.Id)).PaymentDateTime = auction.PaymentDateTime;
                        await _dbContext.SaveChangesAsync();
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> Search(int? page, string Keyword, int? Category)
        {
            var user = _dbContext.Users.Find(User.Identity.GetUserId());
            var auctions = await _dbContext.Auctions
                .AsNoTracking()
                .Include("Product")
                .Include("Seller")
                .Include("Product.Category")
                .Include("Product.Currency")
                .Where(x => ((x.Product.Name.Contains(Keyword.Trim())
                       || x.Product.Description.Contains(Keyword.Trim())
                       || x.Product.Currency.Name.Contains(Keyword.Trim())
                       || x.Product.Status.Contains(Keyword.Trim())
                       || String.IsNullOrEmpty(Keyword))
                       & (x.Product.Category.Id == Category || Category == null))
                       && x.Finish_Date > DateTime.Now)
                .ToListAsync();

            if (User.Identity.IsAuthenticated)
            {
                auctions = auctions.Where(x => x.Seller.Id != user.Id).ToList();
            }

            ViewBag.AuctionsCount = auctions.Count();
            ViewBag.SearchKeyword = "[ " + Keyword + " ]";
            ViewBag.SearchKeyword = "[ " + Keyword + " ] " + Resource.In + " : [ " + _dbContext.Categories.Find(Category).Category_Name + " ]";

            return View(auctions.ToPagedList(page ?? 1, 12));
        }

        [HttpGet]
        public async Task<ActionResult> CloseAuction(int Id)
        {
            var auction = await _dbContext.Auctions.FindAsync(Id);
            if (auction != null)
            {
                auction.Finish_Date = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("MyAuctions", "Auction");
        }

        [HttpPost]
        public async Task<ActionResult> ExtendAuctionTime()
        {
            try
            {
                int auctionId = int.Parse(Request["FormAuctionId"].ToString());
                var auction = await _dbContext.Auctions.SingleOrDefaultAsync(x => x.Id == auctionId && !x.IsPaid);
                var time = Convert.ToDateTime(auction.Finish_Date);
                if (auction.Finish_Date <= DateTime.Now)
                {
                    auction.Finish_Date = DateTime.Now.AddDays(Convert.ToInt32(Request["DurationDays"].ToString()))
                                           .AddHours(Convert.ToInt32(Request["DurationHrs"].ToString()));
                }
                else
                {
                    auction.Finish_Date = time.AddDays(Convert.ToInt32(Request["DurationDays"].ToString()))
                                            .AddHours(Convert.ToInt32(Request["DurationHrs"].ToString()));
                }
                await _dbContext.SaveChangesAsync();
            }
            catch { }

            return RedirectToAction("MyAuctions", "Auction");
        }

        [HttpPost]
        public async Task<ActionResult> ChangeOpeningBid()
        {
            try
            {
                int id = Convert.ToInt32(Request["OpeningBidFormAuctionId"].ToString());
                var auction = await _dbContext.Auctions
                    .Include("Product")
                    .Include("Product.Category")
                    .Include("Product.Currency")   
                    .Where(x => x.Buyer == null)
                    .SingleOrDefaultAsync(x => x.Id == id);
                if (auction.Buyer == null)
                {
                    auction.Product.Price = Convert.ToDouble(Request["CurrentBid"].ToString());
                    auction.Current_Bid = auction.Product.Price;

                    await _dbContext.SaveChangesAsync();
                }
            }
            catch { }

            return RedirectToAction("MyAuctions", "Auction");
        }

        [HttpGet]
        public async Task<ActionResult> ReportToAdmin(int Id)
        {
            var auction = await _dbContext.Auctions.FindAsync(Id);
            var user = _dbContext.Users.Find(User.Identity.GetUserId());
            var report = new Report()
            {
                auction = auction,
                Seen = false,
                Reporter = user,
                ReportDateAndTime = DateTime.Now
            };

            _dbContext.Reports.Add(report);

            await _dbContext.SaveChangesAsync();

            int reportsCount = _dbContext.Reports
                .AsNoTracking()
                .Include("Auction")
                .Count(x => x.auction.Id == Id);
            
            if (reportsCount >= 50)
            {
                Session.Add("AllowAuctionRemove", true);
                return RedirectToAction("AutomaticRemove", new { Id = Id });
            }

            return RedirectToAction("ViewAuction", "Auction", new { Id = Id });
        }

        [HttpGet]
        public async Task<ActionResult> PaidAuctions(int? page)
        {
            var user = _dbContext.Users.Find(User.Identity.GetUserId());
            var paidAuctions = await _dbContext.Auctions
                .AsNoTracking()
                .Include("Product")
                .Include("Product.Currency")
                .Include("Product.Category")
                .Where(x => x.Buyer.Id == user.Id && x.IsPaid)
                .ToListAsync();

            return View(paidAuctions.ToPagedList(page ?? 1, 6));
        }

        private bool CheckMimeType(string fileName)
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