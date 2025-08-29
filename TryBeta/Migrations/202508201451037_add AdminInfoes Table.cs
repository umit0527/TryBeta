namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class addAdminInfoesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdminInfoes",
                c => new
                {
                    UserId = c.Int(nullable: false),  // PK
                    Name = c.String(nullable: false, maxLength: 100)
                })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.AdminInfoes", "UserId", "dbo.Users");
            DropIndex("dbo.AdminInfoes", new[] { "UserId" });
            DropTable("dbo.AdminInfoes");
        }
    }
}
