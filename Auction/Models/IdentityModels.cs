using Microsoft.AspNet.Identity.EntityFramework;
using OnlineAuctionProject.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace OnlineAuctionProject.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "FirstName", ResourceType = typeof(Resource))]
        public string FirstName { get; set; }

        [Display(Name = "LastName", ResourceType = typeof(Resource))]
        public string LastName { get; set; }

        [Display(Name = "Gender", ResourceType = typeof(Resource))]
        public string Gender { get; set; }

        [Display(Name = "Email", ResourceType = typeof(Resource))]
        public string Email { get; set; }

        [Display(Name = "PhNumber", ResourceType = typeof(Resource))]
        public string PhoneNumber { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Resource))]
        public string Country { get; set; }

        [Display(Name = "City", ResourceType = typeof(Resource))]
        public string City { get; set; }

        [Display(Name = "Street", ResourceType = typeof(Resource))]
        public string Street { get; set; }

        [Display(Name = "ZipCode", ResourceType = typeof(Resource))]
        public string ZipCode { get; set; }
        public bool Active { get; set; }

        [Display(Name = "RegistrationDateTime", ResourceType = typeof(Resource))]
        public DateTime? RegistrationDateTime { get; set; }

        [Display(Name = "LastLoginDateTime", ResourceType = typeof(Resource))]
        public DateTime? LastLoginDateTime { get; set; }

        [Display(Name = "LoginsCount", ResourceType = typeof(Resource))]
        public int LoginCount { get; set; }

        [Display(Name = "AccountNumber", ResourceType = typeof(Resource))]
        public string AccountNumber { get; set; }
        public string PreferredInterfaceLanguage { get; set; }
    }

    public class AuctionContext : IdentityDbContext<ApplicationUser>
    {
        public AuctionContext()
            : base("DefaultConnection")
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ProductPhoto> Images { get; set; }
        public DbSet<Statistic> Statistics { get; set; }
        public DbSet<Report> Reports { get; set; }
    }

}