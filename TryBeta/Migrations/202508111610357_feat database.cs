namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class featdatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompanyContacts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        JobTitle = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 200),
                        Phone = c.String(nullable: false, maxLength: 50),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompanyInfoes", t => t.CompanyId, cascadeDelete: true)
                .Index(t => t.CompanyId);
            
            CreateTable(
                "dbo.CompanyInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        IndustryId = c.Int(nullable: false),
                        Tax = c.Int(nullable: false),
                        Address = c.String(nullable: false, maxLength: 200),
                        Website = c.String(),
                        Intro = c.String(nullable: false, maxLength: 1000),
                        ScaleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Role = c.String(nullable: false, maxLength: 20),
                        Account = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 100),
                        PasswordHash = c.String(nullable: false, maxLength: 200),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email, unique: true, name: "IX_Unique_Email");
            
            CreateTable(
                "dbo.CompanyImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        Type = c.String(nullable: false, maxLength: 100),
                        ImgPath = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompanyInfoes", t => t.CompanyId, cascadeDelete: true)
                .Index(t => t.CompanyId);
            
            CreateTable(
                "dbo.CompanyScales",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmployeeNum = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CompanyImages", "CompanyId", "dbo.CompanyInfoes");
            DropForeignKey("dbo.CompanyContacts", "CompanyId", "dbo.CompanyInfoes");
            DropForeignKey("dbo.CompanyInfoes", "UserId", "dbo.Users");
            DropIndex("dbo.CompanyImages", new[] { "CompanyId" });
            DropIndex("dbo.Users", "IX_Unique_Email");
            DropIndex("dbo.CompanyInfoes", new[] { "UserId" });
            DropIndex("dbo.CompanyContacts", new[] { "CompanyId" });
            DropTable("dbo.CompanyScales");
            DropTable("dbo.CompanyImages");
            DropTable("dbo.Users");
            DropTable("dbo.CompanyInfoes");
            DropTable("dbo.CompanyContacts");
        }
    }
}
