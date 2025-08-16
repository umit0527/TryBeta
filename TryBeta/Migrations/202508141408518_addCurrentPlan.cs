namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCurrentPlan : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlanUsages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        PlanId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        RemainingPeople = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompanyInfoes", t => t.CompanyId, cascadeDelete: true)
                .ForeignKey("dbo.Plans", t => t.PlanId, cascadeDelete: true)
                .Index(t => t.CompanyId)
                .Index(t => t.PlanId);
            
            AddColumn("dbo.CompanyPlanOrders", "LastCardNum", c => c.String(nullable: false, maxLength: 4));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlanUsages", "PlanId", "dbo.Plans");
            DropForeignKey("dbo.PlanUsages", "CompanyId", "dbo.CompanyInfoes");
            DropIndex("dbo.PlanUsages", new[] { "PlanId" });
            DropIndex("dbo.PlanUsages", new[] { "CompanyId" });
            DropColumn("dbo.CompanyPlanOrders", "LastCardNum");
            DropTable("dbo.PlanUsages");
        }
    }
}
