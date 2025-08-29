namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixProgramPlan : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProgramPlans", "PlanId", "dbo.Plans");
            DropIndex("dbo.ProgramPlans", new[] { "PlanId" });
            AlterColumn("dbo.ProgramPlans", "PlanId", c => c.Int());
            CreateIndex("dbo.ProgramPlans", "PlanId");
            AddForeignKey("dbo.ProgramPlans", "PlanId", "dbo.Plans", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramPlans", "PlanId", "dbo.Plans");
            DropIndex("dbo.ProgramPlans", new[] { "PlanId" });
            AlterColumn("dbo.ProgramPlans", "PlanId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProgramPlans", "PlanId");
            AddForeignKey("dbo.ProgramPlans", "PlanId", "dbo.Plans", "Id", cascadeDelete: true);
        }
    }
}
