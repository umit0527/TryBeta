namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPaidAtinCompanyPlanOrderTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyPlanOrders", "PaidAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompanyPlanOrders", "PaidAt");
        }
    }
}
