namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addParticipantLoginApi : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UserInfoes", newName: "ParticipantInfoes");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ParticipantInfoes", newName: "UserInfoes");
        }
    }
}
