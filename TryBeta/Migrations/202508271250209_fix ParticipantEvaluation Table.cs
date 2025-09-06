namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixParticipantEvaluationTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantEvaluations", "ProgramSubmitId", c => c.Int());
            CreateIndex("dbo.ParticipantEvaluations", "ProgramSubmitId");
            AddForeignKey("dbo.ParticipantEvaluations", "ProgramSubmitId", "dbo.ProgramSubmits", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantEvaluations", "ProgramSubmitId", "dbo.ProgramSubmits");
            DropIndex("dbo.ParticipantEvaluations", new[] { "ProgramSubmitId" });
            DropColumn("dbo.ParticipantEvaluations", "ProgramSubmitId");
        }
    }
}
