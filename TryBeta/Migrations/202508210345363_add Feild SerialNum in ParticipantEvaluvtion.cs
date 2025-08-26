namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFeildSerialNuminParticipantEvaluvtion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantEvaluations", "SerialNum", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantEvaluations", "SerialNum");
        }
    }
}
