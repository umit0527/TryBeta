namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTopProgramPlan : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TopProgramPlans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramPlanId = c.Int(nullable: false),
                        Score = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramPlans", t => t.ProgramPlanId, cascadeDelete: true)
                .Index(t => t.ProgramPlanId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TopProgramPlans", "ProgramPlanId", "dbo.ProgramPlans");
            DropIndex("dbo.TopProgramPlans", new[] { "ProgramPlanId" });
            DropTable("dbo.TopProgramPlans");
        }
    }
}
