using OnlineAuctionProject.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Models
{
    public class Country
    {
        public int Id { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CountryReq")]
        public string Name { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CountryReq")]
        public string Name_Ar { get; set; }
        public bool Visible { get; set; }

    }
}