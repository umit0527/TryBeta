namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustResumeEducatinFeild : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ResumeEducations", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ResumeEducations", "Status");
        }
    }
}
