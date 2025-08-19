namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustexisitingsimpleresumeisactivefeildtype : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ExistingResumes", "IsActive", c => c.Boolean(nullable: false));
            AlterColumn("dbo.SimpleResumes", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SimpleResumes", "IsActive", c => c.Int(nullable: false));
            AlterColumn("dbo.ExistingResumes", "IsActive", c => c.Int(nullable: false));
        }
    }
}
