namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixcompanyPlanOrder : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.CompanyPlanOrders", new[] { "OrderNum" });
            AlterColumn("dbo.CompanyPlanOrders", "OrderNum", c => c.String(nullable: false, maxLength: 50));
            CreateIndex("dbo.CompanyPlanOrders", "OrderNum", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.CompanyPlanOrders", new[] { "OrderNum" });
            AlterColumn("dbo.CompanyPlanOrders", "OrderNum", c => c.String(nullable: false));
            CreateIndex("dbo.CompanyPlanOrders", "OrderNum", unique: true);
        }
    }
}
