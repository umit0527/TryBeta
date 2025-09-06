namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProgramPlansController : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramPlans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 255),
                        Intro = c.String(maxLength: 1000),
                        IndustryId = c.Int(),
                        JobTitleId = c.Int(nullable: false),
                        Address = c.String(maxLength: 255),
                        ContactName = c.String(maxLength: 50),
                        ContactPhone = c.String(maxLength: 50),
                        StepId = c.Int(),
                        ProgramCount = c.Int(nullable: false),
                        PublishStartDate = c.DateTime(nullable: false),
                        PublishDurationDays = c.Int(nullable: false),
                        ProgramStartDate = c.DateTime(nullable: false),
                        ProgramEndDate = c.DateTime(nullable: false),
                        ProgramDurationDays = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 50),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompanyInfoes", t => t.CompanyId, cascadeDelete: true)
                .Index(t => t.CompanyId);
            
            AlterColumn("dbo.Users", "Status", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramPlans", "CompanyId", "dbo.CompanyInfoes");
            DropIndex("dbo.ProgramPlans", new[] { "CompanyId" });
            AlterColumn("dbo.Users", "Status", c => c.Int(nullable: false));
            DropTable("dbo.ProgramPlans");
        }
    }
}
