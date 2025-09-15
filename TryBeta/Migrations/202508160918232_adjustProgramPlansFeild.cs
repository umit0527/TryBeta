namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustProgramPlansFeild : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPlans", "PublishEndDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramPlans", "PublishEndDate");
        }
    }
}
