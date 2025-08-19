namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustPlanUsageStatusFeild : DbMigration
    {
        public override void Up()
        {
            //CreateTable(
            //    "dbo.PlanUsageStatus",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Title = c.String(nullable: false, maxLength: 50),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //AddColumn("dbo.PlanUsages", "StatusId", c => c.Int(nullable: false));
            CreateIndex("dbo.PlanUsages", "StatusId");
            AddForeignKey("dbo.PlanUsages", "StatusId", "dbo.PlanUsageStatus", "Id", cascadeDelete: true);
            //DropColumn("dbo.PlanUsages", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlanUsages", "Status", c => c.String(nullable: false, maxLength: 20));
            DropForeignKey("dbo.PlanUsages", "StatusId", "dbo.PlanUsageStatus");
            DropIndex("dbo.PlanUsages", new[] { "StatusId" });
            DropColumn("dbo.PlanUsages", "StatusId");
            DropTable("dbo.PlanUsageStatus");
        }
    }
}
