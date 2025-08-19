namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class getcompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyInfoes", "TaxIdNum", c => c.String());
            DropColumn("dbo.CompanyInfoes", "Tax");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CompanyInfoes", "Tax", c => c.Int(nullable: false));
            DropColumn("dbo.CompanyInfoes", "TaxIdNum");
        }
    }
}
