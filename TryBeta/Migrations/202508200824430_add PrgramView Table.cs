namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPrgramViewTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramViews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramPlanId = c.Int(nullable: false),
                        ViewedAt = c.DateTime(nullable: false),
                        ViewerIp = c.String(),
                        ViewerUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramPlans", t => t.ProgramPlanId, cascadeDelete: true)
                .Index(t => t.ProgramPlanId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramViews", "ProgramPlanId", "dbo.ProgramPlans");
            DropIndex("dbo.ProgramViews", new[] { "ProgramPlanId" });
            DropTable("dbo.ProgramViews");
        }
    }
}
