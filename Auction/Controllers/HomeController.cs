using Microsoft.AspNet.Identity;
using OnlineAuctionProject.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OnlineAuctionProject.Controllers
{
    public class HomeController : Controller
    {
        AuctionContext db = new AuctionContext();
        
        //GET //All categories
        [HttpGet]
        public ActionResult Index(int? page, string Search = "")
        {
            //Get list of visible categories
            IEnumerable<Category> categories = db.Categories.Where(x => x.Visible)
                                                            .ToList();

            IPagedList<Category> orderedCategories = null;

            orderedCategories = categories.Where(x => x.Category_Name.ToLower().Contains(Search.ToLower()))
                                                  .OrderBy(x => x.Category_Name).ToPagedList(page ?? 1, 12);

            return View(orderedCategories);
        }

        //GET //About page
        [HttpGet]
        public ActionResult About()
        {
            return View();
        }

        //GET //Contact administration
        [HttpGet]
        public ActionResult Contact()
        {
            //If a user logged in, set the email as the user's email
            if(User.Identity.IsAuthenticated)
            {
                ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
                Message message = new Message()
                {
                    Email = user.Email
                };
                ViewBag.WelcomeMessage = Resources.Resource.Welcome + " " + user.UserName.ToString();
                return View(message);
            }
            return View();
        }

        //POST //Contact administration
        [HttpPost]
        public ActionResult Contact(Message message)
        {
            if (ModelState.IsValid)
            {
                //Creating a message
                Message msg = new Message()
                {
                    Email = message.Email,
                    MessageText = message.MessageText,
                    MessageDateAndTime = DateTime.Now,
                    IsSeen = false
                };
                ApplicationUser user = db.Users.SingleOrDefault(x => x.Email == message.Email);
                if(user != null)
                {
                    msg.SenderType = "User";
                }
                else
                {
                    msg.SenderType = "Visitor";
                }
                db.Messages.Add(msg);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(message);
        }
    }
}