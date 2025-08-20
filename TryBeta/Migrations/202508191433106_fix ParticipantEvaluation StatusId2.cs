namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixParticipantEvaluationStatusId2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus");
            DropIndex("dbo.ParticipantEvaluations", new[] { "StatusId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.ParticipantEvaluations", "StatusId");
            AddForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus", "Id", cascadeDelete: true);
        }
    }
}
