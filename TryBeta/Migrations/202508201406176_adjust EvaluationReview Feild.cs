namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustEvaluationReviewFeild : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EvaluationReviews", "ReviewTypeId", c => c.Int(nullable: false));
            DropColumn("dbo.EvaluationReviews", "ReviewType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EvaluationReviews", "ReviewType", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.EvaluationReviews", "ReviewTypeId");
        }
    }
}
