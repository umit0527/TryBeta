namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addforeignkeyincompanyinfoesscaleidtocompanyscales : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.CompanyInfoes", "ScaleId");
            AddForeignKey("dbo.CompanyInfoes", "ScaleId", "dbo.CompanyScales", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.CompanyInfoes", "ScaleId", "dbo.CompanyScales");
            DropIndex("dbo.CompanyInfoes", new[] { "ScaleId" });
        }
    }
}
