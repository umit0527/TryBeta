namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userregister : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExistingResumes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FilePath = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Identities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        title = c.String(nullable: false, maxLength: 50),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ResumeEducations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResumeId = c.Int(nullable: false),
                        School = c.String(nullable: false, maxLength: 200),
                        Major = c.String(maxLength: 100),
                        Degree = c.String(maxLength: 50),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SimpleResumes", t => t.ResumeId, cascadeDelete: true)
                .Index(t => t.ResumeId);
            
            CreateTable(
                "dbo.SimpleResumes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Intro = c.String(nullable: false, maxLength: 2000),
                        IsActive = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ResumeExperiences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResumeId = c.Int(nullable: false),
                        Company = c.String(nullable: false, maxLength: 200),
                        JobTitle = c.String(nullable: false, maxLength: 100),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        Description = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SimpleResumes", t => t.ResumeId, cascadeDelete: true)
                .Index(t => t.ResumeId);
            
            CreateTable(
                "dbo.ResumeLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResumeId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 100),
                        PortfolioPath = c.String(maxLength: 500),
                        Url = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SimpleResumes", t => t.ResumeId, cascadeDelete: true)
                .Index(t => t.ResumeId);
            
            CreateTable(
                "dbo.ResumeSkills",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResumeId = c.Int(nullable: false),
                        SkillName = c.String(nullable: false, maxLength: 100),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SimpleResumes", t => t.ResumeId, cascadeDelete: true)
                .Index(t => t.ResumeId);
            
            CreateTable(
                "dbo.UserInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Phone = c.String(nullable: false, maxLength: 50),
                        Birthday = c.DateTime(nullable: false),
                        CityId = c.Int(nullable: false),
                        DistrictId = c.Int(nullable: false),
                        Street = c.String(nullable: false, maxLength: 100),
                        IdentityId = c.Int(nullable: false),
                        IdentityElse = c.String(maxLength: 50),
                        Gender = c.Boolean(nullable: false),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Identities", t => t.IdentityId, cascadeDelete: false)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: false)
                .Index(t => t.IdentityId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserInfoes", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserInfoes", "IdentityId", "dbo.Identities");
            DropForeignKey("dbo.ResumeSkills", "ResumeId", "dbo.SimpleResumes");
            DropForeignKey("dbo.ResumeLinks", "ResumeId", "dbo.SimpleResumes");
            DropForeignKey("dbo.ResumeExperiences", "ResumeId", "dbo.SimpleResumes");
            DropForeignKey("dbo.ResumeEducations", "ResumeId", "dbo.SimpleResumes");
            DropForeignKey("dbo.SimpleResumes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Identities", "UserId", "dbo.Users");
            DropForeignKey("dbo.ExistingResumes", "UserId", "dbo.Users");
            DropIndex("dbo.UserInfoes", new[] { "UserId" });
            DropIndex("dbo.UserInfoes", new[] { "IdentityId" });
            DropIndex("dbo.ResumeSkills", new[] { "ResumeId" });
            DropIndex("dbo.ResumeLinks", new[] { "ResumeId" });
            DropIndex("dbo.ResumeExperiences", new[] { "ResumeId" });
            DropIndex("dbo.SimpleResumes", new[] { "UserId" });
            DropIndex("dbo.ResumeEducations", new[] { "ResumeId" });
            DropIndex("dbo.Identities", new[] { "UserId" });
            DropIndex("dbo.ExistingResumes", new[] { "UserId" });
            DropTable("dbo.UserInfoes");
            DropTable("dbo.ResumeSkills");
            DropTable("dbo.ResumeLinks");
            DropTable("dbo.ResumeExperiences");
            DropTable("dbo.SimpleResumes");
            DropTable("dbo.ResumeEducations");
            DropTable("dbo.Identities");
            DropTable("dbo.ExistingResumes");
        }
    }
}
