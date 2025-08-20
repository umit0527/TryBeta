namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addParticipantEducationParticipantDetail : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ParticipantEducations",
                c => new
                    {
                        ParticipantId = c.Int(nullable: false),
                        SchoolName = c.String(nullable: false, maxLength: 100),
                        Major = c.String(maxLength: 100),
                        StatusId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ParticipantId)
                .ForeignKey("dbo.ParticipantInfoes", t => t.ParticipantId)
                .Index(t => t.ParticipantId);
            
            CreateIndex("dbo.ParticipantInfoes", "CityId");
            CreateIndex("dbo.ParticipantInfoes", "DistrictId");
            AddForeignKey("dbo.ParticipantInfoes", "CityId", "dbo.Cities", "Id", cascadeDelete: false);
            AddForeignKey("dbo.ParticipantInfoes", "DistrictId", "dbo.Districts", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantEducations", "ParticipantId", "dbo.ParticipantInfoes");
            DropForeignKey("dbo.ParticipantInfoes", "DistrictId", "dbo.Districts");
            DropForeignKey("dbo.ParticipantInfoes", "CityId", "dbo.Cities");
            DropIndex("dbo.ParticipantEducations", new[] { "ParticipantId" });
            DropIndex("dbo.ParticipantInfoes", new[] { "DistrictId" });
            DropIndex("dbo.ParticipantInfoes", new[] { "CityId" });
            DropTable("dbo.ParticipantEducations");
        }
    }
}
