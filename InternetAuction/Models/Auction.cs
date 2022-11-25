using OnlineAuctionProject.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Models
{
    public class Auction
    {
        public int Id { get; set; }
        public ApplicationUser Seller { get; set; }
        public ApplicationUser Buyer { get; set; }
        public Product Product { get; set; }

        [Display(Name = "CurrentBid", ResourceType = typeof(Resource))]
        public double Current_Bid { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime? Finish_Date { get; set; }
        public virtual List<Bid> Bids { get; set; }
        public bool IsPaid { get; set; }

        [Display(Name = "PaymentDateTime", ResourceType = typeof(Resource))]
        public DateTime? PaymentDateTime { get; set; }
    }

    public class AddAuctionModel
    {
        public Product Product { get; set; }

        [Display(Name = "DurationDays", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "DurationDaysReq")]
        [Range(0, int.MaxValue,ErrorMessageResourceType=typeof(Resource),ErrorMessageResourceName="DurationDaysRange")]
        public int DurationDays { get; set; }


        [Display(Name = "DurationHrs", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "DurationHrsReq")]
        [Range(0, 24,ErrorMessageResourceType=typeof(Resource),ErrorMessageResourceName="DurationHrsRange")]
        public int DurationHrs { get; set; }

        [Display(Name = "AccountNumber", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "AccountNumberReq")]
        [DataType(DataType.CreditCard)]
        public string AccountNumber { get; set; }

    }
}