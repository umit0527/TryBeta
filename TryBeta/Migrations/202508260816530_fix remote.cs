namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixremote : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CompanyPlanOrders", "PaidAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CompanyPlanOrders", "PaidAt", c => c.DateTime(nullable: false));
        }
    }
}
