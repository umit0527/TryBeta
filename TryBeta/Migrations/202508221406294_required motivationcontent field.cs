namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class requiredmotivationcontentfield : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProgramSubmits", "MotivationContent", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProgramSubmits", "MotivationContent", c => c.String());
        }
    }
}
