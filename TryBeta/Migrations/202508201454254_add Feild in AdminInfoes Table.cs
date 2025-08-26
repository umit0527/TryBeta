namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addFeildinAdminInfoesTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdminInfoes", "CreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.AdminInfoes", "UpdatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AdminInfoes", "UpdatedAt");
            DropColumn("dbo.AdminInfoes", "CreatedAt");
        }
    }
}
