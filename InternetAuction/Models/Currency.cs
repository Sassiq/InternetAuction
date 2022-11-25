using OnlineAuctionProject.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Models
{
    public class Currency
    {
        public int Id { get; set; }

        [Display(Name = "Currency", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CurrencyReq")]
        public string Name { get; set; }
        public bool Visible { get; set; }
    }
}