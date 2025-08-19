namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixProgramSubmitStatus : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramPlanStatus");
            AddForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramPlanStatus", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramPlanStatus");
            AddForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramPlanStatus", "Id", cascadeDelete: true);
        }
    }
}
