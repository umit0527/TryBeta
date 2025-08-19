namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixuserlogin500 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ParticipantInfoes", "CityId", "dbo.Cities");
            DropForeignKey("dbo.ParticipantInfoes", "DistrictId", "dbo.Districts");
            AddForeignKey("dbo.ParticipantInfoes", "CityId", "dbo.Cities", "Id");
            AddForeignKey("dbo.ParticipantInfoes", "DistrictId", "dbo.Districts", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantInfoes", "DistrictId", "dbo.Districts");
            DropForeignKey("dbo.ParticipantInfoes", "CityId", "dbo.Cities");
            AddForeignKey("dbo.ParticipantInfoes", "DistrictId", "dbo.Districts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ParticipantInfoes", "CityId", "dbo.Cities", "Id", cascadeDelete: true);
        }
    }
}
