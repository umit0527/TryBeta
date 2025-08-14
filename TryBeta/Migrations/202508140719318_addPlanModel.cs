namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPlanModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompanyPlanOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        PlanId = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PurchaseDate = c.DateTime(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        PaymentStatus = c.String(nullable: false, maxLength: 50),
                        PaymentMethod = c.String(nullable: false, maxLength: 50),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompanyInfoes", t => t.CompanyId, cascadeDelete: true)
                .ForeignKey("dbo.Plans", t => t.PlanId, cascadeDelete: true)
                .Index(t => t.CompanyId)
                .Index(t => t.PlanId);
            
            CreateTable(
                "dbo.Plans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DurationDays = c.Int(nullable: false),
                        MaxParticipants = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CompanyPlanOrders", "PlanId", "dbo.Plans");
            DropForeignKey("dbo.CompanyPlanOrders", "CompanyId", "dbo.CompanyInfoes");
            DropIndex("dbo.CompanyPlanOrders", new[] { "PlanId" });
            DropIndex("dbo.CompanyPlanOrders", new[] { "CompanyId" });
            DropTable("dbo.Plans");
            DropTable("dbo.CompanyPlanOrders");
        }
    }
}
