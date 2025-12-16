namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPaymentController : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CompanyPlanOrders", "LastCardNum", c => c.String(maxLength: 4));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CompanyPlanOrders", "LastCardNum", c => c.String(nullable: false, maxLength: 4));
        }
    }
}
