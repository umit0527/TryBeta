namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCompanyPlanOrderDto : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyPlanOrders", "OrderNum", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompanyPlanOrders", "OrderNum");
        }
    }
}
