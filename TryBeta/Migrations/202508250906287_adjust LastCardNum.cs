namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustLastCardNum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyPlanOrders", "Card4No", c => c.String(maxLength: 4));
            DropColumn("dbo.CompanyPlanOrders", "LastCardNum");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CompanyPlanOrders", "LastCardNum", c => c.String(maxLength: 4));
            DropColumn("dbo.CompanyPlanOrders", "Card4No");
        }
    }
}
