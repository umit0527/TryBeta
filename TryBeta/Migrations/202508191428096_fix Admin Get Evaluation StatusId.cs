namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixAdminGetEvaluationStatusId : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ParticipantEvaluations", "StatusId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantEvaluations", "StatusId", "dbo.ProgramPlanStatus");
            DropIndex("dbo.ParticipantEvaluations", new[] { "StatusId" });
        }
    }
}
