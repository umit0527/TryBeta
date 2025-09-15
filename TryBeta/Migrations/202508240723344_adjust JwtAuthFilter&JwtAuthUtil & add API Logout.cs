namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustJwtAuthFilterJwtAuthUtiladdAPILogout : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TokenBlacklists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Token = c.String(),
                        UserId = c.Int(nullable: false),
                        ExpiredAt = c.DateTime(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TokenBlacklists");
        }
    }
}
