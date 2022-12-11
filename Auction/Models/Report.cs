using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Models
{
    public class Report
    {
        public int Id { get; set; }
        public Auction auction { get; set; }
        public virtual ApplicationUser Reporter { get; set; }
        public DateTime ReportDateAndTime { get; set; }
        public bool Seen { get; set; }
    }
}