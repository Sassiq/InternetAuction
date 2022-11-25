using OnlineAuctionProject.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Display(Name = "Category", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CategoryReq")]
        public string Category_Name { get; set; }

        [Display(Name = "Category", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CategoryReq")]
        public string Category_Name_Ar { get; set; }

        [Display(Name = "CategoryImage", ResourceType = typeof(Resource))]
        public string Image { get; set; }
        public virtual List<Product> Products { get; set; }
        public bool Visible { get; set; }
    }
}