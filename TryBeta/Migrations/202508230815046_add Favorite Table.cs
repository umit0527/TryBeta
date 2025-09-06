namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFavoriteTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Favorites",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParticipantId = c.Int(nullable: false),
                        ProgramPlanId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CanceledAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ParticipantInfoes", t => t.ParticipantId, cascadeDelete: true)
                .ForeignKey("dbo.ProgramPlans", t => t.ProgramPlanId, cascadeDelete: true)
                .Index(t => t.ParticipantId)
                .Index(t => t.ProgramPlanId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Favorites", "ProgramPlanId", "dbo.ProgramPlans");
            DropForeignKey("dbo.Favorites", "ParticipantId", "dbo.ParticipantInfoes");
            DropIndex("dbo.Favorites", new[] { "ProgramPlanId" });
            DropIndex("dbo.Favorites", new[] { "ParticipantId" });
            DropTable("dbo.Favorites");
        }
    }
}
