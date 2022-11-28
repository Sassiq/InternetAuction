using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using OnlineAuctionProject.Resources;

namespace OnlineAuctionProject.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "ProductName", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ProductNameReq")]
        [StringLength(25,ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ProductNameLength")]
        public string Name { get; set; }

        [Display(Name = "ProductStatus", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ProductStatusReq")]
        public string Status { get; set; }

        [Display(Name = "ProductDescription", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ProductDescriptionReq")]
        public string Description { get; set; }

        [Display(Name = "ProductPrice", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ProductPriceReq")]
        [Range(0,double.MaxValue ,ErrorMessageResourceType = typeof(Resource) ,ErrorMessageResourceName = "ProductPriceCurrency")]
        public double Price { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CurrencyReq")]
        public Currency Currency { get; set; }

        [Display(Name = "ProductImage", ResourceType = typeof(Resource))]
        public virtual List<ProductPhoto> Images { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CategoryReq")]
        public Category Category { get; set; }

    }
}