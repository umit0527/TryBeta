namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cancelProgramPlansFeild : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ProgramPlans", "StepId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProgramPlans", "StepId", c => c.Int(nullable: false));
        }
    }
}
