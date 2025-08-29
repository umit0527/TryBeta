namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixPUTReviewParticipant : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProgramSubmits", "ReviewedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProgramSubmits", "ReviewedAt", c => c.DateTime(nullable: false));
        }
    }
}
