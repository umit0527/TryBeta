namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProgramRegistration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgramRegistrations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ParticipantInfoes", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.ProgramPlans", t => t.ProgramId, cascadeDelete: true)
                .Index(t => t.ProgramId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.ProgramPlans", "MinPeople", c => c.Int(nullable: false));
            AddColumn("dbo.ProgramPlans", "MaxPeople", c => c.Int(nullable: false));
            DropColumn("dbo.ProgramPlans", "ProgramCount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProgramPlans", "ProgramCount", c => c.Int(nullable: false));
            DropForeignKey("dbo.ProgramRegistrations", "ProgramId", "dbo.ProgramPlans");
            DropForeignKey("dbo.ProgramRegistrations", "UserId", "dbo.ParticipantInfoes");
            DropIndex("dbo.ProgramRegistrations", new[] { "UserId" });
            DropIndex("dbo.ProgramRegistrations", new[] { "ProgramId" });
            DropColumn("dbo.ProgramPlans", "MaxPeople");
            DropColumn("dbo.ProgramPlans", "MinPeople");
            DropTable("dbo.ProgramRegistrations");
        }
    }
}
