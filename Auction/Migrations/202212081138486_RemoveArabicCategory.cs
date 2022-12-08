namespace OnlineAuctionProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveArabicCategory : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Categories", "Category_Name_Ar");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Categories", "Category_Name_Ar", c => c.String(nullable: false));
        }
    }
}
