namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProgramPlanStatusModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramPlanStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ProgramPlans", "StatusId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProgramPlans", "IndustryId");
            CreateIndex("dbo.ProgramPlans", "JobTitleId");
            CreateIndex("dbo.ProgramPlans", "StatusId");
            AddForeignKey("dbo.ProgramPlans", "IndustryId", "dbo.Industries", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProgramPlans", "JobTitleId", "dbo.Positions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProgramPlans", "StatusId", "dbo.ProgramPlanStatus", "Id", cascadeDelete: true);
            DropColumn("dbo.ProgramPlans", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProgramPlans", "Status", c => c.String(nullable: false, maxLength: 50));
            DropForeignKey("dbo.ProgramPlans", "StatusId", "dbo.ProgramPlanStatus");
            DropForeignKey("dbo.ProgramPlans", "JobTitleId", "dbo.Positions");
            DropForeignKey("dbo.ProgramPlans", "IndustryId", "dbo.Industries");
            DropIndex("dbo.ProgramPlans", new[] { "StatusId" });
            DropIndex("dbo.ProgramPlans", new[] { "JobTitleId" });
            DropIndex("dbo.ProgramPlans", new[] { "IndustryId" });
            DropColumn("dbo.ProgramPlans", "StatusId");
            DropTable("dbo.ProgramPlanStatus");
        }
    }
}
