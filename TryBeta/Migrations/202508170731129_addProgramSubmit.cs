namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProgramSubmit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramSubmits",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProgramPlanId = c.Int(nullable: false),
                    ParticipantId = c.Int(nullable: false),
                    ParticipantsCount = c.Int(nullable: false),
                    Note = c.String(maxLength: 500),
                    SubmitAt = c.DateTime(nullable: false),
                    StatusId = c.Int(nullable: false), // 使用 StatusId 作為外鍵
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ParticipantInfoes", t => t.ParticipantId, cascadeDelete: true)
                .ForeignKey("dbo.ProgramPlans", t => t.ProgramPlanId, cascadeDelete: true)
                .ForeignKey("dbo.ProgramSubmitStatus", t => t.StatusId)
                .Index(t => t.ProgramPlanId)
                .Index(t => t.ParticipantId)
                .Index(t => t.StatusId);

            CreateTable(
                "dbo.ProgramSubmitStatus",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Title = c.String(nullable: false, maxLength: 100),
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ProgramSubmits", "StatusId", "dbo.ProgramSubmitStatus");
            DropForeignKey("dbo.ProgramSubmits", "ProgramPlanId", "dbo.ProgramPlans");
            DropForeignKey("dbo.ProgramSubmits", "ParticipantId", "dbo.ParticipantInfoes");
            DropIndex("dbo.ProgramSubmits", new[] { "StatusId" });
            DropIndex("dbo.ProgramSubmits", new[] { "ParticipantId" });
            DropIndex("dbo.ProgramSubmits", new[] { "ProgramPlanId" });
            DropTable("dbo.ProgramSubmitStatus");
            DropTable("dbo.ProgramSubmits");
        }

    }
}
