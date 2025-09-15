namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEvaluationReviewTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EvaluationReviews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EvaluationId = c.Int(nullable: false),
                        ReviewedAt = c.DateTime(nullable: false),
                        ReviewerId = c.Int(nullable: false),
                        ReviewType = c.String(nullable: false, maxLength: 50),
                        Comment = c.String(maxLength: 500),
                        StatusId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ParticipantEvaluations", t => t.EvaluationId, cascadeDelete: false)
                .ForeignKey("dbo.Users", t => t.ReviewerId, cascadeDelete: false)
                .ForeignKey("dbo.ProgramPlanStatus", t => t.StatusId, cascadeDelete: false)
                .Index(t => t.EvaluationId)
                .Index(t => t.ReviewerId)
                .Index(t => t.StatusId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EvaluationReviews", "StatusId", "dbo.ProgramPlanStatus");
            DropForeignKey("dbo.EvaluationReviews", "ReviewerId", "dbo.Users");
            DropForeignKey("dbo.EvaluationReviews", "EvaluationId", "dbo.ParticipantEvaluations");
            DropIndex("dbo.EvaluationReviews", new[] { "StatusId" });
            DropIndex("dbo.EvaluationReviews", new[] { "ReviewerId" });
            DropIndex("dbo.EvaluationReviews", new[] { "EvaluationId" });
            DropTable("dbo.EvaluationReviews");
        }
    }
}
