namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProgramPlanImages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramPlanImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramPlanId = c.Int(nullable: false),
                        ImgPath = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramPlans", t => t.ProgramPlanId, cascadeDelete: true)
                .Index(t => t.ProgramPlanId);
            
            AddColumn("dbo.ProgramPlans", "ContactEmail", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramPlanImages", "ProgramPlanId", "dbo.ProgramPlans");
            DropIndex("dbo.ProgramPlanImages", new[] { "ProgramPlanId" });
            DropColumn("dbo.ProgramPlans", "ContactEmail");
            DropTable("dbo.ProgramPlanImages");
        }
    }
}
