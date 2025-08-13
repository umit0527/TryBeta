namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustParticipatDtoName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ParticipantInfoes", "Headshot", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ParticipantInfoes", "Headshot");
        }
    }
}
