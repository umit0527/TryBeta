namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustmodelfeild : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyInfoes", "Scales_Id", c => c.Int());
            CreateIndex("dbo.CompanyInfoes", "IndustryId");
            CreateIndex("dbo.CompanyInfoes", "Scales_Id");
            AddForeignKey("dbo.CompanyInfoes", "IndustryId", "dbo.Industries", "Id", cascadeDelete: false);
            AddForeignKey("dbo.CompanyInfoes", "Scales_Id", "dbo.CompanyScales", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CompanyInfoes", "Scales_Id", "dbo.CompanyScales");
            DropForeignKey("dbo.CompanyInfoes", "IndustryId", "dbo.Industries");
            DropIndex("dbo.CompanyInfoes", new[] { "Scales_Id" });
            DropIndex("dbo.CompanyInfoes", new[] { "IndustryId" });
            DropColumn("dbo.CompanyInfoes", "Scales_Id");
        }
    }
}
