using OnlineAuctionProject.Resources;
using System.ComponentModel.DataAnnotations;

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