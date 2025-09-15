namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addGooglePost : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "GoogleId", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "GoogleId");
        }
    }
}
