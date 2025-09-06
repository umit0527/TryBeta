namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustProgramPlanFeildSerialNum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPlans", "SerialNum", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.ProgramPlans", "SerialNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProgramPlans", "SerialNumber", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.ProgramPlans", "SerialNum");
        }
    }
}
