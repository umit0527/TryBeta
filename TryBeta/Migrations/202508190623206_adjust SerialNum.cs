namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustSerialNum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramSubmits", "ParticipantSerialNum", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.ProgramSubmits", "ParticipantSerialNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProgramSubmits", "ParticipantSerialNumber", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.ProgramSubmits", "ParticipantSerialNum");
        }
    }
}
