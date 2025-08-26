namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustCompanyPlanOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyPlanOrders", "OrderStatus", c => c.String(nullable: false, maxLength: 50));

            // 修改 OrderNum 欄位型態，確保可以建立索引
            AlterColumn("dbo.CompanyPlanOrders", "OrderNum", c => c.String(nullable: false, maxLength: 50));

            CreateIndex("dbo.CompanyPlanOrders", "OrderNum", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.CompanyPlanOrders", new[] { "OrderNum" });
            AlterColumn("dbo.CompanyPlanOrders", "OrderNum", c => c.String()); // 回復成舊型態
            DropColumn("dbo.CompanyPlanOrders", "OrderStatus");
        }
    }
}
