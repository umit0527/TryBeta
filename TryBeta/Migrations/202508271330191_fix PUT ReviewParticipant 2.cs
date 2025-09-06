namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixPUTReviewParticipant2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ParticipantEvaluations", "ProgramSubmitId", "dbo.ProgramSubmits");
            DropIndex("dbo.ParticipantEvaluations", new[] { "ProgramSubmitId" });
            RenameColumn(table: "dbo.ParticipantEvaluations", name: "ProgramSubmitId", newName: "ProgramSubmit_Id");
            AlterColumn("dbo.ParticipantEvaluations", "ProgramSubmit_Id", c => c.Int());
            CreateIndex("dbo.ParticipantEvaluations", "ProgramSubmit_Id");
            AddForeignKey("dbo.ParticipantEvaluations", "ProgramSubmit_Id", "dbo.ProgramSubmits", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantEvaluations", "ProgramSubmit_Id", "dbo.ProgramSubmits");
            DropIndex("dbo.ParticipantEvaluations", new[] { "ProgramSubmit_Id" });
            AlterColumn("dbo.ParticipantEvaluations", "ProgramSubmit_Id", c => c.Int(nullable: false));
            RenameColumn(table: "dbo.ParticipantEvaluations", name: "ProgramSubmit_Id", newName: "ProgramSubmitId");
            CreateIndex("dbo.ParticipantEvaluations", "ProgramSubmitId");
            AddForeignKey("dbo.ParticipantEvaluations", "ProgramSubmitId", "dbo.ProgramSubmits", "Id", cascadeDelete: true);
        }
    }
}
