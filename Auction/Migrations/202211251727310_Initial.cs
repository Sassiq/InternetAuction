namespace OnlineAuctionProject.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Auctions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Current_Bid = c.Double(nullable: false),
                        Start_Date = c.DateTime(nullable: false),
                        Finish_Date = c.DateTime(),
                        IsPaid = c.Boolean(nullable: false),
                        PaymentDateTime = c.DateTime(),
                        Buyer_Id = c.String(maxLength: 128),
                        Product_Id = c.Int(),
                        Seller_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Buyer_Id)
                .ForeignKey("dbo.Products", t => t.Product_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Seller_Id)
                .Index(t => t.Buyer_Id)
                .Index(t => t.Product_Id)
                .Index(t => t.Seller_Id);
            
            CreateTable(
                "dbo.Bids",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BidValue = c.Double(nullable: false),
                        BidDateTime = c.DateTime(),
                        Auction_Id = c.Int(),
                        Bidder_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Auctions", t => t.Auction_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Bidder_Id)
                .Index(t => t.Auction_Id)
                .Index(t => t.Bidder_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Gender = c.String(),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        Country = c.String(),
                        City = c.String(),
                        Street = c.String(),
                        ZipCode = c.String(),
                        Active = c.Boolean(),
                        RegistrationDateTime = c.DateTime(),
                        LastLoginDateTime = c.DateTime(),
                        LoginCount = c.Int(),
                        AccountNumber = c.String(),
                        PreferredInterfaceLanguage = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 25),
                        Status = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        Price = c.Double(nullable: false),
                        Category_Id = c.Int(nullable: false),
                        Currency_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.Category_Id, cascadeDelete: true)
                .ForeignKey("dbo.Currencies", t => t.Currency_Id, cascadeDelete: true)
                .Index(t => t.Category_Id)
                .Index(t => t.Currency_Id);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Category_Name = c.String(nullable: false),
                        Category_Name_Ar = c.String(nullable: false),
                        Image = c.String(),
                        Visible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Currencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Visible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductPhotoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Path = c.String(),
                        Product_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.Product_Id)
                .Index(t => t.Product_Id);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Name_Ar = c.String(nullable: false),
                        Visible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false),
                        SenderType = c.String(),
                        MessageText = c.String(nullable: false),
                        MessageDateAndTime = c.DateTime(),
                        IsSeen = c.Boolean(nullable: false),
                        RepliedBy_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.RepliedBy_Id)
                .Index(t => t.RepliedBy_Id);
            
            CreateTable(
                "dbo.Reports",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReportDateAndTime = c.DateTime(nullable: false),
                        Seen = c.Boolean(nullable: false),
                        auction_Id = c.Int(),
                        Reporter_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Auctions", t => t.auction_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Reporter_Id)
                .Index(t => t.auction_Id)
                .Index(t => t.Reporter_Id);
            
            CreateTable(
                "dbo.Statistics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserAuctionsCount = c.Int(nullable: false),
                        UserBiddingsCount = c.Int(nullable: false),
                        UserWinningAuctionsCount = c.Int(nullable: false),
                        PaidAuctions = c.Int(nullable: false),
                        LoginsCount = c.Int(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Statistics", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Reports", "Reporter_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Reports", "auction_Id", "dbo.Auctions");
            DropForeignKey("dbo.Messages", "RepliedBy_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Auctions", "Seller_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Auctions", "Product_Id", "dbo.Products");
            DropForeignKey("dbo.ProductPhotoes", "Product_Id", "dbo.Products");
            DropForeignKey("dbo.Products", "Currency_Id", "dbo.Currencies");
            DropForeignKey("dbo.Products", "Category_Id", "dbo.Categories");
            DropForeignKey("dbo.Auctions", "Buyer_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Bids", "Bidder_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Bids", "Auction_Id", "dbo.Auctions");
            DropIndex("dbo.Statistics", new[] { "User_Id" });
            DropIndex("dbo.Reports", new[] { "Reporter_Id" });
            DropIndex("dbo.Reports", new[] { "auction_Id" });
            DropIndex("dbo.Messages", new[] { "RepliedBy_Id" });
            DropIndex("dbo.ProductPhotoes", new[] { "Product_Id" });
            DropIndex("dbo.Products", new[] { "Currency_Id" });
            DropIndex("dbo.Products", new[] { "Category_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "User_Id" });
            DropIndex("dbo.Bids", new[] { "Bidder_Id" });
            DropIndex("dbo.Bids", new[] { "Auction_Id" });
            DropIndex("dbo.Auctions", new[] { "Seller_Id" });
            DropIndex("dbo.Auctions", new[] { "Product_Id" });
            DropIndex("dbo.Auctions", new[] { "Buyer_Id" });
            DropTable("dbo.Statistics");
            DropTable("dbo.Reports");
            DropTable("dbo.Messages");
            DropTable("dbo.Countries");
            DropTable("dbo.ProductPhotoes");
            DropTable("dbo.Currencies");
            DropTable("dbo.Categories");
            DropTable("dbo.Products");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Bids");
            DropTable("dbo.Auctions");
        }
    }
}
