namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class connectParticpantEvaluationStatusIdtoProgramPlanStatus : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ParticipantEvaluations", "StatusId");
            AddForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus");
            DropIndex("dbo.ParticipantEvaluations", new[] { "StatusId" });
        }
    }
}
