namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustUserStats : DbMigration
    {
        public override void Up()
        {
            //CreateTable(
            //    "dbo.UserStatus",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Title = c.String(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Id);

            AlterColumn("dbo.Users", "StatusId", c => c.Int(nullable: false));
            CreateIndex("dbo.Users", "StatusId");
            AddForeignKey("dbo.Users", "StatusId", "dbo.UserStatus", "Id", cascadeDelete: true);
            //DropColumn("dbo.Users", "Status");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.Users", "Status", c => c.String(nullable: false));
            DropForeignKey("dbo.Users", "StatusId", "dbo.UserStatus");
            DropIndex("dbo.Users", new[] { "StatusId" });
            DropColumn("dbo.Users", "StatusId");
            DropTable("dbo.UserStatus");
        }
    }
}
