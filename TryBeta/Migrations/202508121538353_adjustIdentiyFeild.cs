namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustIdentiyFeild : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Identities", "UserId", "dbo.Users");
            DropIndex("dbo.Identities", new[] { "UserId" });
            DropColumn("dbo.Identities", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Identities", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.Identities", "UserId");
            AddForeignKey("dbo.Identities", "UserId", "dbo.Users", "Id");
        }
    }
}
