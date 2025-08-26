namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAPIGETUsersProgram : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ProgramSubmits", "StatusId");
            AddForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramPlanStatus", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramPlanStatus");
            DropIndex("dbo.ProgramSubmits", new[] { "StatusId" });
        }
    }
}
