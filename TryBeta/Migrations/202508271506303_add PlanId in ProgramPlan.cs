namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPlanIdinProgramPlan : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPlans", "PlanId", c => c.Int());
            CreateIndex("dbo.ProgramPlans", "PlanId");
            AddForeignKey("dbo.ProgramPlans", "PlanId", "dbo.Plans", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramPlans", "PlanId", "dbo.Plans");
            DropIndex("dbo.ProgramPlans", new[] { "PlanId" });
            DropColumn("dbo.ProgramPlans", "PlanId");
        }
    }
}
