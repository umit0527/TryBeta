namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustcommentinProgramSubmitReview : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProgramSubmitReviews", "Comment", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProgramSubmitReviews", "Comment", c => c.String(nullable: false, maxLength: 1000));
        }
    }
}
