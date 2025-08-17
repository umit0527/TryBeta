namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPostApiParticipantSubmit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramSubmits", "ResumeType", c => c.String(nullable: false, maxLength: 10));
            AddColumn("dbo.ProgramSubmits", "ExistingResumeId", c => c.Int());
            AddColumn("dbo.ProgramSubmits", "SimpleResumeId", c => c.Int());
            AddColumn("dbo.ProgramSubmits", "MotivationContent", c => c.String());
            AddColumn("dbo.ProgramSubmits", "AgreeTerms", c => c.Boolean(nullable: false));
            CreateIndex("dbo.ProgramSubmits", "ExistingResumeId");
            CreateIndex("dbo.ProgramSubmits", "SimpleResumeId");
            AddForeignKey("dbo.ProgramSubmits", "ExistingResumeId", "dbo.ExistingResumes", "Id");
            AddForeignKey("dbo.ProgramSubmits", "SimpleResumeId", "dbo.SimpleResumes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProgramSubmits", "SimpleResumeId", "dbo.SimpleResumes");
            DropForeignKey("dbo.ProgramSubmits", "ExistingResumeId", "dbo.ExistingResumes");
            DropIndex("dbo.ProgramSubmits", new[] { "SimpleResumeId" });
            DropIndex("dbo.ProgramSubmits", new[] { "ExistingResumeId" });
            DropColumn("dbo.ProgramSubmits", "AgreeTerms");
            DropColumn("dbo.ProgramSubmits", "MotivationContent");
            DropColumn("dbo.ProgramSubmits", "SimpleResumeId");
            DropColumn("dbo.ProgramSubmits", "ExistingResumeId");
            DropColumn("dbo.ProgramSubmits", "ResumeType");
        }
    }
}
