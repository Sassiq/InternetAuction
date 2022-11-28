using OnlineAuctionProject.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Models
{
    public class Statistic
    {
        public int Id { get; set; }
        public ApplicationUser User { get; set; }

        [Display(Name = "UserAuctionsCount", ResourceType = typeof(Resource))]
        public int UserAuctionsCount { get; set; }

        [Display(Name = "UserBiddingsCount", ResourceType = typeof(Resource))]
        public int UserBiddingsCount { get; set; }

        [Display(Name = "UserWinningAuctionsCount", ResourceType = typeof(Resource))]
        public int UserWinningAuctionsCount { get; set; }

        [Display(Name = "PaidAuctions", ResourceType = typeof(Resource))]
        public int PaidAuctions { get; set; }

        [Display(Name = "LoginsCount", ResourceType = typeof(Resource))]
        public int LoginsCount { get; set; }
    }
}