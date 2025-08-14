namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustPlanSeed : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CompanyPlanOrders", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CompanyPlanOrders", "EndDate", c => c.DateTime(nullable: false));
        }
    }
}
