namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustProgramSubitFeild : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProgramSubmits", "ResumeType", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProgramSubmits", "ResumeType", c => c.String(nullable: false, maxLength: 10));
        }
    }
}
