namespace TryBeta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUrlFeildinProgramPlanImagesTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPlanImages", "Url", c => c.String());
            AlterColumn("dbo.ProgramPlanImages", "ImgPath", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProgramPlanImages", "ImgPath", c => c.String(nullable: false));
            DropColumn("dbo.ProgramPlanImages", "Url");
        }
    }
}
