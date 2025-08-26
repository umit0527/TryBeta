namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProgramSubmitReview : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramSubmitReviews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramSubmitId = c.Int(nullable: false),
                        StatusId = c.Int(nullable: false),
                        Comment = c.String(nullable: false, maxLength: 1000),
                        ReviewedAt = c.DateTime(nullable: false),
                        ReviewerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramSubmits", t => t.ProgramSubmitId, cascadeDelete: true)
                .Index(t => t.ProgramSubmitId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramSubmitReviews", "ProgramSubmitId", "dbo.ProgramSubmits");
            DropIndex("dbo.ProgramSubmitReviews", new[] { "ProgramSubmitId" });
            DropTable("dbo.ProgramSubmitReviews");
        }
    }
}
