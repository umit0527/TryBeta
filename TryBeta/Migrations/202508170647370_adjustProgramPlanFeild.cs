namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustProgramPlanFeild : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProgramPlans", "Intro", c => c.String(nullable: false, maxLength: 1000));
            AlterColumn("dbo.ProgramPlans", "ContactName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.ProgramPlans", "ContactPhone", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.ProgramPlans", "ContactEmail", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProgramPlans", "ContactEmail", c => c.String(maxLength: 100));
            AlterColumn("dbo.ProgramPlans", "ContactPhone", c => c.String(maxLength: 50));
            AlterColumn("dbo.ProgramPlans", "ContactName", c => c.String(maxLength: 50));
            AlterColumn("dbo.ProgramPlans", "Intro", c => c.String(maxLength: 1000));
        }
    }
}
