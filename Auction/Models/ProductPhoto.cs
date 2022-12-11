using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Models
{
    public class ProductPhoto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public Product Product { get; set; }
    }
}