namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProgramStep : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramSteps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramPlanId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 1000),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramPlans", t => t.ProgramPlanId, cascadeDelete: true)
                .Index(t => t.ProgramPlanId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramSteps", "ProgramPlanId", "dbo.ProgramPlans");
            DropIndex("dbo.ProgramSteps", new[] { "ProgramPlanId" });
            DropTable("dbo.ProgramSteps");
        }
    }
}
