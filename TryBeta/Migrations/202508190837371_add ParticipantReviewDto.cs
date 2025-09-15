namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addParticipantReviewDto : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramSubmits", "ReviewedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramSubmits", "ReviewedAt");
        }
    }
}
