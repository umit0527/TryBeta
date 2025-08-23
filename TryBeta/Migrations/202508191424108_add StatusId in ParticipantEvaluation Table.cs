namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStatusIdinParticipantEvaluationTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantEvaluations", "StatusId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantEvaluations", "StatusId");
        }
    }
}
