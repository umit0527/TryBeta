namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addParticipantReviewModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ParticipantReviews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParticipantId = c.Int(nullable: false),
                        ProgramPlanId = c.Int(nullable: false),
                        Score = c.Int(nullable: false),
                        Comment = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ParticipantInfoes", t => t.ParticipantId, cascadeDelete: true)
                .ForeignKey("dbo.ProgramPlans", t => t.ProgramPlanId, cascadeDelete: true)
                .Index(t => t.ParticipantId)
                .Index(t => t.ProgramPlanId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ParticipantReviews", "ProgramPlanId", "dbo.ProgramPlans");
            DropForeignKey("dbo.ParticipantReviews", "ParticipantId", "dbo.ParticipantInfoes");
            DropIndex("dbo.ParticipantReviews", new[] { "ProgramPlanId" });
            DropIndex("dbo.ParticipantReviews", new[] { "ParticipantId" });
            DropTable("dbo.ParticipantReviews");
        }
    }
}
