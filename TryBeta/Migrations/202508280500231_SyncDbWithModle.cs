namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncDbWithModle : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProgramPlans", "StatusId", "dbo.ProgramPlanStatus");
            DropForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus");
            AddForeignKey("dbo.ProgramPlans", "StatusId", "dbo.ProgramPlanStatus", "Id");
            AddForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus");
            DropForeignKey("dbo.ProgramPlans", "StatusId", "dbo.ProgramPlanStatus");
            AddForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProgramPlans", "StatusId", "dbo.ProgramPlanStatus", "Id", cascadeDelete: true);
        }
    }
}
