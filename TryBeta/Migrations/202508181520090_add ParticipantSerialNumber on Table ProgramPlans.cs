namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addParticipantSerialNumberonTableProgramPlans : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramSubmits", "ParticipantSerialNumber", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramSubmits", "ParticipantSerialNumber");
        }
    }
}
