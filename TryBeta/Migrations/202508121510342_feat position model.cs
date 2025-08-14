namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class featpositionmodel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Positions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Industries", "Title", c => c.String());
            DropColumn("dbo.Industries", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Industries", "Name", c => c.String());
            DropColumn("dbo.Industries", "Title");
            DropTable("dbo.Positions");
        }
    }
}
