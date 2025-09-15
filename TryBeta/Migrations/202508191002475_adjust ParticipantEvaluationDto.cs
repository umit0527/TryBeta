namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustParticipantEvaluationDto : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ParticipantReviews", newName: "ParticipantEvaluations");
            AddColumn("dbo.ParticipantEvaluations", "CreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.ParticipantEvaluations", "UpdatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.ProgramSubmits", "CancelReason", c => c.String(maxLength: 500));
            DropColumn("dbo.ProgramSubmits", "Note");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProgramSubmits", "Note", c => c.String(maxLength: 500));
            DropColumn("dbo.ProgramSubmits", "CancelReason");
            DropColumn("dbo.ParticipantEvaluations", "UpdatedAt");
            DropColumn("dbo.ParticipantEvaluations", "CreatedAt");
            RenameTable(name: "dbo.ParticipantEvaluations", newName: "ParticipantReviews");
        }
    }
}
