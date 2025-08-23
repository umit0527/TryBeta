namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustProgramPlanFeildType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProgramPlans", "IndustryId", c => c.Int(nullable: false));
            AlterColumn("dbo.ProgramPlans", "StepId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProgramPlans", "StepId", c => c.Int());
            AlterColumn("dbo.ProgramPlans", "IndustryId", c => c.Int());
        }
    }
}
