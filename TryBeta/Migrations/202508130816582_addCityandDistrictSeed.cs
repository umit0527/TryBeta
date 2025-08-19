namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCityandDistrictSeed : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ResumeLinks", newName: "PortfolioFiles");
            CreateTable(
                "dbo.PortfolioLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResumeId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 100),
                        Url = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.ResumeId);
            
            AddColumn("dbo.PortfolioFiles", "FileSize", c => c.String(maxLength: 50));
            DropColumn("dbo.PortfolioFiles", "Url");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PortfolioFiles", "Url", c => c.String(maxLength: 500));
            DropIndex("dbo.PortfolioLinks", new[] { "ResumeId" });
            DropColumn("dbo.PortfolioFiles", "FileSize");
            DropTable("dbo.PortfolioLinks");
            RenameTable(name: "dbo.PortfolioFiles", newName: "ResumeLinks");
        }
    }
}
